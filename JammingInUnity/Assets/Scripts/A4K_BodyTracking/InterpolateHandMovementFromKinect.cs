using UnityEngine;

public class InterpolateHandMovementFromKinect : MonoBehaviour
{
    public AzureKinectBodyTrackingProvider trackingDataProvider;
    Animator PuppetAnimator;

    [Range(0f, 120f)]
    public float maxFingerJointRotation = 60.0f;

    [Range(0.2f, 2.5f)]
    public float handFlexionAmplification = 1.6f;

    private void Awake()
    {

        PuppetAnimator = GetComponent<Animator>();
    }

    public void Update()
    {
        handleLeftHandOpenClose();
        handleRightHandOpenClose();
    }

    private void handleLeftHandOpenClose()
    {
        HumanBodyBones[] bones = { HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftLittleProximal,
                                   HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftLittleIntermediate };
        fakeFingerTwoBoneIK(trackingDataProvider.body.WRIST_LEFT, trackingDataProvider.body.HAND_LEFT, trackingDataProvider.body.HANDTIP_LEFT, bones, true);
    }

    private void handleRightHandOpenClose()
    {
        HumanBodyBones[] bones = { HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightIndexProximal, HumanBodyBones.RightRingProximal, HumanBodyBones.RightLittleProximal,
                                   HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightLittleIntermediate };
        fakeFingerTwoBoneIK(trackingDataProvider.body.WRIST_RIGHT, trackingDataProvider.body.HAND_RIGHT, trackingDataProvider.body.HANDTIP_RIGHT, bones);
    }

    /// <summary>
    ///  Simulates inverse Kinematics for fingers based on the positions of three joints (which should be connected in the skeleton).
    ///  Takes the angle between the two vectors (base to middle) and (middle to tip) and applies this angle to all provided bones (as local rotation around the z-Axis).
    ///  This (sort of) simulates opening and closing of the hands when applied to the finger proxmal and intermediate bones.
    /// </summary>
    /// <param name="unityBonesToRotate">The bones of the animator of this script that will be rotated</param>
    private void fakeFingerTwoBoneIK(AzureKinectJoint baseJoint, AzureKinectJoint middleJoint, AzureKinectJoint tipJoint, HumanBodyBones[] unityBonesToRotate, bool flipRotations = false)
    {
        Vector3 baseJointPos = baseJoint.getPosition();
        Vector3 middleJointPos = middleJoint.getPosition();
        Vector3 tipJointPos = tipJoint.getPosition();

        Vector3 baseToMiddle = middleJointPos - baseJointPos;
        Vector3 middleToTip = tipJointPos - middleJointPos;

        //Debug.DrawRay(baseJointPos, baseJointPos + baseToMiddle.normalized * 0.05f, flipRotations ? Color.red : Color.blue, 0f, true);
        //Debug.DrawRay(middleJointPos, middleJointPos + middleToTip.normalized * 0.05f, flipRotations ? Color.red : Color.blue, 0f, true);

        float angle = Vector3.Angle(baseToMiddle, middleToTip);

        angle *= flipRotations ? -handFlexionAmplification : handFlexionAmplification; //modify value computed straight from the Kinect as its tracking of hand flexion is a little wonky

        angle = flipRotations ? Mathf.Clamp(angle, -maxFingerJointRotation, 0.0f) : Mathf.Clamp(angle, 0.0f, maxFingerJointRotation); //clamp to values that look good on a hand!

        Vector3 rotationAroundZ = new Vector3(0, 0, angle);

        foreach (HumanBodyBones bone in unityBonesToRotate)
        {
            var boneTransform = PuppetAnimator.GetBoneTransform(bone);
            boneTransform.localRotation = Quaternion.Euler(rotationAroundZ.x, rotationAroundZ.y, rotationAroundZ.z);
        }
    }
}
