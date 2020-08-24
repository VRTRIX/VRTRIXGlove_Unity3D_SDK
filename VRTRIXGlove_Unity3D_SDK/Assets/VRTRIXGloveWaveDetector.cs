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
        // Minimum duration between two detection of waving
        public float minDetectionInterval = 1;

        private int waveLFrameCount_RightHand = 0;
        private int waveRFrameCount_RightHand = 0;
        private int waveUFrameCount_RightHand = 0;
        private int waveDFrameCount_RightHand = 0;

        private int waveLFrameCount_LeftHand = 0;
        private int waveRFrameCount_LeftHand = 0;
        private int waveUFrameCount_LeftHand = 0;
        private int waveDFrameCount_LeftHand = 0;

        private bool bIsDetectStarted_RightHand = false;
        private Quaternion lastWristRotation_RightHand;

        private bool bIsDetectStarted_LeftHand = false;
        private Quaternion lastWristRotation_LeftHand;

        private float lastDetectionTimeStamp;
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
            //Debug.Log("OnHandHoverBegin " + hand.name);
            bIsDetectStarted_RightHand = false;
            waveLFrameCount_RightHand = 0;
            waveRFrameCount_RightHand = 0;

            bIsDetectStarted_LeftHand = false;
            waveLFrameCount_LeftHand = 0;
            waveRFrameCount_LeftHand = 0;

            Transform wrist = hand.getWristTransform();
            if(hand.GetHandType() == HANDTYPE.RIGHT_HAND && wrist.rotation!= Quaternion.identity)
            {
                Vector3 wrist_yAxis = wrist.rotation * Vector3.up;
                if (Vector3.Angle(wrist_yAxis, Vector3.up) < 20)
                {
                    bIsDetectStarted_RightHand = true;
                    Debug.Log("[开始检测]: 右手处于开始检测姿态");
                }
            }
            else if(hand.GetHandType() == HANDTYPE.LEFT_HAND && wrist.rotation != Quaternion.identity)
            {
                Vector3 wrist_yAxis = wrist.rotation * Vector3.up;
                if (Vector3.Angle(wrist_yAxis, Vector3.up) < 20)
                {
                    bIsDetectStarted_LeftHand = true;
                    Debug.Log("[开始检测]: 左手处于开始检测姿态");
                }
            }
        }

        //-------------------------------------------------
        // Called when a Hand stops hovering over this object
        //-------------------------------------------------
        private void OnHandHoverEnd(VRTRIXGloveGrab hand)
        {
            //Debug.Log("OnHandHoverEnd " + hand.name);
            if(hand.GetHandType() == HANDTYPE.RIGHT_HAND)
            {
                bIsDetectStarted_RightHand = false;
                waveLFrameCount_RightHand = 0;
                waveRFrameCount_RightHand = 0;
            }
            else if (hand.GetHandType() == HANDTYPE.LEFT_HAND)
            {
                bIsDetectStarted_LeftHand = false;
                waveLFrameCount_LeftHand = 0;
                waveRFrameCount_LeftHand = 0;
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
                if (Vector3.Angle(wrist_yAxis, Vector3.up) < 40)
                {
                    if (!bIsDetectStarted_RightHand)
                    {
                        bIsDetectStarted_RightHand = true;
                        Debug.Log("[开始检测]: 右手处于开始检测姿态");
                    }
                }
                else if (bIsDetectStarted_RightHand)
                {
                    bIsDetectStarted_RightHand = false;
                    waveLFrameCount_RightHand = 0;
                    waveRFrameCount_RightHand = 0;
                    Debug.Log("[结束检测]: 右手结束姿态检测");
                }

                if (bIsDetectStarted_RightHand)
                {
                    Quaternion currentWristRotation = wrist.rotation;
                    if (lastWristRotation_RightHand == Quaternion.identity)
                    {
                        lastWristRotation_RightHand = currentWristRotation;
                    }
                    else
                    {
                        Vector3 deltaRotationZ = Vector3.Cross(lastWristRotation_RightHand * Vector3.forward, currentWristRotation * Vector3.forward);
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
                        lastWristRotation_RightHand = currentWristRotation;
                    }
                }
                if (waveLFrameCount_RightHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveLFrameCount_RightHand = 0;
                    Debug.Log("[检测成功]: 右手向左挥动!");
                    lastDetectionTimeStamp = Time.time;
                }
                if (waveRFrameCount_RightHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveRFrameCount_RightHand = 0;
                    Debug.Log("[检测成功]: 右手向右挥动!");
                    lastDetectionTimeStamp = Time.time;
                }
            }


            else if (hand.GetHandType() == HANDTYPE.LEFT_HAND && wrist.rotation != Quaternion.identity)
            {
                Vector3 wrist_yAxis = wrist.rotation * Vector3.up;
                if (Vector3.Angle(wrist_yAxis, Vector3.up) < 40)
                {
                    if (!bIsDetectStarted_LeftHand)
                    {
                        bIsDetectStarted_LeftHand = true;
                        Debug.Log("[开始检测]: 左手处于开始检测姿态");
                    }
                }
                else if (bIsDetectStarted_LeftHand)
                {
                    bIsDetectStarted_LeftHand = false;
                    waveLFrameCount_LeftHand = 0;
                    waveRFrameCount_LeftHand = 0;
                    Debug.Log("[结束检测]: 左手结束姿态检测");
                }

                if (bIsDetectStarted_LeftHand)
                {
                    Quaternion currentWristRotation = wrist.rotation;
                    if (lastWristRotation_LeftHand == Quaternion.identity)
                    {
                        lastWristRotation_LeftHand = currentWristRotation;
                    }
                    else
                    {
                        Vector3 deltaRotationZ = Vector3.Cross(lastWristRotation_LeftHand * Vector3.forward, currentWristRotation * Vector3.forward);
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
                        lastWristRotation_LeftHand = currentWristRotation;
                    }
                }
                if (waveLFrameCount_LeftHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveLFrameCount_LeftHand = 0;
                    Debug.Log("[检测成功]: 左手向左挥动!");
                    lastDetectionTimeStamp = Time.time;
                }
                if (waveRFrameCount_LeftHand >= minFramesWaving && Time.time - lastDetectionTimeStamp > minDetectionInterval)
                {
                    waveRFrameCount_LeftHand = 0;
                    Debug.Log("[检测成功]: 左手向右挥动!");
                    lastDetectionTimeStamp = Time.time;
                }
            }

        }

        /// <summary>
        /// Returns the angle between two vectos
        /// </summary>
        public static double GetAngle(Vector3 A, Vector3 B, Vector3 normal)
        {
            return System.Math.Atan2(Vector3.Dot(Vector3.Cross(A, B), normal), Vector3.Dot(A, B)) * (180 / System.Math.PI);
        }
    }
}

