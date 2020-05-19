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
        private GameObject m_ServerPort;
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

        private GameObject m_ThumbProximalOffsetXSlider;
        private GameObject m_ThumbProximalOffsetYSlider;
        private GameObject m_ThumbProximalOffsetZSlider;
        private GameObject m_ThumbMiddleOffsetXSlider;
        private GameObject m_ThumbMiddleOffsetYSlider;
        private GameObject m_ThumbMiddleOffsetZSlider;
        private GameObject m_ThumbDistalOffsetXSlider;
        private GameObject m_ThumbDistalOffsetYSlider;
        private GameObject m_ThumbDistalOffsetZSlider;
        private GameObject m_AlignFingerButton;
        private GameObject m_HardwareCalButton;

        private VRTRIXGloveDataStreaming glove3D;

        private Vector3[] thumb_offset = new Vector3[3];
        private int m_frameCounter = 0;
        private float m_timeCounter = 0.0f;
        private float m_lastFramerate = 0.0f;
        private float m_lastFrameArriveTime = 0.0f;
        private int m_radioStrengthLH = 0;
        private int m_radioStrengthRH = 0;
        public float m_refreshTime = 1.0f;
        private Quaternion m_lastFrameQuat = Quaternion.identity;
        // Use this for initialization
        void Start()
        {
            glove3D = this.gameObject.GetComponent<VRTRIXGloveDataStreaming>();

            m_HardwareVersionDropDown = FindDeepChild(m_canvas.gameObject.transform, "HardwareVersion").gameObject;
            m_ServerIP = FindDeepChild(m_canvas.gameObject.transform, "ServerIP").gameObject;
            m_ServerPort = FindDeepChild(m_canvas.gameObject.transform, "ServerPort").gameObject;
            m_AdvancedModeToggle = FindDeepChild(m_canvas.gameObject.transform, "AdvancedMode").gameObject;
            m_FPS = FindDeepChild(m_canvas.gameObject.transform, "FPS").gameObject;
            m_ConnectButton = FindDeepChild(m_canvas.gameObject.transform, "ConnectButton").gameObject;
            m_LHHapticsButton = FindDeepChild(m_canvas.gameObject.transform, "LH_Haptics").gameObject;
            m_RHHapticsButton = FindDeepChild(m_canvas.gameObject.transform, "RH_Haptics").gameObject;
            m_LHResetButton = FindDeepChild(m_canvas.gameObject.transform, "LH_Reset").gameObject;
            m_RHResetButton = FindDeepChild(m_canvas.gameObject.transform, "RH_Reset").gameObject;
            m_ParameterPanelToggle = FindDeepChild(m_canvas.gameObject.transform, "ParameterPanelToggle").gameObject;
            m_ParametersPanel = FindDeepChild(m_canvas.gameObject.transform, "ParametersPanel").gameObject;

            m_Status = FindDeepChild(m_canvas.gameObject.transform, "STATUS").gameObject;
            m_LHRadio = FindDeepChild(m_canvas.gameObject.transform, "LH_RADIO").gameObject;
            m_RHRadio = FindDeepChild(m_canvas.gameObject.transform, "RH_RADIO").gameObject;
            m_LHBattery = FindDeepChild(m_canvas.gameObject.transform, "LH_BATTERY").gameObject;
            m_RHBattery = FindDeepChild(m_canvas.gameObject.transform, "RH_BATTERY").gameObject;
            m_LHRadioBar = FindDeepChild(m_canvas.gameObject.transform, "LH_RADIO_BAR").gameObject;
            m_RHRadioBar = FindDeepChild(m_canvas.gameObject.transform, "RH_RADIO_BAR").gameObject;
            m_LHCal = FindDeepChild(m_canvas.gameObject.transform, "LH_CAL").gameObject;
            m_RHCal = FindDeepChild(m_canvas.gameObject.transform, "RH_CAL").gameObject;
            m_LHCalBar = FindDeepChild(m_canvas.gameObject.transform, "LH_CAL_BAR").gameObject;
            m_RHCalBar = FindDeepChild(m_canvas.gameObject.transform, "RH_CAL_BAR").gameObject;
            m_LHCalStat = FindDeepChild(m_canvas.gameObject.transform, "LH_Cal_Status").gameObject;
            m_RHCalStat = FindDeepChild(m_canvas.gameObject.transform, "RH_Cal_Status").gameObject;
            m_LHDataRate = FindDeepChild(m_canvas.gameObject.transform, "LH_DATARATE").gameObject;
            m_RHDataRate = FindDeepChild(m_canvas.gameObject.transform, "RH_DATARATE").gameObject;

            m_HandTypeDropDown = FindDeepChild(m_canvas.gameObject.transform, "HandTypeDropDown").gameObject;
            m_FingerSpacingSlider = FindDeepChild(m_canvas.gameObject.transform, "FingerSpacingSlider").gameObject;
            m_FingerCurvedSpacingSlider = FindDeepChild(m_canvas.gameObject.transform, "FingerCurvedSpacingSlider").gameObject;
            m_ThumbProximalSlerpSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbProximalSlerpSlider").gameObject;
            m_ThumbMiddleSlerpSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbMiddleSlerpSlider").gameObject;

            m_ThumbProximalOffsetXSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbProximalOffsetXSlider").gameObject;
            m_ThumbProximalOffsetYSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbProximalOffsetYSlider").gameObject;
            m_ThumbProximalOffsetZSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbProximalOffsetZSlider").gameObject;
            m_ThumbMiddleOffsetXSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbMiddleOffsetXSlider").gameObject;
            m_ThumbMiddleOffsetYSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbMiddleOffsetYSlider").gameObject;
            m_ThumbMiddleOffsetZSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbMiddleOffsetZSlider").gameObject;
            m_ThumbDistalOffsetXSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbDistalOffsetXSlider").gameObject;
            m_ThumbDistalOffsetYSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbDistalOffsetYSlider").gameObject;
            m_ThumbDistalOffsetZSlider = FindDeepChild(m_canvas.gameObject.transform, "ThumbDistalOffsetZSlider").gameObject;
            m_AlignFingerButton = FindDeepChild(m_canvas.gameObject.transform, "AlignFingerButton").gameObject;
            m_HardwareCalButton = FindDeepChild(m_canvas.gameObject.transform, "8FigureCalButton").gameObject;

            m_ServerIP.GetComponent<InputField>().text = "127.0.0.1";
            m_ServerPort.GetComponent<InputField>().text = "11002";
            m_ConnectButton.GetComponentInChildren<Text>().text = "Connect";
            m_ConnectButton.GetComponent<Button>().onClick.AddListener(OnConnectGlovePressed);
            m_LHHapticsButton.GetComponent<Button>().onClick.AddListener(OnLHTriggerHapticsPressed);
            m_RHHapticsButton.GetComponent<Button>().onClick.AddListener(OnRHTriggerHapticsPressed);
            m_LHResetButton.GetComponent<Button>().onClick.AddListener(OnLHResetPressed);
            m_RHResetButton.GetComponent<Button>().onClick.AddListener(OnRHResetPressed);
            m_ParameterPanelToggle.GetComponent<Toggle>().onValueChanged.AddListener(OnToggleParamPanel);
            m_ParametersPanel.SetActive(false);

            UpdateUIValue((HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2);
            m_AdvancedModeToggle.GetComponent<Toggle>().onValueChanged.AddListener(OnToggleAdvancedMode);
            m_FingerSpacingSlider.GetComponent<Slider>().onValueChanged.AddListener(OnFingerSpacingChanged);
            m_FingerCurvedSpacingSlider.GetComponent<Slider>().onValueChanged.AddListener(OnFingerCurvedSpacingChanged);
            m_ThumbProximalSlerpSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbProximalSlerpChanged);
            m_ThumbMiddleSlerpSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbMiddleSlerpChanged);

            m_ThumbProximalOffsetXSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbProximalOffsetXChanged);
            m_ThumbProximalOffsetYSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbProximalOffsetYChanged);
            m_ThumbProximalOffsetZSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbProximalOffsetZChanged);
            m_ThumbMiddleOffsetXSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbMiddleOffsetXChanged);
            m_ThumbMiddleOffsetYSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbMiddleOffsetYChanged);
            m_ThumbMiddleOffsetZSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbMiddleOffsetZChanged);
            m_ThumbDistalOffsetXSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbDistalOffsetXChanged);
            m_ThumbDistalOffsetYSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbDistalOffsetYChanged);
            m_ThumbDistalOffsetZSlider.GetComponent<Slider>().onValueChanged.AddListener(OnThumbDistalOffsetZChanged);
            m_AlignFingerButton.GetComponent<Button>().onClick.AddListener(OnAlignFinger);
            m_HardwareCalButton.GetComponent<Button>().onClick.AddListener(OnHardwareCal);
        }

        // Update is called once per frame
        void Update()
        {
            try
            {
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
                Console.WriteLine(e);
            }
        }
        public void OnToggleAdvancedMode(bool enabled)
        {
            glove3D.SetAdvancedMode(enabled);
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            UpdateUIValue(type);
        }

        public void OnSelectHardwareVersion(int index)
        {
            if (index == 0)
            {
                glove3D.SetHardwareVersion(GLOVEVERSION.DK1);
            }
            else if (index == 1)
            {
                glove3D.SetHardwareVersion(GLOVEVERSION.DK2);
            }
            else if (index == 2)
            {
                glove3D.SetHardwareVersion(GLOVEVERSION.PRO);
            }
        }
        public void OnConnectGlovePressed()
        {
            glove3D.OnConnectGlove();
            m_ConnectButton.GetComponentInChildren<Text>().text = "Disconnect";
            m_ConnectButton.GetComponent<Button>().onClick.RemoveAllListeners();
            m_ConnectButton.GetComponent<Button>().onClick.AddListener(OnDisconnectGlovePressed);
        }
        public void OnDisconnectGlovePressed()
        {
            glove3D.OnDisconnectGlove();
            m_ConnectButton.GetComponentInChildren<Text>().text = "Connect";
            m_ConnectButton.GetComponent<Button>().onClick.RemoveAllListeners();
            m_ConnectButton.GetComponent<Button>().onClick.AddListener(OnConnectGlovePressed);
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
            FindDeepChild(m_canvas.gameObject.transform, "ParametersPanel").gameObject.SetActive(bIsEnabled);
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
        public string GetServerPort()
        {
            if (m_ServerPort == null) return "11002";
            return m_ServerPort.GetComponent<InputField>().text;
        }
        public bool GetAdvancedMode()
        {
            m_AdvancedModeToggle = FindDeepChild(m_canvas.gameObject.transform, "AdvancedMode").gameObject;
            return m_AdvancedModeToggle.GetComponent<Toggle>().isOn;
        }
        public GLOVEVERSION GetHardwareVersion()
        {
            m_HardwareVersionDropDown = FindDeepChild(m_canvas.gameObject.transform, "HardwareVersion").gameObject;
            return (GLOVEVERSION)m_HardwareVersionDropDown.GetComponent<Dropdown>().value;
        }

        public void UpdateUIValue(HANDTYPE type)
        {
            if (m_AdvancedModeToggle.GetComponent<Toggle>().isOn)
            {
                m_FingerSpacingSlider.GetComponent<Slider>().interactable = false;
                m_FingerCurvedSpacingSlider.GetComponent<Slider>().interactable = false;
                m_AlignFingerButton.GetComponent<Button>().interactable = true;
            }
            else
            {
                m_FingerSpacingSlider.GetComponent<Slider>().interactable = true;
                m_FingerCurvedSpacingSlider.GetComponent<Slider>().interactable = true;
                m_AlignFingerButton.GetComponent<Button>().interactable = true;
            }

            m_FingerSpacingSlider.GetComponent<Slider>().value = (float)glove3D.finger_spacing;
            m_FingerSpacingSlider.transform.Find("FingerSpacing").GetComponent<Text>().text = glove3D.finger_spacing.ToString("F1");

            m_FingerCurvedSpacingSlider.GetComponent<Slider>().value = (float)glove3D.final_finger_spacing;
            m_FingerCurvedSpacingSlider.transform.Find("CurvedSpacing").GetComponent<Text>().text = glove3D.final_finger_spacing.ToString("F1");

            m_ThumbProximalSlerpSlider.GetComponent<Slider>().value = (float)glove3D.thumb_proximal_slerp;
            m_ThumbProximalSlerpSlider.transform.Find("ThumbProximalSlerp").GetComponent<Text>().text = glove3D.thumb_proximal_slerp.ToString("F1");

            m_ThumbMiddleSlerpSlider.GetComponent<Slider>().value = (float)glove3D.thumb_middle_slerp;
            m_ThumbMiddleSlerpSlider.transform.Find("ThumbMiddleSlerp").GetComponent<Text>().text = glove3D.thumb_middle_slerp.ToString("F1");

            if(type == HANDTYPE.LEFT_HAND)
            {
                thumb_offset[0] = glove3D.thumb_offset_L[0];
                thumb_offset[1] = glove3D.thumb_offset_L[1];
                thumb_offset[2] = glove3D.thumb_offset_L[2];
            }
            else if(type == HANDTYPE.RIGHT_HAND)
            {
                thumb_offset[0] = glove3D.thumb_offset_R[0];
                thumb_offset[1] = glove3D.thumb_offset_R[1];
                thumb_offset[2] = glove3D.thumb_offset_R[2];
            }
            m_ThumbProximalOffsetXSlider.GetComponent<Slider>().value = (float)thumb_offset[0].x;
            m_ThumbProximalOffsetXSlider.transform.Find("ThumbProximalOffsetX").GetComponent<Text>().text = thumb_offset[0].x.ToString("F1");

            m_ThumbProximalOffsetYSlider.GetComponent<Slider>().value = (float)thumb_offset[0].y;
            m_ThumbProximalOffsetYSlider.transform.Find("ThumbProximalOffsetY").GetComponent<Text>().text = thumb_offset[0].y.ToString("F1");

            m_ThumbProximalOffsetZSlider.GetComponent<Slider>().value = (float)thumb_offset[0].z;
            m_ThumbProximalOffsetZSlider.transform.Find("ThumbProximalOffsetZ").GetComponent<Text>().text = thumb_offset[0].z.ToString("F1");

            m_ThumbMiddleOffsetXSlider.GetComponent<Slider>().value = (float)thumb_offset[1].x;
            m_ThumbMiddleOffsetXSlider.transform.Find("ThumbMiddleOffsetX").GetComponent<Text>().text = thumb_offset[1].x.ToString("F1");

            m_ThumbMiddleOffsetYSlider.GetComponent<Slider>().value = (float)thumb_offset[1].y;
            m_ThumbMiddleOffsetYSlider.transform.Find("ThumbMiddleOffsetY").GetComponent<Text>().text = thumb_offset[1].y.ToString("F1");

            m_ThumbMiddleOffsetZSlider.GetComponent<Slider>().value = (float)thumb_offset[1].z;
            m_ThumbMiddleOffsetZSlider.transform.Find("ThumbMiddleOffsetZ").GetComponent<Text>().text = thumb_offset[1].z.ToString("F1");

            m_ThumbDistalOffsetXSlider.GetComponent<Slider>().value = (float)thumb_offset[2].x;
            m_ThumbDistalOffsetXSlider.transform.Find("ThumbDistalOffsetX").GetComponent<Text>().text = thumb_offset[2].x.ToString("F1");

            m_ThumbDistalOffsetYSlider.GetComponent<Slider>().value = (float)thumb_offset[2].y;
            m_ThumbDistalOffsetYSlider.transform.Find("ThumbDistalOffsetY").GetComponent<Text>().text = thumb_offset[2].y.ToString("F1");

            m_ThumbDistalOffsetZSlider.GetComponent<Slider>().value = (float)thumb_offset[2].z;
            m_ThumbDistalOffsetZSlider.transform.Find("ThumbDistalOffsetZ").GetComponent<Text>().text = thumb_offset[2].z.ToString("F1");
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
            if(type == HANDTYPE.LEFT_HAND) glove3D.LH.SetFingerSpacing(value);
            else if(type == HANDTYPE.RIGHT_HAND) glove3D.RH.SetFingerSpacing(value);
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

        public void OnThumbProximalOffsetXChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[0].x = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[0], VRTRIXBones.L_Thumb_1);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[0].x = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[0], VRTRIXBones.R_Thumb_1);
            }
            UpdateUIValue(type);
        }

        public void OnThumbProximalOffsetYChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[0].y = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[0], VRTRIXBones.L_Thumb_1);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[0].y = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[0], VRTRIXBones.R_Thumb_1);
            }
            UpdateUIValue(type);
        }

        public void OnThumbProximalOffsetZChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[0].z = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[0], VRTRIXBones.L_Thumb_1);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[0].z = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[0], VRTRIXBones.R_Thumb_1);
            }
            UpdateUIValue(type);
        }

        public void OnThumbMiddleOffsetXChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[1].x = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[1], VRTRIXBones.L_Thumb_2);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[1].x = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[1], VRTRIXBones.R_Thumb_2);
            }
            UpdateUIValue(type);
        }

        public void OnThumbMiddleOffsetYChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[1].y = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[1], VRTRIXBones.L_Thumb_2);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[1].y = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[1], VRTRIXBones.R_Thumb_2);
            }
            UpdateUIValue(type);
        }

        public void OnThumbMiddleOffsetZChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[1].z = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[1], VRTRIXBones.L_Thumb_2);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[1].z = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[1], VRTRIXBones.R_Thumb_2);
            }
            UpdateUIValue(type);
        }

        public void OnThumbDistalOffsetXChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[2].x = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[2], VRTRIXBones.L_Thumb_3);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[2].x = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[2], VRTRIXBones.R_Thumb_3);
            }
            UpdateUIValue(type);
        }

        public void OnThumbDistalOffsetYChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[2].y = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[2], VRTRIXBones.L_Thumb_3);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[2].y = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[2], VRTRIXBones.R_Thumb_3);
            }
            UpdateUIValue(type);
        }

        public void OnThumbDistalOffsetZChanged(float value)
        {
            HANDTYPE type = (HANDTYPE)m_HandTypeDropDown.GetComponent<Dropdown>().value + 2;
            if (type == HANDTYPE.LEFT_HAND)
            {
                glove3D.thumb_offset_L[2].z = value;
                glove3D.LH.SetThumbOffset(glove3D.thumb_offset_L[2], VRTRIXBones.L_Thumb_3);
            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                glove3D.thumb_offset_R[2].z = value;
                glove3D.RH.SetThumbOffset(glove3D.thumb_offset_R[2], VRTRIXBones.R_Thumb_3);
            }
            UpdateUIValue(type);
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
        public static Transform FindDeepChild(Transform aParent, string aName)
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

                foreach (Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }
    }
}
    
