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
using System;
using System.Runtime.InteropServices;
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
        PRO
    };
    
    //! Glove connection status.
    /*! Define the glove connection status. */
    public enum VRTRIXGloveStatus
    {
        CLOSED,
        NORMAL,
        PAUSED,
        DISCONNECTED,
        MAGANOMALY
    };

    //! Glove event enum.
    /*! Define the glove events while running. */
    public enum VRTRIXGloveEvent
    {
        VRTRIXGloveEvent_None,
        VRTRIXGloveEvent_Connected,
        VRTRIXGloveEvent_Disconnected,
        VRTRIXGloveEvent_ChannelHopping,
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
        private IntPtr sp;
        private int index;
        private int data_rate;
        private int radio_strength;
        private float battery;
        private HANDTYPE hand_type;
        private int radio_channel;
        private int[] calscore = new int[6];
        private bool port_opened = false;
        private Quaternion[] data = new Quaternion[16];
        private VRTRIXGloveStatus stat = VRTRIXGloveStatus.CLOSED;

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
        public delegate void ReceivedDataCallback(IntPtr pUserParam, IntPtr ptr, int data_rate, byte radio_strength, IntPtr cal_score_ptr, float battery, HANDTYPE hand_type, int radio_channel);
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
        /// <returns>The serial port object as IntPtr</returns>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr Init(bool AdvancedMode, GLOVEVERSION HardwareVersion);
        /// <summary>
        /// Open the serial port
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="glove_id">Data glove index id (from 0 - 15), if anything larger is set,then only one pair of glove is supported</param>
        /// <param name="type">Hand type.</param>
        /// <returns>Whether the port is opened successfully</returns>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool OpenPort(IntPtr sp, int glove_id, HANDTYPE type);
        /// <summary>
        /// Read the data from serial port asynchronously.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <returns>Whether the read process successfully</returns>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void StartStreaming(IntPtr sp);
        /// <summary>
        /// Close the serial port
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <returns>Whether the serial port is closed successfully</returns>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool ClosePort(IntPtr sp);
        /// <summary>
        /// Save calibration result to hardware
        /// </summary>
        /// <param name="sp">The serial port object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void OnSaveCalibration(IntPtr sp);
        /// <summary>
        /// Align the close finger pose
        /// </summary>
        /// <param name="sp">The serial port object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void OnCloseFingerAlignment(IntPtr sp);
        /// <summary>
        /// Align the OK finger pose
        /// </summary>
        /// <param name="sp">The serial port object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void OnOkPoseAlignment(IntPtr sp);
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="pUserParam">User defined parameter/pointer passed into plugin interface, which will return in callback function.</param>
        /// <param name="sp">The serial port object</param>
        /// <param name="receivedDataCallback">received data callback.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "RegisterDataCallback")]
        public static extern void RegisterDataCallback(IntPtr pUserParam, IntPtr sp, ReceivedDataCallback receivedDataCallback);
        /// <summary>
        /// Register receiving hardware event callback
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="receivedEventCallback">received event callback.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "RegisterEventCallback")]
        public static extern void RegisterEventCallback(IntPtr sp, ReceivedEventCallback receivedEventCallback);
        /// <summary>
        /// Vibrate the data glove for given time period.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="msDurationMillisec">Vibration duration in milliseconds</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void VibratePeriod(IntPtr sp, int msDurationMillisec);
        /// <summary>
        /// Randomly channel hopping.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void ChannelHopping(IntPtr sp);
        /// <summary>
        /// Set Advanced Mode.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="bIsAdvancedMode">The boolean value to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetAdvancedMode(IntPtr sp, bool bIsAdvancedMode);
        /// <summary>
        /// Set data gloves hardware version.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="version">Data glove hardware version to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetHardwareVersion(IntPtr sp, GLOVEVERSION version);
        /// <summary>
        /// Set Proximal Thumb Offset.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="offset_x">x-axis offset to set</param>
        /// <param name="offset_y">y-axis offset to set</param>
        /// <param name="offset_z">z-axis offset to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetProximalThumbOffset(IntPtr sp, double offset_x, double offset_y, double offset_z);
        /// <summary>
        /// Set Intermediate Thumb Offset.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="offset_x">x-axis offset to set</param>
        /// <param name="offset_y">y-axis offset to set</param>
        /// <param name="offset_z">z-axis offset to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetIntermediateThumbOffset(IntPtr sp, double offset_x, double offset_y, double offset_z);
        /// <summary>
        /// Set Distal Thumb Offset.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="offset_x">x-axis offset to set</param>
        /// <param name="offset_y">y-axis offset to set</param>
        /// <param name="offset_z">z-axis offset to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetDistalThumbOffset(IntPtr sp, double offset_x, double offset_y, double offset_z);
        /// <summary>
        /// Set Thumb Slerp Rate.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="slerp_proximal">thumb proximal joint slerp rate to set</param>
        /// <param name="slerp_middle">thumb middle joint slerp rate to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetThumbSlerpRate(IntPtr sp, double slerp_proximal, double slerp_middle);
        /// <summary>
        /// Set finger spacing when advanced mode is NOT enabled.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="spacing">spacing value to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetFingerSpacing(IntPtr sp, double spacing);
        /// <summary>
        /// Set final finger spacing when fingers are fully bended.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        /// <param name="spacing">spacing value to set</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void SetFinalFingerSpacing(IntPtr sp, double spacing);
        #endregion


        //! Wrapper class construction method
        /*! 
         * \param AdvancedMode Whether the advanced mode is activated
         * \param GloveIndex Data glove index (Maximum is 15, if larger number is set, then only one pair of glove per PC is supported).
         * \param HardwareVersion Data glove hardware version, currently DK1, DK2 and PRO are supported.
         */
        public VRTRIXDataWrapper(bool AdvancedMode, int GloveIndex, GLOVEVERSION HardwareVersion)
        {
            this.index = GloveIndex;
            sp = Init(AdvancedMode, HardwareVersion);
        }


        //! Initialization method
        /*! 
         * \param type Data glove hand type.
         * \return whether data glove is initialized successfully.         
         */
        public bool Init(HANDTYPE type)
        {
            hand_type = type;
            for (int i = 0; i < 16; i++)
            {
                data[i] = Quaternion.identity;
            }

            if (sp != null)
            {
                try
                {
                    if (type == HANDTYPE.RIGHT_HAND)
                    {
                        Debug.Log("Try to connect RH index: " + index);
                    }
                    else if(type == HANDTYPE.LEFT_HAND)
                    {
                        Debug.Log("Try to connect LH index: " + index);
                    }
                    
                    if (OpenPort(this.sp, index, type))
                    {
                        port_opened = true;
                        stat = VRTRIXGloveStatus.DISCONNECTED;
                    }
                    else
                    {
                        Debug.LogError("PORT Open Failed");
                    }
                    return port_opened;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return port_opened;
        }

        //! Close port to stop data streaming.
        /*! 
         * \return whether data streaming is stopped successfully.         
         */
        public bool ClosePort()
        {
            stat = VRTRIXGloveStatus.CLOSED;
            return (ClosePort(this.sp));
        }

        //! Register call back function to the C++ umanaged dll.
        public void RegisterCallBack()
        {
            receivedDataCallback =
            (IntPtr pUserParam, IntPtr ptr, int data_rate, byte radio_strength, IntPtr cal_score_ptr, float battery, HANDTYPE hand_type, int radio_channel) =>
            {
                GCHandle handle_consume = (GCHandle)pUserParam;
                VRTRIXDataWrapper objDataGlove = (handle_consume.Target as VRTRIXDataWrapper);
                VRTRIX_Quat[] quat = new VRTRIX_Quat[16];
                MarshalUnmananagedArray2Struct<VRTRIX_Quat>(ptr, 16, out quat);
                for (int i = 0; i < 16; i++)
                {
                    objDataGlove.data[i] = new Quaternion(quat[i].qx, quat[i].qy, quat[i].qz, quat[i].qw);
                }
                objDataGlove.data_rate = data_rate;
                objDataGlove.radio_strength = radio_strength;
                objDataGlove.battery = battery;
                objDataGlove.hand_type = hand_type;
                objDataGlove.radio_channel = radio_channel;
                Marshal.Copy(cal_score_ptr, objDataGlove.calscore, 0, 6);
            };

            receivedEventCallback =
            (IntPtr pUserParam, VRTRIXGloveEvent pEvent) =>
            {
                GCHandle handle_consume = (GCHandle)pUserParam;
                VRTRIXDataWrapper objDataGlove = (handle_consume.Target as VRTRIXDataWrapper);
                if (pEvent == VRTRIXGloveEvent.VRTRIXGloveEvent_Connected)
                {
                    objDataGlove.stat= VRTRIXGloveStatus.NORMAL;
                    if (objDataGlove.hand_type == HANDTYPE.RIGHT_HAND)
                    {
                        Debug.Log("Right hand event: VRTRIXGloveEvent_Connected");
                    }
                    else if(objDataGlove.hand_type == HANDTYPE.LEFT_HAND)
                    {
                        Debug.Log("Left hand event: VRTRIXGloveEvent_Connected");
                    }
                }
                else if (pEvent == VRTRIXGloveEvent.VRTRIXGloveEvent_Disconnected)
                {
                    objDataGlove.stat = VRTRIXGloveStatus.DISCONNECTED;
                    if (objDataGlove.hand_type == HANDTYPE.RIGHT_HAND)
                    {
                        Debug.Log("Right hand event: VRTRIXGloveEvent_Disconnected");
                    }
                    else if(objDataGlove.hand_type == HANDTYPE.LEFT_HAND)
                    {
                        Debug.Log("Left hand event: VRTRIXGloveEvent_Disconnected");
                    }
                }
                else if (pEvent == VRTRIXGloveEvent.VRTRIXGloveEvent_ChannelHopping)
                {
                    if (objDataGlove.hand_type == HANDTYPE.RIGHT_HAND)
                    {
                        Debug.Log("Right hand event: VRTRIXGloveEvent_ChannelHopping");
                    }
                    else if(objDataGlove.hand_type == HANDTYPE.LEFT_HAND)
                    {
                        Debug.Log("Left hand event: VRTRIXGloveEvent_ChannelHopping");
                    }
                }
            };

            if (sp != IntPtr.Zero)
            {
                GCHandle handle_reg = GCHandle.Alloc(this);
                IntPtr pUserParam = (IntPtr)handle_reg;
                RegisterDataCallback(pUserParam, this.sp, receivedDataCallback);
                RegisterEventCallback(this.sp, receivedEventCallback);
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
        public float GetReceivedGestureAngle (VRTRIXBones bone)
        {
            Quaternion fingerOffest = Quaternion.identity;
            if (bone == VRTRIXBones.L_Thumb_2)
            {
                fingerOffest = new Quaternion(0.49673f, 0.409576f, 0.286788f, 0.709406f); //(sin(70/2), 0, 0, cos(70/2))*(0, sin(60/2), 0, cos(60/2))
            }
            else if(bone == VRTRIXBones.R_Thumb_2)
            {
                fingerOffest = new Quaternion(-0.49673f, -0.409576f, 0.286788f, 0.709406f); //(sin(-70/2), 0, 0, cos(-70/2))*(0, sin(-60/2), 0, cos(-60/2))
            }
            return ((int)bone < 16) ? (Quaternion.Inverse(data[(int)bone]) * (data[(int)VRTRIXBones.R_Hand] * fingerOffest)).eulerAngles.z :
                                      (Quaternion.Inverse(data[(int)bone - 16]) * (data[(int)VRTRIXBones.L_Hand - 16] * fingerOffest)).eulerAngles.z ;
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
            return -(int)radio_strength;
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
            switch (bone)
            {
                case VRTRIXBones.R_Index_2:
                    return calscore[3];
                case VRTRIXBones.R_Middle_2:
                    return calscore[2];
                case VRTRIXBones.R_Ring_2:
                    return calscore[1];
                case VRTRIXBones.R_Pinky_2:
                    return calscore[0];
                case VRTRIXBones.R_Thumb_2:
                    return calscore[4];
                case VRTRIXBones.R_Hand:
                    return calscore[5];

                case VRTRIXBones.L_Index_2:
                    return calscore[3];
                case VRTRIXBones.L_Middle_2:
                    return calscore[2];
                case VRTRIXBones.L_Ring_2:
                    return calscore[1];
                case VRTRIXBones.L_Pinky_2:
                    return calscore[0];
                case VRTRIXBones.L_Thumb_2:
                    return calscore[4];
                case VRTRIXBones.L_Hand:
                    return calscore[5];

                default:
                    return 0;
            }
        }

        //! Get current calibration score average value
        /*! 
         * \return current calibration score average value.
         */
        public int GetReceivedCalScoreMean()
        {
            return (calscore[0] + calscore[1] + calscore[2] + calscore[3] + calscore[4] + calscore[5]) / 6;
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
            OnSaveCalibration(this.sp);
        }
        
        //! Trigger a haptic vibration for a certain period
        /*! 
         * \param msDurationMillisec vibration period
         */
        public void VibratePeriod(int msDurationMillisec)
        {
            VibratePeriod(sp, msDurationMillisec);
        }

        //! Align current gesture to finger close pose, used for calibration when advanced mode is activated
        /*! 
         * \param type Hand type of data glove
         */
        public void OnCloseFingerAlignment(HANDTYPE type)
        {
            OnCloseFingerAlignment(sp);
        }
        
        //! Start data streaming of data glove
        public void StartStreaming()
        {
            StartStreaming(sp);
        }
        
        //! Trigger channel switching mannually, only used in testing/debuging.
        public void ChannelHopping()
        {
            ChannelHopping(sp);
        }
        
        //! Activate advanced mode so that finger's yaw data will be unlocked.
        /*! 
         * \param bIsAdvancedMode Advanced mode will be activated if set to true.
         */
        public void SetAdvancedMode(bool bIsAdvancedMode)
        {
            SetAdvancedMode(sp, bIsAdvancedMode);
        }

        //! Set data gloves hardware version.
        /*! 
         * \param version Data glove hardware version.
         */
        public void SetHardwareVersion(GLOVEVERSION version)
        {
            SetHardwareVersion(sp, version);
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
                case (VRTRIXBones.R_Thumb_1): SetProximalThumbOffset(sp, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.R_Thumb_2): SetIntermediateThumbOffset(sp, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.R_Thumb_3): SetDistalThumbOffset(sp, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.L_Thumb_1): SetProximalThumbOffset(sp, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.L_Thumb_2): SetIntermediateThumbOffset(sp, offset.x, offset.y, offset.z); break;
                case (VRTRIXBones.L_Thumb_3): SetDistalThumbOffset(sp, offset.x, offset.y, offset.z); break;
            }
        }
        
        //! Set thumb slerp rate to counteract the difference between hands & gloves sensor installation.
        /*! 
         * \param slerp_proximal Proximal joint slerp rate to set.
         * \param slerp_middle Middle joint slerp rate to set.
         */
        public void SetThumbSlerpRate(double slerp_proximal, double slerp_middle)
        {
            SetThumbSlerpRate(sp, slerp_proximal, slerp_middle);
        }


        //! Set finger spacing when advanced mode is NOT enabled.
        /*! 
         * \param spacing spacing value to set.
         */
        public void SetFingerSpacing(double spacing)
        {
            SetFingerSpacing(sp, spacing);
        }

        //! Set final finger spacing when fingers are fully bended.
        /*! 
         * \param spacing spacing value to set.
         */
        public void SetFinalFingerSpacing(double spacing)
        {
            SetFinalFingerSpacing(sp, spacing);
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


