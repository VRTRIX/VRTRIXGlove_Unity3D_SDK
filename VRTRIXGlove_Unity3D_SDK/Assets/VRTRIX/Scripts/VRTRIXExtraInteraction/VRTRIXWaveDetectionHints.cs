//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Demonstrates hand wave detection with VRTRIX Data Glove.
//
//=============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTRIX
{
    public class VRTRIXWaveDetectionHints : MonoBehaviour
    {
        public GameObject LeftArrow;
        public GameObject RightArrow;
        public GameObject UpArrow;
        public GameObject DownArrow;
        public Material highlightedMaterial;
        public Material normalMaterial;
        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void HandWaveLeftDetected(VRTRIXGloveGrab hand)
        {
            SetMaterial(LeftArrow, highlightedMaterial);
            SetMaterial(RightArrow, normalMaterial);
            SetMaterial(UpArrow, normalMaterial);
            SetMaterial(DownArrow, normalMaterial);
            if(hand.GetHandType() == HANDTYPE.LEFT_HAND)
            {
                Debug.Log("[检测成功]: 左手向左挥动!");
            }
            else if (hand.GetHandType() == HANDTYPE.RIGHT_HAND)
            {
                Debug.Log("[检测成功]: 右手向左挥动!");
            }
        }

        public void HandWaveRightDetected(VRTRIXGloveGrab hand)
        {
            SetMaterial(LeftArrow, normalMaterial);
            SetMaterial(RightArrow, highlightedMaterial);
            SetMaterial(UpArrow, normalMaterial);
            SetMaterial(DownArrow, normalMaterial);
            if (hand.GetHandType() == HANDTYPE.LEFT_HAND)
            {
                Debug.Log("[检测成功]: 左手向右挥动!");
            }
            else if (hand.GetHandType() == HANDTYPE.RIGHT_HAND)
            {
                Debug.Log("[检测成功]: 右手向右挥动!");
            }
        }

        public void HandWaveUpDetected(VRTRIXGloveGrab hand)
        {
            SetMaterial(LeftArrow, normalMaterial);
            SetMaterial(RightArrow, normalMaterial);
            SetMaterial(UpArrow, highlightedMaterial);
            SetMaterial(DownArrow, normalMaterial);
            if (hand.GetHandType() == HANDTYPE.LEFT_HAND)
            {
                Debug.Log("[检测成功]: 左手向上挥动!");
            }
            else if (hand.GetHandType() == HANDTYPE.RIGHT_HAND)
            {
                Debug.Log("[检测成功]: 右手向上挥动!");
            }
        }

        public void HandWaveDownDetected(VRTRIXGloveGrab hand)
        {
            SetMaterial(LeftArrow, normalMaterial);
            SetMaterial(RightArrow, normalMaterial);
            SetMaterial(UpArrow, normalMaterial);
            SetMaterial(DownArrow, highlightedMaterial);
            if (hand.GetHandType() == HANDTYPE.LEFT_HAND)
            {
                Debug.Log("[检测成功]: 左手向下挥动!");
            }
            else if (hand.GetHandType() == HANDTYPE.RIGHT_HAND)
            {
                Debug.Log("[检测成功]: 右手向下挥动!");
            }
        }

        private void SetMaterial(GameObject obj, Material mat)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            if (renderer)
            {
                Material[] materials = renderer.materials;
                materials[0] = mat;
                renderer.materials = materials;
            }
        }
    }
}