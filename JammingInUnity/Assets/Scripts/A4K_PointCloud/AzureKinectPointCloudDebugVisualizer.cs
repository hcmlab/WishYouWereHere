using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using System.Collections;

/// <summary>
/// Example component that uses a pointCloudProvider to visualize an Azure Kinect Point Cloud.
/// Uses Game-Object and thus only displays a very coarse representation of the point cloud for performance reason.
/// Control how coarse the visualisation should be with the parameters in the "Visualisation" section
/// </summary>
/// 

namespace Pcx {
    public class AzureKinectPointCloudDebugVisualizer : MonoBehaviour
    {

        public GameObject PointCloudVisualizer;

        //private System.Diagnostics.Stopwatch m_frameStopwatch = new System.Diagnostics.Stopwatch();
        //long nanosecPerTick = (1000L * 1000L * 1000L) / System.Diagnostics.Stopwatch.Frequency;

        private PointCloudRenderer PCR;
        private PointCloudData PCD;

        private bool working = false;

        [Header("Data Provider")]
        public AzureKinectPointCloudProvider PointCloudProvider;

        void Start()
        {

            PCR = PointCloudVisualizer.GetComponent<PointCloudRenderer>();
            PCD = new PointCloudData();

            PointCloudProvider.networkClient.addOnCompleteFrameReceivedHandler(() =>
            {
                if (working)
                {
                    return;
                }
                UnityMainThreadDispatcher.Instance().Enqueue(RenderPointCloud);
            });
        }

        private void RenderPointCloud()
        {
            working = true;

            //m_frameStopwatch.Start();
            PointCloudProvider.ConvertPointCloudData();
            //m_frameStopwatch.Stop();
            //Debug.Log("--> 1 Data Conversion took " + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000000.0f) + " ms!"); // 18 ms
            //m_frameStopwatch.Reset();

            //m_frameStopwatch.Start();
            PCD.UpdateData(PointCloudProvider.CompleteResult);
            //m_frameStopwatch.Stop();
            //Debug.Log("--> 2 Data Update took " + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000000.0f) + " ms!"); // 3 ms
            //m_frameStopwatch.Reset();


            //m_frameStopwatch.Start();
            PointCloudProvider.DisposeArrays();
            //m_frameStopwatch.Stop();
            //Debug.Log("--> 3 Dispose took " + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000000.0f) + " ms!");
            //m_frameStopwatch.Reset();

            //m_frameStopwatch.Start();
            PCR.sourceData = PCD;
            //m_frameStopwatch.Stop();
            //Debug.Log("--> 4 Data copy " + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000000.0f) + " ms!");
            //m_frameStopwatch.Reset();

            working = false;
        }
    }
}