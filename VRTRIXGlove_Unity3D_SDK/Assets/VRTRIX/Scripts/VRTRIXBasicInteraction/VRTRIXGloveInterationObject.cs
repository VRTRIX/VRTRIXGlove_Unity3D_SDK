//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Demonstrates how to create a simple interactable object
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace VRTRIX
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(VRTRIXInteractable))]
    public class VRTRIXGloveInterationObject : MonoBehaviour
    {
        private TextMesh textMesh;
        private Vector3 oldPosition;
        private Quaternion oldRotation;
        private float attachTime;
        private VRTRIXGloveGrab.AttachmentFlags attachmentFlags = VRTRIXGloveGrab.defaultAttachmentFlags & (~VRTRIXGloveGrab.AttachmentFlags.SnapOnAttach) & (~VRTRIXGloveGrab.AttachmentFlags.DetachOthers);

        //-------------------------------------------------
        void Awake()
        {
            textMesh = GetComponentInChildren<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = "No Hand Hovering";
            }
        }


        //-------------------------------------------------
        // Called when a Hand starts hovering over this object
        //-------------------------------------------------
        private void OnHandHoverBegin(VRTRIXGloveGrab hand)
        {
            if (textMesh != null)
            {
                textMesh.text = "Hovering hand: " + hand.name;
            }
        }


        //-------------------------------------------------
        // Called when a Hand stops hovering over this object
        //-------------------------------------------------
        private void OnHandHoverEnd(VRTRIXGloveGrab hand)
        {
            if (textMesh != null)
            {
                textMesh.text = "No Hand Hovering";
            }
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
                    // Save our position/rotation so that we can restore it when we detach
                    oldPosition = transform.position;
                    oldRotation = transform.rotation;

                    //Call this to continue receiving HandHoverUpdate messages,
                    // and prevent the hand from hovering over anything else
                    hand.HoverLock(GetComponent<VRTRIXInteractable>());

                    // Attach this object to the hand
                    hand.AttachObject(gameObject, attachmentFlags);
                }

            }
        }


        //-------------------------------------------------
        // Called when this GameObject becomes attached to the hand
        //-------------------------------------------------
        private void OnAttachedToHand(VRTRIXGloveGrab hand)
        {
            if (textMesh != null)
            {
                textMesh.text = "Attached to hand: " + hand.name;
            }
            attachTime = Time.time;
        }


        //-------------------------------------------------
        // Called when this GameObject is detached from the hand
        //-------------------------------------------------
        private void OnDetachedFromHand(VRTRIXGloveGrab hand)
        {
            if (textMesh != null)
            {
                textMesh.text = "Detached from hand: " + hand.name;
            }
        }


        //-------------------------------------------------
        // Called every Update() while this GameObject is attached to the hand
        //-------------------------------------------------
        private void HandAttachedUpdate(VRTRIXGloveGrab hand)
        {
            if(textMesh != null)
            {
                textMesh.text = "Attached to hand: " + hand.name + "\nAttached time: " + (Time.time - attachTime).ToString("F2");
            }
            if (!hand.GetStandardInteractionButton())
            {
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

                // Restore position/rotation
                transform.position = oldPosition;
                transform.rotation = oldRotation;
            }
           // hand.DetachObject(gameObject);
        }
    }
}

