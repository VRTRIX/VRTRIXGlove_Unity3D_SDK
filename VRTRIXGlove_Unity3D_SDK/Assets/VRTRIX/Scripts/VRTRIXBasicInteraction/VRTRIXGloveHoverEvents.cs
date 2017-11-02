using UnityEngine;
using UnityEngine.Events;

namespace VRTRIX
{
    [RequireComponent(typeof(VRTRIXInteractable))]
    public class VRTRIXGloveHoverEvents : MonoBehaviour
    {
        // Use this for initialization
        public UnityEvent onHandHoverBegin;
        public UnityEvent onHandHoverEnd;
        public UnityEvent onAttachedToHand;
        public UnityEvent onDetachedFromHand;

        //-------------------------------------------------
        private void OnHandHoverBegin()
        {
            onHandHoverBegin.Invoke();
        }


        //-------------------------------------------------
        private void OnHandHoverEnd()
        {
            onHandHoverEnd.Invoke();
        }


        //-------------------------------------------------
        //private void OnAttachedToHand(VRTRIXGloveGrab hand)
        //{
        //    onAttachedToHand.Invoke();
        //}


        ////-------------------------------------------------
        //private void OnDetachedFromHand(VRTRIXGloveGrab hand)
        //{
        //    onDetachedFromHand.Invoke();
        //}
    }
}

