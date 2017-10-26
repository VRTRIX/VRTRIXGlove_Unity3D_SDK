//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: For controlling in-game objects with tracked devices.
//
//=============================================================================

using System.Collections;
using UnityEngine;
using Valve.VR;

namespace VRTRIX
{
    public class VRTRIXGloveTrackedOjects : MonoBehaviour
    {
 

        [Tooltip("If not set, relative to parent")]
        public Transform origin;

        public enum EIndex
        {
            None = -1,
            Hmd = (int)OpenVR.k_unTrackedDeviceIndex_Hmd,
            Device1,
            Device2,
            Device3,
            Device4,
            Device5,
            Device6,
            Device7,
            Device8,
            Device9,
            Device10,
            Device11,
            Device12,
            Device13,
            Device14,
            Device15
        }

        public EIndex index;


        public bool isValid { get; private set; }
        private HANDTYPE handtype;
        private GameObject Tracker;

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            if (index == EIndex.None)
                return;

            var i = (int)index;

            isValid = false;
            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            isValid = true;

            var pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            if (origin != null)
            {
                transform.position = origin.transform.TransformPoint(pose.pos);
                transform.rotation = origin.rotation * pose.rot;
            }
            else
            {
                transform.localPosition = pose.pos;
                transform.localRotation = pose.rot;
            }
        }

        SteamVR_Events.Action newPosesAction;

        VRTRIXGloveTrackedOjects()
        {
            GetTrackedObjectsIndexID();
            newPosesAction = SteamVR_Events.NewPosesAction(OnNewPoses);
        }



        void OnEnable()
        {
            var render = SteamVR_Render.instance;
            if (render == null)
            {
                enabled = false;
                return;
            }

            newPosesAction.enabled = true;
        }

        void OnDisable()
        {
            newPosesAction.enabled = false;
            isValid = false;
        }

        public void SetDeviceIndex(int index)
        {
            if (System.Enum.IsDefined(typeof(EIndex), index))
                this.index = (EIndex)index;
        }

        private void GetTrackedObjectsIndexID()
        {
            var vr = SteamVR.instance;
            for (int i = 0; i < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount; i++)
            {
                //Debug.Log(vr.hmd.GetTrackedDeviceClass((uint)i));
                if (vr.hmd.GetTrackedDeviceClass((uint)i) != Valve.VR.ETrackedDeviceClass.Controller && vr.hmd.GetTrackedDeviceClass((uint)i) != Valve.VR.ETrackedDeviceClass.GenericTracker)
                {
                    //Debug.Log( string.Format( "Hand - device {0} is not a controller", i ) );
                    continue;
                }

                if (vr.hmd.GetTrackedDeviceClass((uint)i) == Valve.VR.ETrackedDeviceClass.GenericTracker)
                {
                    var system = OpenVR.System;
                    var error = ETrackedPropertyError.TrackedProp_Success;
                    var capacity = system.GetStringTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_RenderModelName_String, null, 0, ref error);
                    if (capacity <= 1)
                    {
                        continue;
                    }

                    var buffer = new System.Text.StringBuilder((int)capacity);
                    system.GetStringTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.Prop_RenderModelName_String, buffer, capacity, ref error);
                    var s = buffer.ToString();
                    if (this.name.Contains("LH"))
                    {
                        if (s.Contains("LH"))
                        {
                            handtype = HANDTYPE.LEFT_HAND;
                            Tracker = this.gameObject;
                            index = (EIndex)i;
                            Debug.Log(string.Format("Device {0} is a Left Hand Tracker", i));
                            break;
                            
                        }
                    }

                    if (this.name.Contains("RH"))
                    {
                        if (s.Contains("RH"))
                        {
                            handtype = HANDTYPE.RIGHT_HAND;
                            Tracker = this.gameObject;
                            index = (EIndex)i;
                            Debug.Log(string.Format("Device {0} is a Right Hand Tracker", i));
                            break;
                            
                        }
                    }
                }
            }
        }
    }
}
