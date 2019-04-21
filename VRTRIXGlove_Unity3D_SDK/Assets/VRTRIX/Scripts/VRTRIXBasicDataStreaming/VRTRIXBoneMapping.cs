using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRTRIXBoneMapping : MonoBehaviour
{
    public Transform[] MyCharacterFingers = new Transform[37];

    [HideInInspector]
    public Quaternion[] TPoseStateJoints;

    [HideInInspector]
    public Quaternion[] TPoseStateParentJoints;

    public static VRTRIXBoneMapping UniqueStance;

    private void Awake()
    {
        TPoseStateJoints = new Quaternion[37];
        TPoseStateParentJoints = new Quaternion[37];
        getTPoseStatePreJoints_trans();
        getTPoseStatePreParentJoints_trans();
        UniqueStance = this;
    }
    void Start ()
    {
       		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
    void getTPoseStatePreJoints_trans()
    {
        
        for (int i = 0; i < MyCharacterFingers.Length; i++)
        {
            if (MyCharacterFingers[i] == null)
            {
                continue;
            }
            else
            {
                TPoseStateJoints[i] = MyCharacterFingers[i].rotation;
                
            }
        }
    }

    void getTPoseStatePreParentJoints_trans()
    {
        for (int i = 0; i < MyCharacterFingers.Length; i++)
        {
            if (MyCharacterFingers[i] == null)
            {
                Debug.Log(" TPoseStateParentJoints  null  " + i);
                continue;
            }
            else
            {
                TPoseStateParentJoints[i] = MyCharacterFingers[i].parent.rotation;
            }
            

        }
    }
    public GameObject MapToVRTRIX_BoneName(string bone_name)
    {
        switch (bone_name)
        {
            case "R_Arm":
                {
                    return MyCharacterFingers[1] ? MyCharacterFingers[1].gameObject : null;
                }
            case "R_Forearm":
                {
                    return MyCharacterFingers[2] ? MyCharacterFingers[2].gameObject : null;
                }
            case "R_Hand":
                {
                    return MyCharacterFingers[3] ? MyCharacterFingers[3].gameObject : null;
                }
            case "R_Thumb_1":
                {
                    return MyCharacterFingers[4] ? MyCharacterFingers[4].gameObject : null;
                }
            case "R_Thumb_2":
                {
                    return MyCharacterFingers[5] ? MyCharacterFingers[5].gameObject : null;
                }
            case "R_Thumb_3":
                {
                    return MyCharacterFingers[6] ? MyCharacterFingers[6].gameObject : null;
                }
            case "R_Index_1":
                {
                    return MyCharacterFingers[7] ? MyCharacterFingers[7].gameObject : null;
                }
            case "R_Index_2":
                {
                    return MyCharacterFingers[8] ? MyCharacterFingers[8].gameObject : null;
                }
            case "R_Index_3":
                {
                    return MyCharacterFingers[9] ? MyCharacterFingers[9].gameObject : null;
                }
            case "R_Middle_1":
                {
                    return MyCharacterFingers[10] ? MyCharacterFingers[10].gameObject : null;
                }
            case "R_Middle_2":
                {
                    return MyCharacterFingers[11] ? MyCharacterFingers[11].gameObject : null;
                }
            case "R_Middle_3":
                {
                    return MyCharacterFingers[12] ? MyCharacterFingers[12].gameObject : null;
                }
            case "R_Ring_1":
                {
                    return MyCharacterFingers[13] ? MyCharacterFingers[13].gameObject : null;
                }
            case "R_Ring_2":
                {
                    return MyCharacterFingers[14] ? MyCharacterFingers[14].gameObject : null;
                }
            case "R_Ring_3":
                {
                    return MyCharacterFingers[15] ? MyCharacterFingers[15].gameObject : null;
                }
            case "R_Pinky_1":
                {
                    return MyCharacterFingers[16] ? MyCharacterFingers[16].gameObject : null;
                }
            case "R_Pinky_2":
                {
                    return MyCharacterFingers[17] ? MyCharacterFingers[17].gameObject : null;
                }
            case "R_Pinky_3":
                {
                    return MyCharacterFingers[18] ? MyCharacterFingers[18].gameObject : null;
                }
            case "L_Arm":
                {
                    return MyCharacterFingers[19] ? MyCharacterFingers[19].gameObject : null;
                }
            case "L_Forearm":
                {
                    return MyCharacterFingers[20] ? MyCharacterFingers[20].gameObject : null;
                }
            case "L_Hand":
                {
                    return MyCharacterFingers[21] ? MyCharacterFingers[21].gameObject : null;
                }
            case "L_Thumb_1":
                {
                    return MyCharacterFingers[22] ? MyCharacterFingers[22].gameObject : null;
                }
            case "L_Thumb_2":
                {
                    return MyCharacterFingers[23] ? MyCharacterFingers[23].gameObject : null;
                }
            case "L_Thumb_3":
                {
                    return MyCharacterFingers[24] ? MyCharacterFingers[24].gameObject : null;
                }
            case "L_Index_1":
                {
                    return MyCharacterFingers[25] ? MyCharacterFingers[25].gameObject : null;
                }
            case "L_Index_2":
                {
                    return MyCharacterFingers[26] ? MyCharacterFingers[26].gameObject : null;
                }
            case "L_Index_3":
                {
                    return MyCharacterFingers[27] ? MyCharacterFingers[27].gameObject : null;
                }
            case "L_Middle_1":
                {
                    return MyCharacterFingers[28] ? MyCharacterFingers[28].gameObject : null;
                }
            case "L_Middle_2":
                {
                    return MyCharacterFingers[29] ? MyCharacterFingers[29].gameObject : null;
                }
            case "L_Middle_3":
                {
                    return MyCharacterFingers[30] ? MyCharacterFingers[30].gameObject : null;
                }
            case "L_Ring_1":
                {
                    return MyCharacterFingers[31] ? MyCharacterFingers[31].gameObject : null;
                }
            case "L_Ring_2":
                {
                    return MyCharacterFingers[32] ? MyCharacterFingers[32].gameObject : null;
                }
            case "L_Ring_3":
                {
                    return MyCharacterFingers[33] ? MyCharacterFingers[33].gameObject : null;
                }
            case "L_Pinky_1":
                {
                    return MyCharacterFingers[34] ? MyCharacterFingers[34].gameObject : null;
                }
            case "L_Pinky_2":
                {
                    return MyCharacterFingers[35] ? MyCharacterFingers[35].gameObject : null;
                }
            case "L_Pinky_3":
                {
                    return MyCharacterFingers[36] ? MyCharacterFingers[36].gameObject : null;
                }
            default:
                {
                    return null;
                }

        }

    }
}
