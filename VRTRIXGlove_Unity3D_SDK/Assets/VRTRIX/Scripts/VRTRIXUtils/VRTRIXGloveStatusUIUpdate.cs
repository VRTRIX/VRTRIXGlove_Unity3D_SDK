//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: A simple GUI inluding VRTRIX Digital Glove status and 
//          sensor data is provided by this script.
//
//=============================================================================
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace VRTRIX
{
    public class VRTRIXGloveStatusUIUpdate : MonoBehaviour
    {
        [Header("UICanvas")]
        public Canvas m_canvas;

        private GameObject m_AdvancedModeToggle;
        private GameObject m_FPS;
        private GameObject m_HardwareVersionDropDown;
        private GameObject m_ServerIP;
        private GameObject m_GloveDeviceID;
        private GameObject m_ConnectButton;
        private GameObject m_LHHapticsButton;
        private GameObject m_RHHapticsButton;
        private GameObject m_LHResetButton;
        private GameObject m_RHResetButton;
        private GameObject m_ParameterPanelToggle;
        private GameObject m_ParametersPanel;

        private GameObject m_Status;
        private GameObject m_LHRadio;
        private GameObject m_RHRadio;
        private GameObject m_LHBattery;
        private GameObject m_RHBattery;
        private GameObject m_LHRadioBar;
        private GameObject m_RHRadioBar;
        private GameObject m_LHCal;
        private GameObject m_RHCal;
        private GameObject m_LHCalBar;
        private GameObject m_RHCalBar;
        private GameObject m_LHCalStat;
        private GameObject m_RHCalStat;
        private GameObject m_LHDataRate;
        private GameObject m_RHDataRate;

        private GameObject m_HandTypeDropDown;
        private GameObject m_FingerSpacingSlider;
        private GameObject m_FingerCurvedSpacingSlider;
        private GameObject m_ThumbProximalSlerpSlider;
        private GameObject m_ThumbMiddleSlerpSlider;

        private GameObject m_ThumbOffsetXSlider;
        private GameObject m_ThumbOffsetYSlider;
        private GameObject m_ThumbOffsetZSlider;
        private GameObject m_ThumbOffsetSelectorDropdown;
        private GameObject m_ThumbOffsetResetButton;

        private GameObject m_AlignFingerButton;
        private GameObject m_HardwareCalButton;

        private VRTRIXGloveDataStreaming glove3D;

        private Vector3[] thumb_offset = new Vector3[3];
        private int m_frameCounter = 0;
        private float m_timeCounter = 0.0f;
        private float m_lastFramerate = 0.0f;
        private int m_radioStrengthLH = 0;
        private int m_radioStrengthRH = 0;
        public float m_refreshTime = 1.0f;
        private Quaternion m_lastFrameQuat = Quaternion.identity;
        // Use this for initialization
        void Start()
        {
            try
            {
                glove3D = this.gameObject.GetComponent<VRTRIXGloveDataStreaming>();

                m_HardwareVersionDropDown = FindDeepChild(m_canvas.gameObject.transform, "HardwareVersion");
                m_ServerIP = FindDeepChild(m_canvas.gameObject.transform, "ServerIP");
                m_GloveDeviceID = FindDeepChild(m_canvas.gameObject.transform, "GloveDeviceID");
                m_FPS = FindDeepChild(m_canvas.gameObject.transform, "FPS");
                m_ConnectButton = FindDeepChild(m_canvas.gameObject.transform, "ConnectButton");
                m_LHHapticsButton = FindDeepChild(m_canvas.gameObject.transform, "LH_Haptics");
                m_RHHapticsButton = FindDeepChild(m_canvas.gameObject.transform, "RH_Haptics");
                m_LHResetButton = FindDeepChild(m_canvas.gameObject.transform, "LH_Reset");
                m_RHResetButton = FindDeepChild(m_canvas.gameObject.transform, "RH_Reset");

                m_Status = FindDeepChild(m_canvas.gameObject.transform, "STATUS");
                m_LHRadio = FindDeepChild(m_canvas.gameObject.transform, "LH_RADIO");
                m_RHRadio = FindDeepChild(m_canvas.gameObject.transform, "RH_RADIO");
                m_LHBattery = FindDeepChild(m_canvas.gameObject.transform, "LH_BATTERY");
                m_RHBattery = FindDeepChild(m_canvas.gameObject.transform, "RH_BATTERY");
                m_LHRadioBar = FindDeepChild(m_canvas.gameObject.transform, "LH_RADIO_BAR");
                m_RHRadioBar = FindDeepChild(m_canvas.gameObject.transform, "RH_RADIO_BAR");
                m_LHCal = FindDeepChild(m_canvas.gameObject.transform, "LH_CAL");
                m_RHCal = FindDeepChild(m_canvas.gameObject.transform, "RH_CAL");
                m_LHCalBar = FindDeepChild(m_canvas.gameObject.transform, "LH_CAL_BAR");
                m_RHCalBar = FindDeepChild(m_canvas.gameObject.transform, "RH_CAL_BAR");
                m_LHCalStat = FindDeepChild(m_canvas.gameObject.transform, "LH_Cal_Status");
                m_RHCalStat = FindDeepChild(m_canvas.gameObject.transform, "RH_Cal_Status");
                m_LHDataRate = FindDeepChild(m_canvas.gameObject.transform, "LH_DATARATE");
                m_RHDataRate = FindDeepChild(m_canvas.gameObject.transform, "RH_DATARATE");

                m_ParameterPanelToggle = FindDeepChild(m_canvas.gameObject.transform, "ParameterPanelToggle");
                m_ParametersPanel = FindDeepChild(m_canvas.gameObject.transform, "ParametersPanel");
                m_AdvancedModeToggle = FindDeepChild(m_canvas.gameObject.transform, "AdvancedMode");
                m_HandTypeDropDown = FindDeepChild(m_canvas.gameObject.transform, "HandTypeDropDown");
                m_FingerSpacingSlider = FindDeepChild(m_canvas.gameObject.transform, "FingerSpacingSlider");
                m_FingerCurvedSpacingSlider = FindDeepChild(m_canvas.gameObject.transform, "FingerCurvedSpacingSlider");
                m_ThumbProximalSlerpSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbProximalSlerpSlider");
                m_ThumbMiddleSlerpSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbMiddleSlerpSlider");

                m_ThumbOffsetXSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbOffsetXSlider");
                m_ThumbOffsetYSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbOffsetYSlider");
                m_ThumbOffsetZSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbOffsetZSlider");
                m_ThumbOffsetSelectorDropdown = FindDeepChild(m_canvas.gameObject.transform, "ThumbOffsetSelectorDropdown");
                m_ThumbOffsetResetButton = FindDeepChild(m_canvas.gameObject.transform, "ThumbOffsetResetButton");
                m_AlignFingerButton = FindDeepChild(m_canvas.gameObject.transform, "AlignFingerButton");
                m_HardwareCalButton = FindDeepChild(m_canvas.gameObject.transform, "8FigureCalButton");

                m_ServerIP.GetComponent<InputField>().text = "127.0.0.1";
                m_ConnectButton.GetComponentInChildren<Text>().text = "Connect";
                m_ConnectButton.GetComponent<Button>().onClick.AddListener(OnConnectGlovePressed);
                m_LHHapticsButton.GetComponent<Button>().onClick.AddListener(OnLHTriggerHapticsPressed);
                m_RHHapticsButton.GetComponent<Button>().onClick.AddListener(OnRHTriggerHapticsPressed);
                m_LHResetButton.GetComponent<Button>().onClick.AddListener(OnLHResetPressed);
                m_RHResetButton.GetComponent<Button>().onClick.AddListener(OnRHResetPressed);
                m_ParameterPanelToggle.GetComponent<Toggle>().onValueChanged.AddListener(OnToggleParamPanel);
                m_ParametersPanel.SetActive(false);


                UpdateUIValue((HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2);
                m_HandTypeDropDown.GetComponent<Dropdown>().onValueChanged.AddListener(OnToggleHandType);
                m_AdvancedModeToggle.GetComponent<Toggle>().onValueChanged.AddListener(OnToggleAdvancedMode);
                m_FingerSpacingSlider.GetComponent<Slider>().onValueChanged.AddListener(OnFingerSpacingChanged);
                m_FingerCurvedSpacingSlider.GetComponent<Slider>().onValueChanged.AddListener(OnFingerCurvedSpacingChanged);
                m_ThumbProximalSlerpSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbProximalSlerpChanged);
                m_ThumbMiddleSlerpSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbMiddleSlerpChanged);

                m_ThumbOffsetXSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbOffsetXChanged);
                m_ThumbOffsetYSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbOffsetYChanged);
                m_ThumbOffsetZSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbOffsetZChanged);
                m_ThumbOffsetSelectorDropdown.GetComponent<Dropdown>().onValueChanged.AddListener(OnToggleThumbIndex);
                m_ThumbOffsetResetButton.GetComponent<Button>().onClick.AddListener(OnResetThumbOffset);
                m_AlignFingerButton.GetComponent<Button>().onClick.AddListener(OnAlignFinger);
                m_HardwareCalButton.GetComponent<Button>().onClick.AddListener(OnHardwareCal);

            }
            catch (Exception e)
            {
                //Debug.Log(e);
            }
        }

        // Update is called once per frame
        void Update()
        {
            try
            {
                if (glove3D.GetGloveConnectionStat(HANDTYPE.RIGHT_HAND) && glove3D.GetGloveConnectionStat(HANDTYPE.LEFT_HAND))
                {

                    if (m_ConnectButton.GetComponentInChildren<Text>().text == "Connect")
                    {
                        m_ConnectButton.GetComponentInChildren<Text>().text = "Disconnect";
                        m_ConnectButton.GetComponent<Button>().onClick.RemoveAllListeners();
                        m_ConnectButton.GetComponent<Button>().onClick.AddListener(OnDisconnectGlovePressed);
                    }
                }
                else if (!glove3D.GetGloveConnectionStat(HANDTYPE.RIGHT_HAND) && !glove3D.GetGloveConnectionStat(HANDTYPE.LEFT_HAND))
                {

                    if (m_ConnectButton.GetComponentInChildren<Text>().text == "Disconnect")
                    {
                        m_ConnectButton.GetComponentInChildren<Text>().text = "Connect";
                        m_ConnectButton.GetComponent<Button>().onClick.RemoveAllListeners();
                        m_ConnectButton.GetComponent<Button>().onClick.AddListener(OnConnectGlovePressed);
                    }
                }

                if (m_timeCounter < m_refreshTime)
                {
                    m_timeCounter += Time.deltaTime;
                    m_radioStrengthRH += glove3D.GetReceiveRadioStrength(HANDTYPE.RIGHT_HAND);
                    m_radioStrengthLH += glove3D.GetReceiveRadioStrength(HANDTYPE.LEFT_HAND);
                    m_frameCounter++;
                }
                else
                {
                    m_lastFramerate = (int)Math.Ceiling((float)m_frameCounter / m_timeCounter);
                    m_RHRadio.GetComponent<Text>().text = "Radio Strength:  " + (m_radioStrengthRH / m_frameCounter).ToString() + " dB";
                    m_LHRadio.GetComponent<Text>().text = "Radio Strength:  " + (m_radioStrengthLH / m_frameCounter).ToString() + " dB";

                    RadioStrengthGUI(m_RHRadioBar.GetComponent<Image>(), m_radioStrengthRH / m_frameCounter);
                    RadioStrengthGUI(m_LHRadioBar.GetComponent<Image>(), m_radioStrengthLH / m_frameCounter);

                    m_frameCounter = 0;
                    m_timeCounter = 0.0f;
                    m_radioStrengthRH = 0;
                    m_radioStrengthLH = 0;
                }

                m_FPS.GetComponent<Text>().text = "Frame Rate:   " + m_lastFramerate.ToString() + " fps";
                if (glove3D.GetGloveConnectionStat(HANDTYPE.RIGHT_HAND) || glove3D.GetGloveConnectionStat(HANDTYPE.LEFT_HAND))
                {
                    m_Status.GetComponent<Text>().text = "Server Status:   CONNECTED";
                }
                else
                {
                    m_Status.GetComponent<Text>().text = "Server Status:   DISCONNECTED";
                }

                //Right hand parameters
                m_RHCalStat.SetActive(glove3D.GetReceivedCalScoreMean(HANDTYPE.RIGHT_HAND) > 5 && glove3D.GetGloveConnectionStat(HANDTYPE.RIGHT_HAND));
                CalScoreGUI(m_RHCalBar.GetComponent<Image>(), glove3D.GetReceivedCalScoreMean(HANDTYPE.RIGHT_HAND));
                m_RHBattery.GetComponent<Text>().text = "Battery:  " + glove3D.GetBatteryLevel(HANDTYPE.RIGHT_HAND).ToString() + " %";
                m_RHDataRate.GetComponent<Text>().text = "Data Rate: " + glove3D.GetReceivedDataRate(HANDTYPE.RIGHT_HAND).ToString() + "/s";


                //Left hand parameters
                m_LHCalStat.SetActive(glove3D.GetReceivedCalScoreMean(HANDTYPE.LEFT_HAND) > 5 && glove3D.GetGloveConnectionStat(HANDTYPE.LEFT_HAND));
                CalScoreGUI(m_LHCalBar.GetComponent<Image>(), glove3D.GetReceivedCalScoreMean(HANDTYPE.LEFT_HAND));
                m_LHBattery.GetComponent<Text>().text = "Battery:  " + glove3D.GetBatteryLevel(HANDTYPE.LEFT_HAND).ToString() + " %";
                m_LHDataRate.GetComponent<Text>().text = "Data Rate: " + glove3D.GetReceivedDataRate(HANDTYPE.LEFT_HAND).ToString() + "/s";
            }
            catch (Exception e)
            {
                //Debug.Log(e);
            }
        }
        public void OnToggleThumbIndex(int index)
        {
            UpdateUIValue((HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2);
        }
        
        public void OnToggleHandType(int index)
        {
            UpdateUIValue((HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2);
        }

        public void OnToggleAdvancedMode(bool enabled)
        {
            glove3D.SetAdvancedMode(enabled);
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            UpdateUIValue(type);
        }

        public void OnSelectHardwareVersion(int index)
        {
            glove3D.SetHardwareVersion((GLOVEVERSION)index);
        }

        public void OnConnectGlovePressed()
        {
            glove3D.OnConnectGlove();
        }
        public void OnDisconnectGlovePressed()
        {
            glove3D.OnDisconnectGlove();
        }

        public void OnLHTriggerHapticsPressed()
        {
            glove3D.OnVibrate(HANDTYPE.LEFT_HAND);
        }
        public void OnRHTriggerHapticsPressed()
        {
            glove3D.OnVibrate(HANDTYPE.RIGHT_HAND);
        }
        public void OnLHResetPressed()
        {
            glove3D.OnAlignWrist(HANDTYPE.LEFT_HAND);
        }
        public void OnRHResetPressed()
        {
            glove3D.OnAlignWrist(HANDTYPE.RIGHT_HAND);
        }
        public void OnToggleParamPanel(bool bIsEnabled)
        {
            FindDeepChild(m_canvas.gameObject.transform, "ParametersPanel").SetActive(bIsEnabled);
        }
        public void OnHandTypeDropDownChanged(int index)
        {
            UpdateUIValue((HANDTYPE)index + 2);
        }
        public string GetServerIP()
        {
            if (m_ServerIP == null) return "127.0.0.1";
            return m_ServerIP.GetComponent<InputField>().text;
        }
        public int GetGloveDeviceID()
        {
            if (m_GloveDeviceID == null) return 0;
            return m_GloveDeviceID.GetComponent<Dropdown>().value;
        }
        public bool GetAdvancedMode()
        {
            m_AdvancedModeToggle = FindDeepChild(m_canvas.gameObject.transform, "AdvancedMode");
            return m_AdvancedModeToggle.GetComponent<Toggle>().isOn;
        }
        public GLOVEVERSION GetHardwareVersion()
        {
            m_HardwareVersionDropDown = FindDeepChild(m_canvas.gameObject.transform, "HardwareVersion");
            return (GLOVEVERSION)m_HardwareVersionDropDown.GetComponent<Dropdown>().value;
        }

        public void UpdateUIValue(HANDTYPE type)
        {
            m_AdvancedModeToggle.GetComponent<Toggle>().isOn = glove3D.AdvancedMode;
            m_HardwareVersionDropDown.GetComponent<Dropdown>().value = (int)glove3D.version;
            GLOVEVERSION version = (GLOVEVERSION)m_HardwareVersionDropDown.GetComponent<Dropdown>().value;
            if (m_AdvancedModeToggle.GetComponent<Toggle>().isOn)
            {
                m_FingerSpacingSlider.GetComponent<Slider>().interactable = false;
                m_FingerCurvedSpacingSlider.GetComponent<Slider>().interactable = false;
                //if(version < GLOVEVERSION.PRO7)
                //{
                //    m_AlignFingerButton.GetComponent<Button>().interactable = true;
                //}
            }
            else
            {
                m_FingerSpacingSlider.GetComponent<Slider>().interactable = true;
                m_FingerCurvedSpacingSlider.GetComponent<Slider>().interactable = true;
                //if (version < GLOVEVERSION.PRO7)
                //{
                //    m_AlignFingerButton.GetComponent<Button>().interactable = false;
                //}
            }

            m_FingerSpacingSlider.GetComponent<Slider>().value = (float)glove3D.finger_spacing;
            m_FingerSpacingSlider.transform.Find("FingerSpacing").GetComponent<Text>().text = glove3D.finger_spacing.ToString("F1");

            m_FingerCurvedSpacingSlider.GetComponent<Slider>().value = (float)glove3D.final_finger_spacing;
            m_FingerCurvedSpacingSlider.transform.Find("CurvedSpacing").GetComponent<Text>().text = glove3D.final_finger_spacing.ToString("F1");

            m_ThumbProximalSlerpSlider.GetComponent<Slider>().value = (float)glove3D.thumb_proximal_slerp;
            m_ThumbProximalSlerpSlider.transform.Find("ThumbProximalSlerp").GetComponent<Text>().text = glove3D.thumb_proximal_slerp.ToString("F1");

            m_ThumbMiddleSlerpSlider.GetComponent<Slider>().value = (float)glove3D.thumb_middle_slerp;
            m_ThumbMiddleSlerpSlider.transform.Find("ThumbMiddleSlerp").GetComponent<Text>().text = glove3D.thumb_middle_slerp.ToString("F1");

            if (type == HANDTYPE.LEFT_HAND)
            {
                thumb_offset[0] = glove3D.thumb_offset_L[0];
                thumb_offset[1] = glove3D.thumb_offset_L[1];
                thumb_offset[2] = glove3D.thumb_offset_L[2];
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                thumb_offset[0] = glove3D.thumb_offset_R[0];
                thumb_offset[1] = glove3D.thumb_offset_R[1];
                thumb_offset[2] = glove3D.thumb_offset_R[2];
            }
            int index = m_ThumbOffsetSelectorDropdown.GetComponent<Dropdown>().value;

            m_ThumbOffsetXSlider.GetComponent<Slider>().value = (float)thumb_offset[index].x;
            m_ThumbOffsetXSlider.transform.Find("ThumbOffsetX").GetComponent<Text>().text = thumb_offset[index].x.ToString("F1");

            m_ThumbOffsetYSlider.GetComponent<Slider>().value = (float)thumb_offset[index].y;
            m_ThumbOffsetYSlider.transform.Find("ThumbOffsetY").GetComponent<Text>().text = thumb_offset[index].y.ToString("F1");

            m_ThumbOffsetZSlider.GetComponent<Slider>().value = (float)thumb_offset[index].z;
            m_ThumbOffsetZSlider.transform.Find("ThumbOffsetZ").GetComponent<Text>().text = thumb_offset[index].z.ToString("F1");

        }

        public void OnAlignFinger()
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            glove3D.OnAlignFingers(type);
        }
        public void OnHardwareCal()
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            glove3D.OnHardwareCalibrate(type);
        }

        public void OnFingerSpacingChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            glove3D.finger_spacing = value;
            if (type == HANDTYPE.LEFT_HAND) glove3D.LH.SetFingerSpacing(value);
            else if (type == HANDTYPE.RIGHT_HAND) glove3D.RH.SetFingerSpacing(value);
            UpdateUIValue(type);
        }

        public void OnFingerCurvedSpacingChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            glove3D.final_finger_spacing = value;
            if (type == HANDTYPE.LEFT_HAND) glove3D.LH.SetFinalFingerSpacing(value);
            else if (type == HANDTYPE.RIGHT_HAND) glove3D.RH.SetFinalFingerSpacing(value);
            UpdateUIValue(type);
        }

        public void OnThumbProximalSlerpChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            glove3D.thumb_proximal_slerp = value;
            if (type == HANDTYPE.LEFT_HAND) glove3D.LH.SetThumbSlerpRate(glove3D.thumb_proximal_slerp, glove3D.thumb_middle_slerp);
            else if (type == HANDTYPE.RIGHT_HAND) glove3D.RH.SetThumbSlerpRate(glove3D.thumb_proximal_slerp, glove3D.thumb_middle_slerp);
            UpdateUIValue(type);
        }

        public void OnThumbMiddleSlerpChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            glove3D.thumb_middle_slerp = value;
            if (type == HANDTYPE.LEFT_HAND) glove3D.LH.SetThumbSlerpRate(glove3D.thumb_proximal_slerp, glove3D.thumb_middle_slerp);
            else if (type == HANDTYPE.RIGHT_HAND) glove3D.RH.SetThumbSlerpRate(glove3D.thumb_proximal_slerp, glove3D.thumb_middle_slerp);
            UpdateUIValue(type);
        }

        public void OnThumbOffsetXChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            int index = m_ThumbOffsetSelectorDropdown.GetComponent<Dropdown>().value;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[index].x = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[index], VRTRIXBones.L_Thumb_1 + index);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[index].x = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[index], VRTRIXBones.R_Thumb_1 + index);
            }
            UpdateUIValue(type);
        }

        public void OnThumbOffsetYChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            int index = m_ThumbOffsetSelectorDropdown.GetComponent<Dropdown>().value;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[index].y = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[index], VRTRIXBones.L_Thumb_1 + index);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[+index].y = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[index], VRTRIXBones.R_Thumb_1 + index);
            }
            UpdateUIValue(type);
        }

        public void OnThumbOffsetZChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            int index = m_ThumbOffsetSelectorDropdown.GetComponent<Dropdown>().value;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[index].z = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[index], VRTRIXBones.L_Thumb_1 + index);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[index].z = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[index], VRTRIXBones.R_Thumb_1 + index);
            }
            UpdateUIValue(type);
        }

        public void OnResetThumbOffset()
        {
            glove3D.OnResetThumbOffset();
            UpdateUIValue((HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2);
        }

        private static void RadioStrengthGUI(Image image, int radiostrength)
        {
            image.fillAmount = ((radiostrength + 100) / 70f > 1) ? 1 : ((radiostrength + 100) / 70f);
            if (radiostrength < -80)
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

        //Breadth-first search
        public static GameObject FindDeepChild(Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                {
                    return c.gameObject;
                }

                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }
    }
}

