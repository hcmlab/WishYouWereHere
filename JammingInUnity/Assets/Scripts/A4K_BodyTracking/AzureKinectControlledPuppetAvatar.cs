using System.Collections.Generic;
using UnityEngine;
using System.Text;

/// <summary>
/// Component that uses BodyTrackingData provided by an AzureKinectBodyTrackingProvider to animate a Humanoid Avatar.
/// </summary>
public class AzureKinectControlledPuppetAvatar : MonoBehaviour
{
    public AzureKinectBodyTrackingProvider trackingDataProvider;
    Dictionary<JointId, Quaternion> absoluteOffsetMap;
    Animator PuppetAnimator;
    public GameObject RootPosition;
    public Transform CharacterRootTransform;
    public float OffsetY;
    public float OffsetZ;
    private static HumanBodyBones MapKinectJoint(JointId joint)
    {
        switch (joint)
        {
            case JointId.PELVIS: return HumanBodyBones.Hips;
            case JointId.SPINE_NAVEL: return HumanBodyBones.Spine;
            case JointId.SPINE_CHEST: return HumanBodyBones.Chest;
            case JointId.NECK: return HumanBodyBones.Neck;
            case JointId.HEAD: return HumanBodyBones.Head;
            case JointId.HIP_LEFT: return HumanBodyBones.LeftUpperLeg;
            case JointId.KNEE_LEFT: return HumanBodyBones.LeftLowerLeg;
            case JointId.ANKLE_LEFT: return HumanBodyBones.LeftFoot;
            case JointId.FOOT_LEFT: return HumanBodyBones.LeftToes;
            case JointId.HIP_RIGHT: return HumanBodyBones.RightUpperLeg;
            case JointId.KNEE_RIGHT: return HumanBodyBones.RightLowerLeg;
            case JointId.ANKLE_RIGHT: return HumanBodyBones.RightFoot;
            case JointId.FOOT_RIGHT: return HumanBodyBones.RightToes;
            case JointId.CLAVICLE_LEFT: return HumanBodyBones.LeftShoulder;
            case JointId.SHOULDER_LEFT: return HumanBodyBones.LeftUpperArm;
            case JointId.ELBOW_LEFT: return HumanBodyBones.LeftLowerArm;
            case JointId.WRIST_LEFT: return HumanBodyBones.LeftHand;
            //don't animate hands directly because Kinect does not have enough fidelity to drive the whole hand properly through forward kinematics
            //case JointId.THUMB_LEFT: return HumanBodyBones.LeftThumbIntermediate;
            //case JointId.HAND_LEFT: return HumanBodyBones.LeftMiddleProximal;
            //case JointId.HANDTIP_LEFT: return HumanBodyBones.LeftMiddleDistal;
            case JointId.CLAVICLE_RIGHT: return HumanBodyBones.RightShoulder;
            case JointId.SHOULDER_RIGHT: return HumanBodyBones.RightUpperArm;
            case JointId.ELBOW_RIGHT: return HumanBodyBones.RightLowerArm;
            case JointId.WRIST_RIGHT: return HumanBodyBones.RightHand;
            //don't animate hands directly because Kinect does not have enough fidelity to drive the whole hand properly through forward kinematics
            //case JointId.THUMB_RIGHT: return HumanBodyBones.RightThumbIntermediate;
            //case JointId.HAND_RIGHT: return HumanBodyBones.RightMiddleProximal;
            //case JointId.HANDTIP_RIGHT: return HumanBodyBones.RightMiddleDistal;
            default: return HumanBodyBones.LastBone;
        }
    }

    private void Awake()
    {
        PuppetAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        Transform _rootJointTransform = CharacterRootTransform;

        absoluteOffsetMap = new Dictionary<JointId, Quaternion>();
        for (int i = 0; i < (int)JointId.Count; i++)
        {
            HumanBodyBones hbb = MapKinectJoint((JointId)i);
            if (hbb != HumanBodyBones.LastBone)
            {
                Transform transform = PuppetAnimator.GetBoneTransform(hbb);
                SkeletonBone bone = GetSkeletonBone(PuppetAnimator, transform.name);
                Quaternion absOffset = bone.rotation;
                // find the absolute offset for the tpose
                while (!ReferenceEquals(transform, _rootJointTransform))
                {
                    transform = transform.parent;
                    absOffset = GetSkeletonBone(PuppetAnimator, transform.name).rotation * absOffset;
                }
                absoluteOffsetMap[(JointId)i] = absOffset;
            }
        }
    }

    private static SkeletonBone GetSkeletonBone(Animator animator, string boneName)
    {
        int count = 0;
        StringBuilder cloneName = new StringBuilder(boneName);
        cloneName.Append("(Clone)");
        foreach (SkeletonBone sb in animator.avatar.humanDescription.skeleton)
        {
            if (sb.name == boneName || sb.name == cloneName.ToString())
            {
                return animator.avatar.humanDescription.skeleton[count];
            }
            count++;
        }
        return new SkeletonBone();
    }

    // Update is called once per frame
    private void Update()
    {
        if (trackingDataProvider.Connected)
        {
            for (int j = 0; j < (int)JointId.Count; j++)
            {
                if (MapKinectJoint((JointId)j) != HumanBodyBones.LastBone && absoluteOffsetMap.ContainsKey((JointId)j))
                {
                    // get the absolute offset
                    Quaternion absOffset = absoluteOffsetMap[(JointId)j];
                    Transform finalJoint = PuppetAnimator.GetBoneTransform(MapKinectJoint((JointId)j));
                    finalJoint.rotation = absOffset * Quaternion.Inverse(absOffset) * trackingDataProvider.absoluteJointRotations[j] * absOffset;
                    finalJoint.position = trackingDataProvider.getAbsoluteJointPosition((JointId)j);
                    /*
                    if (j == 0)
                    {
                        // character root plus translation reading from the kinect, plus the offset from the script public variables
                        //NOTE: disabled because it caused extremely strong jittering of the characters z-position by ca. 10-20cm at least each frame... TODO: Investigate
                        finalJoint.position = CharacterRootTransform.position + new Vector3(RootPosition.transform.localPosition.x, RootPosition.transform.localPosition.y + OffsetY, RootPosition.transform.localPosition.z - OffsetZ);
                    }
                    */
                }
            }
        }
    }
}
