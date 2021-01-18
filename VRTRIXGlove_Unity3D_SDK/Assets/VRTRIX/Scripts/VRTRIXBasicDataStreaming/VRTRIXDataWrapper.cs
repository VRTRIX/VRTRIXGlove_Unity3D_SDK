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
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

namespace VRTRIX
{
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
        TRYTOCONNECT,
        CONNECTED,
        TRYTORECONNECT,
        DISCONNECTED
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

    //!  VRTRIX Data Glove data wrapper class. 
    /*!
        A wrapper class to communicate with low-level unmanaged C++ API.
    */
    public class VRTRIXDataWrapper
    {
        //Define Useful Constant
        private const string ReaderImportor = "VRTRIXGlove_UnityPlugin";

        //Define Useful Parameters & Variables
        public int glove_index;
        public HANDTYPE hand_type;
        private bool advanced_mode;
        private GLOVEVERSION hardware_version;

        private IntPtr glove;
        private int data_rate;
        private int radio_strength;
        private float battery;
        private int radio_channel;
        private int calscore;
        private bool port_opened = false;
        private Quaternion[] data = new Quaternion[16];
        private VRTRIXGloveStatus stat = VRTRIXGloveStatus.DISCONNECTED;

        //! Quaternion data struction used in unmanaged C++ API.
        [StructLayout(LayoutKind.Sequential)]
        public struct VRTRIX_Quat
        {
            public float qx; //!< x component in quaternion
            public float qy; //!< y component in quaternion
            public float qz; //!< z component in quaternion
            public float qw; //!< w component in quaternion
        }

        //! The delegate data receive function called inside unmanaged C++ API.
        /*! 
         * \param pUserParam Pointer of the user defined parameter which registered previously.
         * \param ptr Array of the data received, where contains all joint rotation values.
         * \param data_rate Data rate per second.
         * \param radio_strength Radio transmission strength in dB
         * \param cal_score_ptr Array of the calibration score received.
         * \param battery Current battery level in percentage.
         * \param hand_type The hand type of current hand pose.
         * \param radio_channel Current radio channel used by wireless transmission.
         */
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ReceivedDataCallback(IntPtr pUserParam, IntPtr ptr, int data_rate, int radio_strength, int cal_score, float battery, HANDTYPE hand_type, int radio_channel);
        public static ReceivedDataCallback receivedDataCallback;


        //! The delegate event receive function called inside unmanaged C++ API.
        /*! 
         * \param pUserParam Pointer of the user defined parameter which registered previously.
         * \param pEvent Enum of current event received.
         */
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ReceivedEventCallback(IntPtr pUserParam, VRTRIXGloveEvent pEvent);
        public static ReceivedEventCallback receivedEventCallback;

        #region Functions API
        /// <summary>
        /// Intialize the data glove and returns the interface pointer.
        /// </summary>
        /// <param name="AdvancedMode">Unlock the yaw of fingers if set true</param>
        /// <param name="HardwareVersion">Specify the data glove hardware version</param>
        /// <param name="type">Hand type.</param>
        /// <returns>The data glove object as IntPtr</returns>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr Init(bool AdvancedMode, GLOVEVERSION HardwareVersion, HANDTYPE type);
        /// <summary>
        /// Connect data glove server and start data streaming
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="glove_id">Data glove index id (from 0 - 15), if anything larger is set,then only one pair of glove is supported</param>
        /// <param name="serverIP">Server IP</param>
        /// <param name="port">Server Port</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void ConnectDataGlove(IntPtr glove, int glove_id, string serverIP, string port);
        /// <summary>
        /// Disconnect from data glove server.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void DisconnectDataGlove(IntPtr glove);
        /// <summary>
        /// Save calibration result to hardware
        /// </summary>
        /// <param name="glove">The data glove object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void OnSaveCalibration(IntPtr glove);
        /// <summary>
        /// Align the close finger pose
        /// </summary>
        /// <param name="glove">The data glove object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void OnCloseFingerAlignment(IntPtr glove);
        /// <summary>
        /// Align the OK finger pose
        /// </summary>
        /// <param name="glove">The data glove object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void OnOkPoseAlignment(IntPtr glove);
        /// <summary>
        /// Set radio channel limit for data gloves(valid upperBound and lowerBound between 0-100)
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="upperBound">The upper bound of radio channel</param>
        /// <param name="lowerBound">The lower bound of radio channel</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetRadioChannelLimit(IntPtr glove, int upperBound, int lowerBound);
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="pUserParam">User defined parameter/pointer passed into plugin interface, which will return in callback function.</param>
        /// <param name="glove">The data glove object</param>
        /// <param name="receivedDataCallback">received data callback.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "RegisterDataCallback")]
        public static extern void RegisterDataCallback(IntPtr pUserParam, IntPtr glove, ReceivedDataCallback receivedDataCallback);
        /// <summary>
        /// Register receiving hardware event callback
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="receivedEventCallback">received event callback.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "RegisterEventCallback")]
        public static extern void RegisterEventCallback(IntPtr glove, ReceivedEventCallback receivedEventCallback);
        /// <summary>
        /// Vibrate the data glove for given time period.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="msDurationMillisec">Vibration duration in milliseconds</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void VibratePeriod(IntPtr glove, int msDurationMillisec);
        /// <summary>
        /// Randomly channel hopping.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void ChannelHopping(IntPtr glove);
        /// <summary>
        /// Set Advanced Mode.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="bIsAdvancedMode">The boolean value to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetAdvancedMode(IntPtr glove, bool bIsAdvancedMode);
        /// <summary>
        /// Set data gloves hardware version.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="version">Data glove hardware version to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetHardwareVersion(IntPtr glove, GLOVEVERSION version);
        /// <summary>
        /// Set Proximal Thumb Offset.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="offset_x">x-axis offset to set</param>
        /// <param name="offset_y">y-axis offset to set</param>
        /// <param name="offset_z">z-axis offset to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetProximalThumbOffset(IntPtr glove, double offset_x, double offset_y, double offset_z);
        /// <summary>
        /// Set Intermediate Thumb Offset.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="offset_x">x-axis offset to set</param>
        /// <param name="offset_y">y-axis offset to set</param>
        /// <param name="offset_z">z-axis offset to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetIntermediateThumbOffset(IntPtr glove, double offset_x, double offset_y, double offset_z);
        /// <summary>
        /// Set Distal Thumb Offset.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="offset_x">x-axis offset to set</param>
        /// <param name="offset_y">y-axis offset to set</param>
        /// <param name="offset_z">z-axis offset to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetDistalThumbOffset(IntPtr glove, double offset_x, double offset_y, double offset_z);
        /// <summary>
        /// Set Thumb Slerp Rate.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="slerp_proximal">thumb proximal joint slerp rate to set</param>
        /// <param name="slerp_middle">thumb middle joint slerp rate to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetThumbSlerpRate(IntPtr glove, double slerp_proximal, double slerp_middle);
        /// <summary>
        /// Set finger spacing when advanced mode is NOT enabled.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="spacing">spacing value to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetFingerSpacing(IntPtr glove, double spacing);
        /// <summary>
        /// Set final finger spacing when fingers are fully bended.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="spacing">spacing value to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetFinalFingerSpacing(IntPtr glove, double spacing);
        /// <summary>
        /// Set finger bend threshold.
        /// </summary>
        /// <param name="glove">The data glove object</param>
        /// <param name="benddown_threshold">threshold value to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetBendDownThreshold(IntPtr glove, double benddown_threshold);
        /// <summary>
        /// Get connected glove count.
        /// </summary>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int GetGloveCount();
        /// <summary>
        /// Get connected glove port info.
        /// </summary>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void GetPortInfo(int index, HANDTYPE type, StringBuilder deviceSN);
        #endregion


        //! Wrapper class construction method
        /*! 
         * \param AdvancedMode Whether the advanced mode is activated
         * \param GloveIndex Data glove index (Maximum is 15, if larger number is set, then only one pair of glove per PC is supported).
         * \param HardwareVersion Data glove hardware version, currently DK1, DK2 and PRO are supported.
         * \param type Data glove hand type.
         */
        public VRTRIXDataWrapper(bool AdvancedMode, GLOVEVERSION HardwareVersion, HANDTYPE Type)
        {
            // Create a data glove object
            glove = Init(AdvancedMode, HardwareVersion, Type);
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
            // Register call back function
            RegisterCallBack();
            if(hand_type == HANDTYPE.LEFT_HAND)
            {
                StringBuilder LHdeviceSN = new StringBuilder(10);
                GetPortInfo(id, HANDTYPE.LEFT_HAND, LHdeviceSN);
                Debug.Log("Try to connect glove index " + id + " IP: " + serverIP + " LHdeviceSN: " + LHdeviceSN);
            }
            else if(hand_type == HANDTYPE.RIGHT_HAND)
            {
                StringBuilder RHdeviceSN = new StringBuilder(10);
                GetPortInfo(id, HANDTYPE.RIGHT_HAND, RHdeviceSN);
                Debug.Log("Try to connect glove index " + id + " IP: " + serverIP + " RHdeviceSN: " + RHdeviceSN);
            }

            ConnectDataGlove(glove, id, serverIP, portNum.ToString());
            stat = VRTRIXGloveStatus.TRYTOCONNECT;
        }

        public void OnDisconnectDataGlove()
        {
            DisconnectDataGlove(glove);
        }

        [MonoPInvokeCallback(typeof(ReceivedDataCallback))]
        public static void OnReceivedData(IntPtr pUserParam, IntPtr ptr, int data_rate, int radio_strength, int cal_score, float battery, HANDTYPE hand_type, int radio_channel)
        {
            GCHandle handle_consume = (GCHandle)pUserParam;
            VRTRIXDataWrapper objDataGlove = (handle_consume.Target as VRTRIXDataWrapper);
            VRTRIX_Quat[] quat = new VRTRIX_Quat[16];
            MarshalUnmananagedArray2Struct<VRTRIX_Quat>(ptr, 16, out quat);
            for (int i = 0; i < 16; i++)
            {
                objDataGlove.data[i] = new Quaternion(quat[i].qx, quat[i].qy, quat[i].qz, quat[i].qw);
                //Debug.Log(hand_type.ToString() + " Received data_rate: " + quat[i].qw.ToString() + "," + quat[i].qx.ToString() + "," + quat[i].qy.ToString() + "," + quat[i].qz.ToString());
            }
            objDataGlove.data_rate = data_rate;
            objDataGlove.radio_strength = radio_strength;
            objDataGlove.battery = battery;
            objDataGlove.hand_type = hand_type;
            objDataGlove.radio_channel = radio_channel;
            objDataGlove.calscore = cal_score;
        }

        [MonoPInvokeCallback(typeof(ReceivedEventCallback))]
        public static void OnReceivedEvent(IntPtr pUserParam, VRTRIXGloveEvent pEvent)
        {
            GCHandle handle_consume = (GCHandle)pUserParam;
            VRTRIXDataWrapper objDataGlove = (handle_consume.Target as VRTRIXDataWrapper);
            if (pEvent == VRTRIXGloveEvent.VRTRIXGloveEvent_Connected)
            {
                objDataGlove.stat = VRTRIXGloveStatus.CONNECTED;
                if (objDataGlove.hand_type == HANDTYPE.RIGHT_HAND)
                {
                    Debug.Log("Right hand event: VRTRIXGloveEvent_Connected");
                }
                else if (objDataGlove.hand_type == HANDTYPE.LEFT_HAND)
                {
                    Debug.Log("Left hand event: VRTRIXGloveEvent_Connected");
                }
            }
            else if (pEvent == VRTRIXGloveEvent.VRTRIXGloveEvent_ConnectServerError)
            {
                objDataGlove.stat = VRTRIXGloveStatus.TRYTORECONNECT;
                objDataGlove.data_rate = 0;
                if (objDataGlove.hand_type == HANDTYPE.RIGHT_HAND)
                {
                    Debug.Log("Right hand event: VRTRIXGloveEvent_TryToReconnect");
                }
                else if (objDataGlove.hand_type == HANDTYPE.LEFT_HAND)
                {
                    Debug.Log("Left hand event: VRTRIXGloveEvent_TryToReconnect");
                }
            }
            else if (pEvent == VRTRIXGloveEvent.VRTRIXGloveEvent_Disconnected)
            {
                objDataGlove.stat = VRTRIXGloveStatus.DISCONNECTED;
                objDataGlove.data_rate = 0;
                if (objDataGlove.hand_type == HANDTYPE.RIGHT_HAND)
                {
                    Debug.Log("Right hand event: VRTRIXGloveEvent_Disconnected");
                }
                else if (objDataGlove.hand_type == HANDTYPE.LEFT_HAND)
                {
                    Debug.Log("Left hand event: VRTRIXGloveEvent_Disconnected");
                }
            }
        }
        //! Register call back function to the C++ umanaged dll.
        public void RegisterCallBack()
        {
            receivedDataCallback = OnReceivedData;
            receivedEventCallback = OnReceivedEvent;

            if (glove != IntPtr.Zero)
            {
                GCHandle handle_reg = GCHandle.Alloc(this);
                IntPtr pUserParam = (IntPtr)handle_reg;
                RegisterDataCallback(pUserParam, glove, receivedDataCallback);
                RegisterEventCallback(glove, receivedEventCallback);
            }
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
        public double GetReceivedGestureAngle(VRTRIXBones bone)
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

        //! Get current radio channel of data glove used
        /*! 
         * \return current radio channel of data glove used.         
         */
        public int GetReceiveRadioChannel()
        {
            return radio_channel;
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
            OnSaveCalibration(glove);
        }

        //! Trigger a haptic vibration for a certain period
        /*! 
         * \param msDurationMillisec vibration period
         */
        public void VibratePeriod(int msDurationMillisec)
        {
            VibratePeriod(glove, msDurationMillisec);
        }

        //! Align current gesture to finger close pose, used for calibration when advanced mode is activated
        /*! 
         * \param type Hand type of data glove
         */
        public void OnCloseFingerAlignment(HANDTYPE type)
        {
            OnCloseFingerAlignment(glove);
        }


        //! Trigger channel switching mannually, only used in testing/debuging.
        public void ChannelHopping()
        {
            ChannelHopping(glove);
        }

        //! Activate advanced mode so that finger's yaw data will be unlocked.
        /*! 
         * \param bIsAdvancedMode Advanced mode will be activated if set to true.
         */
        public void SetAdvancedMode(bool bIsAdvancedMode)
        {
            SetAdvancedMode(glove, bIsAdvancedMode);
        }

        //! Set data gloves hardware version.
        /*! 
         * \param version Data glove hardware version.
         */
        public void SetHardwareVersion(GLOVEVERSION version)
        {
            SetHardwareVersion(glove, version);
        }


        //! Set thumb offset to counteract the difference between hands & gloves sensor installation.
        /*! 
         * \param offset Offset vector to set.
         * \param joint the specific thumb joint to set.
         */
        public void SetThumbOffset(Vector3 offset, VRTRIXBones joint)
        {
            switch (joint)
            {
                case (VRTRIXBones.R_Thumb_1): SetProximalThumbOffset(glove, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.R_Thumb_2): SetIntermediateThumbOffset(glove, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.R_Thumb_3): SetDistalThumbOffset(glove, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.L_Thumb_1): SetProximalThumbOffset(glove, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.L_Thumb_2): SetIntermediateThumbOffset(glove, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.L_Thumb_3): SetDistalThumbOffset(glove, offset.x, offset.y, offset.z); break;
            }
        }

        //! Set thumb slerp rate to counteract the difference between hands & gloves sensor installation.
        /*! 
         * \param slerp_proximal Proximal joint slerp rate to set.
         * \param slerp_middle Middle joint slerp rate to set.
         */
        public void SetThumbSlerpRate(double slerp_proximal, double slerp_middle)
        {
            SetThumbSlerpRate(glove, slerp_proximal, slerp_middle);
        }


        //! Set finger spacing when advanced mode is NOT enabled.
        /*! 
         * \param spacing spacing value to set.
         */
        public void SetFingerSpacing(double spacing)
        {
            SetFingerSpacing(glove, spacing);
        }

        //! Set final finger spacing when fingers are fully bended.
        /*! 
         * \param spacing spacing value to set.
         */
        public void SetFinalFingerSpacing(double spacing)
        {
            SetFinalFingerSpacing(glove, spacing);
        }

        //! Set finger bend threshold.
        /*! 
         * \param threshold threshold value to set.
         */
        public void SetBendDownThreshold(double threshold)
        {
            SetBendDownThreshold(glove, threshold);
        }

        //! Set radio channel limit for data gloves.
        /*! 
         * \param upperBound The upper bound of radio channel.
         * \param lowerBound The lower bound of radio channel.
         */
        public void SetRadioChannelLimit(int upperBound, int lowerBound)
        {
            SetRadioChannelLimit(glove, upperBound, lowerBound);
        }

        private static void MarshalUnmananagedArray2Struct<VRTRIX_Quat>(IntPtr unmanagedArray, int length, out VRTRIX_Quat[] mangagedArray)
        {
            var size = Marshal.SizeOf(typeof(VRTRIX_Quat));
            mangagedArray = new VRTRIX_Quat[length];

            for (int i = 0; i < length; i++)
            {
                IntPtr ins = new IntPtr(unmanagedArray.ToInt64() + i * size);
                mangagedArray[i] = (VRTRIX_Quat)Marshal.PtrToStructure(ins, typeof(VRTRIX_Quat));
            }
        }

    }
}
