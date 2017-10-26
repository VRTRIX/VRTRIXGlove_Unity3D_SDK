//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: UIElement that responds to VR hands and generates UnityEvents
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using Valve.VR.InteractionSystem;

namespace VRTRIX
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(VRTRIXInteractable))]
    public class VRTRIXGloveUIElement : MonoBehaviour
    {
        public VRTRIXCustomEvents.VRTRIXEventHand onHandClick;
        private VRTRIXGloveGrab currentHand;
        private Button button;

        //-------------------------------------------------
        void Awake()
        {
            button = GetComponent<Button>();
            //if (button)
            //{
            //    button.onClick.AddListener(OnButtonClick);
            //}
        }


        //-------------------------------------------------
        private void OnHandHoverBegin(VRTRIXGloveGrab hand)
        {
            currentHand = hand;
            //InputModule.instance.HoverBegin(gameObject);
            //ControllerButtonHints.ShowButtonHint(hand, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
        }


        //-------------------------------------------------
        private void OnHandHoverEnd(VRTRIXGloveGrab hand)
        {
            //InputModule.instance.HoverEnd(gameObject);
            //ControllerButtonHints.HideButtonHint(hand, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
            currentHand = null;
            ColorBlock cb = button.colors;
            cb.normalColor = new Color(0f, 0f, 255f);
            cb.normalColor = Color.blue;
            button.colors = cb;
        }


        //-------------------------------------------------
        private void HandHoverUpdate(VRTRIXGloveGrab hand)
        {
            if (hand.GetPressButtonDown())
            {
                //InputModule.instance.Submit(gameObject);
                OnButtonClick();
                //ControllerButtonHints.HideButtonHint(hand, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
            }
        }


        //-------------------------------------------------
        private void OnButtonClick()
        {
 
            onHandClick.Invoke(currentHand);
            //Changes the button's Normal color to the new color.
            ColorBlock cb = button.colors;
            cb.normalColor = Color.cyan;
            button.colors = cb;
        }
    }

#if UNITY_EDITOR
    //-------------------------------------------------------------------------
    [UnityEditor.CustomEditor(typeof(UIElement))]
    public class UIElementEditor : UnityEditor.Editor
    {
        //-------------------------------------------------
        // Custom Inspector GUI allows us to click from within the UI
        //-------------------------------------------------
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UIElement uiElement = (UIElement)target;
            if (GUILayout.Button("Click"))
            {
                InputModule.instance.Submit(uiElement.gameObject);
            }
        }
    }
#endif
}

