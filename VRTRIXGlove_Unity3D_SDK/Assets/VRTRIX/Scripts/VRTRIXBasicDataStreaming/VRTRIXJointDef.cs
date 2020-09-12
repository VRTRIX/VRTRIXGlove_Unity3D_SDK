//============= Copyright (c) VRTRIX INC, All rights reserved. ================
//
// Purpose: Utility file, including enums for glove running mode, hand bone 
//          definitions etc.
//
//=============================================================================
using System;

namespace VRTRIX
{
    //! Hand joints enum.
    /*! Enum of joints for both hands. */
    public enum VRTRIXBones
    {
        R_Hand = 0,
        R_Thumb_1 = 1,
        R_Thumb_2 = 2,
        R_Thumb_3 = 3,
        R_Index_1 = 4,
        R_Index_2 = 5,
        R_Index_3 = 6,
        R_Middle_1 = 7,
        R_Middle_2 = 8,
        R_Middle_3 = 9,
        R_Ring_1 = 10,
        R_Ring_2 = 11,
        R_Ring_3 = 12,
        R_Pinky_1 = 13,
        R_Pinky_2 = 14,
        R_Pinky_3 = 15,

        L_Hand = 16,
        L_Thumb_1 = 17,
        L_Thumb_2 = 18,
        L_Thumb_3 = 19,
        L_Index_1 = 20,
        L_Index_2 = 21,
        L_Index_3 = 22,
        L_Middle_1 = 23,
        L_Middle_2 = 24,
        L_Middle_3 = 25,
        L_Ring_1 = 26,
        L_Ring_2 = 27,
        L_Ring_3 = 28,
        L_Pinky_1 = 29,
        L_Pinky_2 = 30,
        L_Pinky_3 = 31,

        R_Arm = 32,
        L_Arm = 33,
        NumOfBones = 34
    }
    //! Hand joints enum.
    /*! Enum of joints for single hand. */
    public enum JOINT
    {
        Wrist_Joint,
        Thumb_Proximal,
        Thumb_Intermediate,
        Thumb_Distal,
        Index_Proximal,
        Index_Intermediate,
        Index_Distal,
        Middle_Proximal,
        Middle_Intermediate,
        Middle_Distal,
        Ring_Proximal,
        Ring_Intermediate,
        Ring_Distal,
        Pinky_Proximal,
        Pinky_Intermediate,
        Pinky_Distal,
        Joint_MAX = 16
    };
    //! Bone utility functions class
    public class VRTRIXJointDef
    {
        //! Get current bone name for specific bone ID
        /*! 
         * \param id id of bone.
         * \return current bone name for specific bone ID.
         */
        public static string GetBoneName(int id)
        {
            return Enum.GetName(typeof(VRTRIXBones), (VRTRIXBones)id);
        }

        //! Get current bone index for specific bone name
        /*! 
         * \param name Bone name.
         * \return current bone index for specific bone name.
         */
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
