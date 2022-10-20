//============= Copyright (c) VRTRIX INC, All rights reserved. ================
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
using Valve.VRAPI;

namespace VRTRIX
{
    [RequireComponent(typeof(VRTRIXBoneMapping))]

    //!  VRTRIX Data Glove data streaming class. 
    /*!
        A basic data streaming class for demonstration.
    */
    public class VRTRIXGloveDataStreaming : MonoBehaviour
    {
        [Header("VR Settings")]
        //! VR environment enable flag, set to true if run the demo in VR headset
        public bool IsVREnabled = false;

        [DrawIf("IsVREnabled", false)]
        //! If VR is NOT enabled, wrist joint need an object to align, which can be the camera, or parent joint of wrist(if a full body model is used), or can just be any other game objects.
        public GameObject LH_ObjectToAlign;

        [DrawIf("IsVREnabled", false)]
        //! If VR is NOT enabled, wrist joint need an object to align, which can be the camera, or parent joint of wrist(if a full body model is used), or can just be any other game objects.
        public GameObject RH_ObjectToAlign;

        [DrawIf("IsVREnabled", true)]
        //! If VR is enabled, HTC tracker is the default wrist tracking hardware, which is fixed to side part of data glove, this offset represents the offset between tracker origin to right wrist joint origin.
        public Vector3 RHTrackerOffset = new Vector3(0.01f, 0, -0.035f);

        [DrawIf("IsVREnabled", true)]
        //! If VR is enabled, HTC tracker is the default wrist tracking hardware, which is fixed to side part of data glove, this offset represents the offset between tracker origin to left wrist joint origin.
        public Vector3 LHTrackerOffset = new Vector3(-0.01f, 0, -0.035f);

        [DrawIf("IsVREnabled", true)]
        //! If VR is enabled, set the left hand tracker object
        public GameObject LH_tracker;

        [DrawIf("IsVREnabled", true)]
        //! If VR is enabled, set the right hand tracker object
        public GameObject RH_tracker;


        [Header("MoCap Settings")]
        //! Mocap enable flag, set to true if integrate with Mocap, use mocap wrist data.
        public bool IsMoCapEnabled = false;

        [Header("Glove Settings")]
        //! Hardware version of VRTRIX data gloves, currently DK1, DK2 and PRO are supported.
        public GLOVEVERSION version;

        //! Advanced mode toggle.
        public bool AdvancedMode;

        //! Data glove server ip address.
        public string ServerIP;

        //! If mutiple gloves mode is enbaled, specify different index for different pair of gloves. Otherwise, just select None.
        public GloveIndex Index = GloveIndex.Device0;

        [Header("Model Mapping Settings")]
        //! Model mapping parameters for left hand, only used when finger joint axis definition is different from wrist joint, otherwise, just set to 0,0,0.
        public Vector3 ql_modeloffset;

        //! Model mapping parameters for right hand, only used when finger joint axis definition is different from wrist joint, otherwise, just set to 0,0,0.
        public Vector3 qr_modeloffset;

        //! Model mapping parameters for left hand, only used when wrist joint axis definition is different from hardware wrist joint, otherwise, just set to identity matrix {(1,0,0),(0,1,0),(0,0,1)}. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public Vector3[] ql_axisoffset = new Vector3[3];

        //! Model mapping parameters for right hand, only used when wrist joint axis definition is different from hardware wrist joint, otherwise, just set to identity matrix {(1,0,0),(0,1,0),(0,0,1)}. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public Vector3[] qr_axisoffset = new Vector3[3];

        [Header("Thumb Parameters")]
        //! Model mapping parameters for left thumb joint, used to tune thumb offset between the model and hardware sensor placement. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public Vector3[] thumb_offset_L = new Vector3[3];
        
        //! Model mapping parameters for right thumb joint, used to tune thumb offset between the model and hardware sensor placement. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public Vector3[] thumb_offset_R = new Vector3[3];

        //! Model mapping parameters for thumb proximal joint, used to tune thumb slerp algorithm parameter. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public double thumb_proximal_slerp;

        //! Model mapping parameters for thumb middle joint, used to tune thumb slerp algorithm parameter. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public double thumb_middle_slerp;

        [Header("Finger Parameters")]
        //! Finger spacing when advanced mode is NOT enabled. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public double finger_spacing;

        //! Finger spacing when four fingers are fully bended. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public double final_finger_spacing;

        [Header("Gesture Recognition Parameters")]
        //! Finger spacing when advanced mode is NOT enabled. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public double finger_bendup_threshold;

        //! Finger spacing when four fingers are fully bended. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public double finger_benddown_threshold;

        public double finger_curved_threshold;

        //! Finger spacing when advanced mode is NOT enabled. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public double thumb_bendup_threshold;

        //! Finger spacing when four fingers are fully bended. Please read the sdk tutorial documentation to learn how to set this parameter properly.
        public double thumb_benddown_threshold;

        public double thumb_curved_threshold;

        public VRTRIXDataWrapper LH, RH;
        private TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
        private TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];

        private VRTRIXEvents.Action newTrackerPosesAction;
        private uint LH_tracker_index = OpenVR.k_unMaxTrackedDeviceCount;
        private uint RH_tracker_index = OpenVR.k_unMaxTrackedDeviceCount;
        private bool bIsLHTrackerFound, bIsRHTrackerFound;
        private VRTRIXGloveGestureRecognition gestureDetector;
        private Thread LH_receivedData, RH_receivedData;
        private Quaternion qloffset, qroffset;
        private bool qloffset_cal, qroffset_cal;
        private VRTRIXGloveGesture LH_Gesture, RH_Gesture = VRTRIXGloveGesture.BUTTONINVALID;
        private Transform[] fingerTransformArray;
        private Matrix4x4 ml_axisoffset, mr_axisoffset;
        private Vector3[] thumb_offset_L_default = new Vector3[3];
        private Vector3[] thumb_offset_R_default = new Vector3[3];


        void Start()
        {
            LH = new VRTRIXDataWrapper(AdvancedMode, version, HANDTYPE.LEFT_HAND);
            RH = new VRTRIXDataWrapper(AdvancedMode, version, HANDTYPE.RIGHT_HAND);
            int gloveCount = VRTRIXDataWrapper.GetGloveCount();
            print("Found " + gloveCount + " gloves connected to PC.");
            Dictionary<VRTRIXBones, VRTRIXFingerStatusThreshold> thresholdMap = new Dictionary<VRTRIXBones, VRTRIXFingerStatusThreshold>();
            for (int i = 0; i < (int)VRTRIXBones.NumOfBones - 2; ++i)
            {
                if(i == (int)VRTRIXBones.R_Thumb_1 || i == (int)VRTRIXBones.R_Thumb_2 || i == (int)VRTRIXBones.R_Thumb_3 ||
                   i == (int)VRTRIXBones.L_Thumb_1 || i == (int)VRTRIXBones.L_Thumb_2 || i == (int)VRTRIXBones.L_Thumb_3)
                {
                    VRTRIXFingerStatusThreshold threshold = new VRTRIXFingerStatusThreshold(thumb_bendup_threshold, thumb_benddown_threshold, thumb_curved_threshold);
                    thresholdMap.Add((VRTRIXBones)i, threshold);
                }
                else
                {
                    VRTRIXFingerStatusThreshold threshold = new VRTRIXFingerStatusThreshold(finger_bendup_threshold, finger_benddown_threshold, finger_curved_threshold);
                    thresholdMap.Add((VRTRIXBones)i, threshold);
                }
            }

            for(int i = 0; i < 3; ++i)
            {
                thumb_offset_L_default[i] = thumb_offset_L[i];
                thumb_offset_R_default[i] = thumb_offset_R[i];
            }

            gestureDetector = new VRTRIXGloveGestureRecognition(thresholdMap);
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
                    var system = OpenVR.System;
                    if (system == null)
                    {
                        EVRInitError initError = EVRInitError.None;
                        OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref initError);
                        bool needsInit = initError != EVRInitError.None;

                        if (needsInit)
                        {
                            EVRInitError error = EVRInitError.None;
                            OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);

                            if (error != EVRInitError.None)
                            {
                                Debug.LogError("<b>[SteamVR]</b> Error during OpenVR Init: " + error.ToString());
                            }
                        }
                    }

                    newTrackerPosesAction = VRTRIXEvents.NewPosesAction(OnNewTrackerPoses);
                    newTrackerPosesAction.enabled = true;
                    CheckDeviceModelName(HANDTYPE.RIGHT_HAND);
                    CheckDeviceModelName(HANDTYPE.LEFT_HAND);
                }
                catch (Exception e)
                {
                    print("Exception caught: " + e);
                }
            }


            VRTRIXGloveStatusUIUpdate UI = this.gameObject.GetComponent<VRTRIXGloveStatusUIUpdate>();
            if (UI == null)
            {
                OnConnectGlove();
            }
        }

        void Update()
        {
            if (IsVREnabled)
            {
                var compositor = OpenVR.Compositor;
                if (compositor != null)
                {
                    compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseStanding);
                    compositor.GetLastPoses(poses, gamePoses);
                    VRTRIXEvents.NewPoses.Send(poses);
                    VRTRIXEvents.NewPosesApplied.Send();
                }
            }

            if (RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                if (!qroffset_cal && IsValidQuat(RH.GetReceivedRotation(VRTRIXBones.R_Hand)))
                {
                    if (!IsVREnabled ||
                        (IsVREnabled && RH_tracker.transform.rotation != Quaternion.identity))
                    {
                        PerformAlgorithmTuning(HANDTYPE.RIGHT_HAND);
                        qroffset = CalculateStaticOffset(RH, HANDTYPE.RIGHT_HAND);
                        qroffset_cal = true;
                    }
                }

                if (IsVREnabled && bIsRHTrackerFound)
                {
                    SetPosition(VRTRIXBones.R_Arm, RH_tracker.transform.position, RH_tracker.transform.rotation, RHTrackerOffset);
                }
                //以下是设置右手每个骨骼节点全局旋转(global rotation)；
                for(int i = 0; i < (int)VRTRIXBones.L_Hand; ++i)
                {
                    if (IsMoCapEnabled && i == (int)VRTRIXBones.R_Hand) continue;
                    SetRotation((VRTRIXBones)i, RH.GetReceivedRotation((VRTRIXBones)i), HANDTYPE.RIGHT_HAND);
                }
                RH_Gesture = gestureDetector.GestureDetection(RH, HANDTYPE.RIGHT_HAND);
            }



            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                if (!qloffset_cal && IsValidQuat(LH.GetReceivedRotation(VRTRIXBones.L_Hand)))
                {
                    if (!IsVREnabled ||
                        (IsVREnabled && LH_tracker.transform.rotation != Quaternion.identity))
                    { 
                        PerformAlgorithmTuning(HANDTYPE.LEFT_HAND);
                        qloffset = CalculateStaticOffset(LH, HANDTYPE.LEFT_HAND);
                        qloffset_cal = true;
                    }
                }

                if (IsVREnabled && bIsLHTrackerFound)
                {
                    SetPosition(VRTRIXBones.L_Arm, LH_tracker.transform.position, LH_tracker.transform.rotation, LHTrackerOffset);
                }

                //以下是设置左手每个骨骼节点全局旋转(global rotation)；
                for(int i = (int)VRTRIXBones.L_Hand; i < (int)VRTRIXBones.R_Arm; ++i)
                {
                    if (IsMoCapEnabled && i == (int)VRTRIXBones.L_Hand) continue;
                    SetRotation((VRTRIXBones)i, LH.GetReceivedRotation((VRTRIXBones)i), HANDTYPE.LEFT_HAND);
                }
                LH_Gesture = gestureDetector.GestureDetection(LH, HANDTYPE.LEFT_HAND);
            }
        }

        //数据手套初始化，数据服务器连接
        //! Connect data glove and initialization.
        public void OnConnectGlove()
        {
            VRTRIXGloveStatusUIUpdate UI = this.gameObject.GetComponent<VRTRIXGloveStatusUIUpdate>();
            if( UI != null)
            {
                ServerIP = UI.GetServerIP();
                Index = (GloveIndex)UI.GetGloveDeviceID();
            }
            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.DISCONNECTED)
            {
                if((IsVREnabled && bIsLHTrackerFound) || !IsVREnabled)
                {
                    LH.OnConnectDataGlove((int)Index, ServerIP);
                }
            }
            if (RH.GetReceivedStatus() == VRTRIXGloveStatus.DISCONNECTED)
            {
                if ((IsVREnabled && bIsRHTrackerFound) || !IsVREnabled)
                {
                    RH.OnConnectDataGlove((int)Index, ServerIP);
                }
            }
        }
        
        //数据手套反初始化，数据服务器断开连接
        //! Disconnect data glove and uninitialization.
        public void OnDisconnectGlove()
        {
            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.OnDisconnectDataGlove();
            }
            if (RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                RH.OnDisconnectDataGlove();
            }
        }

        //数据手套硬件地磁校准数据储存，仅在磁场大幅度变化后使用。
        //! Save hardware calibration parameters in IMU, only used in magnetic field changed dramatically.
        public void OnHardwareCalibrate(HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.OnSaveCalibration();
            }
            if (type == HANDTYPE.RIGHT_HAND && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                RH.OnSaveCalibration();
            }
            if (type == HANDTYPE.BOTH_HAND
                && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED
                && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.OnSaveCalibration();
                RH.OnSaveCalibration();
            }
        }

        //数据手套振动
        //! Trigger a haptic vibration on data glove.
        public void OnVibrate(HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.VibratePeriod(500);
            }
            if (type == HANDTYPE.RIGHT_HAND && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                RH.VibratePeriod(500);
            }
            if (type == HANDTYPE.BOTH_HAND
                && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED
                && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.VibratePeriod(500);
                RH.VibratePeriod(500);
            }
        }

        //数据手套手动跳频
        //! Switch radio channel of data glove. Only used for testing/debuging. Automatic channel switching is enabled by default in normal mode.
        public void OnChannelHopping(HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.ChannelHopping();
            }
            if (type == HANDTYPE.RIGHT_HAND && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                RH.ChannelHopping();
            }
            if (type == HANDTYPE.BOTH_HAND
                && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED
                && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.ChannelHopping();
                RH.ChannelHopping();
            }
        }

        //数据手套设置手背初始方向。
        //! Align five fingers to closed gesture (only if advanced mode is set to true). Also align wrist to the game object chosen.
        public void OnAlignWrist(HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                qloffset = CalculateStaticOffset(LH, HANDTYPE.LEFT_HAND);
            }
            if (type == HANDTYPE.RIGHT_HAND && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                qroffset = CalculateStaticOffset(RH, HANDTYPE.RIGHT_HAND);
            }
            if (type == HANDTYPE.BOTH_HAND
                && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED
                && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                qloffset = CalculateStaticOffset(LH, HANDTYPE.LEFT_HAND);
                qroffset = CalculateStaticOffset(RH, HANDTYPE.RIGHT_HAND);
            }
        }

        //数据手套软件对齐四指。
        //! Align five fingers to closed gesture (only if advanced mode is set to true). Also align wrist to the game object chosen.
        public void OnAlignFingers(HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.OnCloseFingerAlignment(HANDTYPE.LEFT_HAND);
            }
            if (type == HANDTYPE.RIGHT_HAND && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                RH.OnCloseFingerAlignment(HANDTYPE.RIGHT_HAND);
            }
            if (type == HANDTYPE.BOTH_HAND
                && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED
                && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.OnCloseFingerAlignment(HANDTYPE.LEFT_HAND);
                RH.OnCloseFingerAlignment(HANDTYPE.RIGHT_HAND);
            }
        }

        //数据手套五指张开航向角解锁
        //! Activate advanced mode so that finger's yaw data will be unlocked.
        /*! 
         * \param bIsAdvancedMode Advanced mode will be activated if set to true.
         */
        public void SetAdvancedMode(bool bIsAdvancedMode)
        {
            AdvancedMode = bIsAdvancedMode;
            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.SetAdvancedMode(bIsAdvancedMode);
            }
            if (RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                RH.SetAdvancedMode(bIsAdvancedMode);
            }
        }

        //改变数据手套硬件版本
        //! Set data gloves hardware version.
        /*! 
         * \param version Data glove hardware version.
         */
        public void SetHardwareVersion(GLOVEVERSION version)
        {
            this.version = version;
            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.SetHardwareVersion(version);
            }
            if (RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                RH.SetHardwareVersion(version);
            }
        }

        public void OnResetThumbOffset()
        {
            for (int i = 0; i < 3; ++i)
            {
                thumb_offset_L[i] = thumb_offset_L_default[i];
                thumb_offset_R[i] = thumb_offset_R_default[i];
            }
            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                LH.SetThumbOffset(thumb_offset_L[0], VRTRIXBones.L_Thumb_1);
                LH.SetThumbOffset(thumb_offset_L[1], VRTRIXBones.L_Thumb_2);
                LH.SetThumbOffset(thumb_offset_L[2], VRTRIXBones.L_Thumb_3);
            }
            if (RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                RH.SetThumbOffset(thumb_offset_R[0], VRTRIXBones.R_Thumb_1);
                RH.SetThumbOffset(thumb_offset_R[1], VRTRIXBones.R_Thumb_2);
                RH.SetThumbOffset(thumb_offset_R[2], VRTRIXBones.R_Thumb_3);
            }
        }

        //程序退出
        //! Application quit operation. 
        void OnApplicationQuit()
        {
            if (LH.GetReceivedStatus() != VRTRIXGloveStatus.DISCONNECTED)
            {
                LH.OnDisconnectDataGlove();
            }
            if (RH.GetReceivedStatus() != VRTRIXGloveStatus.DISCONNECTED)
            {
                RH.OnDisconnectDataGlove();
            }
        }

        //! Get current transform of specific joint
        /*! 
         * \param bone specific joint of hand.
         * \return current transform of specific joint.
         */
        public Transform GetTransform(VRTRIXBones bone)
        {
            return fingerTransformArray[(int)bone];
        }

        //获取磁场校准水平，值越小代表效果越好
        //! Get current calibration score for specific IMU sensor
        /*! 
         * \param bone specific joint of hand.
         * \return current calibration score for specific IMU sensor. Lower value of score means better calibration performance.
         */
        public int GetCalScore(VRTRIXBones bone)
        {
            if ((int)bone < 16)
            {
                return RH.GetReceivedCalScore(bone);
            }
            else
            {
                return LH.GetReceivedCalScore(bone);
            }
        }

        //获取信号强度，值越大代表信号越强
        //! Get radio strength of data glove 
        /*! 
         * \param type Data glove hand type.
         * \return radio strength of data glove. Higher value of score means better radio strength.         
         */
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

        //获取当前通信信道，1-100共100个信道
        //! Get current radio channel of data glove used
        /*! 
         * \param type Data glove hand type.
         * \return current radio channel of data glove used.         
         */
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
        //获取电量
        //! Get current battery level in percentage of data glove
        /*! 
         * \param type Data glove hand type.
         * \return current battery level in percentage of data glove.         
         */
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
        //获取磁场校准水平均值
        //! Get current calibration score average value
        /*! 
         * \param type Data glove hand type.
         * \return current calibration score average value.
         */
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

        //获取实际帧率
        //! Get data rate received per second 
        /*! 
         * \param type Data glove hand type.
         * \return data rate received per second.         
         */
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

        //获取连接状态
        //! Get data glove connection status 
        /*! 
         * \param type Data glove hand type.
         * \return data glove connection status.         
         */
        public bool GetGloveConnectionStat(HANDTYPE type)
        {
            return GetReceivedStatus(type) == VRTRIXGloveStatus.CONNECTED;
        }

        //! Get data glove status 
        /*! 
         * \param type Data glove hand type.
         * \return data glove status.         
         */
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
                    return VRTRIXGloveStatus.DISCONNECTED;
            }
        }
        
        //获取姿态
        //! Get the gesture detected
        /*! 
         * \param type Data glove hand type.
         * \return the gesture detected.         
         */
        public VRTRIXGloveGesture GetGesture(HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND && LH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                return LH_Gesture;
            }
            else if (type == HANDTYPE.RIGHT_HAND && RH.GetReceivedStatus() == VRTRIXGloveStatus.CONNECTED)
            {
                return RH_Gesture;
            }
            else
            {
                return VRTRIXGloveGesture.BUTTONINVALID;
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

        //用于计算左手/右手腕关节姿态（由HTC Tracker得到）和左手手背姿态（由数据手套得到）之间的四元数差值，该方法为动态调用，即每一帧都会调用该计算。
        //适用于：当腕关节/手背节点使用HTC Tracker进行追踪时
        private Quaternion CalculateTrackerOffset(GameObject tracker, VRTRIXDataWrapper glove, HANDTYPE type)
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

        //用于计算左手/右手腕关节姿态（由动捕设备得到）和左手手背姿态（由数据手套得到）之间的四元数差值，该方法为动态调用，即每一帧都会调用该计算。
        //适用于：当动捕设备有腕关节/手背节点时
        private Quaternion CalculateMocapOffset(GameObject wrist, VRTRIXDataWrapper glove, HANDTYPE type)
        {
            //计算场景中角色右手腕在unity世界坐标系下的旋转与手套的右手腕在手套追踪系统中世界坐标系下右手腕的旋转之间的角度差值，意在匹配两个坐标系的方向；
            if (type == HANDTYPE.RIGHT_HAND)
            {
                Quaternion rotation = glove.GetReceivedRotation(VRTRIXBones.R_Hand);
                Vector3 quat_vec = mr_axisoffset.MultiplyVector(new Vector3(rotation.x, rotation.y, rotation.z));
                rotation = new Quaternion(quat_vec.x, quat_vec.y, quat_vec.z, rotation.w);
                return wrist.transform.rotation * Quaternion.Inverse(rotation);
            }

            //计算场景中角色左手腕在unity世界坐标系下的旋转与手套的左手腕在手套追踪系统中世界坐标系下左手腕的旋转之间的角度差值，意在匹配两个坐标系的方向；
            else if (type == HANDTYPE.LEFT_HAND)
            {
                Quaternion rotation = glove.GetReceivedRotation(VRTRIXBones.L_Hand);
                Vector3 quat_vec = ml_axisoffset.MultiplyVector(new Vector3(rotation.x, rotation.y, rotation.z));
                rotation = new Quaternion(quat_vec.x, quat_vec.y, quat_vec.z, rotation.w);
                return wrist.transform.rotation * Quaternion.Inverse(rotation);
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
        private void SetRotation(VRTRIXBones bone, Quaternion rotation, HANDTYPE type)
        {
            Transform obj = fingerTransformArray[(int)bone];
            if (obj != null)
            {
                if (!float.IsNaN(rotation.x) && !float.IsNaN(rotation.y) && !float.IsNaN(rotation.z) && !float.IsNaN(rotation.w))
                {
                    if (type == HANDTYPE.LEFT_HAND)
                    {
                        Vector3 quat_vec = ml_axisoffset.MultiplyVector(new Vector3(rotation.x, rotation.y, rotation.z));
                        rotation = new Quaternion(quat_vec.x, quat_vec.y, quat_vec.z, rotation.w);


                        if (IsVREnabled)
                        {
                            //当VR环境下，根据固定在手腕上tracker的方向对齐手背方向。
                            obj.rotation = (bone == VRTRIXBones.L_Hand) ? CalculateTrackerOffset(LH_tracker, LH, HANDTYPE.LEFT_HAND) * rotation :
                                                                     CalculateTrackerOffset(LH_tracker, LH, HANDTYPE.LEFT_HAND)* rotation * Quaternion.Euler(ql_modeloffset);
                        }
                        else if(IsMoCapEnabled)
                        {
                            //当开启动捕环境下，对齐动捕手腕方向。
                            obj.rotation = (bone == VRTRIXBones.L_Hand) ? CalculateMocapOffset(fingerTransformArray[(int)VRTRIXBones.L_Hand].gameObject, LH, HANDTYPE.LEFT_HAND) * rotation :
                                                                     CalculateMocapOffset(fingerTransformArray[(int)VRTRIXBones.L_Hand].gameObject, LH, HANDTYPE.LEFT_HAND) * rotation * Quaternion.Euler(ql_modeloffset);                 
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
                            obj.rotation = (bone == VRTRIXBones.R_Hand) ? CalculateTrackerOffset(RH_tracker, RH, HANDTYPE.RIGHT_HAND)* rotation :
                                                                            CalculateTrackerOffset(RH_tracker, RH, HANDTYPE.RIGHT_HAND)* rotation * Quaternion.Euler(qr_modeloffset);
                        }
                        else if (IsMoCapEnabled)
                        {
                            obj.rotation = (bone == VRTRIXBones.R_Hand) ? CalculateMocapOffset(fingerTransformArray[(int)VRTRIXBones.R_Hand].gameObject, RH, HANDTYPE.RIGHT_HAND) * rotation :
                                                                     CalculateMocapOffset(fingerTransformArray[(int)VRTRIXBones.R_Hand].gameObject, RH, HANDTYPE.RIGHT_HAND) * rotation * Quaternion.Euler(qr_modeloffset);
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

        private bool IsValidQuat(Quaternion quat) {
            return Math.Abs(quat.w * quat.w + quat.x * quat.x + quat.y * quat.y + quat.z * quat.z - 1.0) < 1e-3;
        }
        private void PerformAlgorithmTuning(HANDTYPE type)
        {
            if(type == HANDTYPE.LEFT_HAND)
            {
                LH.SetThumbOffset(thumb_offset_L[0], VRTRIXBones.L_Thumb_1);
                LH.SetThumbOffset(thumb_offset_L[1], VRTRIXBones.L_Thumb_2);
                LH.SetThumbOffset(thumb_offset_L[2], VRTRIXBones.L_Thumb_3);
                LH.SetThumbSlerpRate(thumb_proximal_slerp, thumb_middle_slerp);
                LH.SetFinalFingerSpacing(final_finger_spacing);
                LH.SetFingerSpacing(finger_spacing);
            }
            else if(type == HANDTYPE.RIGHT_HAND)
            {
                RH.SetThumbOffset(thumb_offset_R[0], VRTRIXBones.R_Thumb_1);
                RH.SetThumbOffset(thumb_offset_R[1], VRTRIXBones.R_Thumb_2);
                RH.SetThumbOffset(thumb_offset_R[2], VRTRIXBones.R_Thumb_3);
                RH.SetThumbSlerpRate(thumb_proximal_slerp, thumb_middle_slerp);
                RH.SetFinalFingerSpacing(final_finger_spacing);
                RH.SetFingerSpacing(finger_spacing);
            }
        }

        private Transform[] FindFingerTransform()
        {
            Transform[] transform_array = new Transform[(int)VRTRIXBones.NumOfBones];
            for(int i = 0; i < (int)VRTRIXBones.NumOfBones; ++i)
            {
                string bone_name = VRTRIXJointDef.GetBoneName(i);
                VRTRIXBoneMapping map = gameObject.GetComponent(typeof(VRTRIXBoneMapping)) as VRTRIXBoneMapping;
                if (map != null)
                {
                    GameObject bone = map.MapToVRTRIX_BoneName(bone_name);
                    if(bone != null)
                    {
                        transform_array[i] = bone.transform;
                    }
                    //print(bone);
                }
            }
            return transform_array;
        }

        private void OnNewTrackerPoses(TrackedDevicePose_t[] poses)
        {

            if (bIsLHTrackerFound
                && poses[LH_tracker_index].bDeviceIsConnected
                && poses[LH_tracker_index].bPoseIsValid)
            {
                var pose = new VRTRIXUtils.RigidTransform(poses[LH_tracker_index].mDeviceToAbsoluteTracking);
                LH_tracker.transform.localPosition = pose.pos;
                LH_tracker.transform.localRotation = pose.rot * new Quaternion(0, 0, 1, 0);
            }

            if (bIsRHTrackerFound
                && poses[RH_tracker_index].bDeviceIsConnected
                && poses[RH_tracker_index].bPoseIsValid)
            {
                var pose = new VRTRIXUtils.RigidTransform(poses[RH_tracker_index].mDeviceToAbsoluteTracking);
                RH_tracker.transform.localPosition = pose.pos;
                RH_tracker.transform.localRotation = pose.rot * new Quaternion(0, 0, 1, 0);
            }
        }

        //! Check the tracked device model name stored in hardware config to find specific hardware type. (SteamVR Tracking support)
        /*! 
         * \param type Hand type to check(if wrist tracker for data glove is the hardware to check).
         * \param device Device type to check(if other kind of interactive hardware to check).
         * \return the gameobject of the tracked device.
         */
        public void CheckDeviceModelName(HANDTYPE type = HANDTYPE.NONE)
        {
            var system = OpenVR.System;
            if (system == null) return;
            for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
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
                        LH_tracker_index = i;
                        bIsLHTrackerFound = true;
                        print("Found Left Hand Tracker on index " + LH_tracker_index);
                        return;
                    }
                }
                else if (type == HANDTYPE.RIGHT_HAND)
                {
                    if (s.Contains("RH"))
                    {
                        RH_tracker_index = i;
                        bIsRHTrackerFound = true;
                        print("Found Right Hand Tracker on index " + RH_tracker_index);
                        return;
                    }
                }
            }
        }
    }
}



