using System;
using Unity.Collections;

/// <summary>
/// Interface for components that provide fixed size frames of data (e.g. image/bodytracking/pointcloud/...) received (repeatedly) over a network connection
/// </summary>
public interface INetworkDataFrameProvider
{
    //NativeArray<Byte> CurrentFrame
    //{
    //    get;
    //}

    byte[] CurrentFrame
    {
        get;
    }

    bool Connected
    {
        get;
    }

    void addOnCompleteFrameReceivedHandler(Action handler);
}
