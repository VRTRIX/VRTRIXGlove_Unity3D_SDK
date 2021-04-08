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
        BUTTONROCK = 0x400,

        BUTTONTELEPORTCANCEL = 0x800
    };

    public enum VRTRIXGloveDynamicGesture
    {
        ACTIONINVALID = 0x00,
        ACTIONGRAB = 0x01,
        ACTIONGUN = 0x02,
        ACTIONSCREW = 0x04
    }

    public enum VRTRIXFingerStatus
    {
        BEND_UP = 0,
        STRAIGHT = 1,
        BEND_DOWN = 2,
        CURVED = 3
    }

    public enum VRTRIXDynamicGestureStatus
    {
        TRIGGERED = 0,
        INTERMEDIATE = 1,
        RELEASED = 2,
    }

    public struct VRTRIXGloveDynamicGestureDetector
    {
        public VRTRIXGloveDynamicGesture gesture;
        public VRTRIXDynamicGestureStatus stat;
        public bool bIsClockWise;
        public int triggerCount;
        public int releaseCount;
        public VRTRIXGloveDynamicGestureDetector(VRTRIXGloveDynamicGesture gesture, VRTRIXDynamicGestureStatus stat, bool bIsClockWise = true)
        {
            this.gesture = gesture;
            this.stat = stat;
            this.bIsClockWise = bIsClockWise;
            this.triggerCount = 0;
            this.releaseCount = 0;
        }
    }

    public struct VRTRIXFingerData
    {
        public double thumbBendAngle;
        public double indexBendAngle;
        public double middleBendAngle;
        public double ringBendAngle;
        public double pinkyBendAngle;
        public Quaternion handQuat;

        public VRTRIXFingerData(double thumbBendAngle, double indexBendAngle, double middleBendAngle
            , double ringBendAngle, double pinkyBendAngle, Quaternion handQuat)
        {
            this.thumbBendAngle = thumbBendAngle;
            this.indexBendAngle = indexBendAngle;
            this.middleBendAngle = middleBendAngle;
            this.ringBendAngle = ringBendAngle;
            this.pinkyBendAngle = pinkyBendAngle;
            this.handQuat = handQuat;
        }
    }

    public struct VRTRIXFingersStatus
    {
        public VRTRIXFingerStatus ThumbStat;
        public VRTRIXFingerStatus IndexStat;
        public VRTRIXFingerStatus MiddleStat;
        public VRTRIXFingerStatus RingStat;
        public VRTRIXFingerStatus PinkyStat;
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
        private const int MAXQUEUESIZE = 20;
        private Dictionary<VRTRIXBones, VRTRIXFingerStatusThreshold> m_thresholdMap;
        private Queue<VRTRIXFingerData> m_LHFingerDataQueue, m_RHFingerDataQueue;
        private double m_fingerBendUpThreshold, m_fingerBendDownThreshold, m_fingerCurvedThreshold;
        private double m_thumbBendUpThreshold, m_thumbBendDownThreshold, m_thumbCurvedThreshold;
        private VRTRIXGloveGesture m_LHGesture, m_RHGesture = VRTRIXGloveGesture.BUTTONINVALID;
        private VRTRIXGloveDynamicGestureDetector m_LHScrewDetector, m_RHScrewDetector;
        public VRTRIXGloveGestureRecognition(Dictionary<VRTRIXBones, VRTRIXFingerStatusThreshold> thresholdMap)
        {
            m_thresholdMap = thresholdMap;
            m_LHFingerDataQueue = new Queue<VRTRIXFingerData>(MAXQUEUESIZE);
            m_RHFingerDataQueue = new Queue<VRTRIXFingerData>(MAXQUEUESIZE);
            m_LHScrewDetector = new VRTRIXGloveDynamicGestureDetector(VRTRIXGloveDynamicGesture.ACTIONSCREW, VRTRIXDynamicGestureStatus.INTERMEDIATE);
            m_RHScrewDetector = new VRTRIXGloveDynamicGestureDetector(VRTRIXGloveDynamicGesture.ACTIONSCREW, VRTRIXDynamicGestureStatus.INTERMEDIATE);
        }

        public VRTRIXGloveGesture GestureDetection(VRTRIXDataWrapper hand, HANDTYPE type)
        {
            VRTRIXGloveGesture curGesture = VRTRIXGloveGesture.BUTTONINVALID;
            VRTRIXFingersStatus fingerStat;
            VRTRIXFingerData fingerData;
            if (type == HANDTYPE.LEFT_HAND)
            {
                fingerData = new VRTRIXFingerData (hand.GetReceivedGestureAngle(VRTRIXBones.L_Thumb_2), hand.GetReceivedGestureAngle(VRTRIXBones.L_Index_2), 
                    hand.GetReceivedGestureAngle(VRTRIXBones.L_Middle_2), hand.GetReceivedGestureAngle(VRTRIXBones.L_Ring_2), hand.GetReceivedGestureAngle(VRTRIXBones.L_Pinky_2), hand.GetReceivedRotation(VRTRIXBones.L_Hand) );

                fingerStat.ThumbStat = GetCurrentFingerStatus(fingerData.thumbBendAngle, VRTRIXBones.L_Thumb_2);
                fingerStat.IndexStat = GetCurrentFingerStatus(fingerData.indexBendAngle, VRTRIXBones.L_Index_2);
                fingerStat.MiddleStat = GetCurrentFingerStatus(fingerData.middleBendAngle, VRTRIXBones.L_Middle_2);
                fingerStat.RingStat = GetCurrentFingerStatus(fingerData.ringBendAngle, VRTRIXBones.L_Ring_2);
                fingerStat.PinkyStat = GetCurrentFingerStatus(fingerData.pinkyBendAngle, VRTRIXBones.L_Pinky_2);
                //Debug.Log("ThumbAngle: " + fingerData.thumbBendAngle + ", IndexAngle: " + fingerData.indexBendAngle + ", MiddleAngle: " + fingerData.middleBendAngle 
                //    + ", RingAngle: " + fingerData.ringBendAngle + ", PinkyAngle: " + fingerData.pinkyBendAngle);
                //ScrewGestureDetection(fingerData, fingerStat, HANDTYPE.LEFT_HAND);
            }
            else
            {
                fingerData = new VRTRIXFingerData(hand.GetReceivedGestureAngle(VRTRIXBones.R_Thumb_2), hand.GetReceivedGestureAngle(VRTRIXBones.R_Index_2),
                    hand.GetReceivedGestureAngle(VRTRIXBones.R_Middle_2), hand.GetReceivedGestureAngle(VRTRIXBones.R_Ring_2), hand.GetReceivedGestureAngle(VRTRIXBones.R_Pinky_2), hand.GetReceivedRotation(VRTRIXBones.R_Hand));

                fingerStat.ThumbStat = GetCurrentFingerStatus(fingerData.thumbBendAngle, VRTRIXBones.R_Thumb_2);
                fingerStat.IndexStat = GetCurrentFingerStatus(fingerData.indexBendAngle, VRTRIXBones.R_Index_2);
                fingerStat.MiddleStat = GetCurrentFingerStatus(fingerData.middleBendAngle, VRTRIXBones.R_Middle_2);
                fingerStat.RingStat = GetCurrentFingerStatus(fingerData.ringBendAngle, VRTRIXBones.R_Ring_2);
                fingerStat.PinkyStat = GetCurrentFingerStatus(fingerData.pinkyBendAngle, VRTRIXBones.R_Pinky_2);
                //Debug.Log("ThumbAngle: " + fingerData.thumbBendAngle + ", IndexAngle: " + fingerData.indexBendAngle + ", MiddleAngle: " + fingerData.middleBendAngle
                //    + ", RingAngle: " + fingerData.ringBendAngle + ", PinkyAngle: " + fingerData.pinkyBendAngle);
                //ScrewGestureDetection(fingerData, fingerStat, HANDTYPE.RIGHT_HAND);
            }


            //Button One Detection
            if (fingerStat.ThumbStat >= VRTRIXFingerStatus.CURVED && fingerStat.IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.MiddleStat >= VRTRIXFingerStatus.CURVED && fingerStat.RingStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONONE;
            }

            //Button Two Detection
            if (fingerStat.ThumbStat >= VRTRIXFingerStatus.CURVED && fingerStat.IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.MiddleStat < VRTRIXFingerStatus.BEND_DOWN && fingerStat.RingStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONTWO;
            }

            //Button Three Detection
            if (fingerStat.ThumbStat >= VRTRIXFingerStatus.CURVED && fingerStat.IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.MiddleStat < VRTRIXFingerStatus.BEND_DOWN && fingerStat.RingStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONTHREE;
            }

            //Button Four Detection
            if (fingerStat.ThumbStat >= VRTRIXFingerStatus.CURVED && fingerStat.IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.MiddleStat < VRTRIXFingerStatus.BEND_DOWN && fingerStat.RingStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.PinkyStat < VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONFOUR;
            }

            //Button Five Detection
            if (fingerStat.ThumbStat < VRTRIXFingerStatus.BEND_DOWN && fingerStat.IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.MiddleStat < VRTRIXFingerStatus.BEND_DOWN && fingerStat.RingStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.PinkyStat < VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONFIVE;
            }

            //Button Six Detection
            if (fingerStat.ThumbStat < VRTRIXFingerStatus.BEND_DOWN && fingerStat.IndexStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.MiddleStat >= VRTRIXFingerStatus.CURVED && fingerStat.RingStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.PinkyStat < VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONSIX;
            }

            //Button OK Detection
            if (fingerStat.ThumbStat == VRTRIXFingerStatus.BEND_DOWN && fingerStat.IndexStat == VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.MiddleStat < VRTRIXFingerStatus.BEND_DOWN && fingerStat.RingStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.PinkyStat < VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONOK;
            }

            //Button Teleport Cancel Detection
            if (fingerStat.ThumbStat >= VRTRIXFingerStatus.CURVED && (fingerStat.IndexStat >= VRTRIXFingerStatus.BEND_DOWN ||
                fingerStat.MiddleStat >= VRTRIXFingerStatus.BEND_DOWN) && fingerStat.RingStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONTELEPORTCANCEL;
            }
            
            //Button Grab Detection
            if (fingerStat.ThumbStat >= VRTRIXFingerStatus.BEND_DOWN && fingerStat.IndexStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.MiddleStat >= VRTRIXFingerStatus.CURVED && fingerStat.RingStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONGRAB;
            }

            //Button Pinch Detection
            if (fingerStat.ThumbStat == VRTRIXFingerStatus.BEND_DOWN && fingerStat.IndexStat == VRTRIXFingerStatus.BEND_DOWN)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONPINCH;
            }

            //Button Gun Detection
            if (fingerStat.ThumbStat < VRTRIXFingerStatus.BEND_DOWN && fingerStat.IndexStat < VRTRIXFingerStatus.BEND_DOWN &&
                fingerStat.MiddleStat >= VRTRIXFingerStatus.CURVED && fingerStat.RingStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONGUN;
            }

            //Button Rock Detection
            if (fingerStat.ThumbStat >= VRTRIXFingerStatus.BEND_DOWN && fingerStat.IndexStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.MiddleStat >= VRTRIXFingerStatus.CURVED && fingerStat.RingStat >= VRTRIXFingerStatus.CURVED &&
                fingerStat.PinkyStat >= VRTRIXFingerStatus.CURVED)
            {
                curGesture = curGesture | VRTRIXGloveGesture.BUTTONROCK;
            }

            if (type == HANDTYPE.LEFT_HAND) m_LHGesture = curGesture;
            else m_RHGesture = curGesture;
            return curGesture;
        }

        //Screw Gesture Detection
        public void ScrewGestureDetection(VRTRIXFingerData fingerData, VRTRIXFingersStatus fingerStat, HANDTYPE type)
        {
            if(type == HANDTYPE.LEFT_HAND)
            {
                if (m_LHFingerDataQueue.Count == MAXQUEUESIZE)
                {
                    m_LHFingerDataQueue.Dequeue();
                    m_LHFingerDataQueue.Enqueue(fingerData);
                    double[] indexAngle = new double[MAXQUEUESIZE];
                    double[] thumbAngle = new double[MAXQUEUESIZE];
                    int index = 0;
                    foreach (var data in m_LHFingerDataQueue)
                    {
                        indexAngle[index] = data.indexBendAngle;
                        thumbAngle[index] = data.thumbBendAngle;
                        index++;
                    }
                    double indexAngleSlope = LinearRegression(indexAngle);
                    double thumbAngleSlope = LinearRegression(thumbAngle);

                    if (indexAngleSlope > 0.8 && thumbAngleSlope < -0.8 && m_LHScrewDetector.stat != VRTRIXDynamicGestureStatus.TRIGGERED)
                    {
                        if (m_LHScrewDetector.triggerCount < 10)
                        {
                            m_LHScrewDetector.triggerCount++;
                        }
                        else
                        {
                            Debug.LogWarning("left hand screw started " + m_LHScrewDetector.triggerCount + ", " + m_LHScrewDetector.releaseCount);
                            m_LHScrewDetector.stat = VRTRIXDynamicGestureStatus.TRIGGERED;
                            m_LHScrewDetector.triggerCount = 0;
                            m_LHScrewDetector.releaseCount = 0;
                        }
                    }
                    else if (Math.Abs(indexAngleSlope) < 0.1 && Math.Abs(thumbAngleSlope) < 0.1 && m_LHScrewDetector.stat == VRTRIXDynamicGestureStatus.TRIGGERED && m_LHScrewDetector.triggerCount > 10)
                    {
                        if(m_LHScrewDetector.releaseCount < 10)
                        {
                            m_LHScrewDetector.releaseCount++;
                        }
                        else
                        {
                            Debug.LogWarning("left hand screw released " + m_LHScrewDetector.triggerCount + ", " + m_LHScrewDetector.releaseCount);
                            m_LHScrewDetector.stat = VRTRIXDynamicGestureStatus.RELEASED;
                            m_LHScrewDetector.triggerCount = 0;
                            m_LHScrewDetector.releaseCount = 0;
                        }
                    }
                    else
                    {
                        if (m_LHScrewDetector.triggerCount > 0 && m_LHScrewDetector.stat != VRTRIXDynamicGestureStatus.TRIGGERED)
                        {
                            m_LHScrewDetector.triggerCount--;
                        }
                        else if(m_LHScrewDetector.stat == VRTRIXDynamicGestureStatus.TRIGGERED)
                        {
                            m_LHScrewDetector.triggerCount++;
                        }

                        if (m_LHScrewDetector.releaseCount > 0 && m_LHScrewDetector.stat != VRTRIXDynamicGestureStatus.RELEASED)
                        {
                            m_LHScrewDetector.releaseCount--;
                        }
                    }
                    //Debug.Log("left hand: " +indexAngleSlope + ", " + thumbAngleSlope);
                }
                else
                {
                    m_LHFingerDataQueue.Enqueue(fingerData);
                }
            }
            else
            {
                if (m_RHFingerDataQueue.Count == MAXQUEUESIZE)
                {
                    m_RHFingerDataQueue.Dequeue();
                    m_RHFingerDataQueue.Enqueue(fingerData);
                    double[] indexAngle = new double[MAXQUEUESIZE];
                    double[] thumbAngle = new double[MAXQUEUESIZE];
                    int index = 0;
                    foreach (var data in m_RHFingerDataQueue)
                    {
                        indexAngle[index] = data.indexBendAngle;
                        thumbAngle[index] = data.thumbBendAngle;
                        index++;
                    }
                    double indexAngleSlope = LinearRegression(indexAngle);
                    double thumbAngleSlope = LinearRegression(thumbAngle);

                    if (indexAngleSlope > 0.8 && thumbAngleSlope < -0.8 && m_RHScrewDetector.stat != VRTRIXDynamicGestureStatus.TRIGGERED)
                    {
                        if (m_RHScrewDetector.triggerCount < 10)
                        {
                            m_RHScrewDetector.triggerCount++;
                        }
                        else
                        {
                            Debug.LogWarning("right hand screw started " + m_RHScrewDetector.triggerCount + ", " + m_RHScrewDetector.releaseCount);
                            m_RHScrewDetector.stat = VRTRIXDynamicGestureStatus.TRIGGERED;
                            m_RHScrewDetector.triggerCount = 0;
                            m_RHScrewDetector.releaseCount = 0;
                        }
                    }
                    else if (Math.Abs(indexAngleSlope) < 0.1 && Math.Abs(thumbAngleSlope) < 0.1 && m_RHScrewDetector.stat == VRTRIXDynamicGestureStatus.TRIGGERED && m_RHScrewDetector.triggerCount > 10)
                    {
                        if (m_RHScrewDetector.releaseCount < 10)
                        {
                            m_RHScrewDetector.releaseCount++;
                        }
                        else
                        {
                            Debug.LogWarning("right hand screw released " + m_RHScrewDetector.triggerCount + ", " + m_RHScrewDetector.releaseCount);
                            m_RHScrewDetector.stat = VRTRIXDynamicGestureStatus.RELEASED;
                            m_RHScrewDetector.triggerCount = 0;
                            m_RHScrewDetector.releaseCount = 0;
                        }
                    }
                    else
                    {
                        if (m_RHScrewDetector.triggerCount > 0 && m_RHScrewDetector.stat != VRTRIXDynamicGestureStatus.TRIGGERED)
                        {
                            m_RHScrewDetector.triggerCount--;
                        }
                        else if (m_RHScrewDetector.stat == VRTRIXDynamicGestureStatus.TRIGGERED)
                        {
                            m_RHScrewDetector.triggerCount++;
                        }

                        if (m_RHScrewDetector.releaseCount > 0 && m_RHScrewDetector.stat != VRTRIXDynamicGestureStatus.RELEASED)
                        {
                            m_RHScrewDetector.releaseCount--;
                        }
                    }
                    //Debug.Log("right hand: " +indexAngleSlope + ", " + thumbAngleSlope);
                }
                else
                {
                    m_RHFingerDataQueue.Enqueue(fingerData);
                }
            }
        }

        public VRTRIXFingerStatus GetCurrentFingerStatus(double angle, VRTRIXBones finger)
        {
            VRTRIXFingerStatusThreshold threshold = m_thresholdMap[finger];
            if (angle < threshold.bendUpThreshold) return VRTRIXFingerStatus.BEND_UP;
            else if (angle < threshold.bendDownThreshold) return VRTRIXFingerStatus.STRAIGHT;
            else if (angle < threshold.curvedThreshold) return VRTRIXFingerStatus.BEND_DOWN;
            else return VRTRIXFingerStatus.CURVED;
        }

        /// <summary>
        /// Fits a line to a collection of points.
        /// </summary>
        /// <param name="yVals">The y-axis values.</param>
        /// <param name="rSquared">The r^2 value of the line.</param>
        /// <param name="yIntercept">The y-intercept value of the line (i.e. y = ax + b, yIntercept is b).</param>
        /// <param name="slope">The slop of the line (i.e. y = ax + b, slope is a).</param>
        public static double LinearRegression(
            double[] yVals
            /*, out double rSquared,
            out double yIntercept,*/)
        {

            double sumOfX = 0;
            double sumOfY = 0;
            double sumOfXSq = 0;
            double sumOfYSq = 0;
            double sumCodeviates = 0;

            for (var i = 0; i < yVals.Length; i++)
            {
                var x = i;
                var y = yVals[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }

            var count = yVals.Length;
            var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

            var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
            var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            var meanX = sumOfX / count;
            var meanY = sumOfY / count;
            var dblR = rNumerator / Math.Sqrt(rDenom);

            //rSquared = dblR * dblR;
            //yIntercept = meanY - ((sCo / ssX) * meanX);
            double slope = sCo / ssX;
            return slope;
        }
    }
}


