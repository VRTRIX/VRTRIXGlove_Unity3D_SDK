﻿//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Example CSharp script to read data stream using APIs provided in
//          wrapper class. A simple GUI inluding VRTRIX Digital Glove status and 
//          sensor data is provided by this script.
//
//=============================================================================
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using Valve.VR;

namespace VRTRIX
{
    public class VRTRIXGloveDataStreaming : MonoBehaviour
    {
 
        [Header("VR Settings")]
 
        // An example for a bool
        public bool IsVREnabled = false;
        [DrawIf("IsVREnabled", false)]
        public GameObject LH_ObjectToAlign, RH_ObjectToAlign;

        [DrawIf("IsVREnabled", true)]
        public Vector3 RHTrackerOffset = new Vector3(0.01f, 0, -0.035f);

        [DrawIf("IsVREnabled", true)]
        public Vector3 LHTrackerOffset = new Vector3(-0.01f, 0, -0.035f);

        public bool AdvancedMode;
        public bool IsEnableMultipleGloves;
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
        public GloveIndex Index;
        public Vector3 ql_modeloffset, qr_modeloffset;
        public Vector3[] ql_axisoffset = new Vector3[3];
        public Vector3[] qr_axisoffset = new Vector3[3];
        public VRTRIXDataWrapper LH, RH;
        private GameObject LH_tracker, RH_tracker;
        private VRTRIXGloveGestureRecognition GloveGesture;
        private Thread LH_receivedData, RH_receivedData;
        private Quaternion qloffset, qroffset;
        private bool qloffset_cal, qroffset_cal;
        private VRTRIXGloveGesture LH_Gesture, RH_Gesture = VRTRIXGloveGesture.BUTTONNONE;
        private bool LH_Mode, RH_Mode;
        private Transform[] fingerTransformArray;
        private Matrix4x4 ml_axisoffset, mr_axisoffset;

        void Start()
        {
            if (!IsEnableMultipleGloves)
            {
                Index = GloveIndex.MaxDeviceCount;
            }
            LH = new VRTRIXDataWrapper(AdvancedMode, (int)Index);
            RH = new VRTRIXDataWrapper(AdvancedMode, (int)Index);
            GloveGesture = new VRTRIXGloveGestureRecognition();
            fingerTransformArray = FindFingerTransform();
            for(int i = 0; i < 3; i++)
            {
                ml_axisoffset.SetRow(i, ql_axisoffset[i]);
                mr_axisoffset.SetRow(i, qr_axisoffset[i]);
            }
            ml_axisoffset.SetRow(3, Vector3.forward);
            mr_axisoffset.SetRow(3, Vector3.forward);

            if (IsVREnabled)
            {
                try
                {
                    RH_tracker = CheckDeviceModelName(HANDTYPE.RIGHT_HAND);
                    LH_tracker = CheckDeviceModelName(HANDTYPE.LEFT_HAND);
                }
                catch (Exception e)
                {
                    print("Exception caught: " + e);
                }
            }
        }


        void Update()
        {
            if (RH_Mode && RH.GetReceivedStatus() == VRTRIXGloveStatus.NORMAL)
            {

                if (RH.GetReceivedRotation(VRTRIXBones.R_Hand) != Quaternion.identity && !qroffset_cal)
                {
                    qroffset = CalculateStaticOffset(RH, HANDTYPE.RIGHT_HAND);
                    qroffset_cal = true;
                }

                if (IsVREnabled && RH_tracker != null)
                {
                    SetPosition(VRTRIXBones.R_Arm, RH_tracker.transform.position, RH_tracker.transform.rotation, RHTrackerOffset);
                }
                //以下是设置右手每个骨骼节点全局旋转(global rotation)；
                SetRotation(VRTRIXBones.R_Hand, RH.GetReceivedRotation(VRTRIXBones.R_Hand), RH.DataValidStatus(VRTRIXBones.R_Hand), HANDTYPE.RIGHT_HAND);

                SetRotation(VRTRIXBones.R_Thumb_1, RH.GetReceivedRotation(VRTRIXBones.R_Thumb_1), RH.DataValidStatus(VRTRIXBones.R_Thumb_1), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Thumb_2, RH.GetReceivedRotation(VRTRIXBones.R_Thumb_2), RH.DataValidStatus(VRTRIXBones.R_Thumb_2), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Thumb_3, RH.GetReceivedRotation(VRTRIXBones.R_Thumb_3), RH.DataValidStatus(VRTRIXBones.R_Thumb_3), HANDTYPE.RIGHT_HAND);

                SetRotation(VRTRIXBones.R_Index_1, RH.GetReceivedRotation(VRTRIXBones.R_Index_1), RH.DataValidStatus(VRTRIXBones.R_Index_1), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Index_2, RH.GetReceivedRotation(VRTRIXBones.R_Index_2), RH.DataValidStatus(VRTRIXBones.R_Index_2), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Index_3, RH.GetReceivedRotation(VRTRIXBones.R_Index_3), RH.DataValidStatus(VRTRIXBones.R_Index_3), HANDTYPE.RIGHT_HAND);

                SetRotation(VRTRIXBones.R_Middle_1, RH.GetReceivedRotation(VRTRIXBones.R_Middle_1), RH.DataValidStatus(VRTRIXBones.R_Middle_1), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Middle_2, RH.GetReceivedRotation(VRTRIXBones.R_Middle_2), RH.DataValidStatus(VRTRIXBones.R_Middle_2), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Middle_3, RH.GetReceivedRotation(VRTRIXBones.R_Middle_3), RH.DataValidStatus(VRTRIXBones.R_Middle_3), HANDTYPE.RIGHT_HAND);

                SetRotation(VRTRIXBones.R_Ring_1, RH.GetReceivedRotation(VRTRIXBones.R_Ring_1), RH.DataValidStatus(VRTRIXBones.R_Ring_1), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Ring_2, RH.GetReceivedRotation(VRTRIXBones.R_Ring_2), RH.DataValidStatus(VRTRIXBones.R_Ring_2), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Ring_3, RH.GetReceivedRotation(VRTRIXBones.R_Ring_3), RH.DataValidStatus(VRTRIXBones.R_Ring_3), HANDTYPE.RIGHT_HAND);

                SetRotation(VRTRIXBones.R_Pinky_1, RH.GetReceivedRotation(VRTRIXBones.R_Pinky_1), RH.DataValidStatus(VRTRIXBones.R_Pinky_1), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Pinky_2, RH.GetReceivedRotation(VRTRIXBones.R_Pinky_2), RH.DataValidStatus(VRTRIXBones.R_Pinky_2), HANDTYPE.RIGHT_HAND);
                SetRotation(VRTRIXBones.R_Pinky_3, RH.GetReceivedRotation(VRTRIXBones.R_Pinky_3), RH.DataValidStatus(VRTRIXBones.R_Pinky_3), HANDTYPE.RIGHT_HAND);

                RH_Gesture = GloveGesture.GestureDetection(RH, HANDTYPE.RIGHT_HAND);
            }



            if (LH_Mode && LH.GetReceivedStatus() == VRTRIXGloveStatus.NORMAL)
            {
                if (LH.GetReceivedRotation(VRTRIXBones.L_Hand) != Quaternion.identity && !qloffset_cal)
                {
                    qloffset = CalculateStaticOffset(LH, HANDTYPE.LEFT_HAND);
                    qloffset_cal = true;
                }

                if (IsVREnabled && LH_tracker != null)
                {
                    SetPosition(VRTRIXBones.L_Arm, LH_tracker.transform.position, LH_tracker.transform.rotation, LHTrackerOffset);
                }
                //以下是设置左手每个骨骼节点全局旋转(global rotation)；
                SetRotation(VRTRIXBones.L_Hand, LH.GetReceivedRotation(VRTRIXBones.L_Hand), LH.DataValidStatus(VRTRIXBones.L_Hand), HANDTYPE.LEFT_HAND);

                SetRotation(VRTRIXBones.L_Thumb_1, LH.GetReceivedRotation(VRTRIXBones.L_Thumb_1), LH.DataValidStatus(VRTRIXBones.L_Thumb_1), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Thumb_2, LH.GetReceivedRotation(VRTRIXBones.L_Thumb_2), LH.DataValidStatus(VRTRIXBones.L_Thumb_2), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Thumb_3, LH.GetReceivedRotation(VRTRIXBones.L_Thumb_3), LH.DataValidStatus(VRTRIXBones.L_Thumb_3), HANDTYPE.LEFT_HAND);


                SetRotation(VRTRIXBones.L_Index_1, LH.GetReceivedRotation(VRTRIXBones.L_Index_1), LH.DataValidStatus(VRTRIXBones.L_Index_1), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Index_2, LH.GetReceivedRotation(VRTRIXBones.L_Index_2), LH.DataValidStatus(VRTRIXBones.L_Index_2), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Index_3, LH.GetReceivedRotation(VRTRIXBones.L_Index_3), LH.DataValidStatus(VRTRIXBones.L_Index_3), HANDTYPE.LEFT_HAND);


                SetRotation(VRTRIXBones.L_Middle_1, LH.GetReceivedRotation(VRTRIXBones.L_Middle_1), LH.DataValidStatus(VRTRIXBones.L_Middle_1), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Middle_2, LH.GetReceivedRotation(VRTRIXBones.L_Middle_2), LH.DataValidStatus(VRTRIXBones.L_Middle_2), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Middle_3, LH.GetReceivedRotation(VRTRIXBones.L_Middle_3), LH.DataValidStatus(VRTRIXBones.L_Middle_3), HANDTYPE.LEFT_HAND);


                SetRotation(VRTRIXBones.L_Ring_1, LH.GetReceivedRotation(VRTRIXBones.L_Ring_1), LH.DataValidStatus(VRTRIXBones.L_Ring_1), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Ring_2, LH.GetReceivedRotation(VRTRIXBones.L_Ring_2), LH.DataValidStatus(VRTRIXBones.L_Ring_2), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Ring_3, LH.GetReceivedRotation(VRTRIXBones.L_Ring_3), LH.DataValidStatus(VRTRIXBones.L_Ring_3), HANDTYPE.LEFT_HAND);


                SetRotation(VRTRIXBones.L_Pinky_1, LH.GetReceivedRotation(VRTRIXBones.L_Pinky_1), LH.DataValidStatus(VRTRIXBones.L_Pinky_1), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Pinky_2, LH.GetReceivedRotation(VRTRIXBones.L_Pinky_2), LH.DataValidStatus(VRTRIXBones.L_Pinky_2), HANDTYPE.LEFT_HAND);
                SetRotation(VRTRIXBones.L_Pinky_3, LH.GetReceivedRotation(VRTRIXBones.L_Pinky_3), LH.DataValidStatus(VRTRIXBones.L_Pinky_3), HANDTYPE.LEFT_HAND);

                LH_Gesture = GloveGesture.GestureDetection(LH, HANDTYPE.LEFT_HAND);
            }
        }

        void OnGUI()
        {
            if (IsEnableMultipleGloves) return;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = (int)(25 * (Screen.height / 1440.0));

            if (GetReceivedStatus(HANDTYPE.LEFT_HAND) == VRTRIXGloveStatus.CLOSED && GetReceivedStatus(HANDTYPE.RIGHT_HAND) == VRTRIXGloveStatus.CLOSED)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Connect", buttonStyle))
                {
                    OnConnectGlove();
                }
            }
            else
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Disconnect", buttonStyle))
                {
                    OnDisconnectGlove();
                }
            }

            if (GUI.Button(new Rect(0, Screen.height / 8, Screen.width / 8, Screen.height / 8), "Reset", buttonStyle))
            {
                OnAlignFingers();
            }

            if (!IsVREnabled)
            {
                if (GUI.Button(new Rect(0, Screen.height * (2.0f / 8.0f), Screen.width / 8, Screen.height / 8), "Hardware Calibrate", buttonStyle))
                {
                    OnHardwareCalibrate();
                }
    
                if (GUI.Button(new Rect(0, Screen.height * (3.0f / 8.0f), Screen.width / 8, Screen.height / 8), "Vibrate", buttonStyle))
                {
                    OnVibrate();
                }
    
                if (GUI.Button(new Rect(0, Screen.height * (4.0f / 8.0f), Screen.width / 8, Screen.height / 8), "Channel Hopping", buttonStyle))
                {
                    OnChannelHopping();
                }

            }
        }

        //数据手套初始化，硬件连接
        public void OnConnectGlove()
        {
            if (IsVREnabled && LH_tracker == null && RH_tracker == null) return;
            try
            {
                LH_Mode = LH.Init(HANDTYPE.LEFT_HAND);
                if (LH_Mode)
                {
                    print("Left hand glove connected!");
                    LH.RegisterCallBack();
                    LH.StartStreaming();
                }
                RH_Mode = RH.Init(HANDTYPE.RIGHT_HAND);
                if (RH_Mode)
                {
                    print("Right hand glove connected!");
                    RH.RegisterCallBack();
                    RH.StartStreaming();
                }

            }
            catch (Exception e)
            {
                print("Exception caught: " + e);
            }
        }

        //数据手套反初始化，硬件断开连接
        public void OnDisconnectGlove()
        {
            if (LH_Mode)
            {
                if (LH.ClosePort())
                {
                    LH = new VRTRIXDataWrapper(AdvancedMode, (int)Index);
                }
                LH_Mode = false;
            }
            if (RH_Mode)
            {
                if (RH.ClosePort())
                {
                    RH = new VRTRIXDataWrapper(AdvancedMode, (int)Index);
                }
                RH_Mode = false;
            }
        }

        //数据手套硬件地磁校准数据储存，仅在磁场大幅度变化后使用。
        public void OnHardwareCalibrate()
        {
            if (LH_Mode)
            {
                LH.OnSaveCalibration();
            }
            if (RH_Mode)
            {
                RH.OnSaveCalibration();
            }
        }

        //数据手套振动
        public void OnVibrate()
        {
            if (LH_Mode)
            {
                LH.VibratePeriod(500);
            }
            if (RH_Mode)
            {
                RH.VibratePeriod(500);
            }
        }

        //数据手套手动跳频
        public void OnChannelHopping()
        {
            if (LH_Mode)
            {
                LH.ChannelHopping();
            }
            if (RH_Mode)
            {
                RH.ChannelHopping();
            }
        }

        //数据手套软件对齐手指及设置手背初始方向。
        public void OnAlignFingers()
        {
            if (LH_Mode)
            {
                LH.OnCloseFingerAlignment(HANDTYPE.LEFT_HAND);
                qloffset = CalculateStaticOffset(LH, HANDTYPE.LEFT_HAND);
            }
            if (RH_Mode)
            {
                RH.OnCloseFingerAlignment(HANDTYPE.RIGHT_HAND);
                qroffset = CalculateStaticOffset(RH, HANDTYPE.RIGHT_HAND);
            }
        }

        //程序退出
        void OnApplicationQuit()
        {
            if (LH_Mode && LH.GetReceivedStatus() != VRTRIXGloveStatus.CLOSED)
            {
                LH.ClosePort();
            }
            if (RH_Mode && RH.GetReceivedStatus() != VRTRIXGloveStatus.CLOSED)
            {
                RH.ClosePort();
            }
        }

        //用于计算初始化物体的姿态和手背姿态（由数据手套得到）之间的四元数差值，该方法为静态调用，即只在初始化的时候调用一次，之后所有帧均使用同一个四元数。
        //适用于：当动捕设备没有腕关节/手背节点或者只单独使用手套，无其他定位硬件设备时。
        private Quaternion CalculateStaticOffset(VRTRIXDataWrapper glove, HANDTYPE type)
        {
            if (type == HANDTYPE.RIGHT_HAND)
            {
                if (IsVREnabled)
                {
                    float angle_offset = RH_tracker.transform.rotation.eulerAngles.z;
                    return Quaternion.AngleAxis(-angle_offset, Vector3.forward); 
                }
                else
                {
                    Quaternion rotation = glove.GetReceivedRotation(VRTRIXBones.R_Hand);
                    Vector3 quat_vec = mr_axisoffset.MultiplyVector(new Vector3(rotation.x, rotation.y, rotation.z));
                    rotation = new Quaternion(quat_vec.x, quat_vec.y, quat_vec.z, rotation.w);
                    return RH_ObjectToAlign.transform.rotation * Quaternion.Inverse(rotation);
                }
            }
            else if (type == HANDTYPE.LEFT_HAND)
            {
                if (IsVREnabled)
                {
                    float angle_offset = LH_tracker.transform.rotation.eulerAngles.z;
                    return Quaternion.AngleAxis(-angle_offset, Vector3.forward); 
                }
                else
                {
                    Quaternion rotation = glove.GetReceivedRotation(VRTRIXBones.L_Hand);
                    Vector3 quat_vec = ml_axisoffset.MultiplyVector(new Vector3(rotation.x, rotation.y, rotation.z));
                    rotation = new Quaternion(quat_vec.x, quat_vec.y, quat_vec.z, rotation.w);
                    return LH_ObjectToAlign.transform.rotation * Quaternion.Inverse(rotation);
                }
            }
            else
            {
                return Quaternion.identity;
            }
        }


        //用于计算左手/右手腕关节姿态（由动捕设备得到）和左手手背姿态（由数据手套得到）之间的四元数差值，该方法为动态调用，即每一帧都会调用该计算。
        //适用于：当动捕设备有腕关节/手背节点时
        private Quaternion CalculateDynamicOffset(GameObject tracker, VRTRIXDataWrapper glove, HANDTYPE type)
        {
            //计算场景中角色右手腕在unity世界坐标系下的旋转与手套的右手腕在手套追踪系统中世界坐标系下右手腕的旋转之间的角度差值，意在匹配两个坐标系的方向；
            if (type == HANDTYPE.RIGHT_HAND)
            {
                Quaternion rotation = glove.GetReceivedRotation(VRTRIXBones.R_Hand);
                Vector3 quat_vec = mr_axisoffset.MultiplyVector(new Vector3(rotation.x, rotation.y, rotation.z));
                rotation = new Quaternion(quat_vec.x, quat_vec.y, quat_vec.z, rotation.w);
                Quaternion target =  tracker.transform.rotation * qroffset * Quaternion.Euler(0, -90, 90); 
                return target * Quaternion.Inverse(rotation);
            }

            //计算场景中角色左手腕在unity世界坐标系下的旋转与手套的左手腕在手套追踪系统中世界坐标系下左手腕的旋转之间的角度差值，意在匹配两个坐标系的方向；
            else if (type == HANDTYPE.LEFT_HAND)
            {
                Quaternion rotation = glove.GetReceivedRotation(VRTRIXBones.L_Hand);
                Vector3 quat_vec = ml_axisoffset.MultiplyVector(new Vector3(rotation.x, rotation.y, rotation.z));
                rotation = new Quaternion(quat_vec.x, quat_vec.y, quat_vec.z, rotation.w);
                Quaternion target =  tracker.transform.rotation * qloffset * Quaternion.Euler(0, 90, -90);
                return target * Quaternion.Inverse(rotation);
            }
            else
            {
                return Quaternion.identity;
            }
        }
        
        //手腕关节位置赋值函数，通过手腕外加的定位物体位置计算手部关节位置。（如果模型为全身骨骼，无需使用该函数）
        private void SetPosition(VRTRIXBones bone, Vector3 pos, Quaternion rot, Vector3 offset)
        {
            Transform obj = fingerTransformArray[(int)bone];
            if (obj != null)
            {
                obj.position = pos + rot* offset;
            }
        }

        //手部关节旋转赋值函数，每一帧都会调用，通过从数据手套硬件获取当前姿态，进一步进行处理，然后给模型赋值。
        private void SetRotation(VRTRIXBones bone, Quaternion rotation, bool valid, HANDTYPE type)
        {
            Transform obj = fingerTransformArray[(int)bone];
            if (obj != null)
            {
                if (!float.IsNaN(rotation.x) && !float.IsNaN(rotation.y) && !float.IsNaN(rotation.z) && !float.IsNaN(rotation.w))
                {
                    if (valid)
                    {
                        if (type == HANDTYPE.LEFT_HAND)
                        {
                            Vector3 quat_vec = ml_axisoffset.MultiplyVector(new Vector3(rotation.x, rotation.y, rotation.z));
                            rotation = new Quaternion(quat_vec.x, quat_vec.y, quat_vec.z, rotation.w);
                            if (IsVREnabled)
                            {
                                //当VR环境下，根据固定在手腕上tracker的方向对齐手背方向。
                                obj.rotation = (bone == VRTRIXBones.L_Hand) ? CalculateDynamicOffset(LH_tracker, LH, HANDTYPE.LEFT_HAND) * rotation :
                                                                         CalculateDynamicOffset(LH_tracker, LH, HANDTYPE.LEFT_HAND)* rotation * Quaternion.Euler(ql_modeloffset);
                            }
                            else
                            {
                                //当3D环境下，根据相机视角方向对齐手背方向。
                                obj.rotation = (bone == VRTRIXBones.L_Hand) ? qloffset * rotation :
                                                                         qloffset * rotation * Quaternion.Euler(ql_modeloffset);
                            }
                        }
                        else if (type == HANDTYPE.RIGHT_HAND)
                        {
                            Vector3 quat_vec = mr_axisoffset.MultiplyVector(new Vector3(rotation.x, rotation.y, rotation.z));
                            rotation = new Quaternion(quat_vec.x, quat_vec.y, quat_vec.z, rotation.w);
                            if (IsVREnabled)
                            {
                                obj.rotation = (bone == VRTRIXBones.R_Hand) ? CalculateDynamicOffset(RH_tracker, RH, HANDTYPE.RIGHT_HAND)* rotation :
                                                                                CalculateDynamicOffset(RH_tracker, RH, HANDTYPE.RIGHT_HAND)* rotation * Quaternion.Euler(qr_modeloffset);
                            }
                            else
                            {
                                obj.rotation = (bone == VRTRIXBones.R_Hand) ? qroffset * rotation :
                                                                         qroffset * rotation * Quaternion.Euler(qr_modeloffset);
                            }
                        }
                    }
                }
            }
        }

        public Quaternion GetRotation(VRTRIXBones bone)
        {
            return fingerTransformArray[(int)bone].rotation;
        }

        public int GetCalScore(VRTRIXBones bone)
        {
            if ((int)bone < 19)
            {
                return RH.GetReceivedCalScore(bone);
            }
            else
            {
                return LH.GetReceivedCalScore(bone);
            }
        }

        public int GetReceiveRadioStrength(HANDTYPE type)
        {
            switch (type)
            {
                case HANDTYPE.RIGHT_HAND:
                    {
                        return RH.GetReceiveRadioStrength();
                    }
                case HANDTYPE.LEFT_HAND:
                    {
                        return LH.GetReceiveRadioStrength();
                    }
                default:
                    return 0;
            }
        }

        public int GetReceiveRadioChannel(HANDTYPE type)
        {
            switch (type)
            {
                case HANDTYPE.RIGHT_HAND:
                    {
                        return RH.GetReceiveRadioChannel();
                    }
                case HANDTYPE.LEFT_HAND:
                    {
                        return LH.GetReceiveRadioChannel();
                    }
                default:
                    return 0;
            }
        }
        public float GetBatteryLevel(HANDTYPE type)
        {
            switch (type)
            {
                case HANDTYPE.RIGHT_HAND:
                    {
                        return RH.GetReceiveBattery();
                    }
                case HANDTYPE.LEFT_HAND:
                    {
                        return LH.GetReceiveBattery();
                    }
                default:
                    return 0;
            }
        }

        public int GetReceivedCalScoreMean(HANDTYPE type)
        {
            switch (type)
            {
                case HANDTYPE.RIGHT_HAND:
                    {
                        return RH.GetReceivedCalScoreMean();
                    }
                case HANDTYPE.LEFT_HAND:
                    {
                        return LH.GetReceivedCalScoreMean();
                    }
                default:
                    return 0;
            }
        }

        public int GetReceivedDataRate(HANDTYPE type)
        {
            switch (type)
            {
                case HANDTYPE.RIGHT_HAND:
                    {
                        return RH.GetReceivedDataRate();
                    }
                case HANDTYPE.LEFT_HAND:
                    {
                        return LH.GetReceivedDataRate();
                    }
                default:
                    return 0;
            }
        }

        public bool GetGloveConnectionStat(HANDTYPE type)
        {
            return GetReceivedStatus(type) == VRTRIXGloveStatus.NORMAL;
        }
        public VRTRIXGloveStatus GetReceivedStatus(HANDTYPE type)
        {
            switch (type)
            {
                case HANDTYPE.RIGHT_HAND:
                    {
                        return RH.GetReceivedStatus();
                    }
                case HANDTYPE.LEFT_HAND:
                    {
                        return LH.GetReceivedStatus();
                    }
                default:
                    return VRTRIXGloveStatus.CLOSED;
            }
        }

        public VRTRIXGloveGesture GetGesture(HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND && LH_Mode)
            {
                return LH_Gesture;
            }
            else if (type == HANDTYPE.RIGHT_HAND && RH_Mode)
            {
                return RH_Gesture;
            }
            else
            {
                return VRTRIXGloveGesture.BUTTONNONE;
            }
        }

        private Transform[] FindFingerTransform()
        {
            Transform[] transform_array = new Transform[(int)VRTRIXBones.NumOfBones];
            transform_array[0] = null;
            for(int i = 1; i < (int)VRTRIXBones.NumOfBones; ++i)
            {
                string bone_name = VRTRIXUtilities.GetBoneName(i);
                if (GetComponent("VRTRIXBoneMapping"))
                {
                    GameObject bone = VRTRIXBoneMapping.UniqueStance.MapToVRTRIX_BoneName(bone_name);
                    if(bone != null)
                    {
                        transform_array[i] = bone.transform;
                    }
                }
                else
                {
                    Transform parent = (i < 19) ? transform.GetChild(1) : transform.GetChild(0);
                    Transform obj = TransformDeepChildExtension.FindDeepChild(parent, bone_name);
                    transform_array[i] = obj;
                }

                //print(obj);
            }
            return transform_array;
        }

        public static GameObject CheckDeviceModelName(HANDTYPE type = HANDTYPE.NONE, InteractiveDevice device = InteractiveDevice.NONE)
        {
            var system = OpenVR.System;
            if (system == null)
                return null;
            for (int i = 0; i < 16; i++)
            {
                var error = ETrackedPropertyError.TrackedProp_Success;
                var capacity = system.GetStringTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_RenderModelName_String, null, 0, ref error);
                if (capacity <= 1)
                {
                    continue;
                }

                var buffer = new System.Text.StringBuilder((int)capacity);
                system.GetStringTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_RenderModelName_String, buffer, capacity, ref error);
                var s = buffer.ToString();
                if (type == HANDTYPE.LEFT_HAND)
                {
                    if (s.Contains("LH"))
                    {
                        return GameObject.Find("Device" + i);
                    }
                }
                else if (type == HANDTYPE.RIGHT_HAND)
                {
                    if (s.Contains("RH"))
                    {
                        return GameObject.Find("Device" + i);
                    }
                }

                else if(device == InteractiveDevice.SWORD)
                {
                    if (s.Contains("sword"))
                    {
                        GameObject sword_ref = GameObject.Find("Device" + i);
                        sword_ref.GetComponent<SteamVR_RenderModel>().enabled = false;
                        return sword_ref;
                    }
                }
                else if (device == InteractiveDevice.SHIELD)
                {
                    if (s.Contains("shield"))
                    {
                        GameObject shield_ref = GameObject.Find("Device" + i);
                        shield_ref.GetComponent<SteamVR_RenderModel>().enabled = false;
                        return shield_ref;
                    }
                }
                else if (device == InteractiveDevice.EXTINGUISHER)
                {
                    if (s.Contains("fire"))
                    {
                        GameObject ex_ref = GameObject.Find("Device" + i);
                        return ex_ref;
                    }
                }
            }
            return null;
        }

    }
    public static class TransformDeepChildExtension
    {
        //Breadth-first search
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                {
                    return c;
                }
                
                foreach(Transform t in c)
                queue.Enqueue(t);
            }
            return null;
        }    
    }

}



