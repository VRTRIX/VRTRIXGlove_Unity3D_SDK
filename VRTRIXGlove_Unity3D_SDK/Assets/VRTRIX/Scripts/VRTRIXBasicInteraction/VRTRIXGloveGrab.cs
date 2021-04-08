//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Core Script for VRTRIX data glove interaction. Provide variaties of
//          member functions/events to detect collsion between data glove and 
//          virtual 3D objects/ real tracking objects (sword&shield, extinguisher 
//          etc). 
//          There are some interaction method defined using gesture recognition,
//          (like teleport or grab), they are fully configurable. Just add your
//          design of interaction method and integrate that to your game/content!
//
//=============================================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VRTRIX {

    public class VRTRIXGloveGrab : MonoBehaviour
    {
        public HANDTYPE type;
        public VRTRIXGloveGrab otherHand;
        public Transform teleportArcStartTransform;
        public Transform hoverSphereTransform;
        public Transform thumbSphereTransform;
        public Transform indexSphereTransform;
        public Transform objectAttachTransform;
        public float hoverSphereRadius = 0.05f;
        public float fingerSphereRadius = 0.01f;
        public LayerMask hoverLayerMask = -1;
        public float hoverUpdateInterval = 0.1f;
        public bool hoverLocked { get; private set; }
        public bool fingertipTouchLocked { get; private set; }

        private const int ColliderArraySize = 16;
        private Collider[] overlappingColliders;
        private Collider[] indexOverlappingColliders;
        private Collider[] thumbOverlappingColliders;
        private VRTRIXInteractable _hoveringInteractable;
        private VRTRIXInteractable _fingertipTouchInteractable;
        private VRTRIXGloveDataStreaming gloveVR;
        // The flags used to determine how an object is attached to the hand.
        [Flags]
        public enum AttachmentFlags
        {
            SnapOnAttach = 1 << 0, // The object should snap to the position of the specified attachment point on the hand.
            DetachOthers = 1 << 1, // Other objects attached to this hand will be detached.
            DetachFromOtherHand = 1 << 2, // This object will be detached from the other hand.
            ParentToHand = 1 << 3, // The object will be parented to the hand.
        };

        public const AttachmentFlags defaultAttachmentFlags = AttachmentFlags.ParentToHand |
                                                              AttachmentFlags.DetachOthers |
                                                              AttachmentFlags.DetachFromOtherHand |
                                                              AttachmentFlags.SnapOnAttach;

        public struct AttachedObject
        {
            public GameObject attachedObject;
            public GameObject originalParent;
            public bool isParentedToHand;
        }
        public VRTRIXInteractable hoveringInteractable
        {
            get { return _hoveringInteractable; }
            set
            {
                if (_hoveringInteractable != value)
                {
                    if (_hoveringInteractable != null)
                    {
                        //Debug.Log("HoverEnd " + _hoveringInteractable.gameObject);
                        _hoveringInteractable.SendMessage("OnHandHoverEnd", this, SendMessageOptions.DontRequireReceiver);
                    }

                    _hoveringInteractable = value;

                    if (_hoveringInteractable != null)
                    {
                        //Debug.Log("HoverBegin " + _hoveringInteractable.gameObject);
                        _hoveringInteractable.SendMessage("OnHandHoverBegin", this, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        public VRTRIXInteractable fingertipTouchInteractable
        {
            get { return _fingertipTouchInteractable; }
            set
            {
                if (_fingertipTouchInteractable != value)
                {
                    if (_fingertipTouchInteractable != null)
                    {
                        //Debug.Log("FingertipTouchEnd " + _fingertipTouchInteractable.gameObject);
                        _fingertipTouchInteractable.SendMessage("OnFingertipTouchEnd", this, SendMessageOptions.DontRequireReceiver);
                    }

                    _fingertipTouchInteractable = value;

                    if (_fingertipTouchInteractable != null)
                    {
                        //Debug.Log("FingertipTouchBegin " + _fingertipTouchInteractable.gameObject);
                        _fingertipTouchInteractable.SendMessage("OnFingertipTouchBegin", this, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
         
        // Use this for initialization
        void Start()
        {
            overlappingColliders = new Collider[ColliderArraySize];
            indexOverlappingColliders = new Collider[ColliderArraySize];
            thumbOverlappingColliders = new Collider[ColliderArraySize];
            gloveVR = GetComponentInParent<VRTRIXGloveDataStreaming>();
        }

        // Update is called once per frame
        void Update()
        {
            GameObject attached = currentAttachedObject;
            if (attached)
            {
                attached.SendMessage("HandAttachedUpdate", this, SendMessageOptions.DontRequireReceiver);
            }

            if (hoveringInteractable)
            {
                hoveringInteractable.SendMessage("HandHoverUpdate", this, SendMessageOptions.DontRequireReceiver);
            }
            if (fingertipTouchInteractable)
            {
                fingertipTouchInteractable.SendMessage("FingertipTouchUpdate", this, SendMessageOptions.DontRequireReceiver);
            }
        }

        void LateUpdate()
        {
            ////Re-attach the controller if nothing else is attached to the hand
            //if (attachedObjects.Count == 0)
            //{
            //    AttachObject(controllerObject);
            //}
            UpdateHovering();
            UpdateFingertipTouching();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.5f, 1.0f, 0.5f, 0.9f);
            Gizmos.DrawWireSphere(hoverSphereTransform.position, hoverSphereRadius);
            Gizmos.DrawWireSphere(thumbSphereTransform.position, fingerSphereRadius);
            Gizmos.DrawWireSphere(indexSphereTransform.position, fingerSphereRadius);
        }

        //-------------------------------------------------
        void OnEnable()
        {
            // Stagger updates between hands
            float hoverUpdateBegin = ((otherHand != null) && (otherHand.GetInstanceID() < GetInstanceID())) ? (0.5f * hoverUpdateInterval) : (0.0f);
            InvokeRepeating("UpdateHovering", hoverUpdateBegin, hoverUpdateInterval);
        }


        //-------------------------------------------------
        void OnDisable()
        {
            CancelInvoke();
        }

        private void UpdateHovering()
        {
            if (hoverLocked)
                return;

            float closestDistance = float.MaxValue;
            VRTRIXInteractable closestInteractable = null;

            // Pick the closest hovering
            float flHoverRadiusScale = transform.lossyScale.x;
            float flScaledSphereRadius = hoverSphereRadius * flHoverRadiusScale;
            // if we're close to the floor, increase the radius to make things easier to pick up
            float handDiff = Mathf.Abs(transform.position.y);
            float boxMult = VRTRIXUtils.RemapNumberClamped(handDiff, 0.0f, 0.5f * flHoverRadiusScale, 5.0f, 1.0f) * flHoverRadiusScale;

            // null out old vals
            for (int i = 0; i < overlappingColliders.Length; ++i)
            {
                overlappingColliders[i] = null;
            }
            //Debug.Log(new Vector3(0, flScaledSphereRadius * boxMult - flScaledSphereRadius, 0));
            Physics.OverlapBoxNonAlloc(
                hoverSphereTransform.position - new Vector3(0, flScaledSphereRadius * boxMult - flScaledSphereRadius, 0),
                new Vector3(flScaledSphereRadius, flScaledSphereRadius * boxMult * 2.0f, flScaledSphereRadius),
                overlappingColliders,
                Quaternion.identity,
                hoverLayerMask.value
            );

            foreach (Collider collider in overlappingColliders)
            {
                if (collider == null)
                    continue;

                VRTRIXInteractable contacting = collider.GetComponentInParent<VRTRIXInteractable>();

                // Yeah, it's null, skip
                if (contacting == null)
                    continue;

                // Ignore this collider for hovering
                VRTRIXIgnoreHovering ignore = collider.GetComponent<VRTRIXIgnoreHovering>();
                if (ignore != null)
                {
                    if (ignore.onlyIgnoreHand == null || ignore.onlyIgnoreHand == this)
                    {
                        continue;
                    }
                }

                // Can't hover over the object if it's attached
                if (attachedObjects.FindIndex(l => l.attachedObject == contacting.gameObject) != -1)
                    continue;

                // Occupied by another hand, so we can't touch it
                if (otherHand && otherHand.hoveringInteractable == contacting)
                    continue;

                // Best candidate so far...
                float distance = Vector3.Distance(contacting.transform.position, hoverSphereTransform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = contacting;
                }
            }

            // Hover on this one
            hoveringInteractable = closestInteractable;
        }

        private void UpdateFingertipTouching()
        {
            //if (fingertipTouchLocked)
            //    return;
            VRTRIXInteractable indexClosestInteractable = null;
            VRTRIXInteractable thumbClosestInteractable = null;

            // Pick the closest hovering
            float flHoverRadiusScale = transform.lossyScale.x;
            float flScaledSphereRadius = fingerSphereRadius * flHoverRadiusScale;
            // if we're close to the floor, increase the radius to make things easier to pick up
            float handDiff = Mathf.Abs(transform.position.y);
            float boxMult = VRTRIXUtils.RemapNumberClamped(handDiff, 0.0f, 0.5f * flHoverRadiusScale, 5.0f, 1.0f) * flHoverRadiusScale;

            // null out old vals
            for (int i = 0; i < indexOverlappingColliders.Length; ++i)
            {
                indexOverlappingColliders[i] = null;
            }
            float closestDistance = float.MaxValue;
            Physics.OverlapBoxNonAlloc(
                indexSphereTransform.position - new Vector3(0, flScaledSphereRadius * boxMult - flScaledSphereRadius, 0),
                new Vector3(flScaledSphereRadius, flScaledSphereRadius * boxMult * 2.0f, flScaledSphereRadius),
                indexOverlappingColliders,
                Quaternion.identity,
                hoverLayerMask.value
            );

            foreach (Collider collider in indexOverlappingColliders)
            {
                if (collider == null)
                    continue;

                VRTRIXInteractable contacting = collider.GetComponentInParent<VRTRIXInteractable>();

                // Yeah, it's null, skip
                if (contacting == null)
                    continue;

                // Best candidate so far...
                float distance = Vector3.Distance(contacting.transform.position, hoverSphereTransform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    indexClosestInteractable = contacting;
                }
            }

            // null out old vals
            for (int i = 0; i < thumbOverlappingColliders.Length; ++i)
            {
                thumbOverlappingColliders[i] = null;
            }
            closestDistance = float.MaxValue;
            Physics.OverlapBoxNonAlloc(
               thumbSphereTransform.position - new Vector3(0, flScaledSphereRadius * boxMult - flScaledSphereRadius, 0),
               new Vector3(flScaledSphereRadius, flScaledSphereRadius * boxMult * 2.0f, flScaledSphereRadius),
               thumbOverlappingColliders,
               Quaternion.identity,
               hoverLayerMask.value
           );

            foreach (Collider collider in thumbOverlappingColliders)
            {
                if (collider == null)
                    continue;

                VRTRIXInteractable contacting = collider.GetComponentInParent<VRTRIXInteractable>();

                // Yeah, it's null, skip
                if (contacting == null)
                    continue;

                // Best candidate so far...
                float distance = Vector3.Distance(contacting.transform.position, hoverSphereTransform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    thumbClosestInteractable = contacting;
                }
            }

            // Hover on this one
            if (indexClosestInteractable != null && indexClosestInteractable == thumbClosestInteractable)
            {
                fingertipTouchInteractable = indexClosestInteractable;
            }
            else if(indexClosestInteractable == null || thumbClosestInteractable == null)
            {
                fingertipTouchInteractable = null;
            }
        }

        //-------------------------------------------------
        // Attach a GameObject to this GameObject
        //
        // objectToAttach - The GameObject to attach
        // flags - The flags to use for attaching the object
        // attachmentPoint - Name of the GameObject in the hierarchy of this Hand which should act as the attachment point for this GameObject
        //-------------------------------------------------
        public void AttachObject(GameObject objectToAttach, AttachmentFlags flags = defaultAttachmentFlags, string attachmentPoint = "")
        {
            //Debug.Log("AttachObject1 " + objectToAttach);

            if (flags == 0)
            {
                flags = defaultAttachmentFlags;
            }

            //Make sure top object on stack is non-null
            CleanUpAttachedObjectStack();

            //Detach the object if it is already attached so that it can get re-attached at the top of the stack
            DetachObject(objectToAttach);

            //Detach from the other hand if requested
            if (((flags & AttachmentFlags.DetachFromOtherHand) == AttachmentFlags.DetachFromOtherHand) && otherHand)
            {
                //Debug.Log("Flag: " + flags);
                otherHand.DetachObject(objectToAttach);
            }

            if ((flags & AttachmentFlags.DetachOthers) == AttachmentFlags.DetachOthers)
            {
                //Debug.Log("Flag: " + flags);
                //Detach all the objects from the stack
                while (attachedObjects.Count > 0)
                {
                    DetachObject(attachedObjects[0].attachedObject);
                }
            }

            if (currentAttachedObject)
            {
                //Debug.Log("Flag: " + flags);
                currentAttachedObject.SendMessage("OnHandFocusLost", this, SendMessageOptions.DontRequireReceiver);
            }

            AttachedObject attachedObject = new AttachedObject();
            attachedObject.attachedObject = objectToAttach;
            attachedObject.originalParent = objectToAttach.transform.parent != null ? objectToAttach.transform.parent.gameObject : null;
            //Debug.Log(attachedObject.originalParent);

            if ((flags & AttachmentFlags.ParentToHand) == AttachmentFlags.ParentToHand)
            {
                //Debug.Log("Flag: " + flags);
                //Parent the object to the hand
                objectToAttach.transform.parent = GetAttachmentTransform(attachmentPoint);
                attachedObject.isParentedToHand = true;
            }
            else
            {
                attachedObject.isParentedToHand = false;
            }
            attachedObjects.Add(attachedObject);

            if ((flags & AttachmentFlags.SnapOnAttach) == AttachmentFlags.SnapOnAttach)
            {
                //Debug.Log("Flag: " + flags);
                objectToAttach.transform.localPosition = Vector3.zero;
                objectToAttach.transform.localRotation = Quaternion.identity;
            }

            Debug.Log("AttachObject " + objectToAttach);
            objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);

            UpdateHovering();
        }


        public void AttachLongBow(GameObject objectToAttach, AttachmentFlags flags, string attachmentPoint)
        {
            //Debug.Log("AttachObject1 " + objectToAttach);
            Debug.Log("attachmentPoint: " + attachmentPoint);
            if (flags == 0)
            {
                flags = defaultAttachmentFlags;
            }

            //Make sure top object on stack is non-null
            CleanUpAttachedObjectStack();

            //Detach the object if it is already attached so that it can get re-attached at the top of the stack
            DetachObject(objectToAttach);

            //Detach from the other hand if requested
            if (((flags & AttachmentFlags.DetachFromOtherHand) == AttachmentFlags.DetachFromOtherHand) && otherHand)
            {
                Debug.Log("Flag: " + flags);
                otherHand.DetachObject(objectToAttach);
            }

            if ((flags & AttachmentFlags.DetachOthers) == AttachmentFlags.DetachOthers)
            {
                Debug.Log("Flag: " + flags);
                //Detach all the objects from the stack
                while (attachedObjects.Count > 0)
                {
                    DetachObject(attachedObjects[0].attachedObject);
                }
            }

            if (currentAttachedObject)
            {
                Debug.Log("Flag: " + flags);
                currentAttachedObject.SendMessage("OnHandFocusLost", this, SendMessageOptions.DontRequireReceiver);
            }

            AttachedObject attachedObject = new AttachedObject();
            attachedObject.attachedObject = objectToAttach;
            attachedObject.originalParent = objectToAttach.transform.parent != null ? objectToAttach.transform.parent.gameObject : null;
            Debug.Log(attachedObject.originalParent);

            if ((flags & AttachmentFlags.ParentToHand) == AttachmentFlags.ParentToHand)
            {
                Debug.Log("Flag: " + flags);
                //Parent the object to the hand
                objectToAttach.transform.parent = GetBowAttachmentTransform(attachmentPoint);
                attachedObject.isParentedToHand = true;
            }
            else
            {
                attachedObject.isParentedToHand = false;
            }
            attachedObjects.Add(attachedObject);

            if ((flags & AttachmentFlags.SnapOnAttach) == AttachmentFlags.SnapOnAttach)
            {
                Debug.Log("Flag: " + flags);
                objectToAttach.transform.localPosition = Vector3.zero;
                objectToAttach.transform.localRotation = Quaternion.identity;
            }

            Debug.Log("AttachObject " + objectToAttach);
            objectToAttach.SendMessage("OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver);

            UpdateHovering();
        }
        //-------------------------------------------------
        // Detach this GameObject from the attached object stack of this Hand
        //
        // objectToDetach - The GameObject to detach from this Hand
        //-------------------------------------------------
        public void DetachObject(GameObject objectToDetach, bool restoreOriginalParent = true)
        {
            Debug.Log("DetachObject " + objectToDetach);
            int index = attachedObjects.FindIndex(l => l.attachedObject == objectToDetach);
            if (index != -1)
            {
                Debug.Log("DetachObject " + objectToDetach);

                GameObject prevTopObject = currentAttachedObject;

                Transform parentTransform = null;
                if (attachedObjects[index].isParentedToHand)
                {
                    if (restoreOriginalParent && (attachedObjects[index].originalParent != null))
                    {
                        parentTransform = attachedObjects[index].originalParent.transform;
                    }
                    attachedObjects[index].attachedObject.transform.parent = parentTransform;
                }

                attachedObjects[index].attachedObject.SetActive(true);
                attachedObjects[index].attachedObject.SendMessage("OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver);
                attachedObjects.RemoveAt(index);

                GameObject newTopObject = currentAttachedObject;

                //Give focus to the top most object on the stack if it changed
                if (newTopObject != null && newTopObject != prevTopObject)
                {
                    newTopObject.SetActive(true);
                    newTopObject.SendMessage("OnHandFocusAcquired", this, SendMessageOptions.DontRequireReceiver);
                }
            }

            CleanUpAttachedObjectStack();
        }

        private void CleanUpAttachedObjectStack()
        {
            attachedObjects.RemoveAll(l => l.attachedObject == null);
        }

        public GameObject currentAttachedObject
        {
            get
            {
                CleanUpAttachedObjectStack();

                if (attachedObjects.Count > 0)
                {
                    return attachedObjects[attachedObjects.Count - 1].attachedObject;
                }

                return null;
            }
        }

        public Transform GetAttachmentTransform(string attachmentPoint = "")
        {
            Transform attachmentTransform = null;

            if (!string.IsNullOrEmpty(attachmentPoint))
            {
                attachmentTransform = transform.Find(attachmentPoint);
            }

            if (!attachmentTransform)
            {
                attachmentTransform = objectAttachTransform;
            }

            return attachmentTransform;
        }

        public Transform GetBowAttachmentTransform(string attachmentPoint)
        {
            Transform attachmentTransform = null;

            if (!string.IsNullOrEmpty(attachmentPoint))
            {
                attachmentTransform = transform.Find(attachmentPoint);
            }

            if (!attachmentTransform)
            {
                attachmentTransform = objectAttachTransform;
            }

            return attachmentTransform;
        }
        private List<AttachedObject> attachedObjects = new List<AttachedObject>();

        public ReadOnlyCollection<AttachedObject> AttachedObjects
        {
            get { return attachedObjects.AsReadOnly(); }
        }

        public HANDTYPE GetHandType()
        {
            return this.type;
        }


        public bool GetStandardInteractionButton()
        {
            return (gloveVR.GetGesture(this.GetHandType()) & VRTRIXGloveGesture.BUTTONGRAB) != VRTRIXGloveGesture.BUTTONINVALID;
        }

        public bool GetStandardInteractionButtonDown()
        {
            return (gloveVR.GetGesture(this.GetHandType()) & VRTRIXGloveGesture.BUTTONGRAB) != VRTRIXGloveGesture.BUTTONINVALID;
        }
        //-------------------------------------------------
        // Was the standard interaction button just released? In VR, this is a trigger press. In 2D fallback, this is a mouse left-click.
        //-------------------------------------------------
        public bool GetStandardInteractionButtonUp()
        {
            return (gloveVR.GetGesture(this.GetHandType()) & VRTRIXGloveGesture.BUTTONGRAB) != VRTRIXGloveGesture.BUTTONINVALID;
        }

        public bool GetPressButtonDown()
        {
            return (gloveVR.GetGesture(this.GetHandType()) & VRTRIXGloveGesture.BUTTONCLICK) != VRTRIXGloveGesture.BUTTONINVALID;
        }

        public bool GetTeleportButton()
        {
            return (gloveVR.GetGesture(this.GetHandType()) & VRTRIXGloveGesture.BUTTONTELEPORT) != VRTRIXGloveGesture.BUTTONINVALID;
        }

        public bool GetTeleportButtonDown()
        {
            return (gloveVR.GetGesture(this.GetHandType()) & VRTRIXGloveGesture.BUTTONTELEPORT) != VRTRIXGloveGesture.BUTTONINVALID;
        }
        //-------------------------------------------------
        // Was the standard interaction button just released? In VR, this is a trigger press. In 2D fallback, this is a mouse left-click.
        //-------------------------------------------------
        public bool GetTeleportButtonUp()
        {
            return (gloveVR.GetGesture(this.GetHandType()) & VRTRIXGloveGesture.BUTTONTELEPORT) == VRTRIXGloveGesture.BUTTONINVALID
                && (gloveVR.GetGesture(this.GetHandType()) & VRTRIXGloveGesture.BUTTONTELEPORTCANCEL) == VRTRIXGloveGesture.BUTTONINVALID;
        }


        public void vibrate()
        {
            if(this.GetHandType() == HANDTYPE.LEFT_HAND)
            {
                gloveVR.LH.VibratePeriod(100);
            }
            else if (this.GetHandType() == HANDTYPE.RIGHT_HAND)
            {
                gloveVR.RH.VibratePeriod(100);
            }
        }

        //-------------------------------------------------
        // Continue to hover over this object indefinitely, whether or not the Hand moves out of its interaction trigger volume.
        //
        // interactable - The Interactable to hover over indefinitely.
        //-------------------------------------------------
        public void HoverLock(VRTRIXInteractable interactable)
        {
            //Debug.Log("HoverLock " + interactable);
            hoverLocked = true;
            hoveringInteractable = interactable;
        }


        //-------------------------------------------------
        // Stop hovering over this object indefinitely.
        //
        // interactable - The hover-locked Interactable to stop hovering over indefinitely.
        //-------------------------------------------------
        public void HoverUnlock(VRTRIXInteractable interactable)
        {
            //Debug.Log("HoverUnlock " + interactable);
            if (hoveringInteractable == interactable)
            {
                hoverLocked = false;
            }
        }

        //-------------------------------------------------
        // Continue to hover over this object indefinitely, whether or not the Hand moves out of its interaction trigger volume.
        //
        // interactable - The Interactable to hover over indefinitely.
        //-------------------------------------------------
        public void FingertipTouchLock(VRTRIXInteractable interactable)
        {
            //Debug.Log("FingertipTouchLock " + interactable);
            fingertipTouchLocked = true;
            fingertipTouchInteractable = interactable;
        }


        //-------------------------------------------------
        // Stop hovering over this object indefinitely.
        //
        // interactable - The hover-locked Interactable to stop hovering over indefinitely.
        //-------------------------------------------------
        public void FingertipTouchUnlock(VRTRIXInteractable interactable)
        {
            //Debug.Log("FingertipTouchUnlock " + interactable);
            if (fingertipTouchInteractable == interactable)
            {
                fingertipTouchLocked = false;
            }
        }

        public Transform getThumbtipTransform()
        {
            return thumbSphereTransform;
        }

        public Transform getIndextipTransform()
        {
            return indexSphereTransform;
        }

        public Transform getWristTransform()
        {
            if(type == HANDTYPE.LEFT_HAND)
            {
                return gloveVR.GetTransform(VRTRIXBones.L_Hand);
            }
            else
            {
                return gloveVR.GetTransform(VRTRIXBones.R_Hand);
            }

        }
    }

}

