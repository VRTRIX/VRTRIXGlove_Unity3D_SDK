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
        private static VRTRIXDataWrapper RH = new VRTRIXDataWrapper();
        private static VRTRIXDataWrapper LH = new VRTRIXDataWrapper();
        private Thread RH_Thread, LH_Thread;
        private Quaternion qloffset = Quaternion.identity;
        private Quaternion qroffset = Quaternion.identity;
        private bool qroffset_cal = false;
        private bool qloffset_cal = false;
        private int m_frameCounter = 0;
        private float m_timeCounter = 0.0f;
        private float m_lastFramerate = 0.0f;
        public float m_refreshTime = 1.0f;
        private VRTRIXGloveRunningMode Mode = VRTRIXGloveRunningMode.NONE;
        private static bool RH_Mode;
        private static bool LH_Mode;
        private const float degToRad = (float)(Math.PI / 180.0);

        public Text fps { get; private set; }
        public Text mode { get; private set; }
        public Text status { get; private set; }
        public Text lh_radio { get; private set; }
        public Image lh_radio_bar { get; private set; }
        public Text rh_radio { get; private set; }
        public Image rh_radio_bar { get; private set; }
        public Text lh_cal { get; private set; }
        public Image lh_cal_bar { get; private set; }
        public Text rh_cal { get; private set; }
        public Image rh_cal_bar { get; private set; }
        public Text L_data_rate_text { get; private set; }
        public Text L_status_text { get; private set; }
        public Text L_hand_text { get; private set; }
        public Text L_index_text { get; private set; }
        public Text L_middle_text { get; private set; }
        public Text L_ring_text { get; private set; }
        public Text L_pinky_text { get; private set; }
        public Text L_thumb_text { get; private set; }
        public Text R_data_rate_text { get; private set; }
        public Text R_status_text { get; private set; }
        public Text R_hand_text { get; private set; }
        public Text R_index_text { get; private set; }
        public Text R_middle_text { get; private set; }
        public Text R_ring_text { get; private set; }
        public Text R_pinky_text { get; private set; }
        public Text R_thumb_text { get; private set; }
        // Use this for initialization
        void Start()
        {
            fps = GameObject.Find("Abstract/FPS").GetComponent<Text>();
            mode = GameObject.Find("Abstract/MODE").GetComponent<Text>();
            status = GameObject.Find("Abstract/STATUS").GetComponent<Text>();

            lh_radio = GameObject.Find("CAL_RADIO/LH_RADIO").GetComponent<Text>();
            rh_radio = GameObject.Find("CAL_RADIO/RH_RADIO").GetComponent<Text>();
            lh_radio_bar = GameObject.Find("CAL_RADIO/LH_RADIO_BAR").GetComponent<Image>();
            rh_radio_bar = GameObject.Find("CAL_RADIO/RH_RADIO_BAR").GetComponent<Image>();
            lh_cal = GameObject.Find("CAL_RADIO/LH_CAL").GetComponent<Text>();
            rh_cal = GameObject.Find("CAL_RADIO/RH_CAL").GetComponent<Text>();
            lh_cal_bar = GameObject.Find("CAL_RADIO/LH_CAL_BAR").GetComponent<Image>();
            rh_cal_bar = GameObject.Find("CAL_RADIO/RH_CAL_BAR").GetComponent<Image>();

            L_data_rate_text = GameObject.Find("LEFT/DATA_RATE").GetComponent<Text>();
            L_status_text = GameObject.Find("LEFT/STATUS").GetComponent<Text>();
            L_hand_text = GameObject.Find("LEFT/L_HAND").GetComponent<Text>();
            L_index_text = GameObject.Find("LEFT/L_INDEX").GetComponent<Text>();
            L_middle_text = GameObject.Find("LEFT/L_MIDDLE").GetComponent<Text>();
            L_ring_text = GameObject.Find("LEFT/L_RING").GetComponent<Text>();
            L_pinky_text = GameObject.Find("LEFT/L_PINKY").GetComponent<Text>();
            L_thumb_text = GameObject.Find("LEFT/L_THUMB").GetComponent<Text>();

            R_data_rate_text = GameObject.Find("RIGHT/DATA_RATE").GetComponent<Text>();
            R_status_text = GameObject.Find("RIGHT/STATUS").GetComponent<Text>();
            R_hand_text = GameObject.Find("RIGHT/R_HAND").GetComponent<Text>();
            R_index_text = GameObject.Find("RIGHT/R_INDEX").GetComponent<Text>();
            R_middle_text = GameObject.Find("RIGHT/R_MIDDLE").GetComponent<Text>();
            R_ring_text = GameObject.Find("RIGHT/R_RING").GetComponent<Text>();
            R_pinky_text = GameObject.Find("RIGHT/R_PINKY").GetComponent<Text>();
            R_thumb_text = GameObject.Find("RIGHT/R_THUMB").GetComponent<Text>();
        }
        void CheckToStart()
        {
            try
            {
                RH_Thread = new Thread(ReceiveRHData);
                RH_Thread.Start();

                LH_Thread = new Thread(ReceiveLHData);
                LH_Thread.Start();
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
                SetThumbRotation(VRTRIXBones.R_Thumb_1, Quaternion.Euler(15f, -15f, 0f));
                SetRotation(VRTRIXBones.R_Thumb_2, RH.GetReceivedRotation(VRTRIXBones.R_Thumb_2), RH.DataValidStatus(VRTRIXBones.R_Thumb_2), HANDTYPE.RIGHT_HAND);
                SetThumbRotation(VRTRIXBones.R_Thumb_2, Quaternion.Euler(10f, 0f, 0f));
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
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (m_timeCounter < m_refreshTime)
            {
                m_timeCounter += Time.deltaTime;
                m_frameCounter++;
            }
            else
            {
                //This code will break if you set your m_refreshTime to 0, which makes no sense.
                m_lastFramerate = (float)m_frameCounter / m_timeCounter;
                m_frameCounter = 0;
                m_timeCounter = 0.0f;
            }
            fps.text = "FRAME RATE:   " + m_lastFramerate.ToString() + " fps";


            if (RH_Mode)
            {
                Mode = VRTRIXGloveRunningMode.RIGHT;
                status.text = "GLOVE STATUS:   CONNECTED";
                mode.text = "MODE:   RIGHT ONLY";
                R_status_text.text = "RIGHT HAND STATUS: CONNECTED";

                rh_radio.text = "Radio Strength:  " + RH.GetReceiveRadioStrength().ToString() + " dB";
                RadioStrengthGUI(rh_radio_bar, RH.GetReceiveRadioStrength());
                rh_cal.text = "Cal Score:  " + RH.GetReceivedCalScoreMean().ToString();
                CalScoreGUI(rh_cal_bar, RH.GetReceivedCalScoreMean());

                R_data_rate_text.text = "RIGHT HAND DATA RATE: " + RH.GetReceivedDataRate().ToString() + "/s";

                R_hand_text.text = "R_Hand   " + RH.GetReceivedRotation(VRTRIXBones.R_Hand).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Hand).ToString();
                R_thumb_text.text = "R_Thumb   " + RH.GetReceivedRotation(VRTRIXBones.R_Thumb_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Thumb_2).ToString();
                R_index_text.text = "R_Index   " + RH.GetReceivedRotation(VRTRIXBones.R_Index_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Index_2).ToString();
                R_middle_text.text = "R_Middle   " + RH.GetReceivedRotation(VRTRIXBones.R_Middle_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Middle_2).ToString();
                R_ring_text.text = "R_Ring   " + RH.GetReceivedRotation(VRTRIXBones.R_Ring_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Ring_2).ToString();
                R_pinky_text.text = "R_Pinky   " + RH.GetReceivedRotation(VRTRIXBones.R_Pinky_2).ToString() + "    " + RH.GetReceivedCalScore(VRTRIXBones.R_Pinky_2).ToString();
            }

            if (LH_Mode)
            {
                Mode = VRTRIXGloveRunningMode.LEFT;
                status.text = "GLOVE STATUS:   CONNECTED";
                mode.text = "MODE:   LEFT ONLY";
                L_status_text.text = "LEFT HAND STATUS: CONNECTED";

                lh_radio.text = "Radio Strength:  " + LH.GetReceiveRadioStrength().ToString() + " dB";
                RadioStrengthGUI(lh_radio_bar, LH.GetReceiveRadioStrength());
                
                lh_cal.text = "Cal Score:  " + LH.GetReceivedCalScoreMean().ToString();
                CalScoreGUI(lh_cal_bar, LH.GetReceivedCalScoreMean());
                L_data_rate_text.text = "LEFT HAND DATA RATE: " + LH.GetReceivedDataRate().ToString() + "/s";

                L_hand_text.text = "L_HAND:   " + LH.GetReceivedRotation(VRTRIXBones.L_Hand) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Hand).ToString();
                L_thumb_text.text = "L_THUMB:   " + LH.GetReceivedRotation(VRTRIXBones.L_Thumb_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Thumb_2).ToString();
                L_index_text.text = "L_INDEX:   " + LH.GetReceivedRotation(VRTRIXBones.L_Index_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Index_2).ToString();
                L_middle_text.text = "L_MIDDLE:   " + LH.GetReceivedRotation(VRTRIXBones.L_Index_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Index_2).ToString();
                L_ring_text.text = "L_RING:   " + LH.GetReceivedRotation(VRTRIXBones.L_Ring_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Ring_2).ToString();
                L_pinky_text.text = "L_PINKY:   " + LH.GetReceivedRotation(VRTRIXBones.L_Pinky_2) + "    " + LH.GetReceivedCalScore(VRTRIXBones.L_Pinky_2).ToString();
            }

            if (RH_Mode && LH_Mode)
            {
                Mode = VRTRIXGloveRunningMode.PAIR;
                mode.text = "MODE:   PAIR";
            }
        }

        private static void ReceiveLHData()
        {
            LH_Mode = LH.Init(HANDTYPE.LEFT_HAND);
            print("LH_Mode: " + LH_Mode);
            if (LH_Mode)
            {
                LH.receivedData(HANDTYPE.LEFT_HAND);
            }
        }

        private static void ReceiveRHData()
        {
            RH_Mode = RH.Init(HANDTYPE.RIGHT_HAND);
            print("RH_Mode: " + RH_Mode);
            if (RH_Mode)
            {
                RH.receivedData(HANDTYPE.RIGHT_HAND);
            }
            
        }


        void OnApplicationQuit()
        {
            if (LH_Mode)
            {
                LH_Thread.Abort();
                LH.ClosePort();
            }
            if (RH_Mode)
            {
                RH_Thread.Abort();
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
        private void SetThumbRotation(VRTRIXBones bone, Quaternion offset)
        {
            string bone_name = VRTRIXUtilities.GetBoneName((int)bone);
            GameObject obj = GameObject.Find(bone_name);
            if (obj != null)
            {
                obj.transform.rotation = offset * obj.transform.rotation;
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
            image.fillAmount = (calscore / 15f > 1) ? 0 : (1- calscore / 15f);
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



