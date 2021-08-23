using System.Collections.Generic;
using System.Linq;
using Code.CubeMarching.Rendering;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Code.CubeMarching.AsyncReadbackSystem
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

        private static Dictionary<int, ReadbackData> _readbacks = new Dictionary<int, ReadbackData>();

        public static void AddCallbackIfNeeded(int clusterIndex, ComputeBuffer computeBuffer, int frameTimestamp)
        {
            if (!_readbacks.ContainsKey(clusterIndex))
            {
                _readbacks[clusterIndex] = new ReadbackData(clusterIndex);
                AsyncGPUReadback.Request(computeBuffer, request =>
                {
                    if (request.hasError)
                    {
                        _readbacks.Remove(clusterIndex);
                    }

                    _readbacks[clusterIndex].SetData(frameTimestamp, request);
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