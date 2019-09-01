using UnityEngine;
using VRTRIX;

//!  Bone mapping class. 
/*!
    A class to map bone names of custom model with VRTRIX bone setup.
*/
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
	void Update ()
    {
		
	}
    
    //! Get custom model joint as gameobject according to bone name specified.
    /*! 
     * \param bone_name Given VRTRIX bone name.
     * \return joint on the custom model as gameobject.
     */
    public GameObject MapToVRTRIX_BoneName(string bone_name)
    {
        int bone_index = VRTRIXUtilities.GetBoneIndex(bone_name);
        return MyCharacterFingers[bone_index] ? MyCharacterFingers[bone_index].gameObject : null;
    }
}
