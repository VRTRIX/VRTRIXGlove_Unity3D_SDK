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
using UnityEngine.UI;

namespace VRTRIX
{

    public class VRTRIXGloveSimpleDataRead : MonoBehaviour
    {
        public GameObject objectToAlign;
        public bool AdvancedMode = true;
        public bool UIEnabled = true;

        [Header("UIComponent")]
        public GameObject m_FPS;
        public GameObject m_MODE;
        public GameObject m_Status;
        public GameObject m_LHRadio;
        public GameObject m_RHRadio;
        public GameObject m_LHRadioBar;
        public GameObject m_RHRadioBar;
        public GameObject m_LHCal;
        public GameObject m_RHCal;
        public GameObject m_LHCalBar;
        public GameObject m_RHCalBar;

        public GameObject m_LHDataRate;
        public GameObject m_RHDataRate;
        public GameObject m_LHStatus;
        public GameObject m_RHStatus;
        public GameObject m_LHand;
        public GameObject m_LThumb;
        public GameObject m_LIndex;
        public GameObject m_LMiddle;
        public GameObject m_LRing;
        public GameObject m_LPinky;
        public GameObject m_RHand;
        public GameObject m_RThumb;
        public GameObject m_RIndex;
        public GameObject m_RMiddle;
        public GameObject m_RRing;
        public GameObject m_RPinky;
        public float m_refreshTime = 1.0f;


        private static VRTRIXDataWrapper RH;
        private static VRTRIXDataWrapper LH;
        private Thread LH_Thread_read, RH_Thread_read, LH_receivedData, RH_receivedData;
        public static VRTRIXGloveGesture LH_Gesture, RH_Gesture = VRTRIXGloveGesture.BUTTONNONE;
        private Quaternion qloffset = Quaternion.identity;
        private Quaternion qroffset = Quaternion.identity;
        private bool qroffset_cal = false;
        private bool qloffset_cal = false;
        private int m_frameCounter = 0;
        private float m_timeCounter = 0.0f;
        private float m_lastFramerate = 0.0f;
        private VRTRIXGloveRunningMode Mode = VRTRIXGloveRunningMode.NONE;
        private static bool RH_Mode;
        private static bool LH_Mode;
        private const float degToRad = (float)(Math.PI / 180.0);
        private float LastFeedbackTime = 0.0f;

        void Start()
        {
            RH = new VRTRIXDataWrapper(AdvancedMode);
            LH = new VRTRIXDataWrapper(AdvancedMode);
            LastFeedbackTime = Time.time;
        }
        void CheckToStart()
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
        void FixedUpdate()
        {
            if (RH_Mode && RH.GetReceivedStatus() == VRTRIXGloveStatus.NORMAL)
            {
                if (RH.GetReceivedRotation(VRTRIXBones.R_Hand) != Quaternion.identity && !qroffset_cal)
                {
                    qroffset = GetOffset(objectToAlign, RH, HANDTYPE.RIGHT_HAND);
                    qroffset_cal = true;
                }

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

                RH_Gesture = VRTRIXGloveGestureRecognition.GestureDetection(RH, HANDTYPE.RIGHT_HAND);
            }



            if (LH_Mode && LH.GetReceivedStatus() == VRTRIXGloveStatus.NORMAL)
            {
                if (LH.GetReceivedRotation(VRTRIXBones.L_Hand) != Quaternion.identity && !qloffset_cal)
                {
                    qloffset = GetOffset(objectToAlign, LH, HANDTYPE.LEFT_HAND);
                    qloffset_cal = true;
                }

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

                //print(GameObject.Find("L_Thumb_1").transform.rotation * Quaternion.Inverse(LH.GetReceivedRotation(VRTRIXBones.L_Hand)));
                //print(GameObject.Find("L_Index_2").transform.rotation  + " , " + GameObject.Find("L_Hand").transform.rotation + " ," + GameObject.Find("L_Index_2").transform.rotation* Quaternion.Inverse(GameObject.Find("L_Hand").transform.rotation));
                //print(LH.DataValidStatus(VRTRIXBones.L_Index_1) + ", " + LH.DataValidStatus(VRTRIXBones.L_Middle_1) + ", " + LH.DataValidStatus(VRTRIXBones.L_Ring_1) + ", " + LH.DataValidStatus(VRTRIXBones.L_Pinky_1) + ", " + LH.DataValidStatus(VRTRIXBones.L_Thumb_1));
                LH_Gesture = VRTRIXGloveGestureRecognition.GestureDetection(LH, HANDTYPE.LEFT_HAND);
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (UIEnabled)
            {
                if (m_timeCounter < m_refreshTime)
                {
                    m_timeCounter += Time.deltaTime;
                    m_frameCounter++;
                }
                else
                {
                    m_lastFramerate = (float)m_frameCounter / m_timeCounter;
                    m_frameCounter = 0;
                    m_timeCounter = 0.0f;
                }
                m_FPS.GetComponent<Text>().text = "FRAME RATE:   " + m_lastFramerate.ToString() + " fps";


                if (RH_Mode)
                {
                    m_Status.GetComponent<Text>().text = "GLOVE STATUS:   CONNECTED";
                    m_MODE.GetComponent<Text>().text = "MODE:   RIGHT ONLY";
                    m_RHStatus.GetComponent<Text>().text = "RIGHT HAND STATUS: CONNECTED";

                    m_RHRadio.GetComponent<Text>().text = "Radio Strength:  " + RH.GetReceiveRadioStrength().ToString() + " dB";
                    RadioStrengthGUI(m_RHRadioBar.GetComponent<Image>(), RH.GetReceiveRadioStrength());
                    m_RHCal.GetComponent<Text>().text = "Cal Score:  " + RH.GetReceivedCalScoreMean().ToString();
                    CalScoreGUI(m_RHCalBar.GetComponent<Image>(), RH.GetReceivedCalScoreMean());

                    m_RHDataRate.GetComponent<Text>().text = "RIGHT HAND DATA RATE: " + RH.GetReceivedDataRate().ToString() + "/s";

                    m_RHand.GetComponent<Text>().text = "R_Hand   " + RH.GetReceivedRotation(VRTRIXBones.R_Hand).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Hand).ToString();
                    m_RThumb.GetComponent<Text>().text = "R_Thumb   " + RH.GetReceivedRotation(VRTRIXBones.R_Thumb_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Thumb_2).ToString();
                    m_RIndex.GetComponent<Text>().text = "R_Index   " + RH.GetReceivedRotation(VRTRIXBones.R_Index_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Index_2).ToString();
                    m_RMiddle.GetComponent<Text>().text = "R_Middle   " + RH.GetReceivedRotation(VRTRIXBones.R_Middle_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Middle_2).ToString();
                    m_RRing.GetComponent<Text>().text = "R_Ring   " + RH.GetReceivedRotation(VRTRIXBones.R_Ring_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Ring_2).ToString();
                    m_RPinky.GetComponent<Text>().text = "R_Pinky   " + RH.GetReceivedRotation(VRTRIXBones.R_Pinky_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Pinky_2).ToString();
                }

                if (LH_Mode)
                {
                    m_Status.GetComponent<Text>().text = "GLOVE STATUS:   CONNECTED";
                    m_MODE.GetComponent<Text>().text = "MODE:   LEFT ONLY";
                    m_LHStatus.GetComponent<Text>().text = "LEFT HAND STATUS: CONNECTED";

                    m_LHRadio.GetComponent<Text>().text = "Radio Strength:  " + LH.GetReceiveRadioStrength().ToString() + " dB";
                    RadioStrengthGUI(m_LHRadioBar.GetComponent<Image>(), LH.GetReceiveRadioStrength());
                    m_LHCal.GetComponent<Text>().text = "Cal Score:  " + LH.GetReceivedCalScoreMean().ToString();
                    CalScoreGUI(m_LHCalBar.GetComponent<Image>(), LH.GetReceivedCalScoreMean());

                    m_LHDataRate.GetComponent<Text>().text = "LEFT HAND DATA RATE: " + LH.GetReceivedDataRate().ToString() + "/s";

                    m_LHand.GetComponent<Text>().text = "L_HAND:   " + LH.GetReceivedRotation(VRTRIXBones.L_Hand) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Hand).ToString();
                    m_LThumb.GetComponent<Text>().text = "L_THUMB:   " + LH.GetReceivedRotation(VRTRIXBones.L_Thumb_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Thumb_2).ToString();
                    m_LIndex.GetComponent<Text>().text = "L_INDEX:   " + LH.GetReceivedRotation(VRTRIXBones.L_Index_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Index_2).ToString();
                    m_LMiddle.GetComponent<Text>().text = "L_MIDDLE:   " + LH.GetReceivedRotation(VRTRIXBones.L_Index_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Index_2).ToString();
                    m_LRing.GetComponent<Text>().text = "L_RING:   " + LH.GetReceivedRotation(VRTRIXBones.L_Ring_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Ring_2).ToString();
                    m_LPinky.GetComponent<Text>().text = "L_PINKY:   " + LH.GetReceivedRotation(VRTRIXBones.L_Pinky_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Pinky_2).ToString();
                }

                if (RH_Mode && LH_Mode)
                {
                    m_MODE.GetComponent<Text>().text = "MODE:   PAIR";
                }
            }

            if (RH_Mode)
            {
                Mode = VRTRIXGloveRunningMode.RIGHT;
            }
            if (LH_Mode)
            {
                Mode = VRTRIXGloveRunningMode.LEFT;
            }
            if (RH_Mode && LH_Mode)
            {
                Mode = VRTRIXGloveRunningMode.PAIR;
            }
        }

        private void ReceiveLHData()
        {
            LH_Mode = LH.Init(HANDTYPE.LEFT_HAND);
            print("LH_Mode: " + LH_Mode);
            if (LH_Mode)
            {
                LH_Thread_read = new Thread(LH.streaming_read_begin);
                LH_Thread_read.Start();
                LH_receivedData = new Thread(ReceiveLHDataAsync);
                LH_receivedData.Start();
                //LH.receivedData(HANDTYPE.LEFT_HAND);
            }
        }
        

        private void ReceiveRHData()
        {
            RH_Mode = RH.Init(HANDTYPE.RIGHT_HAND);
            print("RH_Mode: " + RH_Mode);
            if (RH_Mode)
            {
                RH_Thread_read = new Thread(RH.streaming_read_begin);
                RH_Thread_read.Start();
                RH_receivedData = new Thread(ReceiveRHDataAsync);
                RH_receivedData.Start();
                //RH.receivedData(HANDTYPE.RIGHT_HAND);
            }
        }

        private static void ReceiveLHDataAsync()
        {
            LH.receivedData(HANDTYPE.LEFT_HAND);
        }

        private static void ReceiveRHDataAsync()
        {
            RH.receivedData(HANDTYPE.RIGHT_HAND);
        }

        public static VRTRIXGloveGesture GetGesture(HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND && LH_Mode)
            {
                return LH_Gesture;
            }
            else if(type == HANDTYPE.RIGHT_HAND && RH_Mode)
            {
                return RH_Gesture;
            }
            else
            {
                return VRTRIXGloveGesture.BUTTONNONE;
            }
        }

        void OnApplicationQuit()
        {
            if (LH_Mode)
            {
                //  LH_Thread_read.Abort();
                LH.ClosePort();
            }
            if (RH_Mode)
            {
                //  RH_Thread_read.Abort();
                RH.ClosePort();

            }
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(0, Screen.height / 8, Screen.width / 8, Screen.height / 8), "Reset"))
            {
                if (LH_Mode)
                {
                    LH.alignmentCheck(HANDTYPE.LEFT_HAND);
                }
                if (RH_Mode)
                {
                    RH.alignmentCheck(HANDTYPE.RIGHT_HAND);
                }
            }

            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.CLOSED && RH.GetReceivedStatus() == VRTRIXGloveStatus.CLOSED)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Connect"))
                {
                    //ThreadPool.QueueUserWorkItem(CheckToStart);
                    CheckToStart();
                }
            }

            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.NORMAL || RH.GetReceivedStatus() == VRTRIXGloveStatus.NORMAL)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Pause"))
                {
                    if (LH_Mode)
                    {
                        LH.SetReceivedStatus(VRTRIXGloveStatus.PAUSED);
                    }
                    if (RH_Mode)
                    {
                        RH.SetReceivedStatus(VRTRIXGloveStatus.PAUSED);
                    }
                }
            }

            if (LH.GetReceivedStatus() == VRTRIXGloveStatus.PAUSED || RH.GetReceivedStatus() == VRTRIXGloveStatus.PAUSED)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Resume"))
                {
                    if (LH_Mode)
                    {
                        LH.SetReceivedStatus(VRTRIXGloveStatus.NORMAL);
                    }
                    if (RH_Mode)
                    {
                        RH.SetReceivedStatus(VRTRIXGloveStatus.NORMAL);
                    }
                }
            }

            if (GUI.Button(new Rect(0, Screen.height / 4, Screen.width / 8, Screen.height / 8), "Calibrate"))
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

            if (GUI.Button(new Rect(0, Screen.height * (3.0f / 8.0f), Screen.width / 8, Screen.height / 8), "Vibrate"))
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

        }

        private void SetRotation(VRTRIXBones bone, Quaternion rotation, bool valid, HANDTYPE type)
        {
            string bone_name = VRTRIXUtilities.GetBoneName((int)bone);
            Quaternion qr = new Quaternion(1f, 0f, 0f, 0f);

            GameObject obj = GameObject.Find(bone_name);
            //print(obj);
            if (obj != null)
            {
                if (!float.IsNaN(rotation.x) && !float.IsNaN(rotation.y) && !float.IsNaN(rotation.z) && !float.IsNaN(rotation.w))
                {
                    if (valid)
                    {

                        if (type == HANDTYPE.LEFT_HAND)
                        {
                            //obj.transform.rotation = (rotation * qr);
                            obj.transform.rotation = qloffset * (rotation * qr);
                            //print(obj.transform.rotation);
                        }
                        else if (type == HANDTYPE.RIGHT_HAND)
                        {
                            //obj.transform.rotation =  (rotation * qr);
                            obj.transform.rotation = qroffset * (rotation * qr);

                        }
                    }
                }
            }
        }

        private static Quaternion GetOffset(GameObject objectToAlign, VRTRIXDataWrapper glove, HANDTYPE type)
        {
            //print("IMU: " + glove.GetReceivedRotation(VRTRIXBones.L_Hand).eulerAngles.y);
            //print("objectToAlign: " + objectToAlign.transform.rotation.eulerAngles.y);
            float offset = glove.GetReceivedRotation(VRTRIXBones.L_Hand).eulerAngles.y - objectToAlign.transform.rotation.eulerAngles.y - 90f;

            if (offset > 180f)
            {
                offset = 360f - offset;
            }
            else if (offset < -180f)
            {
                offset = 360f + offset;
            }
            //print(offset);
            return new Quaternion(0, Mathf.Sin(-offset * degToRad / 2), 0, Mathf.Cos(-offset * degToRad / 2));
        }

        private static void RadioStrengthGUI(Image image, int radiostrength)
        {
            image.fillAmount = ((radiostrength + 100) / 70f > 1) ? 1 : ((radiostrength + 100) / 70f);
            if (radiostrength < -70)
            {
                image.color = Color.red;
            }
            else if (radiostrength < -55)
            {
                image.color = Color.yellow;
            }
            else
            {
                image.color = Color.green;
            }
        }

        private static void CalScoreGUI(Image image, int calscore)
        {
            image.fillAmount = (calscore / 15f > 1) ? 0 : (1 - calscore / 15f);
            if (calscore > 9)
            {
                image.color = Color.red;
            }
            else if (calscore > 5)
            {
                image.color = Color.yellow;
            }
            else
            {
                image.color = Color.green;
            }
        }

    }
}



