using UnityEngine;

namespace VRTRIX
{
    public class VRTRIXGloveGestureDetection : MonoBehaviour
    {

        [Header("GestureComponent")]
        public GameObject m_Glove;
        public GameObject m_Scissors;
        public GameObject m_Rock;
        public GameObject m_Paper;
        private VRTRIXGloveSimpleDataRead glove3D;
        // Use this for initialization
        void Start()
        {
            glove3D = m_Glove.GetComponent<VRTRIXGloveSimpleDataRead>();
        }

        // Update is called once per frame
        void Update()
        {
            if (GetScissorsButtonDown(HANDTYPE.LEFT_HAND) || GetScissorsButtonDown(HANDTYPE.RIGHT_HAND))
            {
                print("Scissors!");
                m_Scissors.GetComponent<Renderer>().materials[0].color = new Color(99f/255f, 1f, 1f, 1f);
            }
            else
            {
                m_Scissors.GetComponent<Renderer>().materials[0].color = Color.white;
            }

            if (GetRockButtonDown(HANDTYPE.LEFT_HAND) || GetRockButtonDown(HANDTYPE.RIGHT_HAND))
            {
                print("Rock!");
                m_Rock.GetComponent<Renderer>().materials[0].color = new Color(0f, 1f, 146f / 255f, 1f);
            }
            else
            {
                m_Rock.GetComponent<Renderer>().materials[0].color = Color.white;
            }

            if (GetPaperButtonDown(HANDTYPE.LEFT_HAND) || GetPaperButtonDown(HANDTYPE.RIGHT_HAND))
            //if (GetPaperButtonDown(HANDTYPE.LEFT_HAND))
            {
                print("Paper!");
                m_Paper.GetComponent<Renderer>().materials[0].color = new Color(1f, 1f, 157f / 255f, 1f);
            }
            else
            {
                m_Paper.GetComponent<Renderer>().materials[0].color = Color.white;
            }
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(0, Screen.height / 8, Screen.width / 8, Screen.height / 8), "Reset"))
            {
                glove3D.OnAlignFingers();
            }

            if (glove3D.GetReceivedStatus(HANDTYPE.LEFT_HAND) == VRTRIXGloveStatus.CLOSED && glove3D.GetReceivedStatus(HANDTYPE.RIGHT_HAND) == VRTRIXGloveStatus.CLOSED)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Connect"))
                {
                    glove3D.OnConnectGlove();
                }
            }

            if (glove3D.GetReceivedStatus(HANDTYPE.LEFT_HAND) == VRTRIXGloveStatus.NORMAL || glove3D.GetReceivedStatus(HANDTYPE.RIGHT_HAND) == VRTRIXGloveStatus.NORMAL)
            {
                if (GUI.Button(new Rect(0, 0, Screen.width / 8, Screen.height / 8), "Disconnect"))
                {
                    glove3D.OnDisconnectGlove();
                }
            }

            if (GUI.Button(new Rect(0, Screen.height / 4, Screen.width / 8, Screen.height / 8), "Hardware Calibrate"))
            {
                glove3D.OnHardwareCalibrate();
            }

            if (GUI.Button(new Rect(0, Screen.height * (3.0f / 8.0f), Screen.width / 8, Screen.height / 8), "Vibrate"))
            {
                glove3D.OnVibrate();
            }

        }

        private bool GetScissorsButtonDown(HANDTYPE tpye)
        {
            return glove3D.GetGesture(tpye) == VRTRIXGloveGesture.BUTTONTELEPORT;
        }

        private bool GetRockButtonDown(HANDTYPE tpye)
        {
            return glove3D.GetGesture(tpye) == VRTRIXGloveGesture.BUTTONGRAB;
        }

        private bool GetPaperButtonDown(HANDTYPE tpye)
        {
            return glove3D.GetGesture(tpye) == VRTRIXGloveGesture.BUTTONPAPER;
        }
    }
}
