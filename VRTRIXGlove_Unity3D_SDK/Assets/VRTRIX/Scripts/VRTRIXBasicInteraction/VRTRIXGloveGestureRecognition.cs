//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Gesture Recognition Class. You are free to define more gestures 
//          mapping with your design of interaction functions.
//
//=============================================================================

namespace VRTRIX
{
    public enum VRTRIXGloveGesture
    {
        BUTTONCLICK,
        BUTTONOK,
        BUTTONGRAB,
        BUTTONTELEPORT,
        BUTTONNONE
    };
    public class VRTRIXGloveGestureRecognition
    {
        public static VRTRIXGloveGesture GestureDetection(VRTRIXDataWrapper hand, HANDTYPE type)
        {
            if (type == HANDTYPE.LEFT_HAND)
            {
                float ThumbAngle = hand.GetReceivedGestureAngle(VRTRIXBones.L_Thumb_2);
                float IndexAngle = hand.GetReceivedGestureAngle(VRTRIXBones.L_Index_2);
                float MiddleAngle = hand.GetReceivedGestureAngle(VRTRIXBones.L_Middle_2);
                float RingAngle = hand.GetReceivedGestureAngle(VRTRIXBones.L_Ring_2);
                float PinkyAngle = hand.GetReceivedGestureAngle(VRTRIXBones.L_Pinky_2);
                bool ThumbCurve = (ThumbAngle < 350f) && (ThumbAngle > 170f);
                bool IndexCurve = (IndexAngle < 270f) && (IndexAngle > 95f);
                bool MiddleCurve = (MiddleAngle < 270f) && (MiddleAngle > 95f);
                bool RingCurve = (RingAngle < 270f) && (RingAngle > 95f);
                bool PinkyCurve = (PinkyAngle < 270f) && (PinkyAngle > 95f);
                bool TeleportCheck = (RingAngle - MiddleAngle < -110f || RingAngle - MiddleAngle > 200) && (PinkyAngle - IndexAngle < -110f || PinkyAngle - IndexAngle > 200f);
                //Debug.Log("ThumbAngle: " + ThumbAngle + ", ThumbCurve: " + ThumbCurve);
                //Debug.Log("TeleportCheck1: " + (RingAngle - MiddleAngle) + ", TeleportCheck2: " + (PinkyAngle - IndexAngle));
                if (ThumbCurve && MiddleCurve && RingCurve && PinkyCurve && !IndexCurve)
                {
                    return VRTRIXGloveGesture.BUTTONCLICK;
                }

                if (ThumbCurve && RingCurve && PinkyCurve && !IndexCurve && !MiddleCurve && TeleportCheck)
                {
                    return VRTRIXGloveGesture.BUTTONTELEPORT;
                }

                if (ThumbCurve && IndexCurve && !MiddleCurve && !RingCurve && !PinkyCurve)
                {
                    return VRTRIXGloveGesture.BUTTONOK;
                }

                if (ThumbCurve && MiddleCurve && RingCurve && PinkyCurve && IndexCurve)
                {
                    return VRTRIXGloveGesture.BUTTONGRAB;
                }

            }
            else if (type == HANDTYPE.RIGHT_HAND)
            {
                float ThumbAngle = hand.GetReceivedGestureAngle(VRTRIXBones.R_Thumb_2);
                float IndexAngle = hand.GetReceivedGestureAngle(VRTRIXBones.R_Index_2);
                float MiddleAngle = hand.GetReceivedGestureAngle(VRTRIXBones.R_Middle_2);
                float RingAngle = hand.GetReceivedGestureAngle(VRTRIXBones.R_Ring_2);
                float PinkyAngle = hand.GetReceivedGestureAngle(VRTRIXBones.R_Pinky_2);
                bool ThumbCurve = (ThumbAngle < 350f) && (ThumbAngle > 170f);
                bool IndexCurve = (IndexAngle < 270f) && (IndexAngle > 95f);
                bool MiddleCurve = (MiddleAngle < 270f) && (MiddleAngle > 95f);
                bool RingCurve = (RingAngle < 270f) && (RingAngle > 95f);
                bool PinkyCurve = (PinkyAngle < 270f) && (PinkyAngle > 95f);
                bool TeleportCheck = (RingAngle - MiddleAngle < -80f || RingAngle - MiddleAngle > 200) && (PinkyAngle - IndexAngle < -90f || PinkyAngle - IndexAngle > 210f);
                //Debug.Log("ThumbAngle: " + ThumbAngle + ", ThumbCurve: " + ThumbCurve);
                if (ThumbCurve && MiddleCurve && RingCurve && PinkyCurve && !IndexCurve)
                {
                    return VRTRIXGloveGesture.BUTTONCLICK;
                }

                if (ThumbCurve && RingCurve && PinkyCurve && !IndexCurve && !MiddleCurve && TeleportCheck)
                {
                    return VRTRIXGloveGesture.BUTTONTELEPORT;
                }

                if (ThumbCurve && IndexCurve && !MiddleCurve && !RingCurve && !PinkyCurve)
                {
                    return VRTRIXGloveGesture.BUTTONOK;
                }

                if (ThumbCurve && MiddleCurve && RingCurve && PinkyCurve && IndexCurve)
                {
                    return VRTRIXGloveGesture.BUTTONGRAB;
                }
            }
            return VRTRIXGloveGesture.BUTTONNONE;
        }
    }
}


