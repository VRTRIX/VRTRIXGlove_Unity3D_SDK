using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VRTRIX
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(VRTRIXInteractable))]
    public class VRTRIXGloveWaveDetector : MonoBehaviour
    {
        // Minimum delta angle (degree) required to consider as start waving
        public float minDeltaAngle = 0.5f;
        // Minimum delta angle (degree) required to consider as a full waving gesture
        public int minFramesWaving = 20;
        // Maximum angle tolerent for wave left/right detection start gesture.
        public float maxLRDetectionStartTolerent = 40.0f;
        // Maximum angle tolerent for wave up/down detection start gesture.
        public float maxUDDetectionStartTolerent = 40.0f;
        // Minimum duration between two detection of waving
        public float minDetectionInterval = 1;
        // Show Debug Info
        public bool IsShowDebugInfo = false;

        private int waveLFrameCount_RightHand = 0;
        private int waveRFrameCount_RightHand = 0;
        private int waveUFrameCount_RightHand = 0;
        private int waveDFrameCount_RightHand = 0;

        private int waveLFrameCount_LeftHand = 0;
        private int waveRFrameCount_LeftHand = 0;
        private int waveUFrameCount_LeftHand = 0;
        private int waveDFrameCount_LeftHand = 0;

        private bool bIsDetectLRStarted_RightHand = false;
        private bool bIsDetectLRStarted_LeftHand = false;
        private bool bIsDetectUDStarted_RightHand = false;
        private bool bIsDetectUDStarted_LeftHand = false;

        private Quaternion lastWristRotation_RightHand;
        private Quaternion lastWristRotation_LeftHand;

        private float lastDetectionTimeStamp;

        public VRTRIXCustomEvents.VRTRIXEventHand onHandWaveLeftDetected;
        public VRTRIXCustomEvents.VRTRIXEventHand onHandWaveRightDetected;
        public VRTRIXCustomEvents.VRTRIXEventHand onHandWaveUpDetected;
        public VRTRIXCustomEvents.VRTRIXEventHand onHandWaveDownDetected;
        private VRTRIXGloveGrab currentHand;
        // Use this for initialization
        void Start()
        {
            lastWristRotation_RightHand = Quaternion.identity;
            lastWristRotation_LeftHand = Quaternion.identity;
            lastDetectionTimeStamp = Time.time;
        }

        // Update is called once per frame
        void Update()
        {

        }

        //-------------------------------------------------
        // Called when a Hand starts hovering over this object
        //-------------------------------------------------
        private void OnHandHoverBegin(VRTRIXGloveGrab hand)
        {
            currentHand = hand;
            //Debug.Log("OnHandHoverBegin " + hand.name);
            bIsDetectLRStarted_RightHand = false;
            waveLFrameCount_RightHand = 0;
            waveRFrameCount_RightHand = 0;

            bIsDetectLRStarted_LeftHand = false;
            waveLFrameCount_LeftHand = 0;
            waveRFrameCount_LeftHand = 0;

            bIsDetectUDStarted_RightHand = false;
            waveUFrameCount_RightHand = 0;
            waveDFrameCount_RightHand = 0;

            bIsDetectUDStarted_LeftHand = false;
            waveUFrameCount_LeftHand = 0;
            waveDFrameCount_LeftHand = 0;

            Transform wrist = hand.getWristTransform();
            Vector3 wrist_yAxis = wrist.rotation * Vector3.up;
            Vector3 wrist_xAxis = wrist.rotation * Vector3.right;
            if (hand.GetHandType() == HANDTYPE.RIGHT_HAND && wrist.rotation!= Quaternion.identity)
            {
                if (Vector3.Angle(wrist_yAxis, Vector3.up) < maxLRDetectionStartTolerent)
                {
                    bIsDetectLRStarted_RightHand = true;
                    if(IsShowDebugInfo) Debug.Log("[开始检测]: 右手处于开始左右挥动检测姿态");
                }
                else if(Mathf.Abs(Vector3.Angle(wrist_yAxis, Vector3.up) - 90) < maxUDDetectionStartTolerent && Vector3.Dot(wrist_xAxis, Vector3.up) > 0)
                {
                    bIsDetectUDStarted_RightHand = true;
                    if (IsShowDebugInfo) Debug.Log("[开始检测]: 右手处于开始上下挥动检测姿态");
                }
            }
            else if(hand.GetHandType() == HANDTYPE.LEFT_HAND && wrist.rotation != Quaternion.identity)
            {
                if (Vector3.Angle(wrist_yAxis, Vector3.up) < maxLRDetectionStartTolerent)
                {
                    bIsDetectLRStarted_LeftHand = true;
                    if (IsShowDebugInfo) Debug.Log("[开始检测]: 左手处于开始左右挥动检测姿态");
                }
                else if (Mathf.Abs(Vector3.Angle(wrist_yAxis, Vector3.up) - 90) < maxUDDetectionStartTolerent && Vector3.Dot(wrist_xAxis, Vector3.up) < 0)
                {
                    bIsDetectUDStarted_LeftHand = true;
                    if (IsShowDebugInfo) Debug.Log("[开始检测]: 左手处于开始上下挥动检测姿态");
                }
            }
        }

        //-------------------------------------------------
        // Called when a Hand stops hovering over this object
        //-------------------------------------------------
        private void OnHandHoverEnd(VRTRIXGloveGrab hand)
        {
            currentHand = null;
            //Debug.Log("OnHandHoverEnd " + hand.name);
            if (hand.GetHandType() == HANDTYPE.RIGHT_HAND)
            {
                if (bIsDetectLRStarted_RightHand)
                {
                    if (IsShowDebugInfo) Debug.Log("[结束检测]: 右手结束左右挥动姿态检测");
                }
                if (bIsDetectUDStarted_RightHand)
                {
                    if (IsShowDebugInfo) Debug.Log("[结束检测]: 右手结束上下挥动姿态检测");
                }
                bIsDetectLRStarted_RightHand = false;
                bIsDetectUDStarted_RightHand = false;
                waveLFrameCount_RightHand = 0;
                waveRFrameCount_RightHand = 0;
                waveUFrameCount_RightHand = 0;
                waveDFrameCount_RightHand = 0;
            }
            else if (hand.GetHandType() == HANDTYPE.LEFT_HAND)
            {
                if (bIsDetectLRStarted_LeftHand)
                {
                    if (IsShowDebugInfo) Debug.Log("[结束检测]: 左手结束左右挥动姿态检测");
                }
                if (bIsDetectUDStarted_LeftHand)
                {
                    if (IsShowDebugInfo) Debug.Log("[结束检测]: 左手结束上下挥动姿态检测");
                }
                bIsDetectLRStarted_LeftHand = false;
                bIsDetectUDStarted_LeftHand = false;
                waveLFrameCount_LeftHand = 0;
                waveRFrameCount_LeftHand = 0;
                waveUFrameCount_RightHand = 0;
                waveDFrameCount_RightHand = 0;
            }
        }

        //-------------------------------------------------
        // Called every Update() while a Hand is hovering over this object
        //-------------------------------------------------
        private void HandHoverUpdate(VRTRIXGloveGrab hand)
        {
            Transform wrist = hand.getWristTransform();
            if (hand.GetHandType() == HANDTYPE.RIGHT_HAND&& wrist.rotation!= Quaternion.identity)
            {
                Vector3 wrist_yAxis = wrist.rotation * Vector3.up;
                Vector3 wrist_xAxis = wrist.rotation * Vector3.right;
                //左右挥手检测开始姿态
                if (Vector3.Angle(wrist_yAxis, Vector3.up) < maxLRDetectionStartTolerent)
                {
                    if (!bIsDetectLRStarted_RightHand)
                    {
                        bIsDetectLRStarted_RightHand = true;
                        if (IsShowDebugInfo) Debug.Log("[开始检测]: 右手处于开始左右挥动检测姿态");
                    }
                }
                else if (bIsDetectLRStarted_RightHand)
                {
                    bIsDetectLRStarted_RightHand = false;
                    waveLFrameCount_RightHand = 0;
                    waveRFrameCount_RightHand = 0;
                    if (IsShowDebugInfo) Debug.Log("[结束检测]: 右手结束左右挥动姿态检测");
                }

                //上下挥手检测开始姿态
                if (Mathf.Abs(Vector3.Angle(wrist_yAxis, Vector3.up) - 90) < maxUDDetectionStartTolerent)
                {
                    if (!bIsDetectUDStarted_RightHand && Vector3.Dot(wrist_xAxis, Vector3.up) > 0)
                    {
                        bIsDetectUDStarted_RightHand = true;
                        if (IsShowDebugInfo) Debug.Log("[开始检测]: 右手处于开始上下挥动检测姿态");
                    }
                }
                else if (bIsDetectUDStarted_RightHand)
                {
                    bIsDetectUDStarted_RightHand = false;
                    waveUFrameCount_RightHand = 0;
                    waveDFrameCount_RightHand = 0;
                    if (IsShowDebugInfo) Debug.Log("[结束检测]: 右手结束上下挥动姿态检测");
                }

                //左右挥手检测
                if (bIsDetectLRStarted_RightHand)
                {
                    if (lastWristRotation_RightHand != Quaternion.identity)
                    {
                        Vector3 deltaRotationZ = Vector3.Cross(lastWristRotation_RightHand * Vector3.forward, wrist.rotation * Vector3.forward);
                        if (deltaRotationZ.magnitude >= Mathf.Sin(Mathf.Deg2Rad * minDeltaAngle) && Vector3.Dot(deltaRotationZ, Vector3.up) < 0)
                        {
                            waveLFrameCount_RightHand++;
                            waveRFrameCount_RightHand--;
                            if (waveRFrameCount_RightHand < 0) waveRFrameCount_RightHand = 0;
                        }
                        else if (deltaRotationZ.magnitude >= Mathf.Sin(Mathf.Deg2Rad * minDeltaAngle) && Vector3.Dot(deltaRotationZ, Vector3.up) > 0)
                        {
                            waveRFrameCount_RightHand++;
                            waveLFrameCount_RightHand--;
                            if (waveLFrameCount_RightHand < 0) waveLFrameCount_RightHand = 0;
                        }
                        else
                        {
                            waveRFrameCount_RightHand--;
                            waveLFrameCount_RightHand--;
                            if (waveRFrameCount_RightHand < 0) waveRFrameCount_RightHand = 0;
                            if (waveLFrameCount_RightHand < 0) waveLFrameCount_RightHand = 0;
                        }
                    }
                }
                if (waveLFrameCount_RightHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveLFrameCount_RightHand = 0;
                    lastDetectionTimeStamp = Time.time;
                    onHandWaveLeftDetected.Invoke(currentHand);
                }
                if (waveRFrameCount_RightHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveRFrameCount_RightHand = 0;
                    lastDetectionTimeStamp = Time.time;
                    onHandWaveRightDetected.Invoke(currentHand);
                }

                //上下挥手检测
                if (bIsDetectUDStarted_RightHand)
                {
                    if (lastWristRotation_RightHand != Quaternion.identity)
                    {
                        Vector3 deltaRotationZ = Vector3.Cross(lastWristRotation_RightHand * Vector3.forward, wrist.rotation * Vector3.forward);
                        if (deltaRotationZ.magnitude >= Mathf.Sin(Mathf.Deg2Rad * minDeltaAngle) && Vector3.Dot(deltaRotationZ, wrist.rotation * Vector3.up) < 0 && Mathf.Abs(Vector3.Angle(deltaRotationZ, wrist.rotation * Vector3.up) - 180) < 30)
                        {
                            waveDFrameCount_RightHand++;
                            waveUFrameCount_RightHand--;
                            if (waveUFrameCount_RightHand < 0) waveUFrameCount_RightHand = 0;

                        }
                        else if (deltaRotationZ.magnitude >= Mathf.Sin(Mathf.Deg2Rad * minDeltaAngle) && Vector3.Dot(deltaRotationZ, wrist.rotation * Vector3.up) > 0 && Vector3.Angle(deltaRotationZ, wrist.rotation * Vector3.up) < 30)
                        {
                            waveUFrameCount_RightHand++;
                            waveDFrameCount_RightHand--;
                            if (waveDFrameCount_RightHand < 0) waveDFrameCount_RightHand = 0;
                        }
                        else
                        {
                            waveUFrameCount_RightHand--;
                            waveDFrameCount_RightHand--;
                            if (waveUFrameCount_RightHand < 0) waveUFrameCount_RightHand = 0;
                            if (waveDFrameCount_RightHand < 0) waveDFrameCount_RightHand = 0;
                        }
                    }
                }
                if (waveUFrameCount_RightHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveUFrameCount_RightHand = 0;
                    lastDetectionTimeStamp = Time.time;
                    onHandWaveUpDetected.Invoke(currentHand);
                }
                if (waveDFrameCount_RightHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveDFrameCount_RightHand = 0;
                    lastDetectionTimeStamp = Time.time;
                    onHandWaveDownDetected.Invoke(currentHand);
                }

                lastWristRotation_RightHand = wrist.rotation;
            }


            else if (hand.GetHandType() == HANDTYPE.LEFT_HAND && wrist.rotation != Quaternion.identity)
            {
                Vector3 wrist_yAxis = wrist.rotation * Vector3.up;
                Vector3 wrist_xAxis = wrist.rotation * Vector3.right;
                //左右挥手检测开始姿态
                if (Vector3.Angle(wrist_yAxis, Vector3.up) < maxLRDetectionStartTolerent)
                {
                    if (!bIsDetectLRStarted_LeftHand)
                    {
                        bIsDetectLRStarted_LeftHand = true;
                        if (IsShowDebugInfo) Debug.Log("[开始检测]: 左手处于开始左右挥动检测姿态");
                    }
                }
                else if (bIsDetectLRStarted_LeftHand)
                {
                    bIsDetectLRStarted_LeftHand = false;
                    waveLFrameCount_LeftHand = 0;
                    waveRFrameCount_LeftHand = 0;
                    if (IsShowDebugInfo) Debug.Log("[结束检测]: 左手结束左右挥动姿态检测");
                }

                //上下挥手检测开始姿态
                if (Mathf.Abs(Vector3.Angle(wrist_yAxis, Vector3.up) - 90) < maxUDDetectionStartTolerent)
                {
                    if (!bIsDetectUDStarted_LeftHand && Vector3.Dot(wrist_xAxis, Vector3.up) < 0)
                    {
                        bIsDetectUDStarted_LeftHand = true;
                        if (IsShowDebugInfo) Debug.Log("[开始检测]: 左手处于开始上下挥动检测姿态");
                    }
                }
                else if (bIsDetectUDStarted_LeftHand)
                {
                    bIsDetectUDStarted_LeftHand = false;
                    waveUFrameCount_LeftHand = 0;
                    waveDFrameCount_LeftHand = 0;
                    if (IsShowDebugInfo) Debug.Log("[结束检测]: 左手结束上下挥动姿态检测");
                }

                //左右挥手检测
                if (bIsDetectLRStarted_LeftHand)
                {
                    if (lastWristRotation_LeftHand != Quaternion.identity)
                    {
                        Vector3 deltaRotationZ = Vector3.Cross(lastWristRotation_LeftHand * Vector3.forward, wrist.rotation * Vector3.forward);
                        if (deltaRotationZ.magnitude >= Mathf.Sin(Mathf.Deg2Rad * minDeltaAngle) && Vector3.Dot(deltaRotationZ, Vector3.up) < 0)
                        {
                            waveLFrameCount_LeftHand++;
                            waveRFrameCount_LeftHand--;
                            if (waveRFrameCount_LeftHand < 0) waveRFrameCount_LeftHand = 0;
                        }
                        else if (deltaRotationZ.magnitude >= Mathf.Sin(Mathf.Deg2Rad * minDeltaAngle) && Vector3.Dot(deltaRotationZ, Vector3.up) > 0)
                        {
                            waveRFrameCount_LeftHand++;
                            waveLFrameCount_LeftHand--;
                            if (waveLFrameCount_LeftHand < 0) waveLFrameCount_LeftHand = 0;
                        }
                        else
                        {
                            waveRFrameCount_LeftHand--;
                            waveLFrameCount_LeftHand--;
                            if (waveRFrameCount_LeftHand < 0) waveRFrameCount_LeftHand = 0;
                            if (waveLFrameCount_LeftHand < 0) waveLFrameCount_LeftHand = 0;
                        }
                    }
                }
                if (waveLFrameCount_LeftHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveLFrameCount_LeftHand = 0;
                    lastDetectionTimeStamp = Time.time;
                    onHandWaveLeftDetected.Invoke(currentHand);
                }
                if (waveRFrameCount_LeftHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveRFrameCount_LeftHand = 0;
                    lastDetectionTimeStamp = Time.time;
                    onHandWaveRightDetected.Invoke(currentHand);
                }

                //上下挥手检测
                if (bIsDetectUDStarted_LeftHand)
                {
                    if (lastWristRotation_LeftHand != Quaternion.identity)
                    {
                        Vector3 deltaRotationZ = Vector3.Cross(lastWristRotation_LeftHand * Vector3.forward, wrist.rotation * Vector3.forward);
                        if (deltaRotationZ.magnitude >= Mathf.Sin(Mathf.Deg2Rad * minDeltaAngle) && Vector3.Dot(deltaRotationZ, wrist.rotation * Vector3.up) < 0 && Mathf.Abs(Vector3.Angle(deltaRotationZ, wrist.rotation * Vector3.up) - 180) < 30)
                        {
                            waveUFrameCount_LeftHand++;
                            waveDFrameCount_LeftHand--;
                            if (waveDFrameCount_LeftHand < 0) waveDFrameCount_LeftHand = 0;
                        }
                        else if (deltaRotationZ.magnitude >= Mathf.Sin(Mathf.Deg2Rad * minDeltaAngle) && Vector3.Dot(deltaRotationZ, wrist.rotation * Vector3.up) > 0 && Vector3.Angle(deltaRotationZ, wrist.rotation * Vector3.up) < 30)
                        {
                            waveDFrameCount_LeftHand++;
                            waveUFrameCount_LeftHand--;
                            if (waveUFrameCount_LeftHand < 0) waveUFrameCount_LeftHand = 0;
                        }
                        else
                        {
                            waveUFrameCount_LeftHand--;
                            waveDFrameCount_LeftHand--;
                            if (waveUFrameCount_LeftHand < 0) waveUFrameCount_LeftHand = 0;
                            if (waveDFrameCount_LeftHand < 0) waveDFrameCount_LeftHand = 0;
                        }
                    }
                }
                if (waveUFrameCount_LeftHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveUFrameCount_LeftHand = 0;
                    lastDetectionTimeStamp = Time.time;
                    onHandWaveUpDetected.Invoke(currentHand);
                }
                if (waveDFrameCount_LeftHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveDFrameCount_LeftHand = 0;
                    lastDetectionTimeStamp = Time.time;
                    onHandWaveDownDetected.Invoke(currentHand);
                }

                lastWristRotation_LeftHand = wrist.rotation;
            }
        }
    }
}

