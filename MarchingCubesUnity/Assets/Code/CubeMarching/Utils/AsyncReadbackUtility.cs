using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.Rendering;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Code.CubeMarching.Utils
{
    //todo use native arrays

    public struct ClusterVertexCountGPUReadbackData
    {
        public NativeArray<int> vertexCounts;
        public int clusterIndex;
        public int frameTimestamp;

        public ClusterVertexCountGPUReadbackData(AsyncReadbackUtility.ReadbackData readbackData)
        {
            vertexCounts = new NativeArray<int>(Constants.SubChunksInCluster, Allocator.TempJob);
            frameTimestamp = readbackData.frameTimestamp;    
            
            for (int i = 0; i < 4096; i++)
            {
                vertexCounts[i] = readbackData.vertexCount[i];
            }

            clusterIndex = readbackData.clusterIndex;
        }

        public void Dispose(JobHandle dependency)
        {
            vertexCounts.Dispose(dependency);
        }
    }

    public static class AsyncReadbackUtility
    {
        public class ReadbackData
        {
            public int clusterIndex;
            public bool hasData;
            public int[] vertexCount;
            public int frameTimestamp;

            public ReadbackData(int clusterIndex)
            {
                this.clusterIndex = clusterIndex;
            }


            public void SetData(int frameTimeStamp, AsyncGPUReadbackRequest request)
            {
                hasData = true;
                this.frameTimestamp = frameTimeStamp;
                vertexCount = request.GetData<int>().ToArray();
            }
        }

        private static Dictionary<int, ReadbackData> _readbacks = new();
        
        /// <summary>
        /// stores the information when a clusters data was last requested. We can use this to prevent requesting data for chunks that don't have data yet
        /// </summary>
        private static Dictionary<int, int> _readbackTimestamps = new();
        

        public static void AddCallbackIfNeeded(int clusterIndex, ComputeBuffer computeBuffer, int frameTimeStamp, int lastVertexBufferChangeTimestamp)
        {
            if (_readbackTimestamps.ContainsKey(clusterIndex) && _readbackTimestamps[clusterIndex] >= lastVertexBufferChangeTimestamp)
            {
                return;
            }

            _readbackTimestamps[clusterIndex] = lastVertexBufferChangeTimestamp;
            
            if (!_readbacks.ContainsKey(clusterIndex))
            {
                _readbacks[clusterIndex] = new ReadbackData(clusterIndex);
                AsyncGPUReadback.Request(computeBuffer, request =>
                {
                    if (request.hasError)
                    {
                        _readbacks.Remove(clusterIndex);
                    }

                    _readbacks[clusterIndex].SetData(frameTimeStamp, request);
                });
            }
        }

        public static List<ClusterVertexCountGPUReadbackData> GetDataReadbacks()
        {
            List<ClusterVertexCountGPUReadbackData> readback = new List<ClusterVertexCountGPUReadbackData>();
            
            var readbacksValues = _readbacks.Values.ToList();
            foreach (var readbacksValue in readbacksValues)
            {
                if (readbacksValue.hasData)
                {
                    _readbacks.Remove(readbacksValue.clusterIndex);
                    readback.Add(new ClusterVertexCountGPUReadbackData(readbacksValue));
                }
            }

            return readback;
        }
    }
}