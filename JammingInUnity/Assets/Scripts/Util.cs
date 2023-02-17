using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class Util
{
    public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        T stuff = default(T);
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        }
        catch (Exception)
        {
            UnityEngine.Debug.LogError("Could not unmarshal byte array into type " + typeof(T).FullName);
        }
        finally
        {
            handle.Free();
        }
        return stuff;
    }
}

public enum JointId
{
    PELVIS = 0,
    SPINE_NAVEL,
    SPINE_CHEST,
    NECK,
    CLAVICLE_LEFT,
    SHOULDER_LEFT,
    ELBOW_LEFT,
    WRIST_LEFT,
    HAND_LEFT,
    HANDTIP_LEFT,
    THUMB_LEFT,
    CLAVICLE_RIGHT,
    SHOULDER_RIGHT,
    ELBOW_RIGHT,
    WRIST_RIGHT,
    HAND_RIGHT,
    HANDTIP_RIGHT,
    THUMB_RIGHT,
    HIP_LEFT,
    KNEE_LEFT,
    ANKLE_LEFT,
    FOOT_LEFT,
    HIP_RIGHT,
    KNEE_RIGHT,
    ANKLE_RIGHT,
    FOOT_RIGHT,
    HEAD,
    NOSE,
    EYE_LEFT,
    EAR_LEFT,
    EYE_RIGHT,
    EAR_RIGHT,
    Count
}

public struct AzureKinectJoint
{
    //Vector3 position;
    //Quaternion rotation;
    public float posX, posY, posZ;
    public float rotW, rotX, rotY, rotZ;
    public float confidence;

    public Vector3 getPosition()
    {
        //Kinect positions are in mm and Y negative is world up
        return new Vector3(posX / 1000.0f, -posY / 1000.0f, posZ / 1000.0f);
    }

    public Quaternion getOrientation()
    {
        return new Quaternion(rotX, rotY, rotZ, rotW);
    }
}

public struct AzureKinectBody
{
    public AzureKinectJoint PELVIS;
    public AzureKinectJoint SPINE_NAVEL;
    public AzureKinectJoint SPINE_CHEST;
    public AzureKinectJoint NECK;
    public AzureKinectJoint CLAVICLE_LEFT;
    public AzureKinectJoint SHOULDER_LEFT;
    public AzureKinectJoint ELBOW_LEFT;
    public AzureKinectJoint WRIST_LEFT;
    public AzureKinectJoint HAND_LEFT;
    public AzureKinectJoint HANDTIP_LEFT;
    public AzureKinectJoint THUMB_LEFT;
    public AzureKinectJoint CLAVICLE_RIGHT;
    public AzureKinectJoint SHOULDER_RIGHT;
    public AzureKinectJoint ELBOW_RIGHT;
    public AzureKinectJoint WRIST_RIGHT;
    public AzureKinectJoint HAND_RIGHT;
    public AzureKinectJoint HANDTIP_RIGHT;
    public AzureKinectJoint THUMB_RIGHT;
    public AzureKinectJoint HIP_LEFT;
    public AzureKinectJoint KNEE_LEFT;
    public AzureKinectJoint ANKLE_LEFT;
    public AzureKinectJoint FOOT_LEFT;
    public AzureKinectJoint HIP_RIGHT;
    public AzureKinectJoint KNEE_RIGHT;
    public AzureKinectJoint ANKLE_RIGHT;
    public AzureKinectJoint FOOT_RIGHT;
    public AzureKinectJoint HEAD;
    public AzureKinectJoint NOSE;
    public AzureKinectJoint EYE_LEFT;
    public AzureKinectJoint EAR_LEFT;
    public AzureKinectJoint EYE_RIGHT;
    public AzureKinectJoint EAR_RIGHT;

    public AzureKinectJoint GetById(JointId id)
    {
        switch (id)
        {
            case JointId.PELVIS: return PELVIS;
            case JointId.SPINE_NAVEL: return SPINE_NAVEL;
            case JointId.SPINE_CHEST: return SPINE_CHEST;
            case JointId.NECK: return NECK;
            case JointId.CLAVICLE_LEFT: return CLAVICLE_LEFT;
            case JointId.SHOULDER_LEFT: return SHOULDER_LEFT;
            case JointId.ELBOW_LEFT: return ELBOW_LEFT;
            case JointId.WRIST_LEFT: return WRIST_LEFT;
            case JointId.HAND_LEFT: return HAND_LEFT;
            case JointId.HANDTIP_LEFT: return HANDTIP_LEFT;
            case JointId.THUMB_LEFT: return THUMB_LEFT;
            case JointId.CLAVICLE_RIGHT: return CLAVICLE_RIGHT;
            case JointId.SHOULDER_RIGHT: return SHOULDER_RIGHT;
            case JointId.ELBOW_RIGHT: return ELBOW_RIGHT;
            case JointId.WRIST_RIGHT: return WRIST_RIGHT;
            case JointId.HAND_RIGHT: return HAND_RIGHT;
            case JointId.HANDTIP_RIGHT: return HANDTIP_RIGHT;
            case JointId.THUMB_RIGHT: return THUMB_RIGHT;
            case JointId.HIP_LEFT: return HIP_LEFT;
            case JointId.KNEE_LEFT: return KNEE_LEFT;
            case JointId.ANKLE_LEFT: return ANKLE_LEFT;
            case JointId.FOOT_LEFT: return FOOT_LEFT;
            case JointId.HIP_RIGHT: return HIP_RIGHT;
            case JointId.KNEE_RIGHT: return KNEE_RIGHT;
            case JointId.ANKLE_RIGHT: return ANKLE_RIGHT;
            case JointId.FOOT_RIGHT: return FOOT_RIGHT;
            case JointId.HEAD: return HEAD;
            case JointId.NOSE: return NOSE;
            case JointId.EYE_LEFT: return EYE_LEFT;
            case JointId.EAR_LEFT: return EAR_LEFT;
            case JointId.EYE_RIGHT: return EYE_RIGHT;
            case JointId.EAR_RIGHT: return EAR_RIGHT;
            default: return PELVIS;
        }
    }
}