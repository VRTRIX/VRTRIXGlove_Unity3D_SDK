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
        private VRTRIXGloveDataStreaming glove3D;
        // Use this for initialization
        void Start()
        {
            glove3D = m_Glove.GetComponent<VRTRIXGloveDataStreaming>();
        }

        // Update is called once per frame
        void Update()
        {
            if (GetScissorsButtonDown(HANDTYPE.LEFT_HAND) || GetScissorsButtonDown(HANDTYPE.RIGHT_HAND))
            {
                //print("Scissors!");
                m_Scissors.GetComponentInChildren<Renderer>().materials[0].color = new Color(99f/255f, 1f, 1f, 1f);
            }
            else
            {
                m_Scissors.GetComponentInChildren<Renderer>().materials[0].color = Color.white;
            }

            if (GetRockButtonDown(HANDTYPE.LEFT_HAND) || GetRockButtonDown(HANDTYPE.RIGHT_HAND))
            {
                //print("Rock!");
                m_Rock.GetComponentInChildren<Renderer>().materials[0].color = new Color(0f, 1f, 146f / 255f, 1f);
            }
            else
            {
                m_Rock.GetComponentInChildren<Renderer>().materials[0].color = Color.white;
            }

            if (GetPaperButtonDown(HANDTYPE.LEFT_HAND) || GetPaperButtonDown(HANDTYPE.RIGHT_HAND))
            {
                //print("Paper!");
                m_Paper.GetComponentInChildren<Renderer>().materials[0].color = new Color(1f, 1f, 157f / 255f, 1f);
            }
            else
            {
                m_Paper.GetComponentInChildren<Renderer>().materials[0].color = Color.white;
            }
        }

        private bool GetScissorsButtonDown(HANDTYPE type)
        {
            return (glove3D.GetGesture(type) & VRTRIXGloveGesture.BUTTONSCISSOR) != VRTRIXGloveGesture.BUTTONINVALID;
        }

        private bool GetRockButtonDown(HANDTYPE type)
        {
            return (glove3D.GetGesture(type) & VRTRIXGloveGesture.BUTTONROCK) != VRTRIXGloveGesture.BUTTONINVALID;
        }

        private bool GetPaperButtonDown(HANDTYPE type)
        {
            return (glove3D.GetGesture(type) & VRTRIXGloveGesture.BUTTONPAPER) != VRTRIXGloveGesture.BUTTONINVALID;
        }
    }
}
