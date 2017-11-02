//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Makes this object ignore any hovering by the hands
//
//=============================================================================

using UnityEngine;
using VRTRIX;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class IgnoreHovering : MonoBehaviour
	{
		[Tooltip( "If Hand is not null, only ignore the specified hand" )]
		public VRTRIXGloveGrab onlyIgnoreHand = null;
	}
}
