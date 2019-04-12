//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Utility file, including enums for glove running mode, hand bone 
//          definitions etc.
//
//=============================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VRTRIX
{
    public enum VRTRIXGloveRunningMode
    {
        LEFT,
        RIGHT,
        PAIR,
        NONE
    };
    public enum VRTRIXBones
    {
        R_Arm = 1,
        R_Forearm = 2,
        R_Hand = 3,
        R_Thumb_1 = 4,
        R_Thumb_2 = 5,
        R_Thumb_3 = 6,
        R_Index_1 = 7,
        R_Index_2 = 8,
        R_Index_3 = 9,
        R_Middle_1 = 10,
        R_Middle_2 = 11,
        R_Middle_3 = 12,
        R_Ring_1 = 13,
        R_Ring_2 = 14,
        R_Ring_3 = 15,
        R_Pinky_1 = 16,
        R_Pinky_2 = 17,
        R_Pinky_3 = 18,

        L_Arm = 19,
        L_Forearm = 20,
        L_Hand = 21,
        L_Thumb_1 = 22,
        L_Thumb_2 = 23,
        L_Thumb_3 = 24,
        L_Index_1 = 25,
        L_Index_2 = 26,
        L_Index_3 = 27,
        L_Middle_1 = 28,
        L_Middle_2 = 29,
        L_Middle_3 = 30,
        L_Ring_1 = 31,
        L_Ring_2 = 32,
        L_Ring_3 = 33,
        L_Pinky_1 = 34,
        L_Pinky_2 = 35,
        L_Pinky_3 = 36,
        NumOfBones = 37
    }



    public class VRTRIXUtilities
    {

        public static string GetBoneName(int id)
        {
            return Enum.GetName(typeof(VRTRIXBones), (VRTRIXBones)id);
        }

        public static int GetBoneIndex(string name)
        {
            for (int i = 0; i < (int)VRTRIXBones.NumOfBones; ++i)
            {
                if (GetBoneName(i) == name)
                {
                    return i;
                }
            }

            return -1;
        }

    }
}
