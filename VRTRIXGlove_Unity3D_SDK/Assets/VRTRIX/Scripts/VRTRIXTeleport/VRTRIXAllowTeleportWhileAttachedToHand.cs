//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Adding this component to an object will allow the player to 
//			initiate teleporting while that object is attached to their hand
//
//=============================================================================

using UnityEngine;

namespace VRTRIX
{
    //-------------------------------------------------------------------------
    public class VRTRIXAllowTeleportWhileAttachedToHand : MonoBehaviour
    {
        public bool teleportAllowed = true;
        public bool overrideHoverLock = true;
    }
}
