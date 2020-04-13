using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VRTRIX
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(VRTRIXInteractable))]
    public class VRTRIXGloveScrewObject : MonoBehaviour
    {
        //! VR environment enable flag, set to true if run the demo in VR headset
        public Vector3 rotateAxis;
        public bool IsVerticleMovement = false;

        [DrawIf("IsVerticleMovement", true)]
        public Transform ObjectToMove;

        [DrawIf("IsVerticleMovement", true)]
        public float upMaxOffset;

        [DrawIf("IsVerticleMovement", true)]
        public float downMaxOffset;

        [DrawIf("IsVerticleMovement", true)]
        public float speed;

        private Vector3 lastThumbFingertipVector;
        private Vector3 lastIndexFingertipVector;
        private bool bIsFingertipTouched;
        private Vector3 origObjectPosition;
        // Use this for initialization
        void Start()
        {
            lastThumbFingertipVector = Vector3.zero;
            lastIndexFingertipVector = Vector3.zero;
            if (IsVerticleMovement)
            {
                origObjectPosition = ObjectToMove.position;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        //-------------------------------------------------
        // Called when a Hand starts hovering over this object
        //-------------------------------------------------
        private void OnFingertipTouchBegin(VRTRIXGloveGrab hand)
        {
            bIsFingertipTouched = true;
            Debug.Log("FingertipTouchBegin " + hand.name);
            Transform thumbtip = hand.getThumbtipTransform();
            Transform indextip = hand.getIndextipTransform();
            lastThumbFingertipVector = thumbtip.position - this.transform.position;
            lastThumbFingertipVector = Vector3.ProjectOnPlane(lastThumbFingertipVector, rotateAxis);
            lastThumbFingertipVector.Normalize();


            lastIndexFingertipVector = indextip.position - this.transform.position;
            lastIndexFingertipVector = Vector3.ProjectOnPlane(lastIndexFingertipVector, rotateAxis);
            lastIndexFingertipVector.Normalize();
        }

        //-------------------------------------------------
        // Called when a Hand stops hovering over this object
        //-------------------------------------------------
        private void OnFingertipTouchEnd(VRTRIXGloveGrab hand)
        {
            bIsFingertipTouched = false;
            Debug.Log("FingertipTouchEnd " + hand.name);
        }

        //-------------------------------------------------
        // Called every Update() while a Hand is hovering over this object
        //-------------------------------------------------
        private void FingertipTouchUpdate(VRTRIXGloveGrab hand)
        {
            Transform thumbtip = hand.getThumbtipTransform();
            Transform indextip = hand.getIndextipTransform();

            Vector3 curIndexFingertipVector = indextip.position - this.transform.position;
            curIndexFingertipVector = Vector3.ProjectOnPlane(curIndexFingertipVector, rotateAxis);
            curIndexFingertipVector.Normalize();

            Vector3 curThumbFingertipVector = thumbtip.position - this.transform.position;
            curThumbFingertipVector = Vector3.ProjectOnPlane(curThumbFingertipVector, rotateAxis);
            curThumbFingertipVector.Normalize();

            double indexAngle = GetAngle(lastIndexFingertipVector, curIndexFingertipVector, rotateAxis);
            double thumbAngle = GetAngle(lastThumbFingertipVector, curThumbFingertipVector, rotateAxis);
            if (Mathf.Abs((float)indexAngle) < 2 || Mathf.Abs((float)thumbAngle) < 2 || Mathf.Abs((float)indexAngle) > 60 || Mathf.Abs((float)thumbAngle) > 60) return;

            if(thumbAngle > 0)
            {
                this.transform.RotateAround(this.transform.position, rotateAxis, (float)indexAngle * 0.5f);
                if (IsVerticleMovement)
                {
                    Vector3 newPosition = ObjectToMove.transform.position + new Vector3(0, -(float)indexAngle * 0.5f * speed / 10000f, 0);
                    if ((newPosition - origObjectPosition).y <= upMaxOffset && (newPosition - origObjectPosition).y >= downMaxOffset)
                    {
                        ObjectToMove.transform.position = newPosition;
                    }
                }
            }
            else
            {
                this.transform.RotateAround(this.transform.position, rotateAxis, (float)thumbAngle * 0.5f);
                if (IsVerticleMovement)
                {
                    Vector3 newPosition = ObjectToMove.transform.position + new Vector3(0, -(float)thumbAngle * 0.5f * speed / 10000f, 0);
                    if ((newPosition - origObjectPosition).y <= upMaxOffset && (newPosition - origObjectPosition).y >= downMaxOffset)
                    {
                        ObjectToMove.transform.position = newPosition;
                    }
                }
            }
            lastIndexFingertipVector = curIndexFingertipVector;
            lastThumbFingertipVector = curThumbFingertipVector;
        }

        /// <summary>
        /// Returns the angle between two vectos
        /// </summary>
        public static double GetAngle(Vector3 A, Vector3 B, Vector3 normal)
        {
            return System.Math.Atan2(Vector3.Dot(Vector3.Cross(A, B), normal), Vector3.Dot(A, B))* (180 / System.Math.PI);
        }
    }
}
