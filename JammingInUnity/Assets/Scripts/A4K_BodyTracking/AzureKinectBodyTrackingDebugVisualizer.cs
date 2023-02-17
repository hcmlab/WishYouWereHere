using UnityEngine;

public class AzureKinectBodyTrackingDebugVisualizer : MonoBehaviour
{
    [Header("Data Provider")]
    public AzureKinectBodyTrackingProvider trackingDataProvider;

    [Header("Visualisation")]
    public GameObject jointPrefab;
    public Vector3 offset;

    GameObject[] jointObjects;

    public bool drawJointCoordinateSystems = true;


    // Start is called before the first frame update
    void Start()
    {
        jointObjects = new GameObject[32];

        for (int i = 0; i < jointObjects.Length; i++)
        {
            jointObjects[i] = Instantiate(jointPrefab, transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        apply(trackingDataProvider.body.PELVIS, jointObjects[0]);
        apply(trackingDataProvider.body.SPINE_NAVEL, jointObjects[1]);
        apply(trackingDataProvider.body.SPINE_CHEST, jointObjects[2]);
        apply(trackingDataProvider.body.NECK, jointObjects[3]);
        apply(trackingDataProvider.body.CLAVICLE_LEFT, jointObjects[4]);
        apply(trackingDataProvider.body.SHOULDER_LEFT, jointObjects[5]);
        apply(trackingDataProvider.body.ELBOW_LEFT, jointObjects[6]);
        apply(trackingDataProvider.body.WRIST_LEFT, jointObjects[7]);
        apply(trackingDataProvider.body.HAND_LEFT, jointObjects[8]);
        apply(trackingDataProvider.body.HANDTIP_LEFT, jointObjects[9]);
        apply(trackingDataProvider.body.THUMB_LEFT, jointObjects[10]);
        apply(trackingDataProvider.body.CLAVICLE_RIGHT, jointObjects[11]);
        apply(trackingDataProvider.body.SHOULDER_RIGHT, jointObjects[12]);
        apply(trackingDataProvider.body.ELBOW_RIGHT, jointObjects[13]);
        apply(trackingDataProvider.body.WRIST_RIGHT, jointObjects[14]);
        apply(trackingDataProvider.body.HAND_RIGHT, jointObjects[15]);
        apply(trackingDataProvider.body.HANDTIP_RIGHT, jointObjects[16]);
        apply(trackingDataProvider.body.THUMB_RIGHT, jointObjects[17]);
        apply(trackingDataProvider.body.HIP_LEFT, jointObjects[18]);
        apply(trackingDataProvider.body.KNEE_LEFT, jointObjects[19]);
        apply(trackingDataProvider.body.ANKLE_LEFT, jointObjects[20]);
        apply(trackingDataProvider.body.FOOT_LEFT, jointObjects[21]);
        apply(trackingDataProvider.body.HIP_RIGHT, jointObjects[22]);
        apply(trackingDataProvider.body.KNEE_RIGHT, jointObjects[23]);
        apply(trackingDataProvider.body.ANKLE_RIGHT, jointObjects[24]);
        apply(trackingDataProvider.body.FOOT_RIGHT, jointObjects[25]);
        apply(trackingDataProvider.body.HEAD, jointObjects[26]);
        apply(trackingDataProvider.body.NOSE, jointObjects[27]);
        apply(trackingDataProvider.body.EYE_LEFT, jointObjects[28]);
        apply(trackingDataProvider.body.EAR_LEFT, jointObjects[29]);
        apply(trackingDataProvider.body.EYE_RIGHT, jointObjects[30]);
        apply(trackingDataProvider.body.EAR_RIGHT, jointObjects[31]);

        Debug.DrawRay(jointObjects[9].transform.position, jointObjects[8].transform.position - jointObjects[9].transform.position, Color.white);
        Debug.DrawRay(jointObjects[1].transform.position, jointObjects[0].transform.position - jointObjects[1].transform.position, Color.white);
        Debug.DrawRay(jointObjects[2].transform.position, jointObjects[1].transform.position - jointObjects[2].transform.position, Color.white);
        Debug.DrawRay(jointObjects[3].transform.position, jointObjects[2].transform.position - jointObjects[3].transform.position, Color.white);
        Debug.DrawRay(jointObjects[4].transform.position, jointObjects[2].transform.position - jointObjects[4].transform.position, Color.white);
        Debug.DrawRay(jointObjects[5].transform.position, jointObjects[4].transform.position - jointObjects[5].transform.position, Color.white);
        Debug.DrawRay(jointObjects[6].transform.position, jointObjects[5].transform.position - jointObjects[6].transform.position, Color.white);
        Debug.DrawRay(jointObjects[7].transform.position, jointObjects[6].transform.position - jointObjects[7].transform.position, Color.white);
        Debug.DrawRay(jointObjects[8].transform.position, jointObjects[7].transform.position - jointObjects[8].transform.position, Color.white);
        Debug.DrawRay(jointObjects[10].transform.position, jointObjects[7].transform.position - jointObjects[10].transform.position, Color.white);
        Debug.DrawRay(jointObjects[11].transform.position, jointObjects[2].transform.position - jointObjects[11].transform.position, Color.white);
        Debug.DrawRay(jointObjects[12].transform.position, jointObjects[11].transform.position - jointObjects[12].transform.position, Color.white);
        Debug.DrawRay(jointObjects[13].transform.position, jointObjects[12].transform.position - jointObjects[13].transform.position, Color.white);
        Debug.DrawRay(jointObjects[14].transform.position, jointObjects[13].transform.position - jointObjects[14].transform.position, Color.white);
        Debug.DrawRay(jointObjects[15].transform.position, jointObjects[14].transform.position - jointObjects[15].transform.position, Color.white);
        Debug.DrawRay(jointObjects[16].transform.position, jointObjects[15].transform.position - jointObjects[16].transform.position, Color.white);
        Debug.DrawRay(jointObjects[17].transform.position, jointObjects[14].transform.position - jointObjects[17].transform.position, Color.white);
        Debug.DrawRay(jointObjects[18].transform.position, jointObjects[0].transform.position - jointObjects[18].transform.position, Color.white);
        Debug.DrawRay(jointObjects[19].transform.position, jointObjects[18].transform.position - jointObjects[19].transform.position, Color.white);
        Debug.DrawRay(jointObjects[20].transform.position, jointObjects[19].transform.position - jointObjects[20].transform.position, Color.white);
        Debug.DrawRay(jointObjects[21].transform.position, jointObjects[20].transform.position - jointObjects[21].transform.position, Color.white);
        Debug.DrawRay(jointObjects[22].transform.position, jointObjects[0].transform.position - jointObjects[22].transform.position, Color.white);
        Debug.DrawRay(jointObjects[23].transform.position, jointObjects[22].transform.position - jointObjects[23].transform.position, Color.white);
        Debug.DrawRay(jointObjects[24].transform.position, jointObjects[23].transform.position - jointObjects[24].transform.position, Color.white);
        Debug.DrawRay(jointObjects[25].transform.position, jointObjects[24].transform.position - jointObjects[25].transform.position, Color.white);
        Debug.DrawRay(jointObjects[26].transform.position, jointObjects[3].transform.position - jointObjects[26].transform.position, Color.white);
        Debug.DrawRay(jointObjects[27].transform.position, jointObjects[26].transform.position - jointObjects[27].transform.position, Color.white);
        Debug.DrawRay(jointObjects[28].transform.position, jointObjects[26].transform.position - jointObjects[28].transform.position, Color.white);
        Debug.DrawRay(jointObjects[29].transform.position, jointObjects[26].transform.position - jointObjects[29].transform.position, Color.white);
        Debug.DrawRay(jointObjects[30].transform.position, jointObjects[26].transform.position - jointObjects[30].transform.position, Color.white);
        Debug.DrawRay(jointObjects[31].transform.position, jointObjects[26].transform.position - jointObjects[31].transform.position, Color.white);

        if (drawJointCoordinateSystems)
        {
            DrawCoordinateSystem(jointObjects[1]);
            DrawCoordinateSystem(jointObjects[2]);
            DrawCoordinateSystem(jointObjects[3]);
            DrawCoordinateSystem(jointObjects[4]);
            DrawCoordinateSystem(jointObjects[5]);
            DrawCoordinateSystem(jointObjects[6]);
            DrawCoordinateSystem(jointObjects[7]);
            DrawCoordinateSystem(jointObjects[8]);
            DrawCoordinateSystem(jointObjects[10]);
            DrawCoordinateSystem(jointObjects[11]);
            DrawCoordinateSystem(jointObjects[12]);
            DrawCoordinateSystem(jointObjects[13]);
            DrawCoordinateSystem(jointObjects[14]);
            DrawCoordinateSystem(jointObjects[15]);
            DrawCoordinateSystem(jointObjects[16]);
            DrawCoordinateSystem(jointObjects[17]);
            DrawCoordinateSystem(jointObjects[18]);
            DrawCoordinateSystem(jointObjects[19]);
            DrawCoordinateSystem(jointObjects[20]);
            DrawCoordinateSystem(jointObjects[21]);
            DrawCoordinateSystem(jointObjects[22]);
            DrawCoordinateSystem(jointObjects[23]);
            DrawCoordinateSystem(jointObjects[24]);
            DrawCoordinateSystem(jointObjects[25]);
            DrawCoordinateSystem(jointObjects[26]);
            DrawCoordinateSystem(jointObjects[27]);
            DrawCoordinateSystem(jointObjects[28]);
            DrawCoordinateSystem(jointObjects[29]);
            DrawCoordinateSystem(jointObjects[30]);
            DrawCoordinateSystem(jointObjects[31]);
        }
    }

    void apply(AzureKinectJoint joint, GameObject represenation)
    {
        represenation.transform.position = joint.getPosition() + offset;
        represenation.transform.rotation = joint.getOrientation();
    }

    void DrawCoordinateSystem(GameObject forObject)
    {
        Vector3 fwd = forObject.transform.rotation * Vector3.forward * 0.1f;
        Vector3 up = forObject.transform.rotation * Vector3.up * 0.1f;
        Vector3 right = forObject.transform.rotation * Vector3.right * 0.1f;
        Debug.DrawRay(forObject.transform.position, fwd, Color.blue, 0f, true);
        Debug.DrawRay(forObject.transform.position, up, Color.green, 0f, true);
        Debug.DrawRay(forObject.transform.position, right, Color.red, 0f, true);
    }

}
