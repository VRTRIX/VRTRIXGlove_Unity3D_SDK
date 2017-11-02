//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Examples to creat a teleport area (locke & unlocked).
//
//=============================================================================
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VRTRIX
{
    //-------------------------------------------------------------------------
    public class VRTRIXGloveTelportArea : VRTRIXTeleportMarkerBase
    {
        //Public properties
        public Bounds meshBounds { get; private set; }

        //Private data
        private MeshRenderer areaMesh;
        private int tintColorId = 0;
        private Color visibleTintColor = Color.clear;
        private Color highlightedTintColor = Color.clear;
        private Color lockedTintColor = Color.clear;
        private bool highlighted = false;

        //-------------------------------------------------
        public void Awake()
        {
            areaMesh = GetComponent<MeshRenderer>();

            tintColorId = Shader.PropertyToID("_TintColor");

            CalculateBounds();
        }


        //-------------------------------------------------
        public void Start()
        {
            visibleTintColor = VRTRIXGloveTeleport.instance.areaVisibleMaterial.GetColor(tintColorId);
            highlightedTintColor = VRTRIXGloveTeleport.instance.areaHighlightedMaterial.GetColor(tintColorId);
            lockedTintColor = VRTRIXGloveTeleport.instance.areaLockedMaterial.GetColor(tintColorId);
        }


        //-------------------------------------------------
        public override bool ShouldActivate(Vector3 playerPosition)
        {
            return true;
        }


        //-------------------------------------------------
        public override bool ShouldMovePlayer()
        {
            return true;
        }


        //-------------------------------------------------
        public override void Highlight(bool highlight)
        {
            if (!locked)
            {
                highlighted = highlight;

                if (highlight)
                {
                    areaMesh.material = VRTRIXGloveTeleport.instance.areaHighlightedMaterial;
                }
                else
                {
                    areaMesh.material = VRTRIXGloveTeleport.instance.areaVisibleMaterial;
                }
            }
        }


        //-------------------------------------------------
        public override void SetAlpha(float tintAlpha, float alphaPercent)
        {
            Color tintedColor = GetTintColor();
            tintedColor.a *= alphaPercent;
            areaMesh.material.SetColor(tintColorId, tintedColor);
        }


        //-------------------------------------------------
        public override void UpdateVisuals()
        {
            if (locked)
            {
                areaMesh.material = VRTRIXGloveTeleport.instance.areaLockedMaterial;
            }
            else
            {
                areaMesh.material = VRTRIXGloveTeleport.instance.areaVisibleMaterial;
            }
        }


        //-------------------------------------------------
        public void UpdateVisualsInEditor()
        {
            areaMesh = GetComponent<MeshRenderer>();

            if (locked)
            {
                areaMesh.sharedMaterial = VRTRIXGloveTeleport.instance.areaLockedMaterial;
            }
            else
            {
                areaMesh.sharedMaterial = VRTRIXGloveTeleport.instance.areaVisibleMaterial;
            }
        }


        //-------------------------------------------------
        private bool CalculateBounds()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                return false;
            }

            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                return false;
            }

            meshBounds = mesh.bounds;
            return true;
        }


        //-------------------------------------------------
        private Color GetTintColor()
        {
            if (locked)
            {
                return lockedTintColor;
            }
            else
            {
                if (highlighted)
                {
                    return highlightedTintColor;
                }
                else
                {
                    return visibleTintColor;
                }
            }
        }
    }


#if UNITY_EDITOR
    //-------------------------------------------------------------------------
    [CustomEditor(typeof(VRTRIXGloveTelportArea))]
    public class TeleportAreaEditor : Editor
    {
        //-------------------------------------------------
        void OnEnable()
        {
            if (Selection.activeTransform != null)
            {
                VRTRIXGloveTelportArea teleportArea = Selection.activeTransform.GetComponent<VRTRIXGloveTelportArea>();
                if (teleportArea != null)
                {
                    teleportArea.UpdateVisualsInEditor();
                }
            }
        }


        //-------------------------------------------------
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (Selection.activeTransform != null)
            {
                VRTRIXGloveTelportArea teleportArea = Selection.activeTransform.GetComponent<VRTRIXGloveTelportArea>();
                if (GUI.changed && teleportArea != null)
                {
                    teleportArea.UpdateVisualsInEditor();
                }
            }
        }
    }
#endif
}

