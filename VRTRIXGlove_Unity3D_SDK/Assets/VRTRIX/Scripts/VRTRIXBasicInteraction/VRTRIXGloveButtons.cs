//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Demonstrates how to create simple interactable buttons with VRTRIX
//          Data Glove.
//
//=============================================================================
using UnityEngine;
using System.Collections.Generic;

namespace VRTRIX
{
    public class VRTRIXGloveButtons : MonoBehaviour {
        private GameObject[] throwable;
        private Dictionary<GameObject, Vector3> throwable_transform;
        private Dictionary<GameObject, Quaternion> throwable_rotation;
        public Texture emptytexture;
        public Texture logotexture;

        void Start()
        {
            throwable = GameObject.FindGameObjectsWithTag("Throwable");
            throwable_transform = new Dictionary<GameObject, Vector3>();
            throwable_rotation = new Dictionary<GameObject, Quaternion>();
            for (int i = 0; i < throwable.Length; i++)
            {
                throwable_transform.Add(throwable[i], throwable[i].transform.position);
                throwable_rotation.Add(throwable[i], throwable[i].transform.rotation);
                //print(throwable[i]);
            }
        }

        public void ResetScene()
        {
            for (int i = 0; i < throwable.Length; i++)
            {
                Vector3 position;
                Quaternion rotation;
                if (throwable_transform.TryGetValue(throwable[i], out position))
                {
                    throwable[i].transform.position = position;
                }
                if (throwable_rotation.TryGetValue(throwable[i], out rotation))
                {
                    throwable[i].transform.rotation = rotation;
                }
                Rigidbody rb = throwable[i].GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                //print(throwable[i]);
            }
        }

        public void Vibrate(VRTRIXGloveGrab hand)
        {
            hand.vibrate();
        }


        public void HideLogo()
        {
            GameObject wall = GameObject.Find("StickyWall");
            Renderer rend = wall.GetComponent<Renderer>();
            rend.material.mainTexture = emptytexture;
        }

        public void ShowLogo()
        {
            GameObject wall = GameObject.Find("StickyWall");
            Renderer rend = wall.GetComponent<Renderer>();
            rend.material.mainTexture = logotexture;
        }

    }
}
