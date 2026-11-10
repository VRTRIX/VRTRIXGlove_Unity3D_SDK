//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Builds the demo players shipped in the VRTRIX Data Glove Server's
//          demo folder, one Windows x64 player per scene, from the command line:
//
//   Unity.exe -batchmode -quit -projectPath <project>
//             -executeMethod VRTRIX.VRTRIXDemoBuild.BuildAll -vrtrixOut <folder>
//
//          Each player goes to <folder>\<Name>\<Name>.exe, without Virtual
//          Reality Supported: the wrist positions come from the server's
//          frames. The project's VR settings are restored afterwards.
//          DumpScenes logs every VRTRIXGloveDataStreaming inspector value of
//          the built scenes.
//
//=============================================================================
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditorInternal.VR;
using UnityEngine;

namespace VRTRIX
{
    public static class VRTRIXDemoBuild
    {
        private struct DemoPlayer
        {
            public string Name;
            public string Scene;
            public bool VR;
            public DemoPlayer(string name, string scene, bool vr) { Name = name; Scene = scene; VR = vr; }
        }

        // The wrist tracking demo of data protocols 2 and 3 (the extra tracker shows in 3 only).
        private static readonly DemoPlayer[] Players =
        {
            new DemoPlayer("VRTRIXGloveWristTracking", "Assets/VRTRIX/Scenes/VRTRIXGloveDataStreaming.unity", false),
        };

        private static readonly string[] VRDevices = { "OpenVR", "Oculus" };

        public static void BuildAll()
        {
            string outRoot = CommandLineValue("-vrtrixOut");
            if (string.IsNullOrEmpty(outRoot))
            {
                Debug.LogError("[VRTRIXDemoBuild] -vrtrixOut <folder> is missing.");
                EditorApplication.Exit(2);
                return;
            }

            DumpScenes();

            BuildTargetGroup group = BuildTargetGroup.Standalone;
            bool vrBefore = VREditor.GetVREnabledOnTargetGroup(group);
            string[] devicesBefore = VREditor.GetVREnabledDevicesOnTargetGroup(group);
            Debug.Log("[VRTRIXDemoBuild] Project VR before: " + vrBefore + " [" + string.Join(", ", devicesBefore) + "]");

            int failures = 0;
            try
            {
                foreach (DemoPlayer player in Players)
                {
                    VREditor.SetVREnabledOnTargetGroup(group, player.VR);
                    VREditor.SetVREnabledDevicesOnTargetGroup(group, player.VR ? VRDevices : new string[0]);

                    string exe = Path.Combine(Path.Combine(outRoot, player.Name), player.Name + ".exe");
                    BuildPlayerOptions options = new BuildPlayerOptions();
                    options.scenes = new[] { player.Scene };
                    options.locationPathName = exe;
                    options.target = BuildTarget.StandaloneWindows64;
                    options.targetGroup = group;
                    options.options = BuildOptions.None;

                    BuildReport report = BuildPipeline.BuildPlayer(options);
                    BuildResult result = report.summary.result;
                    Debug.Log("[VRTRIXDemoBuild] " + player.Name + ": " + result + ", VR " + player.VR +
                              ", " + report.summary.totalSize + " bytes, " + report.summary.totalErrors + " errors -> " + exe);
                    // Unity 2018.1 can say Succeeded with errors and leave the old player in place
                    // (an IOException in its staging area).
                    if (result != BuildResult.Succeeded || report.summary.totalErrors > 0) failures++;
                }
            }
            finally
            {
                VREditor.SetVREnabledOnTargetGroup(group, vrBefore);
                VREditor.SetVREnabledDevicesOnTargetGroup(group, devicesBefore);
                AssetDatabase.SaveAssets();
            }

            if (failures > 0)
            {
                Debug.LogError("[VRTRIXDemoBuild] " + failures + " player(s) failed.");
                EditorApplication.Exit(1);
            }
        }

        public static void DumpScenes()
        {
            foreach (DemoPlayer player in Players)
            {
                EditorSceneManager.OpenScene(player.Scene, OpenSceneMode.Single);
                VRTRIXGloveDataStreaming[] gloves = UnityEngine.Object.FindObjectsOfType<VRTRIXGloveDataStreaming>();
                Debug.Log("[VRTRIXDemoBuild] " + player.Scene + ": " + gloves.Length + " VRTRIXGloveDataStreaming");
                foreach (VRTRIXGloveDataStreaming glove in gloves)
                {
                    SerializedObject so = new SerializedObject(glove);
                    SerializedProperty p = so.GetIterator();
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.Append("[VRTRIXDemoBuild]   ").Append(PathOf(glove.transform)).Append(':');
                    bool enter = true;
                    while (p.NextVisible(enter))
                    {
                        enter = p.propertyType == SerializedPropertyType.Generic;
                        sb.Append("\n      ").Append(p.propertyPath).Append(" = ").Append(Describe(p));
                    }
                    Debug.Log(sb.ToString());
                }
                // The UI's dropdowns, with the option texts (the hardware version dropdown).
                foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                {
                    foreach (UnityEngine.UI.Dropdown dropdown in root.GetComponentsInChildren<UnityEngine.UI.Dropdown>(true))
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        sb.Append("[VRTRIXDemoBuild]   dropdown ").Append(PathOf(dropdown.transform))
                          .Append(" value=").Append(dropdown.value).Append(" options=");
                        for (int i = 0; i < dropdown.options.Count; ++i)
                        {
                            sb.Append(i == 0 ? "" : " | ").Append(i).Append(':').Append(dropdown.options[i].text);
                        }
                        sb.Append(" listeners=").Append(dropdown.onValueChanged.GetPersistentEventCount());
                        for (int i = 0; i < dropdown.onValueChanged.GetPersistentEventCount(); ++i)
                        {
                            sb.Append(' ').Append(dropdown.onValueChanged.GetPersistentMethodName(i));
                        }
                        Debug.Log(sb.ToString());
                    }
                }
            }
        }

        private static string Describe(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Boolean: return p.boolValue.ToString();
                case SerializedPropertyType.Integer: return p.intValue.ToString();
                case SerializedPropertyType.Float: return p.doubleValue.ToString("R");
                case SerializedPropertyType.String: return "\"" + p.stringValue + "\"";
                case SerializedPropertyType.Enum: return p.enumValueIndex.ToString();
                case SerializedPropertyType.Vector3: return p.vector3Value.ToString("F3");
                case SerializedPropertyType.ObjectReference:
                    if (p.objectReferenceValue == null) return "null";
                    if (p.objectReferenceValue is GameObject) return PathOf(((GameObject)p.objectReferenceValue).transform) + " (GameObject)";
                    return p.objectReferenceValue.name + " (" + p.objectReferenceValue.GetType().Name + ")";
                default: return "<" + p.propertyType + ">";
            }
        }

        private static string PathOf(Transform t)
        {
            string path = t.name;
            for (Transform parent = t.parent; parent != null; parent = parent.parent) path = parent.name + "/" + path;
            return path;
        }

        private static string CommandLineValue(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; ++i)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            }
            return null;
        }
    }
}
