//======= Copyright (c) VRTRIX INC, All rights reserved. ===============
//
// Purpose: Makes this object ignore any hovering by the hands
//
//=============================================================================

using UnityEngine;

namespace VRTRIX
{
    //-------------------------------------------------------------------------
    public class VRTRIXIgnoreHovering : MonoBehaviour
    {
        [Tooltip("If Hand is not null, only ignore the specified hand")]
        public VRTRIXGloveGrab onlyIgnoreHand = null;
    }
}
