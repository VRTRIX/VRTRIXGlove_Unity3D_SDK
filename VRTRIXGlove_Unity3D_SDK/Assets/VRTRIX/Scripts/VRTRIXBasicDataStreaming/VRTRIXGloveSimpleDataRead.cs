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

namespace VRTRIX
{

    public class VRTRIXGloveSimpleDataRead : MonoBehaviour
    {
        public GameObject objectToAlign;
        public bool AdvancedMode;
        public Vector3 ql_modeloffset, qr_modeloffset;

        private VRTRIXDataWrapper LH, RH;
        private VRTRIXGloveGestureRecognition GloveGesture;
        private Thread LH_Thread_read, RH_Thread_read, LH_receivedData, RH_receivedData;
        private Quaternion qloffset, qroffset;
        private bool qloffset_cal, qroffset_cal;
        private VRTRIXGloveGesture LH_Gesture, RH_Gesture = VRTRIXGloveGesture.BUTTONNONE;
        private bool LH_Mode, RH_Mode;

        void Start()
        {
            RH = new VRTRIXDataWrapper(AdvancedMode);
            LH = new VRTRIXDataWrapper(AdvancedMode);
            GloveGesture = new VRTRIXGloveGestureRecognition();
        }


        void FixedUpdate()
        {
            if (RH_Mode && RH.GetReceivedStatus() == VRTRIXGloveStatus.NORMAL)
            {
                
                if (RH.GetReceivedRotation(VRTRIXBones.R_Hand) != Quaternion.identity && !qroffset_cal)
                {
                    qroffset = CalculateStaticOffset(objectToAlign, RH, HANDTYPE.RIGHT_HAND);
                    qroffset_cal = true;
                }
                //以下是设置右手每个骨骼节点全局旋转(global rotation)；
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



            if (LH_Mode && LH.GetReceivedStatus() == VRTRIXGloveStatus.NORMAL)
            {
                if (LH.GetReceivedRotation(VRTRIXBones.L_Hand) != Quaternion.identity && !qloffset_cal)
                {
                    qloffset = CalculateStaticOffset(objectToAlign, LH, HANDTYPE.LEFT_HAND);
                    qloffset_cal = true;
                }
                //以下是设置左手每个骨骼节点全局旋转(global rotation)；
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

        //数据手套初始化，硬件连接
        public void OnConnectGlove()
        {
            try
            {
                RH_receivedData = new Thread(ReceiveRHData);
                LH_receivedData = new Thread(ReceiveLHData);
                RH_receivedData.Start();
                LH_receivedData.Start();
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
                    LH = new VRTRIXDataWrapper(AdvancedMode);
                }
                LH_Mode = false;
            }
            if (RH_Mode)
            {
                if (RH.ClosePort())
                {
                    RH = new VRTRIXDataWrapper(AdvancedMode);
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

        //数据手套软件对齐手指，仅在磁场大幅度变化后使用。
        public void OnAlignFingers()
        {
            if (LH_Mode)
            {
                LH.alignmentCheck(HANDTYPE.LEFT_HAND);
                qloffset = CalculateStaticOffset(objectToAlign, LH, HANDTYPE.LEFT_HAND);
            }
            if (RH_Mode)
            {
                RH.alignmentCheck(HANDTYPE.RIGHT_HAND);
                qroffset = CalculateStaticOffset(objectToAlign, RH, HANDTYPE.RIGHT_HAND);
            }
        }

        //左手手套接受数据线程
        private void ReceiveLHData()
        {
            LH_Mode = LH.Init(HANDTYPE.LEFT_HAND);
            print("Left hand glove connected!");
            if (LH_Mode)
            {
                LH_Thread_read = new Thread(LH.streaming_read_begin);
                LH_Thread_read.Start();
                LH_receivedData = new Thread(ReceiveLHDataAsync);
                LH_receivedData.Start();
            }
        }
        //右手手套接受数据线程
        private void ReceiveRHData()
        {
            RH_Mode = RH.Init(HANDTYPE.RIGHT_HAND);
            print("Right hand glove connected!");
            if (RH_Mode)
            {
                RH_Thread_read = new Thread(RH.streaming_read_begin);
                RH_Thread_read.Start();
                RH_receivedData = new Thread(ReceiveRHDataAsync);
                RH_receivedData.Start();
            }
        }

        private void ReceiveLHDataAsync()
        {
            LH.receivedData(HANDTYPE.LEFT_HAND);
        }

        private void ReceiveRHDataAsync()
        {
            RH.receivedData(HANDTYPE.RIGHT_HAND);
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
        private Quaternion CalculateStaticOffset(GameObject objectToAlign, VRTRIXDataWrapper glove, HANDTYPE type)
        {
            if (type == HANDTYPE.RIGHT_HAND)
            {
                return objectToAlign.transform.rotation * Quaternion.Inverse(this.transform.rotation) * Quaternion.Inverse(glove.GetReceivedRotation(VRTRIXBones.R_Hand) * Quaternion.Euler(qr_modeloffset));
            }
            else if (type == HANDTYPE.LEFT_HAND)
            {
                return objectToAlign.transform.rotation * Quaternion.Inverse(this.transform.rotation) * Quaternion.Inverse(glove.GetReceivedRotation(VRTRIXBones.L_Hand) * Quaternion.Euler(ql_modeloffset));
            }
            else
            {
                return new Quaternion(0f, 0f, 0f, 1f);
            }
        }

        //手部关节赋值函数，每一帧都会调用，通过从数据手套硬件获取当前姿态，进一步进行处理，然后给模型赋值。
        //重要参数：
        //     ql_modeloffset:该参数指的是模型左手坐标系和左手手套传感器硬件坐标系之间的四元数偏差，往往这个偏差为绕x/y/z轴旋转+/-90，+-/180度，需要根据具体情况而定。
        //     qr_modeloffset:该参数指的是模型右手坐标系和右手手套传感器硬件坐标系之间的四元数偏差，往往这个偏差为绕x/y/z轴旋转+/-90，+-/180度，需要根据具体情况而定。
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
                            //该语句的含义为由手套得到的raw data先经过ql的旋转，再经过计算得到的当前该帧下手背和摄像机的旋转差值的作用之后得到的最终旋转，
                            //该最终的旋转就会直接赋值给骨骼模型。
                            obj.transform.rotation = qloffset * rotation * Quaternion.Euler(ql_modeloffset);
                        }
                        else if (type == HANDTYPE.RIGHT_HAND)
                        {
                            //该语句的含义为由手套得到的raw data先经过ql的旋转，再经过计算得到的当前该帧下手背和摄像机的旋转差值的作用之后得到的最终旋转，
                            //该最终的旋转就会直接赋值给骨骼模型。
                            obj.transform.rotation = qroffset * rotation * Quaternion.Euler(qr_modeloffset);
                        }
                    }
                }
            }
        }


        public Quaternion GetRotation(VRTRIXBones bone)
        {
            string bone_name = VRTRIXUtilities.GetBoneName((int)bone);
            GameObject obj = GameObject.Find(bone_name);
            return obj.transform.rotation;
        }

        public int GetCalScore(VRTRIXBones bone)
        {
            //Righthand
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

        public float GetBatteryLevel(HANDTYPE type)
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
            switch (type)
            {
                case HANDTYPE.RIGHT_HAND:
                    {
                        return RH_Mode;
                    }
                case HANDTYPE.LEFT_HAND:
                    {
                        return LH_Mode;
                    }
                default:
                    return false;
            }
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
    }
}



