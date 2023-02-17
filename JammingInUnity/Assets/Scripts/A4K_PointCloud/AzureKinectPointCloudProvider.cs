using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;

/// <summary>
/// Component that decodes raw data representing an Azure Kinect point cloud, sent over the network from the ssi azurekinect plugin.
/// Assumes that the data buffer provided by the INetworkDataFrameProvider consists of:
///     pointCloudWidth * pointCloudHeight * 6 bytes (2 bytes each for x, y, and z)
///       followed by
///     pointCloudWidth * pointCloudHeight * 4 bytes (1 byte each for b, g, r, and a)
/// </summary>

namespace Pcx
{
    [RequireComponent(typeof(INetworkDataFrameProvider))]
    public class AzureKinectPointCloudProvider : MonoBehaviour
    {
        [Header("Settings for Parallel Job")]
        public int ParallelBatchSize;
        public bool EnablePointFilter;
        public float MinFilterDistance;
        public float MaxFilterDistance;
        public Vector3 FilterPoint;

        public float PointCloudScale;
        public Vector3 PointCloudPosition;
        public Vector3 PointCloudRotation;

        [Header("PointCloudData")]
        public int pointCloudWidth;
        public int pointCloudHeight;
        public int colorDataStartByteIndex;
        public int dataSize;

        private const int bytesPerVoxelPosition = 6;
        private const int bytesPerPixelColor = 4;

        private const int blueByteOffset = 0;
        private const int greenByteOffset = 1;
        private const int redByteOffset = 2;
        private const int alphaByteOffset = 3;


        [SerializeField] public NativeArray<Point> CompleteResult;

        [HideInInspector]
        public bool Connected => networkClient.Connected;
        public NativeArray<Color32> ColorBuffer;

        [HideInInspector]
        public byte[] RawPositionData => networkClient.CurrentFrame;

        public INetworkDataFrameProvider networkClient;
        private byte[] currentDataFrame => networkClient.CurrentFrame;

        //private System.Diagnostics.Stopwatch m_frameStopwatch = new System.Diagnostics.Stopwatch();
        //long nanosecPerTick = (1000L * 1000L * 1000L) / System.Diagnostics.Stopwatch.Frequency;

        private void Awake()
        {
            networkClient = GetComponent<INetworkDataFrameProvider>();
        }

        // Start is called before the first frame update
        void Start()
        {
            dataSize = pointCloudHeight * pointCloudHeight;
            colorDataStartByteIndex = pointCloudWidth * pointCloudHeight * bytesPerVoxelPosition;
        }

        struct DataConversionJob : IJobParallelFor
        {
            [ReadOnly] public int pointCloudWidth;
            [ReadOnly] public int pointCloudHeight;
            [ReadOnly] public int colorDataStartByteIndex;
            [ReadOnly] public float deltaTime;

            [ReadOnly] public bool EnablePointFilter;
            [ReadOnly] public float MaxCameraDistance;
            [ReadOnly] public float MinCameraDistance;
            [ReadOnly] public Vector3 FilterPoint;
            [ReadOnly] public float pointCloudScale;
            [ReadOnly] public Vector3 pointCloudPosition;
            [ReadOnly] public Vector3 pointCloudRotation;

            [ReadOnly] public NativeArray<byte> currentDataFrame;

            public NativeArray<Point> CompleteResult;

            public void Execute(int i) // 20 ms
            {

                int row = i / pointCloudHeight;
                int col = i % pointCloudWidth;
                Point entry = new Point();

                //entry.color = getColorAt(row, col, currentDataFrame, pointCloudWidth, colorDataStartByteIndex); // 8-9 ms
                //Vector3 position = getPointAt(row, col, currentDataFrame, pointCloudWidth, pointCloudPosition);
                //entry.position = RotatePointAroundPivot(position, pointCloudPosition, pointCloudRotation);

                if (!EnablePointFilter)
                {
                    entry.color = getColorAt(row, col, currentDataFrame, pointCloudWidth, colorDataStartByteIndex); // 8-9 ms
                    Vector3 position = getPointAt(row, col, currentDataFrame, pointCloudWidth, pointCloudPosition);
                    entry.position = RotatePointAroundPivot(position, pointCloudPosition, pointCloudRotation);
                    //entry.position = getPointAt(row, col, currentDataFrame, pointCloudWidth, pointCloudPosition); // 8-9 ms
                }
                else
                {
                    Vector3 rawPoint = getPointAt(row, col, currentDataFrame, pointCloudWidth, pointCloudPosition);
                    Vector3 newPoint = RotatePointAroundPivot(rawPoint, pointCloudPosition, pointCloudRotation) * pointCloudScale;
                    Vector3 filterPointWorldSpace = FilterPoint + pointCloudPosition;

                    float distanceToFIlterPoint = Vector3.Distance(newPoint, filterPointWorldSpace);

                    if ((distanceToFIlterPoint < MaxCameraDistance) && (distanceToFIlterPoint > MinCameraDistance))
                    {
                        entry.color = getColorAt(row, col, currentDataFrame, pointCloudWidth, colorDataStartByteIndex); // 8-9 ms
                        entry.position = newPoint;
                    }
                    else
                    {
                        entry.color = 0;
                        entry.position = new Vector3(0.0f, 100.0f, 0.0f);
                    }
                }
                CompleteResult[i] = entry;
            }
        }


        public void ConvertPointCloudData()
        {
            //m_frameStopwatch.Start();

            NativeArray<byte> nativeDataFrame = new NativeArray<byte>(currentDataFrame, Allocator.Persistent);
            NativeArray<Point> completeResult = new NativeArray<Point>(dataSize, Allocator.Persistent);

            //m_frameStopwatch.Stop();
            //Debug.Log("---> 1.0 Init Arrays needed " + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000000.0f) + " ms!");
            //m_frameStopwatch.Reset();

            // Initialize the job data
            var job = new DataConversionJob()
            {
                pointCloudWidth = pointCloudWidth,
                pointCloudHeight = pointCloudHeight,
                colorDataStartByteIndex = colorDataStartByteIndex,
                pointCloudPosition = PointCloudPosition,
                pointCloudRotation = PointCloudRotation,
                pointCloudScale = PointCloudScale,
                currentDataFrame = nativeDataFrame,
                deltaTime = Time.deltaTime,
                EnablePointFilter = EnablePointFilter,
                MinCameraDistance = MinFilterDistance,
                MaxCameraDistance = MaxFilterDistance,
                FilterPoint = FilterPoint,
                CompleteResult = completeResult
            };

            //m_frameStopwatch.Stop();
            //Debug.Log("---> 1.1 Job init needed " + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000000.0f) + " ms!");
            //m_frameStopwatch.Reset();

            //m_frameStopwatch.Start();
            JobHandle jobHandle = job.Schedule(dataSize, ParallelBatchSize);

            jobHandle.Complete();

            //m_frameStopwatch.Stop();
            //Debug.Log("---> 1.2 Job execution needed " + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000000.0f) + " ms!");
            //m_frameStopwatch.Reset();

            //m_frameStopwatch.Start();
            CompleteResult = completeResult;

            nativeDataFrame.Dispose();

            //m_frameStopwatch.Stop();
            //Debug.Log("---> 1.3 Rest needed " + (m_frameStopwatch.ElapsedTicks * nanosecPerTick / 1000000.0f) + " ms!");
            //m_frameStopwatch.Reset();
        }

        public void DisposeArrays()
        {
            CompleteResult.Dispose();
        }

        static uint getColorAt(int row, int col, NativeArray<byte> currentDataFrame, int pointCloudWidth, int colorDataStartByteIndex)
        {
            int rowStartByteNr = row * pointCloudWidth;
            int voxelColorStartByteIdx = colorDataStartByteIndex + (rowStartByteNr + col) * bytesPerPixelColor;

            byte r = currentDataFrame[voxelColorStartByteIdx + redByteOffset];
            byte g = currentDataFrame[voxelColorStartByteIdx + greenByteOffset];
            byte b = currentDataFrame[voxelColorStartByteIdx + blueByteOffset];
            byte a = currentDataFrame[voxelColorStartByteIdx + alphaByteOffset];

            Color32 color32 = new Color32(r, g, b, a);

            return EncodeColor(color32);

        }

        static Vector3 getPointAt(int row, int col, NativeArray<byte> currentDataFrame, int pointCloudWidth, Vector3 pointCloudPosition)
        {

            int rowStartByteNr = row * pointCloudWidth;
            int voxelPosStartByteIdx = (rowStartByteNr + col) * bytesPerVoxelPosition;

            var reinterpretedArray = currentDataFrame.GetSubArray(voxelPosStartByteIdx, 6).Reinterpret<short>(UnsafeUtility.SizeOf<byte>());

            return new Vector3(reinterpretedArray[0] / 1000f, reinterpretedArray[1] / -1000f, reinterpretedArray[2] / 1000f) + pointCloudPosition; //flip y dimension because Kinect uses -Y up coordinate system
        }

        static uint EncodeColor(Color c)
        {
            const float kMaxBrightness = 16;

            var y = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
            y = Mathf.Clamp(Mathf.Ceil(y * 255 / kMaxBrightness), 1, 255);

            uint r = (uint)(c.r * 255 * 255 / (y * kMaxBrightness));
            uint g = (uint)(c.g * 255 * 255 / (y * kMaxBrightness));
            uint b = (uint)(c.b * 255 * 255 / (y * kMaxBrightness));

            return (r) | (g << 8) | (b << 16) | ((uint)y << 24);
        }

        static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }

    }
}