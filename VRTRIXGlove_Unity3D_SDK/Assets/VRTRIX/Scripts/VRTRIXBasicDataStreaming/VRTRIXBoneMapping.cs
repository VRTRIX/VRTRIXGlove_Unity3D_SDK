using UnityEngine;
using VRTRIX;
public class VRTRIXBoneMapping : MonoBehaviour
{
    public Transform[] MyCharacterFingers = new Transform[(int)VRTRIXBones.NumOfBones];
    public static VRTRIXBoneMapping UniqueStance;

    private void Awake()
    {
        UniqueStance = this;
    }
    void Start ()
    {
       		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public GameObject MapToVRTRIX_BoneName(string bone_name)
    {
        int bone_index = VRTRIXUtilities.GetBoneIndex(bone_name);
        return MyCharacterFingers[bone_index] ? MyCharacterFingers[bone_index].gameObject : null;
    }
}
