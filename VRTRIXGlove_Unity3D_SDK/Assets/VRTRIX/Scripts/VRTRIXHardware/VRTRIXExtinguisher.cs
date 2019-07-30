//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Demonstrates how to interact with VRTRIX Extinguishers using VRTRIX
//          Data Gloves.
//
//=============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VRTRIX
{
    [RequireComponent(typeof(VRTRIXInteractable))]
    public class VRTRIXExtinguisher : MonoBehaviour
    {
        private GameObject Ex_Ref;
        private GameObject Ex;
        private bool isHoveredbyHand;
        private VRTRIXGloveGrab.AttachmentFlags attachmentFlags = VRTRIXGloveGrab.defaultAttachmentFlags & (~VRTRIXGloveGrab.AttachmentFlags.SnapOnAttach) & (~VRTRIXGloveGrab.AttachmentFlags.DetachOthers);
        // Use this for initialization
        void Start()
        {
            Ex_Ref = VRTRIXGloveDataStreaming.CheckDeviceModelName(HANDTYPE.NONE, InteractiveDevice.EXTINGUISHER);
            Ex = gameObject;
            isHoveredbyHand = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isHoveredbyHand && Ex_Ref != null)
            {

                Ex.transform.position = Ex_Ref.transform.position;
                Ex.transform.rotation = Ex_Ref.transform.rotation;

            }
        }

        //-------------------------------------------------
        // Called when a Hand starts hovering over this object
        //-------------------------------------------------
        private void OnHandHoverBegin(VRTRIXGloveGrab hand)
        {
            //if (textMesh != null)
            //{
            //    textMesh.text = "Hovering hand: " + hand.name;
            //}

            //isHoveredbyHand = true;
        }


        //-------------------------------------------------
        // Called when a Hand stops hovering over this object
        //-------------------------------------------------
        private void OnHandHoverEnd(VRTRIXGloveGrab hand)
        {
            //if (textMesh != null)
            //{
            //    textMesh.text = "No Hand Hovering";
            //}
            //isHoveredbyHand = false;
        }


        //-------------------------------------------------
        // Called every Update() while a Hand is hovering over this object
        //-------------------------------------------------
        private void HandHoverUpdate(VRTRIXGloveGrab hand)
        {
            //Debug.Log(VRTRIXGloveVRInteraction.GetGesture(hand.GetHandType()));
            if (hand.GetStandardInteractionButtonDown())
            {
                if (hand.currentAttachedObject != gameObject)
                {
                    isHoveredbyHand = true;
                    //Call this to continue receiving HandHoverUpdate messages,
                    // and prevent the hand from hovering over anything else
                    hand.HoverLock(GetComponent<VRTRIXInteractable>());
                    if (hand.GetHandType() == HANDTYPE.LEFT_HAND)
                    {
                        // Attach this object to the left hand
                        //hand.AttachObject(gameObject, attachmentFlags);
                        hand.AttachLongBow(gameObject, attachmentFlags, "L_Middle_1");
                        gameObject.transform.localPosition = new Vector3(-0.311f, 0.072f, 0.04f);
                        gameObject.transform.localRotation = Quaternion.Euler(0, 200f, 90f);

                    }
                    else if (hand.GetHandType() == HANDTYPE.RIGHT_HAND)
                    {
                        hand.AttachLongBow(gameObject, attachmentFlags, "R_Middle_1");
                        gameObject.transform.localPosition = new Vector3(-0.311f, 0.072f, 0.04f);
                        gameObject.transform.localRotation = Quaternion.Euler(0, -20f, -90f);
                    }


                }

            }
        }


        ////-------------------------------------------------
        //// Called when this GameObject becomes attached to the hand
        ////-------------------------------------------------
        //private void OnAttachedToHand(VRTRIXGloveGrab hand)
        //{
        //    if (textMesh != null)
        //    {
        //        textMesh.text = "Attached to hand: " + hand.name;
        //    }
        //    attachTime = Time.time;
        //}


        ////-------------------------------------------------
        //// Called when this GameObject is detached from the hand
        ////-------------------------------------------------
        //private void OnDetachedFromHand(VRTRIXGloveGrab hand)
        //{
        //    if (textMesh != null)
        //    {
        //        textMesh.text = "Detached from hand: " + hand.name;
        //    }
        //}


        //-------------------------------------------------
        // Called every Update() while this GameObject is attached to the hand
        //-------------------------------------------------
        private void HandAttachedUpdate(VRTRIXGloveGrab hand)
        {
            //if (textMesh != null)
            //{
            //    textMesh.text = "Attached to hand: " + hand.name + "\nAttached time: " + (Time.time - attachTime).ToString("F2");
            //}
            if (!hand.GetStandardInteractionButton())
            {
                isHoveredbyHand = false;
                // Detach ourselves late in the frame.
                // This is so that any vehicles the player is attached to
                // have a chance to finish updating themselves.
                // If we detach now, our position could be behind what it
                // will be at the end of the frame, and the object may appear
                // to teleport behind the hand when the player releases it.
                StartCoroutine(LateDetach(hand));
            }
        }

        private IEnumerator LateDetach(VRTRIXGloveGrab hand)
        {
            yield return new WaitForEndOfFrame();
            //Debug.Log(hand.currentAttachedObject);
            if (hand.currentAttachedObject == gameObject)
            {
                // Detach this object from the hand
                hand.DetachObject(gameObject);

                // Call this to undo HoverLock
                hand.HoverUnlock(GetComponent<VRTRIXInteractable>());

                isHoveredbyHand = false;
            }
            // hand.DetachObject(gameObject);
        }
    }
}

