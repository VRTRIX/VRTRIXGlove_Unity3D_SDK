//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Sets this GameObject as inactive when it loses focus from the hand
//
//=============================================================================
using UnityEngine;
namespace VRTRIX
{
    //-------------------------------------------------------------------------
    public class VRTRIXGloveHideOnHandFocus : MonoBehaviour
    {
        //-------------------------------------------------
        private void OnHandFocusLost(VRTRIXGloveGrab hand)
        {
            gameObject.SetActive(false);
        }
    }
}
