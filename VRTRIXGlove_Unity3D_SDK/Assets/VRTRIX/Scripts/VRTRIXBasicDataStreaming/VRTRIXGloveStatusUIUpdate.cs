//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: A simple GUI inluding VRTRIX Digital Glove status and 
//          sensor data is provided by this script.
//
//=============================================================================
using UnityEngine;
using UnityEngine.UI;

namespace VRTRIX
{
    public class VRTRIXGloveStatusUIUpdate : MonoBehaviour
    {
        [Header("UIComponent")]
        public GameObject m_FPS;
        public GameObject m_MODE;
        public GameObject m_Status;
        public GameObject m_LHRadio;
        public GameObject m_RHRadio;
        public GameObject m_LHBattery;
        public GameObject m_RHBattery;
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

        private VRTRIXGloveSimpleDataRead glove3D;

        private int m_frameCounter = 0;
        private float m_timeCounter = 0.0f;
        private float m_lastFramerate = 0.0f;
        public float m_refreshTime = 1.0f;


        // Use this for initialization
        void Start()
        {
            glove3D = this.gameObject.GetComponent<VRTRIXGloveSimpleDataRead>();
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
                m_lastFramerate = (float)m_frameCounter / m_timeCounter;
                m_frameCounter = 0;
                m_timeCounter = 0.0f;
            }
            m_FPS.GetComponent<Text>().text = "FRAME RATE:   " + m_lastFramerate.ToString() + " fps";

            if (glove3D.GetGloveConnectionStat(HANDTYPE.RIGHT_HAND) || glove3D.GetGloveConnectionStat(HANDTYPE.LEFT_HAND))
            {
                m_Status.GetComponent<Text>().text = "GLOVE STATUS:   CONNECTED";
            }
            else
            {
                m_Status.GetComponent<Text>().text = "GLOVE STATUS:   DISCONNECTED";
            }

            if (glove3D.GetGloveConnectionStat(HANDTYPE.RIGHT_HAND))
            {
                m_MODE.GetComponent<Text>().text = "MODE:   RIGHT ONLY";
                m_RHStatus.GetComponent<Text>().text = "RIGHT HAND STATUS: CONNECTED";
            }
            else
            {
                m_RHStatus.GetComponent<Text>().text = "RIGHT HAND STATUS: DISCONNECTED";
            }

            if(glove3D.GetGloveConnectionStat(HANDTYPE.LEFT_HAND))
            {
                m_MODE.GetComponent<Text>().text = "MODE:   LEFT ONLY";
                m_LHStatus.GetComponent<Text>().text = "LEFT HAND STATUS: CONNECTED";
            }
            else
            {
                m_LHStatus.GetComponent<Text>().text = "LEFT HAND STATUS: DISCONNECTED";
            }

            if (glove3D.GetGloveConnectionStat(HANDTYPE.RIGHT_HAND) && glove3D.GetGloveConnectionStat(HANDTYPE.LEFT_HAND))
            {
                m_MODE.GetComponent<Text>().text = "MODE:   PAIR";
            }

            //Right hand parameters
            m_RHRadio.GetComponent<Text>().text = "Radio Strength:  " + glove3D.GetReceiveRadioStrength(HANDTYPE.RIGHT_HAND).ToString() + " dB";
            RadioStrengthGUI(m_RHRadioBar.GetComponent<Image>(), glove3D.GetReceiveRadioStrength(HANDTYPE.RIGHT_HAND));
            m_RHCal.GetComponent<Text>().text = "Cal Score:  " + glove3D.GetReceivedCalScoreMean(HANDTYPE.RIGHT_HAND).ToString();
            CalScoreGUI(m_RHCalBar.GetComponent<Image>(), glove3D.GetReceivedCalScoreMean(HANDTYPE.RIGHT_HAND));
            m_RHBattery.GetComponent<Text>().text = "Battery:  " + glove3D.GetBatteryLevel(HANDTYPE.RIGHT_HAND).ToString() + " %";

            m_RHDataRate.GetComponent<Text>().text = "RIGHT HAND DATA RATE: " + glove3D.GetReceivedDataRate(HANDTYPE.RIGHT_HAND).ToString() + "/s";

            m_RHand.GetComponent<Text>().text = "R_Hand   " + glove3D.GetRotation(VRTRIXBones.R_Hand).ToString() + "    " + glove3D.GetCalScore(VRTRIXBones.R_Hand).ToString();
            m_RThumb.GetComponent<Text>().text = "R_Thumb   " + glove3D.GetRotation(VRTRIXBones.R_Thumb_2).ToString() + "    " + glove3D.GetCalScore(VRTRIXBones.R_Thumb_2).ToString();
            m_RIndex.GetComponent<Text>().text = "R_Index   " + glove3D.GetRotation(VRTRIXBones.R_Index_2).ToString() + "    " + glove3D.GetCalScore(VRTRIXBones.R_Index_2).ToString();
            m_RMiddle.GetComponent<Text>().text = "R_Middle   " + glove3D.GetRotation(VRTRIXBones.R_Middle_2).ToString() + "    " + glove3D.GetCalScore(VRTRIXBones.R_Middle_2).ToString();
            m_RRing.GetComponent<Text>().text = "R_Ring   " + glove3D.GetRotation(VRTRIXBones.R_Ring_2).ToString() + "    " + glove3D.GetCalScore(VRTRIXBones.R_Ring_2).ToString();
            m_RPinky.GetComponent<Text>().text = "R_Pinky   " + glove3D.GetRotation(VRTRIXBones.R_Pinky_2).ToString() + "    " + glove3D.GetCalScore(VRTRIXBones.R_Pinky_2).ToString();

            //Left hand parameters
            m_LHRadio.GetComponent<Text>().text = "Radio Strength:  " + glove3D.GetReceiveRadioStrength(HANDTYPE.LEFT_HAND).ToString() + " dB";
            RadioStrengthGUI(m_LHRadioBar.GetComponent<Image>(), glove3D.GetReceiveRadioStrength(HANDTYPE.LEFT_HAND));
            m_LHCal.GetComponent<Text>().text = "Cal Score:  " + glove3D.GetReceivedCalScoreMean(HANDTYPE.LEFT_HAND).ToString();
            CalScoreGUI(m_LHCalBar.GetComponent<Image>(), glove3D.GetReceivedCalScoreMean(HANDTYPE.LEFT_HAND));
            m_LHBattery.GetComponent<Text>().text = "Battery:  " + glove3D.GetBatteryLevel(HANDTYPE.LEFT_HAND).ToString() + " %";

            m_LHDataRate.GetComponent<Text>().text = "LEFT HAND DATA RATE: " + glove3D.GetReceivedDataRate(HANDTYPE.LEFT_HAND).ToString() + "/s";

            m_LHand.GetComponent<Text>().text = "L_HAND:   " + glove3D.GetRotation(VRTRIXBones.L_Hand) + "    " + glove3D.GetCalScore(VRTRIXBones.L_Hand).ToString();
            m_LThumb.GetComponent<Text>().text = "L_THUMB:   " + glove3D.GetRotation(VRTRIXBones.L_Thumb_2) + "    " + glove3D.GetCalScore(VRTRIXBones.L_Thumb_2).ToString();
            m_LIndex.GetComponent<Text>().text = "L_INDEX:   " + glove3D.GetRotation(VRTRIXBones.L_Index_2) + "    " + glove3D.GetCalScore(VRTRIXBones.L_Index_2).ToString();
            m_LMiddle.GetComponent<Text>().text = "L_MIDDLE:   " + glove3D.GetRotation(VRTRIXBones.L_Index_2) + "    " + glove3D.GetCalScore(VRTRIXBones.L_Index_2).ToString();
            m_LRing.GetComponent<Text>().text = "L_RING:   " + glove3D.GetRotation(VRTRIXBones.L_Ring_2) + "    " + glove3D.GetCalScore(VRTRIXBones.L_Ring_2).ToString();
            m_LPinky.GetComponent<Text>().text = "L_PINKY:   " + glove3D.GetRotation(VRTRIXBones.L_Pinky_2) + "    " + glove3D.GetCalScore(VRTRIXBones.L_Pinky_2).ToString();
            
        }
        void OnGUI()
        {
            if (GUI.Button(new Rect(0, Screen.height / 8, Screen.width / 8, Screen.height / 8), "Reset"))
            {
                glove3D.OnAlignFingers();
            }

            if (glove3D.GetReceivedStatus(HANDTYPE.LEFT_HAND) == VRTRIXGloveStatus.CLOSED && glove3D.GetReceivedStatus(HANDTYPE.RIGHT_HAND) == VRTRIXGloveStatus.CLOSED)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Connect"))
                {
                    glove3D.OnConnectGlove();
                }
            }

            if (glove3D.GetReceivedStatus(HANDTYPE.LEFT_HAND) == VRTRIXGloveStatus.NORMAL || glove3D.GetReceivedStatus(HANDTYPE.RIGHT_HAND) == VRTRIXGloveStatus.NORMAL)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Disconnect"))
                {
                    glove3D.OnDisconnectGlove();
                }
            }

            if (GUI.Button(new Rect(0, Screen.height / 4, Screen.width / 8, Screen.height / 8), "Hardware Calibrate"))
            {
                glove3D.OnHardwareCalibrate();
            }

            if (GUI.Button(new Rect(0, Screen.height * (3.0f / 8.0f), Screen.width / 8, Screen.height / 8), "Vibrate"))
            {
                glove3D.OnVibrate();
            }
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
    
