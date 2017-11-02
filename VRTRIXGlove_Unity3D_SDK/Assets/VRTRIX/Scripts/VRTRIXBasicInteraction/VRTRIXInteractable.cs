//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Interacble class should be attached to any objects in your scene that
//          you want to be interacble by our VRTRIX Data Gloves. This is also the
//          the basic class for other more complex interaction class.
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace VRTRIX
{
    //-------------------------------------------------------------------------
    public class VRTRIXInteractable : MonoBehaviour
    {
        public delegate void OnAttachedToHandDelegate(VRTRIXGloveGrab hand);
        public delegate void OnDetachedFromHandDelegate(VRTRIXGloveGrab hand);

        [HideInInspector]
        public event OnAttachedToHandDelegate onAttachedToHand;
        [HideInInspector]
        public event OnDetachedFromHandDelegate onDetachedFromHand;

        //-------------------------------------------------
        private void OnAttachedToHand(VRTRIXGloveGrab hand)
        {
            if (onAttachedToHand != null)
            {
                onAttachedToHand.Invoke(hand);
            }
        }


        //-------------------------------------------------
        private void OnDetachedFromHand(VRTRIXGloveGrab hand)
        {
            if (onDetachedFromHand != null)
            {
                onDetachedFromHand.Invoke(hand);
            }
        }
    }
}
