//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: VRTRIX Data Glove CSharp Wrapper Class. Used to read Sensor Data
//          from low-level C++ dll and hardware. Useful APIs are provided to
//          read sensor data/ check hardware status/ vibration etc.
//          To see more detailed docs, please refer to our github wiki page:
//          https://github.com/VRTRIX/VRTRIXGlove_Unity3D_SDK/wiki
//          Updating Continously...
//
//=============================================================================
using AOT;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Timers;
using UnityEngine;


namespace VRTRIX {
    //! GloveIndex enum.
    /*! Enum of supported gloves hardware index. */
    public enum GloveIndex
	{
		None = -1,
        Device0 = 0,
		Device1 = 1,
		Device2 = 2,
		Device3 = 3,
		Device4 = 4,
		Device5 = 5,
		Device6 = 6,
		Device7 = 7,
		Device8 = 8,
		Device9 = 9,
		Device10 = 10,
		Device11 = 11,
		Device12 = 12,
		Device13 = 13,
		Device14 = 14,
	 	Device15 = 15,
        MaxDeviceCount = 16
    }

    //! Hand type enum.
    /*! The chirality of the hand, used to identify data glove attribute. */
    public enum HANDTYPE
    {
        NONE,
        OTHER_HAND,
        LEFT_HAND,
        RIGHT_HAND,
        BOTH_HAND,  // Currently not supported
    };

    //! Glove hardware version.
    /*! Supported hardware version, currently DK1, DK2 & PRO are supported. */
    public enum GLOVEVERSION
    {
        DK1,
        DK2,
        PRO,
        PRO7,
        PRO11,
        PRO12,
    };
    
    //! Glove connection status.
    /*! Define the glove connection status. */
    public enum VRTRIXGloveStatus
    {
        CLOSED,
        CONNECTED,
        TRYTORECONNECT,
        DISCONNECTED,
        MAGANOMALY
    };

    //! Glove event enum.
    /*! Define the glove events while running. */
    public enum VRTRIXGloveEvent
    {
        VRTRIXGloveEvent_None,
        VRTRIXGloveEvent_Connected,
        VRTRIXGloveEvent_ConnectServerError,
        VRTRIXGloveEvent_Disconnected,
        VRTRIXGloveEvent_LowBattery,
        VRTRIXGloveEvent_BatteryFull,
        VRTRIXGloveEvent_Paired,
        VRTRIXGloveEvent_MagAbnormal,
    }

    public enum CommandType
    {
        CMD_STOP,
        CMD_TriggerHaptics,
        CMD_ToggleHaptics,
        CMD_HardwareCalibration,
        CMD_TPoseCalibration,
        CMD_SetAdvancedMode,
        CMD_SetHardwareVersion,
        CMD_SetRadioLimit,
        CMD_ChannelHopping,
        CMD_AlgorithmTuning,  
    };

    //! Algorithm config type enum.
    /*!	Enum values to pass into algorithm tuning methods.*/
    public enum AlgorithmConfig
    {
        AlgorithmConfig_ProximalSlerpUp,
        AlgorithmConfig_ProximalSlerpDown,
        AlgorithmConfig_DistalSlerpUp,
        AlgorithmConfig_DistalSlerpDown,
        AlgorithmConfig_FingerSpcaing,
        AlgorithmConfig_FingerBendUpThreshold,
        AlgorithmConfig_FingerBendDownThreshold,
        AlgorithmConfig_ThumbOffset,
        AlgorithmConfig_FinalFingerSpacing,
        AlgorithmConfig_ThumbSlerpOffset,
        AlgorithmConfig_Max = 10,
    };

    // State object for receiving data from remote device.  
    public class StateObject
    {
        // Client socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 273;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
    }

    //!  VRTRIX Data Glove data wrapper class. 
    /*!
        A wrapper class to communicate with low-level unmanaged C++ API.
    */
    public class VRTRIXDataWrapper
    {
        private Socket client;
        private IPEndPoint remoteEP;
        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        //Define Useful Parameters & Variables
        public int glove_index;
        public HANDTYPE hand_type;
        private bool advanced_mode;
        private GLOVEVERSION hardware_version;

        private int data_rate;
        private int packet_received;
        private System.Timers.Timer dataCountTimer;
        private int radio_strength;
        private float battery;
        private int calscore;
        private Quaternion[] data = new Quaternion[16];
        private VRTRIXGloveStatus stat = VRTRIXGloveStatus.CLOSED;

        //! Wrapper class construction method
        /*! 
         * \param AdvancedMode Whether the advanced mode is activated
         * \param GloveIndex Data glove index (Maximum is 15, if larger number is set, then only one pair of glove per PC is supported).
         * \param HardwareVersion Data glove hardware version, currently DK1, DK2 and PRO are supported.
         * \param type Data glove hand type.
         */
        public VRTRIXDataWrapper(bool AdvancedMode, GLOVEVERSION HardwareVersion, HANDTYPE Type)
        {
            advanced_mode = AdvancedMode;
            hardware_version = HardwareVersion;
            hand_type = Type;
        }

        //! Initialization method
        /*! 
         * \param type Data glove hand type.
         * \return whether data glove is initialized successfully.         
         */
        public void OnConnectDataGlove(int id, string serverIP)
        {
            glove_index = id;
            // Calculate port according to id
            //Index      Port
            //  0------ 11002
            //  1------ 11003
            //  2------ 11004
            //  3------ 11005
            //  4------ 11006
            //  5------ 11007
            int portNum = 11002 + id;

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket. 
                IPAddress ipAddress = IPAddress.Parse(serverIP);
                remoteEP = new IPEndPoint(ipAddress, portNum);

                // Create a TCP/IP socket.  
                client = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        public void OnDisconnectDataGlove()
        {
            // DisConnect from a remote device.  
            try
            {
                if(stat == VRTRIXGloveStatus.CONNECTED)
                {
                    //Stop timer
                    dataCountTimer.Stop();
                    dataCountTimer.Close();

                    // Release the socket.  
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    Debug.Log("Socket Disconnected.");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
            stat = VRTRIXGloveStatus.DISCONNECTED;
        }



        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  
                client.EndConnect(ar);

                Debug.Log("Socket connected to "+
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();

                stat = VRTRIXGloveStatus.CONNECTED;

                // Receive the response from the remote server.  
                Receive(client);
                receiveDone.WaitOne();

                // Create a 1s timer 
                dataCountTimer = new System.Timers.Timer(1000);
                // Hook up the Elapsed event for the timer.
                dataCountTimer.Elapsed += OnRefreshDataRate;
                dataCountTimer.Enabled = true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                if (stat == VRTRIXGloveStatus.DISCONNECTED) return;

                stat = VRTRIXGloveStatus.TRYTORECONNECT;
                Debug.Log("Try to reconnect socket...");

                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), client);
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    if(bytesRead == StateObject.BufferSize)
                    {
                        ProcessData(state.buffer);
                    }
                    // Signal that all bytes have been received.  
                    receiveDone.Set();

                    Receive(client);
                    receiveDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, byte[] byteData)
        {
            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ProcessData(byte[] dataPacket)
        {
            string header = Encoding.ASCII.GetString(dataPacket, 0, 6);
            if (header == "VRTRIX")
            {
                if (dataPacket[6] == 'L' && hand_type == HANDTYPE.LEFT_HAND)
                {
                    //Debug.Log("On Receive Left Hand Data Packet.");
                    for (int i = 0; i < data.Length; ++i)
                    {
                        Quaternion quat;
                        quat.w = BitConverter.ToSingle(dataPacket, 9 + 16 * i);
                        quat.x = BitConverter.ToSingle(dataPacket, 13 + 16 * i);
                        quat.y = BitConverter.ToSingle(dataPacket, 17 + 16 * i);
                        quat.z = BitConverter.ToSingle(dataPacket, 21 + 16 * i);
                        data[i] = quat;
                    }
                    radio_strength = -BitConverter.ToInt16(dataPacket, 265);
                    battery = BitConverter.ToSingle(dataPacket, 267);
                    calscore = BitConverter.ToInt16(dataPacket, 271);
                    packet_received++;
                }
                else if (dataPacket[6] == 'R' && hand_type == HANDTYPE.RIGHT_HAND)
                {
                    //Debug.Log("On Receive Right Hand Data Packet.");
                    for (int i = 0; i < data.Length; ++i)
                    {
                        Quaternion quat;
                        quat.w = BitConverter.ToSingle(dataPacket, 9 + 16 * i);
                        quat.x = BitConverter.ToSingle(dataPacket, 13 + 16 * i);
                        quat.y = BitConverter.ToSingle(dataPacket, 17 + 16 * i);
                        quat.z = BitConverter.ToSingle(dataPacket, 21 + 16 * i);
                        data[i] = quat;
                    }
                    radio_strength = -BitConverter.ToInt16(dataPacket, 265);
                    battery = BitConverter.ToSingle(dataPacket, 267);
                    calscore = BitConverter.ToInt16(dataPacket, 271);
                    packet_received++;
                }
            }
        }

        private void OnRefreshDataRate(object source, ElapsedEventArgs e)
        {
            //Debug.Log("packet_received: " + packet_received);
            data_rate = packet_received;
            packet_received = 0;
        }

        //! Get data glove status 
        /*! 
         * \return data glove status.         
         */
        public VRTRIXGloveStatus GetReceivedStatus()
        {
            return stat;
        }

        //! Get the angle between specific joint and wrist joint to detect gesture
        /*! 
         * \param bone specific joint of hand.
         * \return the gesture angle for specific joint.         
         */
        public double GetReceivedGestureAngle (VRTRIXBones bone)
        {
            Quaternion finger = GetReceivedRotation(bone);
            Quaternion wrist = Quaternion.identity;
            if (hand_type == HANDTYPE.LEFT_HAND)
            {
                wrist = GetReceivedRotation(VRTRIXBones.L_Hand);
            }
            else if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                wrist = GetReceivedRotation(VRTRIXBones.R_Hand);
            }
            Quaternion offset = Quaternion.Inverse(wrist) * finger;
            return (Mathf.Atan2(2.0f * offset.w * offset.z + 2.0f * offset.x * offset.y, 1 - 2.0f * (offset.z * offset.z + offset.x * offset.x))) * Mathf.Rad2Deg;
        }

        //! Get data rate received per second 
        /*! 
         * \return data rate received per second.         
         */
        public int GetReceivedDataRate()
        {
            return data_rate;
        }

        //! Get radio strength of data glove 
        /*! 
         * \return radio strength of data glove.         
         */
        public int GetReceiveRadioStrength()
        {
            return radio_strength;
        }
        
        //! Get current battery level in percentage of data glove
        /*! 
         * \return current battery level in percentage of data glove.         
         */
        public float GetReceiveBattery()
        {
            return battery;
        }

        //! Get current calibration score for specific IMU sensor
        /*! 
         * \param bone specific joint of hand.
         * \return current calibration score for specific IMU sensor. Lower value of score means better calibration performance.
         */
        public int GetReceivedCalScore(VRTRIXBones bone)
        {
            return calscore;
        }

        //! Get current calibration score average value
        /*! 
         * \return current calibration score average value.
         */
        public int GetReceivedCalScoreMean()
        {
            return calscore;
        }

        //! Get current rotation for specfic joint
        /*! 
         * \param bone specific joint of hand.
         * \return current calibration score average value.
         */
        public Quaternion GetReceivedRotation(VRTRIXBones bone)
        {
            if ((int)bone < (int)VRTRIXBones.L_Hand) return data[(int)bone];
            else if ((int)bone < (int)VRTRIXBones.R_Arm) return data[(int)bone - 16];
            else return Quaternion.identity;
        }

        //! Save calibration parameters to hardware flash
        public void OnSaveCalibration()
        {
            // Send calibration command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_HardwareCalibration), 0, command, 0, 2);
            if(hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if(hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)0), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)0), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();
        }

        //! Trigger a haptic vibration for a certain period
        /*! 
         * \param msDurationMillisec vibration period
         */
        public void VibratePeriod(int msDurationMillisec)
        {
            // Send vibrate command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_TriggerHaptics), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)msDurationMillisec), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)0), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();
        }

        //! Align current gesture to finger close pose, used for calibration when advanced mode is activated
        /*! 
         * \param type Hand type of data glove
         */
        public void OnCloseFingerAlignment(HANDTYPE type)
        {
            // Send t-pose calibration command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_TPoseCalibration), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)0), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)0), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();
        }
        
        //! Activate advanced mode so that finger's yaw data will be unlocked.
        /*! 
         * \param bIsAdvancedMode Advanced mode will be activated if set to true.
         */
        public void SetAdvancedMode(bool bIsAdvancedMode)
        {
            advanced_mode = bIsAdvancedMode;
            // Send set advanced mode command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_SetAdvancedMode), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes(Convert.ToInt16(bIsAdvancedMode)), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)0), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();
        }

        //! Set data gloves hardware version.
        /*! 
         * \param version Data glove hardware version.
         */
        public void SetHardwareVersion(GLOVEVERSION version)
        {
            hardware_version = version;
            // Send set hardware version command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_SetHardwareVersion), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)version), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)0), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();
        }


        //! Set thumb offset to counteract the difference between hands & gloves sensor installation.
        /*! 
         * \param offset Offset vector to set.
         * \param joint the specific thumb joint to set.
         */
        public void SetThumbOffset(Vector3 offset, VRTRIXBones joint)
        {
            // Send set algorithm tuning command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_AlgorithmTuning), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }


            switch (joint)
            {
                case (VRTRIXBones.R_Thumb_1):
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Thumb_1), 0, command, 3, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_ThumbOffset), 0, command, 5, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.x), 0, command, 11, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.y), 0, command, 15, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.z), 0, command, 19, 4);
                        break;
                    }
                case (VRTRIXBones.R_Thumb_2):
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Thumb_2), 0, command, 3, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_ThumbOffset), 0, command, 5, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.x), 0, command, 11, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.y), 0, command, 15, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.z), 0, command, 19, 4);
                        break;
                    }

                case (VRTRIXBones.R_Thumb_3):
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Thumb_3), 0, command, 3, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_ThumbOffset), 0, command, 5, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.x), 0, command, 11, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.y), 0, command, 15, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.z), 0, command, 19, 4);
                        break;
                    }

                case (VRTRIXBones.L_Thumb_1):
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Thumb_1), 0, command, 3, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_ThumbOffset), 0, command, 5, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.x), 0, command, 11, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.y), 0, command, 15, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.z), 0, command, 19, 4);
                        break;
                    }

                case (VRTRIXBones.L_Thumb_2):
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Thumb_2), 0, command, 3, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_ThumbOffset), 0, command, 5, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.x), 0, command, 11, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.y), 0, command, 15, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.z), 0, command, 19, 4);
                        break;
                    }

                case (VRTRIXBones.L_Thumb_3):
                    {
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Thumb_3), 0, command, 3, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_ThumbOffset), 0, command, 5, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.x), 0, command, 11, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.y), 0, command, 15, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((float)offset.z), 0, command, 19, 4);
                        break;
                    }
            }

            Send(client, command);
            sendDone.WaitOne();
        }
        
        //! Set thumb slerp rate to counteract the difference between hands & gloves sensor installation.
        /*! 
         * \param slerp_proximal Proximal joint slerp rate to set.
         * \param slerp_middle Middle joint slerp rate to set.
         */
        public void SetThumbSlerpRate(double slerp_proximal, double slerp_middle)
        {
            // Send set algorithm tuning command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_AlgorithmTuning), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Thumb_1), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_ProximalSlerpDown), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)slerp_proximal), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();

            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_AlgorithmTuning), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Thumb_3), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_DistalSlerpDown), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)slerp_middle), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();
        }


        //! Set finger spacing when advanced mode is NOT enabled.
        /*! 
         * \param spacing spacing value to set.
         */
        public void SetFingerSpacing(double spacing)
        {
            // Send set algorithm tuning command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_AlgorithmTuning), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Hand), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_FingerSpcaing), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)spacing), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();
        }

        //! Set final finger spacing when fingers are fully bended.
        /*! 
         * \param spacing spacing value to set.
         */
        public void SetFinalFingerSpacing(double spacing)
        {
            // Send set algorithm tuning command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_AlgorithmTuning), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)VRTRIXBones.R_Hand), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)AlgorithmConfig.AlgorithmConfig_FinalFingerSpacing), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)spacing), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();
        }

        //! Set radio channel limit for data gloves.
        /*! 
         * \param upperBound The upper bound of radio channel.
         * \param lowerBound The lower bound of radio channel.
         */
        public void SetRadioChannelLimit(int upperBound, int lowerBound)
        {
            // Send set hardware version command to the remote server.  
            var command = new byte[23];
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)CommandType.CMD_SetRadioLimit), 0, command, 0, 2);
            if (hand_type == HANDTYPE.RIGHT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'R'), 0, command, 2, 1);
            }
            else if (hand_type == HANDTYPE.LEFT_HAND)
            {
                Buffer.BlockCopy(BitConverter.GetBytes((char)'L'), 0, command, 2, 1);
            }
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)lowerBound), 0, command, 3, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((UInt16)upperBound), 0, command, 5, 2);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 7, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 11, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 15, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((float)0.0), 0, command, 19, 4);

            Send(client, command);
            sendDone.WaitOne();
        }

    }
}


