using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component that decodes raw data representing an Azure Kinect body tracking frame, sent over the network from the ssi azurekinect plugin.
/// Assumes that the data buffer provided by the INetworkDataFrameProvider consists of:
///     number of tracked bodies * 1024 bytes (32 joints with 8 float variables each per body)
/// </summary>
[RequireComponent(typeof(INetworkDataFrameProvider))]
public class AzureKinectBodyTrackingProvider : MonoBehaviour
{
    private INetworkDataFrameProvider bodyTrackingDataProvider;

    [HideInInspector]
    public bool Connected => bodyTrackingDataProvider.Connected;

    #region bodytracking variables
    [HideInInspector]
    public Dictionary<JointId, JointId> parentJointMap;

    Dictionary<JointId, Quaternion> basisJointMap;

    [HideInInspector]
    public Quaternion[] absoluteJointRotations = new Quaternion[(int)JointId.Count];

    Quaternion Y_180_FLIP = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);

    [HideInInspector]
    public AzureKinectBody body; //expose body tracking struct
#endregion

    // Setup Azure Kinect skeleton specific mappings.
    void Awake()
    {
        parentJointMap = new Dictionary<JointId, JointId>();

        // pelvis has no parent so set to count
        parentJointMap[JointId.PELVIS] = JointId.Count;
        parentJointMap[JointId.SPINE_NAVEL] = JointId.PELVIS;
        parentJointMap[JointId.SPINE_CHEST] = JointId.SPINE_NAVEL;
        parentJointMap[JointId.NECK] = JointId.SPINE_CHEST;
        parentJointMap[JointId.CLAVICLE_LEFT] = JointId.SPINE_CHEST;
        parentJointMap[JointId.SHOULDER_LEFT] = JointId.CLAVICLE_LEFT;
        parentJointMap[JointId.ELBOW_LEFT] = JointId.SHOULDER_LEFT;
        parentJointMap[JointId.WRIST_LEFT] = JointId.ELBOW_LEFT;
        parentJointMap[JointId.HAND_LEFT] = JointId.WRIST_LEFT;
        parentJointMap[JointId.HANDTIP_LEFT] = JointId.HAND_LEFT;
        parentJointMap[JointId.THUMB_LEFT] = JointId.HAND_LEFT;
        parentJointMap[JointId.CLAVICLE_RIGHT] = JointId.SPINE_CHEST;
        parentJointMap[JointId.SHOULDER_RIGHT] = JointId.CLAVICLE_RIGHT;
        parentJointMap[JointId.ELBOW_RIGHT] = JointId.SHOULDER_RIGHT;
        parentJointMap[JointId.WRIST_RIGHT] = JointId.ELBOW_RIGHT;
        parentJointMap[JointId.HAND_RIGHT] = JointId.WRIST_RIGHT;
        parentJointMap[JointId.HANDTIP_RIGHT] = JointId.HAND_RIGHT;
        parentJointMap[JointId.THUMB_RIGHT] = JointId.HAND_RIGHT;
        parentJointMap[JointId.HIP_LEFT] = JointId.SPINE_NAVEL;
        parentJointMap[JointId.KNEE_LEFT] = JointId.HIP_LEFT;
        parentJointMap[JointId.ANKLE_LEFT] = JointId.KNEE_LEFT;
        parentJointMap[JointId.FOOT_LEFT] = JointId.ANKLE_LEFT;
        parentJointMap[JointId.HIP_RIGHT] = JointId.SPINE_NAVEL;
        parentJointMap[JointId.KNEE_RIGHT] = JointId.HIP_RIGHT;
        parentJointMap[JointId.ANKLE_RIGHT] = JointId.KNEE_RIGHT;
        parentJointMap[JointId.FOOT_RIGHT] = JointId.ANKLE_RIGHT;
        parentJointMap[JointId.HEAD] = JointId.PELVIS;
        parentJointMap[JointId.NOSE] = JointId.HEAD;
        parentJointMap[JointId.EYE_LEFT] = JointId.HEAD;
        parentJointMap[JointId.EAR_LEFT] = JointId.HEAD;
        parentJointMap[JointId.EYE_RIGHT] = JointId.HEAD;
        parentJointMap[JointId.EAR_RIGHT] = JointId.HEAD;

        Vector3 zpositive = Vector3.forward;
        Vector3 xpositive = Vector3.right;
        Vector3 ypositive = Vector3.up;
        // spine and left hip are the same
        Quaternion leftHipBasis = Quaternion.LookRotation(xpositive, -zpositive);
        Quaternion spineHipBasis = Quaternion.LookRotation(xpositive, -zpositive);
        Quaternion rightHipBasis = Quaternion.LookRotation(xpositive, zpositive);
        // arms and thumbs share the same basis
        Quaternion leftArmBasis = Quaternion.LookRotation(ypositive, -zpositive);
        Quaternion rightArmBasis = Quaternion.LookRotation(-ypositive, zpositive);
        Quaternion leftHandBasis = Quaternion.LookRotation(-zpositive, -ypositive);
        Quaternion rightHandBasis = Quaternion.identity;
        Quaternion leftFootBasis = Quaternion.LookRotation(xpositive, ypositive);
        Quaternion rightFootBasis = Quaternion.LookRotation(xpositive, -ypositive);

        basisJointMap = new Dictionary<JointId, Quaternion>();

        // pelvis has no parent so set to count
        basisJointMap[JointId.PELVIS] = spineHipBasis;
        basisJointMap[JointId.SPINE_NAVEL] = spineHipBasis;
        basisJointMap[JointId.SPINE_CHEST] = spineHipBasis;
        basisJointMap[JointId.NECK] = spineHipBasis;
        basisJointMap[JointId.CLAVICLE_LEFT] = leftArmBasis;
        basisJointMap[JointId.SHOULDER_LEFT] = leftArmBasis;
        basisJointMap[JointId.ELBOW_LEFT] = leftArmBasis;
        basisJointMap[JointId.WRIST_LEFT] = leftHandBasis;
        basisJointMap[JointId.HAND_LEFT] = leftHandBasis;
        basisJointMap[JointId.HANDTIP_LEFT] = leftHandBasis;
        basisJointMap[JointId.THUMB_LEFT] = leftArmBasis;
        basisJointMap[JointId.CLAVICLE_RIGHT] = rightArmBasis;
        basisJointMap[JointId.SHOULDER_RIGHT] = rightArmBasis;
        basisJointMap[JointId.ELBOW_RIGHT] = rightArmBasis;
        basisJointMap[JointId.WRIST_RIGHT] = rightHandBasis;
        basisJointMap[JointId.HAND_RIGHT] = rightHandBasis;
        basisJointMap[JointId.HANDTIP_RIGHT] = rightHandBasis;
        basisJointMap[JointId.THUMB_RIGHT] = rightArmBasis;
        basisJointMap[JointId.HIP_LEFT] = leftHipBasis;
        basisJointMap[JointId.KNEE_LEFT] = leftHipBasis;
        basisJointMap[JointId.ANKLE_LEFT] = leftHipBasis;
        basisJointMap[JointId.FOOT_LEFT] = leftFootBasis;
        basisJointMap[JointId.HIP_RIGHT] = rightHipBasis;
        basisJointMap[JointId.KNEE_RIGHT] = rightHipBasis;
        basisJointMap[JointId.ANKLE_RIGHT] = rightHipBasis;
        basisJointMap[JointId.FOOT_RIGHT] = rightFootBasis;
        basisJointMap[JointId.HEAD] = spineHipBasis;
        basisJointMap[JointId.NOSE] = spineHipBasis;
        basisJointMap[JointId.EYE_LEFT] = spineHipBasis;
        basisJointMap[JointId.EAR_LEFT] = spineHipBasis;
        basisJointMap[JointId.EYE_RIGHT] = spineHipBasis;
        basisJointMap[JointId.EAR_RIGHT] = spineHipBasis;
    }

    private void Start()
    {
        bodyTrackingDataProvider = GetComponent<INetworkDataFrameProvider>();
        bodyTrackingDataProvider.addOnCompleteFrameReceivedHandler(() =>
        {
            //Debug.Log("Received new frame from data provider");
            if (bodyTrackingDataProvider.CurrentFrame.Length == 1024)
            {
                //Debug.Log("Converting dataframe to body struct");
                body = Util.ByteArrayToStructure<AzureKinectBody>(bodyTrackingDataProvider.CurrentFrame);
                updateAbsoluteRotations();
            } else
            {
                Debug.LogError("NetworkdataProvider for Bodytracking data should be configured with 1024 bytes per frame");
            }
        });
    }

    private void updateAbsoluteRotations()
    {
        for (int jointNum = 0; jointNum < (int)JointId.Count; jointNum++)
        {
            AzureKinectJoint joint = body.GetById((JointId)jointNum);
            Quaternion jointRot = Y_180_FLIP * joint.getOrientation() * Quaternion.Inverse(basisJointMap[(JointId)jointNum]);
            absoluteJointRotations[jointNum] = jointRot;
        }
    }

    public Vector3 getAbsoluteJointPosition(JointId id)
    {
        AzureKinectJoint joint = body.GetById(id);
        return joint.getPosition();
    }

    public Quaternion GetRelativeJointRotation(JointId jointId)
    {
        JointId parent = parentJointMap[jointId];
        Quaternion parentJointRotationBodySpace = Quaternion.identity;
        if (parent == JointId.Count)
        {
            parentJointRotationBodySpace = Y_180_FLIP;
        }
        else
        {
            parentJointRotationBodySpace = absoluteJointRotations[(int)parent];
        }
        Quaternion jointRotationBodySpace = absoluteJointRotations[(int)jointId];
        Quaternion relativeRotation = Quaternion.Inverse(parentJointRotationBodySpace) * jointRotationBodySpace;

        return relativeRotation;
    }
}
