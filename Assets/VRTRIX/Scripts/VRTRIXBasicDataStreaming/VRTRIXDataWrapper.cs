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
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Timers;
using System.Text;

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
        MAGANOMALY
    };


    public class VRTRIXDataWrapper
    {
        //Define Useful Constant
        private const string ReaderImportor = "VRTRIX_IMU";
        private const int baud_rate = 1000000;
        private const float SQRT1_2 = 0.70710678118f;
        private const float radToDeg = (float)(180.0 / Math.PI);
        private const float degToRad = (float)(Math.PI / 180.0);
        //Define Useful Parameters & Variables
        private IntPtr sp;
        private int data_rate;
        private bool port_opened = false;
        private bool[] valid = new bool[6];
        private Quaternion[] data = new Quaternion[16];
        private Quaternion[] offset = new Quaternion[16];
        private Quaternion L_thumb_offset = new Quaternion(0.49673f, 0.409576f, 0.286788f, 0.709406f); //(sin(70/2), 0, 0, cos(70/2))*(0, sin(60/2), 0, cos(60/2))
        //private Quaternion L_thumb_offset = new Quaternion(0.556670f, 0.383022f, 0.3213938f, 0.663413f); //(sin(80/2), 0, 0, cos(80/2))*(0, sin(60/2), 0, cos(60/2))
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
        public delegate void ReceivedDataCallback(IntPtr sp, int data_rate);
        public static ReceivedDataCallback receivedDataCallback;


        #region Functions API
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="customedObj">Client defined object. Can be null</param>
        /// <param name="handle">Client defined function.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool get_RH_port(byte[] buf);
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="customedObj">Client defined object. Can be null</param>
        /// <param name="handle">Client defined function.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool get_LH_port(byte[] buf);
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="customedObj">Client defined object. Can be null</param>
        /// <param name="handle">Client defined function.</param>
        /// <returns></returns>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr init_port();
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="customedObj">Client defined object. Can be null</param>
        /// <param name="handle">Client defined function.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool open_port(IntPtr sp, byte[] com_port_name, int baud_rate, HANDTYPE type);
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="customedObj">Client defined object. Can be null</param>
        /// <param name="handle">Client defined function.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int data_write(IntPtr sp, byte[] buf);
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="customedObj">Client defined object. Can be null</param>
        /// <param name="handle">Client defined function.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool close_port(IntPtr sp);
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="customedObj">Client defined object. Can be null</param>
        /// <param name="handle">Client defined function.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void alignment_check(IntPtr sp);
        /// <summary>
        /// Register receiving and parsed frame calculation data callback
        /// </summary>
        /// <param name="customedObj">Client defined object. Can be null</param>
        /// <param name="handle">Client defined function.</param>
        [DllImport(ReaderImportor, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "RegisterDataCallback")]
        public static extern void RegisterDataCallback(IntPtr sp, ReceivedDataCallback receivedDataCallback);
        #endregion

        public VRTRIXDataWrapper()
        {
            
            this.sp = init_port();
        }

        public bool Init(HANDTYPE type)
        {

            byte[] buf = new byte[300];
            for (int i = 0; i < 6; i++)
            {
                valid[i] = true;
            }
            for (int i = 0; i < 16; i++)
            {
                data[i] = Quaternion.identity;
            }
                
            if (type == HANDTYPE.RIGHT_HAND)
            {
                if (get_RH_port(buf))
                {
                    Console.WriteLine("Try to opening RH Port: " + System.Text.Encoding.ASCII.GetString(buf));
                    if (open_port(this.sp, buf, baud_rate, type))
                    {
                        Console.WriteLine("COM_PORT Opened: " + System.Text.Encoding.ASCII.GetString(buf));
                        port_opened = true;
                        stat = VRTRIXGloveStatus.NORMAL;
                    }
                    else
                    {
                        Console.WriteLine("COM_PORT Open Failed");
                    }
                }
            }
            else if (type == HANDTYPE.LEFT_HAND)
            {
                if (get_LH_port(buf))
                {
                    Console.WriteLine("Try to opening LH Port: " + System.Text.Encoding.ASCII.GetString(buf));
                    if (open_port(this.sp, buf, baud_rate, type))
                    {
                        Console.WriteLine("COM_PORT Opened: " + System.Text.Encoding.ASCII.GetString(buf));
                        port_opened = true;
                        stat = VRTRIXGloveStatus.NORMAL;
                    }
                    else
                    {
                        Console.WriteLine("COM_PORT Open Failed");
                    }
                }
            }
            
            return port_opened;
        }
        public bool ClosePort()
        {
            stat = VRTRIXGloveStatus.CLOSED;
            return (close_port(this.sp));
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

        public void receivedData(HANDTYPE type)
        {
            receivedDataCallback =
            (IntPtr ptr, int data_rate) =>
            {
                this.data_rate = data_rate;
                VRTRIX_Quat[] quat = new VRTRIX_Quat[16];
                MarshalUnmananagedArray2Struct<VRTRIX_Quat>(ptr, 16, out quat);
                for (int i = 0; i < 16; i++)
                {
                    data[i] = new Quaternion(quat[i].qx, quat[i].qy, quat[i].qz, quat[i].qw);
                }
            };

            if (this.sp != IntPtr.Zero)
            {
                RegisterDataCallback(this.sp, receivedDataCallback);
            }
        }
        public bool sendData(string s)
        {
            byte[] buf = Encoding.ASCII.GetBytes(s);
            return (data_write(this.sp, buf) == s.Length);
        }
        public bool calibration()
        {
            return sendData("c");
        }

        public bool vibrate()
        {
            return sendData("v");
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
            //UnityEngine.Debug.Log((Quaternion.Inverse(data[5]) * data[3]).eulerAngles.y);
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
        //TODO: Add AlignmentCheck for Righthand
        public void alignmentCheck(HANDTYPE type)
        {
            alignment_check(this.sp);
        }
    }
}


