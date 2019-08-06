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
using System.Text;
using System.Timers;

namespace VRTRIX {
    public enum HANDTYPE
    {
        RIGHT_HAND,
        LEFT_HAND,
        BOTH_HAND,
        NONE
    };
    public enum VRTRIXGloveStatus
    {
        CLOSED,
        NORMAL,
        PAUSED,
        DISCONNECTED,
        MAGANOMALY
    };
    public enum VRTRIXGloveEvent
    {
 	    VRTRIXGloveEvent_None = 0,
 		VRTRIXGloveEvent_Idle = 1,
 		VRTRIXGloveEvent_Connected = 2,
 		VRTRIXGloveEvent_Disconnected = 3,
 		VRTRIXGloveEvent_PortClosed = 4,
 		VRTRIXGloveEvent_LowBattery = 5,
 		VRTRIXGloveEvent_BatteryFull = 6,
 		VRTRIXGloveEvent_Paired = 7,
 		VRTRIXGloveEvent_MagAbnormal = 8,
 		VRTRIXGloveEvent_TrackerConnected = 9,
 		VRTRIXGloveEvent_TrackerDisconnected = 10,
 		VRTRIXGloveEvent_ChannelHopping = 11,
    }
    public class VRTRIXDataWrapper
    {
        //Define Useful Constant
        private const string ReaderImportor = "VRTRIX_IMU";

        //Define Useful Parameters & Variables
        private IntPtr sp;
        private int index;
        private int data_rate;
        private int radio_strength;
        private float battery;
        public HANDTYPE hand_type;
        public int radio_channel;
        private int[] calscore = new int[6];
        private bool port_opened = false;
        private bool[] valid = new bool[6];
        private Quaternion[] data = new Quaternion[16];
        private Quaternion L_thumb_offset = new Quaternion(0.49673f, 0.409576f, 0.286788f, 0.709406f); //(sin(70/2), 0, 0, cos(70/2))*(0, sin(60/2), 0, cos(60/2))
        private Quaternion R_thumb_offset = new Quaternion(-0.49673f, -0.409576f, 0.286788f, 0.709406f); //(sin(-70/2), 0, 0, cos(-70/2))*(0, sin(-60/2), 0, cos(-60/2))
   
        private VRTRIXGloveStatus stat = VRTRIXGloveStatus.CLOSED;

        [StructLayout(LayoutKind.Sequential)]
        public struct VRTRIX_Quat
        {
            public float qx;
            public float qy;
            public float qz;
            public float qw;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ReceivedDataCallback(IntPtr pUserParam, IntPtr ptr, int data_rate, byte radio_strength, IntPtr cal_score_ptr, float battery, int hand_type, int radio_channel);
        public static ReceivedDataCallback receivedDataCallback;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ReceivedEventCallback(IntPtr pUserParam, IntPtr pEvent);
        public static ReceivedEventCallback receivedEventCallback;

        #region Functions API
        /// </summary>
        /// <param name="AdvancedMode">Unlock the yaw of fingers if set true</param>
        /// <returns>The serial port object as IntPtr</returns>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr InitDataGlove(bool AdvancedMode);
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
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void VibratePeriod(IntPtr sp, int msDurationMillisec);
        /// <summary>
        /// Randomly channel hopping.
        /// </summary>
        /// <param name="sp">The serial port object</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void ChannelHopping(IntPtr sp);

        #endregion

        public VRTRIXDataWrapper(bool AdvancedMode, int GloveIndex)
        {
            this.index = GloveIndex;
            sp = InitDataGlove(AdvancedMode);
        }

        public bool Init(HANDTYPE type)
        {
            for (int i = 0; i < 6; i++)
            {
                valid[i] = true;
            }
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
                        stat = VRTRIXGloveStatus.NORMAL;
                    }
                    else
                    {
                        Debug.Log("PORT Open Failed");
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
        public bool ClosePort()
        {
            stat = VRTRIXGloveStatus.CLOSED;
            return (ClosePort(this.sp));
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

        public void RegisterCallBack()
        {
            receivedDataCallback =
            (IntPtr pUserParam, IntPtr ptr, int data_rate, byte radio_strength, IntPtr cal_score_ptr, float battery, int hand_type, int radio_channel) =>
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
                objDataGlove.hand_type = (HANDTYPE)hand_type;
                objDataGlove.radio_channel = radio_channel;
                Marshal.Copy(cal_score_ptr, objDataGlove.calscore, 0, 6);
            };

            receivedEventCallback =
            (IntPtr pUserParam, IntPtr pEvent) =>
            {
                GCHandle handle_consume = (GCHandle)pUserParam;
                VRTRIXDataWrapper objDataGlove = (handle_consume.Target as VRTRIXDataWrapper);
                VRTRIXGloveEvent gloveEvent = (VRTRIXGloveEvent)pEvent;
                if (gloveEvent == VRTRIXGloveEvent.VRTRIXGloveEvent_Connected)
                {
                    objDataGlove.stat= VRTRIXGloveStatus.NORMAL;
                    Debug.Log("VRTRIXGloveEvent_Connected");
                }
                else if (gloveEvent == VRTRIXGloveEvent.VRTRIXGloveEvent_Disconnected)
                {
                    objDataGlove.stat = VRTRIXGloveStatus.DISCONNECTED;
                    Debug.Log("VRTRIXGloveEvent_Disconnected");
                }
                else if (gloveEvent == VRTRIXGloveEvent.VRTRIXGloveEvent_ChannelHopping)
                {
                    Debug.Log("VRTRIXGloveEvent_ChannelHopping");
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
        public VRTRIXGloveStatus GetReceivedStatus()
        {
            return stat;
        }

        public void SetReceivedStatus(VRTRIXGloveStatus stat)
        {
            this.stat = stat;
        }

        public float GetReceivedGestureAngle (VRTRIXBones bone)
        {
            switch (bone)
            {
                case VRTRIXBones.R_Index_2:
                    return (Quaternion.Inverse(data[3]) * data[5]).eulerAngles.z;
                case VRTRIXBones.R_Middle_2:
                    return (Quaternion.Inverse(data[2]) * data[5]).eulerAngles.z;
                case VRTRIXBones.R_Ring_2:
                    return (Quaternion.Inverse(data[1]) * data[5]).eulerAngles.z;
                case VRTRIXBones.R_Pinky_2:
                    return (Quaternion.Inverse(data[0]) * data[5]).eulerAngles.z;
                case VRTRIXBones.R_Thumb_2:
                    return (Quaternion.Inverse(data[4]) * (data[5] * R_thumb_offset)).eulerAngles.z;

                case VRTRIXBones.L_Index_2:
                    return (Quaternion.Inverse(data[3]) * data[5]).eulerAngles.z;
                case VRTRIXBones.L_Middle_2:
                    return (Quaternion.Inverse(data[2]) * data[5]).eulerAngles.z;
                case VRTRIXBones.L_Ring_2:
                    return (Quaternion.Inverse(data[1]) * data[5]).eulerAngles.z;
                case VRTRIXBones.L_Pinky_2:
                    return (Quaternion.Inverse(data[0]) * data[5]).eulerAngles.z;
                case VRTRIXBones.L_Thumb_2:
                    return (Quaternion.Inverse(data[4]) * (data[5] * L_thumb_offset)).eulerAngles.z;

                default:
                    return 0f;
            }
        }
        public int GetReceivedDataRate()
        {
            return data_rate;
        }

        public int GetReceiveRadioStrength()
        {
            return -(int)radio_strength;
        }

        public int GetReceiveRadioChannel()
        {
            return radio_channel;
        }

        public float GetReceiveBattery()
        {
            return battery;
        }
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
        public int GetReceivedCalScoreMean()
        {
            return (calscore[0] + calscore[1] + calscore[2] + calscore[3] + calscore[4] + calscore[5]) / 6;
        }
        public bool DataValidStatus (VRTRIXBones bone)
        {
            switch (bone)
            {
                case VRTRIXBones.R_Index_1:
                    return valid[3];
                case VRTRIXBones.R_Middle_1:
                    return valid[2];
                case VRTRIXBones.R_Ring_1:
                    return valid[1];
                case VRTRIXBones.R_Pinky_1:
                    return valid[0];
                case VRTRIXBones.R_Thumb_1:
                    return valid[4];


                case VRTRIXBones.L_Index_1:
                    return valid[3];
                case VRTRIXBones.L_Middle_1:
                    return valid[2];
                case VRTRIXBones.L_Ring_1:
                    return valid[1];
                case VRTRIXBones.L_Pinky_1:
                    return valid[0];
                case VRTRIXBones.L_Thumb_1:
                    return valid[4];

                default:
                    return true;
            }
        }
        public Quaternion GetReceivedRotation(VRTRIXBones bone)
        {
            switch (bone)
            {
                case VRTRIXBones.R_Forearm:
                    return data[5];
                case VRTRIXBones.R_Hand:
                    return data[5];
                case VRTRIXBones.R_Index_2:
                    return data[3];
                case VRTRIXBones.R_Middle_2:
                    return data[2];
                case VRTRIXBones.R_Ring_2:
                    return data[1];
                case VRTRIXBones.R_Pinky_2:
                    return data[0];
                case VRTRIXBones.R_Thumb_2:
                    return data[15];

                case VRTRIXBones.R_Index_1:
                    return data[9];
                case VRTRIXBones.R_Middle_1:
                    return data[8];
                case VRTRIXBones.R_Ring_1:
                    return data[7];
                case VRTRIXBones.R_Pinky_1:
                    return data[6];
                case VRTRIXBones.R_Thumb_1:
                    return data[10];

                case VRTRIXBones.R_Index_3:
                    return data[14];
                case VRTRIXBones.R_Middle_3:
                    return data[13];
                case VRTRIXBones.R_Ring_3:
                    return data[12];
                case VRTRIXBones.R_Pinky_3:
                    return data[11];
                case VRTRIXBones.R_Thumb_3:
                    return data[4];


                case VRTRIXBones.L_Forearm:
                    return data[5];
                case VRTRIXBones.L_Hand:
                    return data[5];
                case VRTRIXBones.L_Index_2:
                    return data[3];
                case VRTRIXBones.L_Middle_2:
                    return data[2];
                case VRTRIXBones.L_Ring_2:
                    return data[1];
                case VRTRIXBones.L_Pinky_2:
                    return data[0];
                case VRTRIXBones.L_Thumb_2:
                    return data[15];

                case VRTRIXBones.L_Index_1:
                    return data[9];
                case VRTRIXBones.L_Middle_1:
                    return data[8];
                case VRTRIXBones.L_Ring_1:
                    return data[7];
                case VRTRIXBones.L_Pinky_1:
                    return data[6];
                case VRTRIXBones.L_Thumb_1:
                    return data[10];

                case VRTRIXBones.L_Index_3:
                    return data[14];
                case VRTRIXBones.L_Middle_3:
                    return data[13];
                case VRTRIXBones.L_Ring_3:
                    return data[12];
                case VRTRIXBones.L_Pinky_3:
                    return data[11];
                case VRTRIXBones.L_Thumb_3:
                    return data[4];


                default:
                    return Quaternion.identity;
            }

        }

        public void OnSaveCalibration()
        {
            OnSaveCalibration(this.sp);
        }
        
        public void VibratePeriod(int msDurationMillisec)
        {
            VibratePeriod(sp, msDurationMillisec);
        }
        public void OnCloseFingerAlignment(HANDTYPE type)
        {
            OnCloseFingerAlignment(sp);
        }

        public void StartStreaming()
        {
            StartStreaming(sp);
        }

        public void ChannelHopping()
        {
            ChannelHopping(sp);
        }

    }
}


