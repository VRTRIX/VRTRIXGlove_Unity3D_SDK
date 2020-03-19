//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Gesture Recognition Class. You are free to define more gestures 
//          mapping with your design of interaction functions.
//
//=============================================================================
using UnityEngine;
using System;
using System.Collections.Generic;

namespace VRTRIX
{
    public enum VRTRIXGloveGesture
    {
        //Numbers
        BUTTONINVALID = 0x00,
        BUTTONONE = 0x01,
        BUTTONTWO = 0x02,
        BUTTONTHREE = 0x04,
        BUTTONFOUR = 0x08,
        BUTTONFIVE = 0x10,
        BUTTONSIX = 0x20,

        //Interactions
        BUTTONCLICK = BUTTONONE,
        BUTTONTELEPORT = BUTTONTWO,
        BUTTONOK = 0x40,
        BUTTONGRAB = 0x80,
        BUTTONPINCH = 0x100,
        BUTTONGUN = 0x200,

        //Scissor,rock and paper
        BUTTONPAPER = BUTTONFIVE,
        BUTTONSCISSOR = BUTTONTWO,
        BUTTONROCK = 0x400
    };

    public enum VRTRIXFingerStatus
    {
        BEND_UP = 0,
        STRAIGHT = 1,
        BEND_DOWN = 2,
        CURVED = 3
    }

    public struct VRTRIXFingerStatusThreshold
    {
        public double bendUpThreshold;
        public double bendDownThreshold;
        public double curvedThreshold;

        public VRTRIXFingerStatusThreshold(double bendUpThreshold, double bendDownThreshold, double curvedThreshold)
        {
            this.bendUpThreshold = bendUpThreshold;
            this.bendDownThreshold = bendDownThreshold;
            this.curvedThreshold = curvedThreshold;
        }
    }

    public class VRTRIXGloveGestureRecognition
    {
        public Dictionary<VRTRIXBones, VRTRIXFingerStatusThreshold> m_thresholdMap;
        public double m_fingerBendUpThreshold, m_fingerBendDownThreshold, m_fingerCurvedThreshold;
        public double m_thumbBendUpThreshold, m_thumbBendDownThreshold, m_thumbCurvedThreshold;
        public VRTRIXGloveGestureRecognition(Dictionary<VRTRIXBones, VRTRIXFingerStatusThreshold> thresholdMap)
        {
            m_thresholdMap = thresholdMap;
        }

        public VRTRIXGloveGesture GestureDetection(VRTRIXDataWrapper hand, HANDTYPE type)
        {
            VRTRIXGloveGesture curGesture = VRTRIXGloveGesture.BUTTONINVALID;
            VRTRIXFingerStatus ThumbStat, IndexStat, MiddleStat, RingStat, PinkyStat;
            if (type == HANDTYPE.LEFT_HAND)
            {
                ThumbStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.L_Thumb_2), VRTRIXBones.L_Thumb_2);
                IndexStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.L_Index_2), VRTRIXBones.L_Index_2);
                MiddleStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.L_Middle_2), VRTRIXBones.L_Middle_2);
                RingStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.L_Ring_2), VRTRIXBones.L_Ring_2);
                PinkyStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.L_Pinky_2), VRTRIXBones.L_Pinky_2);
                //Debug.Log("ThumbAngle: " + ThumbAngle + ", IndexAngle: " + IndexAngle + ", MiddleAngle: " + MiddleAngle + ", RingAngle: " + RingAngle + ", PinkyAngle: " + PinkyAngle);
                //Debug.Log("TeleportCheck1: " + (RingAngle - MiddleAngle) + ", TeleportCheck2: " + (PinkyAngle - IndexAngle));
            }
            else
            {
                ThumbStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.R_Thumb_2), VRTRIXBones.R_Thumb_2);
                IndexStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.R_Index_2), VRTRIXBones.R_Index_2);
                MiddleStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.R_Middle_2), VRTRIXBones.R_Middle_2);
                RingStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.R_Ring_2), VRTRIXBones.R_Ring_2);
                PinkyStat = GetCurrentFingerStatus(hand.GetReceivedGestureAngle(VRTRIXBones.R_Pinky_2), VRTRIXBones.R_Pinky_2);
                //Debug.Log("ThumbAngle: " + ThumbAngle + ", IndexAngle: " + IndexAngle + ", MiddleAngle: " + MiddleAngle + ", RingAngle: " + RingAngle + ", PinkyAngle: " + PinkyAngle);
                //Debug.Log("ThumbAngle: " + ThumbAngle + ", ThumbCurve: " + ThumbCurve);
            }

            //Button One Detection
            if (ThumbStat >= VRTRIXFingerStatus.CURVED && IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                MiddleStat >= VRTRIXFingerStatus.CURVED && RingStat >= VRTRIXFingerStatus.CURVED &&
                PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONONE;
            }

            //Button Two Detection
            if (ThumbStat >= VRTRIXFingerStatus.CURVED && IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                MiddleStat < VRTRIXFingerStatus.BEND_DOWN && RingStat >= VRTRIXFingerStatus.CURVED &&
                PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONTWO;
            }

            //Button Three Detection
            if (ThumbStat >= VRTRIXFingerStatus.CURVED && IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                MiddleStat < VRTRIXFingerStatus.BEND_DOWN && RingStat < VRTRIXFingerStatus.BEND_DOWN &&
                PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONTHREE;
            }

            //Button Four Detection
            if (ThumbStat >= VRTRIXFingerStatus.CURVED && IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                MiddleStat < VRTRIXFingerStatus.BEND_DOWN && RingStat < VRTRIXFingerStatus.BEND_DOWN &&
                PinkyStat < VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONFOUR;
            }

            //Button Five Detection
            if (ThumbStat < VRTRIXFingerStatus.BEND_DOWN && IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                MiddleStat < VRTRIXFingerStatus.BEND_DOWN && RingStat < VRTRIXFingerStatus.BEND_DOWN &&
                PinkyStat < VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONFIVE;
            }

            //Button Six Detection
            if (ThumbStat < VRTRIXFingerStatus.BEND_DOWN && IndexStat >= VRTRIXFingerStatus.CURVED &&
                MiddleStat >= VRTRIXFingerStatus.CURVED && RingStat >= VRTRIXFingerStatus.CURVED &&
                PinkyStat < VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONSIX;
            }

            //Button OK Detection
            if (ThumbStat == VRTRIXFingerStatus.BEND_DOWN && IndexStat == VRTRIXFingerStatus.BEND_DOWN &&
                MiddleStat < VRTRIXFingerStatus.BEND_DOWN && RingStat < VRTRIXFingerStatus.BEND_DOWN &&
                PinkyStat < VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONOK;
            }

            //Button Grab Detection
            if (ThumbStat >= VRTRIXFingerStatus.BEND_DOWN && IndexStat >= VRTRIXFingerStatus.BEND_DOWN &&
                MiddleStat >= VRTRIXFingerStatus.BEND_DOWN && RingStat >= VRTRIXFingerStatus.BEND_DOWN &&
                PinkyStat >= VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONGRAB;
            }

            //Button Pinch Detection
            if (ThumbStat == VRTRIXFingerStatus.BEND_DOWN && IndexStat == VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONPINCH;
            }

            //Button Gun Detection
            if (ThumbStat < VRTRIXFingerStatus.BEND_DOWN && IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                MiddleStat >= VRTRIXFingerStatus.CURVED && RingStat >= VRTRIXFingerStatus.CURVED &&
                PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONGUN;
            }

            //Button Rock Detection
            if (ThumbStat >= VRTRIXFingerStatus.BEND_DOWN && IndexStat >= VRTRIXFingerStatus.CURVED &&
                MiddleStat >= VRTRIXFingerStatus.CURVED && RingStat >= VRTRIXFingerStatus.CURVED &&
                PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONROCK;
            }

            return curGesture;
        }

        public VRTRIXFingerStatus GetCurrentFingerStatus(double angle, VRTRIXBones finger)
        {
            VRTRIXFingerStatusThreshold threshold = m_thresholdMap[finger];
            if (angle < threshold.bendUpThreshold) return VRTRIXFingerStatus.BEND_UP;
            else if (angle < threshold.bendDownThreshold) return VRTRIXFingerStatus.STRAIGHT;
            else if (angle < threshold.curvedThreshold) return VRTRIXFingerStatus.BEND_DOWN;
            else return VRTRIXFingerStatus.CURVED;
        }
    }
}


