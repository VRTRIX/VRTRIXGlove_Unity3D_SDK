using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRTRIX
{
    public class VRTRIXMultipleConnection : MonoBehaviour
    {
        private VRTRIXGloveDataStreaming[] gloves;
        // Use this for initialization
        void Start()
        {
            gloves = gameObject.GetComponentsInChildren<VRTRIXGloveDataStreaming>();
            foreach(VRTRIXGloveDataStreaming glove in gloves)
            {
                Debug.Log(glove.gameObject.name);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
        bool IsAllGlovesNotConnected()
        {
            foreach(VRTRIXGloveDataStreaming glove in gloves)
            {
                if(glove.GetReceivedStatus(HANDTYPE.LEFT_HAND) != VRTRIXGloveStatus.CLOSED
                    || glove.GetReceivedStatus(HANDTYPE.RIGHT_HAND) != VRTRIXGloveStatus.CLOSED)
                {
                    return false;
                }
            }
            return true;
        } 

        void OnGUI()
        {
            if (IsAllGlovesNotConnected())
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Connect"))
                {
                    foreach(VRTRIXGloveDataStreaming glove in gloves)
                    {
                        glove.OnConnectGlove();
                    }
                }
            }
            else
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Disconnect"))
                {
                    foreach(VRTRIXGloveDataStreaming glove in gloves)
                    {
                        if(glove.GetReceivedStatus(HANDTYPE.LEFT_HAND) ==VRTRIXGloveStatus.CONNECTED
                            ||glove.GetReceivedStatus(HANDTYPE.RIGHT_HAND) == VRTRIXGloveStatus.CONNECTED)
                        {
                            glove.OnDisconnectGlove();
                        }
                    }
                }
                if (GUI.Button(new Rect(0, Screen.height / 8, Screen.width / 8, Screen.height / 8), "Reset"))
                {
                    foreach(VRTRIXGloveDataStreaming glove in gloves)
                    {
                        if(glove.GetReceivedStatus(HANDTYPE.LEFT_HAND) ==VRTRIXGloveStatus.CONNECTED
                            ||glove.GetReceivedStatus(HANDTYPE.RIGHT_HAND) == VRTRIXGloveStatus.CONNECTED)
                        {
                            glove.OnAlignFingers(HANDTYPE.BOTH_HAND);
                        }
                    }
                }
                if (GUI.Button(new Rect(0, Screen.height * (2.0f / 8.0f), Screen.width / 8, Screen.height / 8), "Vibrate"))
                {
                    foreach(VRTRIXGloveDataStreaming glove in gloves)
                    {
                        if(glove.GetReceivedStatus(HANDTYPE.LEFT_HAND) ==VRTRIXGloveStatus.CONNECTED
                            ||glove.GetReceivedStatus(HANDTYPE.RIGHT_HAND) == VRTRIXGloveStatus.CONNECTED)
                        {
                            glove.OnVibrate(HANDTYPE.BOTH_HAND);
                        }
                    }
                }
            }
        }
    }
}
