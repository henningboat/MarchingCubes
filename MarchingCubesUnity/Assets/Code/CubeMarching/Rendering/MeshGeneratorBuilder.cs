using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Code.CubeMarching.Rendering
{
    public static class MeshGeneratorBuilder
    {
        private const int ClusterVolume = ClusterLength * ClusterLength * ClusterLength;
        private const int MaxTrianglesPerCell = 5;
        private const int ClusterEntityVertexCount = ClusterVolume * MaxTrianglesPerCell;
        private const int ClusterLength = 64;

        private const MeshUpdateFlags MeshUpdateFlagsNone =
            MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds;

        private static readonly VertexAttributeDescriptor[] ClusterMeshTerrainDescriptors =
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
            new(VertexAttribute.Normal)
        };


        public static CClusterMesh GenerateClusterMesh()
        {
            var clusterMesh = new Mesh {name = "ClusterMesh", hideFlags = HideFlags.HideAndDontSave};

            clusterMesh.SetVertexBufferParams(ClusterEntityVertexCount, ClusterMeshTerrainDescriptors);
            clusterMesh.SetIndexBufferParams(ClusterEntityVertexCount, IndexFormat.UInt32);

            //todo optimize and cache
            var indexBuffer = new NativeArray<uint>(ClusterEntityVertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var randomVertexData = new NativeArray<float4>(ClusterEntityVertexCount, Allocator.Temp,NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < indexBuffer.Length; i++)
            {
                indexBuffer[i] = (uint) i;
                randomVertexData[i] = new float4((float3) (Random.insideUnitSphere * 64) + 32, 1);
            }

            clusterMesh.SetVertexBufferData(randomVertexData, 0, 0, indexBuffer.Length, 0, MeshUpdateFlagsNone);
            clusterMesh.SetIndexBufferData(indexBuffer, 0, 0, indexBuffer.Length, MeshUpdateFlagsNone);

            clusterMesh.bounds = new Bounds {min = Vector3.zero, max = Vector3.one * ClusterLength};

            clusterMesh.SetSubMesh(0, new SubMeshDescriptor(0, 33), MeshUpdateFlagsNone);

            indexBuffer.Dispose();
            randomVertexData.Dispose();

            return new CClusterMesh {mesh = clusterMesh};
        }
    }
}