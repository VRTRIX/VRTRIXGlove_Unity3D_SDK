//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: One demo player serves every glove model. The VRTRIX Data Glove
//          Server starts it with "-vrtrix-hardware PRO" or
//          "-vrtrix-hardware PRO11", and Apply sets that model's inspector
//          values before anything reads them (call it from Awake). The demo
//          sends these values to the server when the gloves connect, so they
//          must match the model: they are the values of the PRO and PRO11
//          demo players shipped up to server 2.2.0, where the two models
//          differ only in the hardware version, the finger spacing and the
//          right thumb's proximal offset. A player started without the
//          argument (by hand, or by an older server) takes the PRO values,
//          like the PRO players before; in the Unity editor the inspector
//          values stay.
//
//=============================================================================
using System;
using UnityEngine;

namespace VRTRIX
{
    public static class VRTRIXDemoHardware
    {
        //! Command line argument naming the glove model, followed by PRO or PRO11.
        public const string Argument = "-vrtrix-hardware";

        //! The model named on the command line in upper case, or null without the argument.
        public static string RequestedModel()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; ++i)
            {
                if (string.Equals(args[i], Argument, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1].Trim().ToUpperInvariant();
                }
            }
            return null;
        }

        //! Sets the requested model's inspector values on glove (PRO in a player without the argument); unknown models change nothing.
        public static void Apply(VRTRIXGloveDataStreaming glove)
        {
            string model = RequestedModel();
            if (model == null)
            {
                if (Application.isEditor) return;
                model = "PRO";
            }

            if (glove.thumb_offset_R == null || glove.thumb_offset_R.Length < 3)
            {
                glove.thumb_offset_R = new Vector3[3];
            }
            if (model == "PRO")
            {
                glove.version = GLOVEVERSION.PRO;
                glove.finger_spacing = 5.0;
                glove.thumb_offset_R[0] = new Vector3(0f, 0f, -20f);
            }
            else if (model == "PRO11")
            {
                glove.version = GLOVEVERSION.PRO11;
                glove.finger_spacing = 2.0;
                glove.thumb_offset_R[0] = Vector3.zero;
            }
            else
            {
                Debug.LogWarning("[VRTRIX] Unknown " + Argument + " value '" + model + "', the inspector values stay.");
                return;
            }
            Debug.Log("[VRTRIX] Glove model: " + model + (RequestedModel() == null ? " (no " + Argument + ")" : ""));
        }
    }
}
