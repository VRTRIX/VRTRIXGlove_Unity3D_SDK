using UnityEngine.Events;

namespace VRTRIX
{
    public static class VRTRIXCustomEvents
    {

        [System.Serializable]
        public class VRTRIXEventSingleFloat : UnityEvent<float>
        {
        }


        //-------------------------------------------------
        [System.Serializable]
        public class VRTRIXEventHand : UnityEvent<VRTRIXGloveGrab>
        {
        }
    }
}