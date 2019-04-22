//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Example CSharp script to read data stream under VR environment and
//          perform gesture recognition/ other basic interactions. Other VRTRIX
//          hardware APIs (including shield&sword, extinguisher etc)are also 
//          included in this script so that you will be able to interact with
//          them using our gloves in virtual reality world!
//          To know more about our other products & solutions, please refer to
//          our websit:
//          https://www.vrtrix.com.cn
//
//=============================================================================
using UnityEngine;
using System.Threading;
using Valve.VR;
using System;

namespace VRTRIX
{
    public class VRTRIXGloveVRInteraction : MonoBehaviour
    {
        public bool AdvancedMode;
        public Vector3 ql_modeloffset, qr_modeloffset;
        public VRTRIXDataWrapper LH, RH;
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
	     	Device15 = 15
    	}
        public GloveIndex Index;
        private GameObject LH_tracker, RH_tracker;
        private VRTRIXGloveGesture LH_Gesture, RH_Gesture;
        private VRTRIXGloveGestureRecognition GloveGesture;
        private bool LH_Mode, RH_Mode;
        private float qloffset, qroffset;
        private bool qloffset_cal, qroffset_cal;

        private bool isStopRendering;
        private Quaternion[] LHFingerOffset = new Quaternion[15];
        private Quaternion[] RHFingerOffset = new Quaternion[15];

        private Vector3 troffset = new Vector3(0.01f, 0, -0.035f);
        private Vector3 tloffset = new Vector3(-0.01f, 0, -0.035f);

        void Start()
        {
            RH = new VRTRIXDataWrapper(AdvancedMode, (int)Index);
            LH = new VRTRIXDataWrapper(AdvancedMode, (int)Index);
            GloveGesture = new VRTRIXGloveGestureRecognition();
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
        //数据手套初始化，硬件连接
        public void OnConnectGlove()
        {
            try
            {
                if (RH_tracker != null)
                {
                    RH_Mode = RH.Init(HANDTYPE.RIGHT_HAND);
                    if (RH_Mode)
                    {
                        print("Right hand glove connected!");
                        RH.registerCallBack();
                        RH.startStreaming();
                    }
                }

                if(LH_tracker != null)
                {
                    LH_Mode = LH.Init(HANDTYPE.LEFT_HAND);
                    if (LH_Mode)
                    {
                        print("Left hand glove connected!");
                        LH.registerCallBack();
                        LH.startStreaming();
                    }
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

        //数据手套硬件校准，仅在磁场大幅度变化后使用。
        public void OnHardwareCalibrate()
        {
            if (LH_Mode)
            {
                LH.calibration();
            }
            if (RH_Mode)
            {
                RH.calibration();
            }
        }

        //数据手套振动
        public void OnVibrate()
        {
            if (LH_Mode)
            {
                LH.vibrate();
            }
            if (RH_Mode)
            {
                RH.vibrate();
            }
        }

        public void OnStopRendering()
        {
            Quaternion rot_LH = CalculateDynamicOffset(LH_tracker, LH, HANDTYPE.LEFT_HAND) *
                    (LH.GetReceivedRotation(VRTRIXBones.L_Hand) * Quaternion.Euler(ql_modeloffset));

            for (int i = 0; i < 15; i++)
            {
                Quaternion rot_original = CalculateDynamicOffset(LH_tracker, LH, HANDTYPE.LEFT_HAND) *
                    (LH.GetReceivedRotation((VRTRIXBones)(i+22)) * Quaternion.Euler(ql_modeloffset));
                LHFingerOffset[i] = Quaternion.Inverse(rot_LH) * rot_original;
            }
            LH.SetReceivedStatus(VRTRIXGloveStatus.PAUSED);

            Quaternion rot_RH = CalculateDynamicOffset(RH_tracker, RH, HANDTYPE.RIGHT_HAND) *
                    (RH.GetReceivedRotation(VRTRIXBones.R_Hand) * Quaternion.Euler(qr_modeloffset));

            for (int i = 0; i < 15; i++)
            {
                Quaternion rot_original = CalculateDynamicOffset(RH_tracker, RH, HANDTYPE.RIGHT_HAND) *
                    (RH.GetReceivedRotation((VRTRIXBones)(i + 4)) * Quaternion.Euler(qr_modeloffset));
                RHFingerOffset[i] = Quaternion.Inverse(rot_RH) * rot_original;
            }
            RH.SetReceivedStatus(VRTRIXGloveStatus.PAUSED);

            isStopRendering = true;
        }

        public void OnResumeRendering()
        {
            LH.SetReceivedStatus(VRTRIXGloveStatus.NORMAL);
            RH.SetReceivedStatus(VRTRIXGloveStatus.NORMAL);
            isStopRendering = false;
        }

        //数据手套软件对齐手指，仅在磁场大幅度变化后使用。
        public void OnAlignFingers()
        {
            if (LH_Mode)
            {
                qloffset = Math.Abs(LH_tracker.transform.rotation.eulerAngles.z) - 180;
                LH.alignmentCheck(HANDTYPE.LEFT_HAND);
            }
            if (RH_Mode)
            {
                qroffset = Math.Abs(RH_tracker.transform.rotation.eulerAngles.z) - 180;
                RH.alignmentCheck(HANDTYPE.RIGHT_HAND);
            }

        }

        //数据更新与骨骼赋值。
        void Update()
        {
            if (RH_Mode && RH.GetReceivedStatus() != VRTRIXGloveStatus.CLOSED)
            {
                SetPosition(VRTRIXBones.R_Hand, RH_tracker.transform.position, RH_tracker.transform.rotation, troffset);

                SetRotation(VRTRIXBones.R_Forearm, RH.GetReceivedRotation(VRTRIXBones.R_Forearm), RH.DataValidStatus(VRTRIXBones.R_Forearm), HANDTYPE.RIGHT_HAND);
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



            if (LH_Mode && LH.GetReceivedStatus() != VRTRIXGloveStatus.CLOSED)
            {

                SetPosition(VRTRIXBones.L_Hand, LH_tracker.transform.position, LH_tracker.transform.rotation, tloffset);

                SetRotation(VRTRIXBones.L_Forearm, LH.GetReceivedRotation(VRTRIXBones.L_Forearm), LH.DataValidStatus(VRTRIXBones.L_Forearm), HANDTYPE.LEFT_HAND);
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


        void OnGUI()
        {
            if (GUI.Button(new Rect(0, Screen.height / 8, Screen.width / 8, Screen.height / 8), "Reset"))
            {
                OnAlignFingers();
            }

            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.CLOSED && RH.GetReceivedStatus() == VRTRIXGloveStatus.CLOSED)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Connect"))
                {
                    OnConnectGlove();
                }
            }

            if (LH.GetReceivedStatus() != VRTRIXGloveStatus.CLOSED || RH.GetReceivedStatus() != VRTRIXGloveStatus.CLOSED)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Disconnect"))
                {
                    OnDisconnectGlove();
                }
            }

            if (GUI.Button(new Rect(0, Screen.height * 0.25f, Screen.width / 8, Screen.height / 8), "Vibrate"))
            {
                OnVibrate();
            }


            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.NORMAL)
            {
                if (GUI.Button(new Rect(0, Screen.height * 0.375f, Screen.width / 8, Screen.height / 8), "Stop Rendering"))
                {
                    OnStopRendering();
                }
            }

            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.PAUSED)
            {
                if (GUI.Button(new Rect(0, Screen.height * 0.375f, Screen.width / 8, Screen.height / 8), "Resume Rendering"))
                {
                    OnResumeRendering();
                }
            }
        }
       

        private void SetRotation(VRTRIXBones bone, Quaternion rotation, bool valid, HANDTYPE type)
        {
            string bone_name = VRTRIXUtilities.GetBoneName((int)bone);
            GameObject obj = GameObject.Find(bone_name);
            if (obj != null)
            {
                if (!float.IsNaN(rotation.x) && !float.IsNaN(rotation.y) && !float.IsNaN(rotation.z) && !float.IsNaN(rotation.w))
                {
                    if (valid)
                    {

                        if (type == HANDTYPE.LEFT_HAND)
                        {
                            if(bone == VRTRIXBones.L_Hand || !isStopRendering)
                            {
                                obj.transform.rotation = CalculateDynamicOffset(LH_tracker, LH, HANDTYPE.LEFT_HAND) * (rotation * Quaternion.Euler(ql_modeloffset));
                            }
                            else
                            {
                                obj.transform.rotation = CalculateDynamicOffset(LH_tracker, LH, HANDTYPE.LEFT_HAND) * (LH.GetReceivedRotation(VRTRIXBones.L_Hand) * Quaternion.Euler(ql_modeloffset))
                                    * LHFingerOffset[(int)bone - 22];
                            }
                        }
                        else if (type == HANDTYPE.RIGHT_HAND)
                        {
                            if(bone == VRTRIXBones.R_Hand || !isStopRendering)
                            {
                                obj.transform.rotation = CalculateDynamicOffset(RH_tracker, RH, HANDTYPE.RIGHT_HAND) * (rotation * Quaternion.Euler(qr_modeloffset));
                            }
                            else
                            {
                                obj.transform.rotation = CalculateDynamicOffset(RH_tracker, RH, HANDTYPE.RIGHT_HAND) * (RH.GetReceivedRotation(VRTRIXBones.R_Hand) * Quaternion.Euler(qr_modeloffset))
                                    * RHFingerOffset[(int)bone - 4];
                            }
                        }
                    }
                }
            }
        }

        private void SetPosition(VRTRIXBones bone, Vector3 pos, Quaternion rot, Vector3 offset)
        {
            string bone_name = VRTRIXUtilities.GetBoneName((int)bone);
            GameObject obj = GameObject.Find(bone_name);
            if (obj != null)
            {
                obj.transform.position = pos + rot* offset;
            }
        }

        //用于计算左手/右手腕关节姿态（由动捕设备得到）和左手手背姿态（由数据手套得到）之间的四元数差值，该方法为动态调用，即每一帧都会调用该计算。
        //适用于：当动捕设备有腕关节/手背节点时
        private Quaternion CalculateDynamicOffset(GameObject tracker, VRTRIXDataWrapper glove, HANDTYPE type)
        {
            if (type == HANDTYPE.RIGHT_HAND)
            {
                //MapToVRTRIX_BoneName: 此函数用于将任意的手部骨骼模型中关节名称转化为VRTRIX数据手套可识别的关节名称。
                //GameObject R_hand = MyHandsMapToVrtrixHand.UniqueStance.MapToVRTRIX_BoneName(BoneNameForR_hand);

                //计算场景中角色右手腕在unity世界坐标系下的旋转与手套的右手腕在手套追踪系统中世界坐标系下右手腕的旋转之间的角度差值，意在匹配两个坐标系的方向；
                //return tracker.transform.rotation * Quaternion.Inverse(glove.GetReceivedRotation(VRTRIXBones.R_Hand) * Quaternion.Euler(qr_modeloffset));
                Quaternion offsetTracker = (tracker.transform.rotation * new Quaternion(0, 1f, 0, 0))  * Quaternion.Inverse(RH.GetReceivedRotation(VRTRIXBones.R_Hand) * Quaternion.Euler(qr_modeloffset));
                if (!qroffset_cal)
                {
                    qroffset = Math.Abs(tracker.transform.rotation.eulerAngles.z) - 180;
                    qroffset_cal = true;
                }
                return Quaternion.AngleAxis(-qroffset, Vector3.Normalize(tracker.transform.rotation * Vector3.forward)) * offsetTracker;
            }
            else if (type == HANDTYPE.LEFT_HAND)
            {
                //MapToVRTRIX_BoneName: 此函数用于将任意的手部骨骼模型中关节名称转化为VRTRIX数据手套可识别的关节名称。
                //GameObject L_hand = MyHandsMapToVrtrixHand.UniqueStance.MapToVRTRIX_BoneName(BoneNameForL_hand);

                //计算场景中角色左手腕在unity世界坐标系下的旋转与手套的左手腕在手套追踪系统中世界坐标系下左手腕的旋转之间的角度差值，意在匹配两个坐标系的方向；
                //return tracker.transform.rotation * Quaternion.Inverse(LH.GetReceivedRotation(VRTRIXBones.L_Hand) * Quaternion.Euler(ql_modeloffset));
                Quaternion offsetTracker = tracker.transform.rotation * Quaternion.Inverse(LH.GetReceivedRotation(VRTRIXBones.L_Hand) * Quaternion.Euler(ql_modeloffset));
                if (!qloffset_cal)
                {
                    qloffset = Math.Abs(tracker.transform.rotation.eulerAngles.z) - 180;
                    qloffset_cal = true;
                }
                return Quaternion.AngleAxis(-qloffset, tracker.transform.rotation * Vector3.forward) * offsetTracker;
            }
            else
            {
                return Quaternion.identity;
            }
        }

        public VRTRIXGloveGesture GetGesture (HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND)
            {
                return LH_Gesture;
            }else
            {
                return RH_Gesture;
            }
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
                        //ex_ref.GetComponent<SteamVR_RenderModel>().enabled = false;
                        return ex_ref;
                    }
                }
            }
            return null;
        }
    }
}


