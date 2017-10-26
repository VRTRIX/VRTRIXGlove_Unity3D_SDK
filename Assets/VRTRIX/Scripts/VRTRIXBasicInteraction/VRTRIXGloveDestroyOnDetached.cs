using UnityEngine;

namespace VRTRIX
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(VRTRIXInteractable))]
    public class VRTRIXGloveDestroyOnDetached : MonoBehaviour
    {
        //-------------------------------------------------
        private void OnDetachedFromHand(VRTRIXGloveGrab hand)
        {
            Destroy(gameObject);
        }
    }
}