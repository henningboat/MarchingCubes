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
        private const int VerticesPerTriangle = 3;
        private const int ClusterEntityVertexCount = ClusterVolume * MaxTrianglesPerCell * VerticesPerTriangle;
        private const int ClusterLength = 64;

        public const MeshUpdateFlags MeshUpdateFlagsNone =
            MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontResetBoneBounds;

        private static readonly VertexAttributeDescriptor[] ClusterMeshTerrainDescriptors =
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
            new(VertexAttribute.Normal,VertexAttributeFormat.Float32,3)
        };

        private struct VertexData
        {
            public readonly float4 vertex;
            public readonly float3 normal;

            public VertexData(float3 vertex, float3 normal)
            {
                this.vertex = new float4(vertex, 1);
                this.normal = normal;
            }
        }

        public static CClusterMesh GenerateClusterMesh()
        {
            var clusterMesh = new Mesh {name = "ClusterMesh", hideFlags = HideFlags.HideAndDontSave};
            clusterMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

            clusterMesh.SetVertexBufferParams(ClusterEntityVertexCount, ClusterMeshTerrainDescriptors);
            clusterMesh.SetIndexBufferParams(ClusterEntityVertexCount, IndexFormat.UInt32);

            //todo optimize and cache
            var indexBuffer = new NativeArray<uint>(ClusterEntityVertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var randomVertexData = new NativeArray<VertexData>(ClusterEntityVertexCount, Allocator.Temp,NativeArrayOptions.UninitializedMemory);

            int vertexID = 0;
            
            for (int z = 0; z < 64; z++)
            {
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        randomVertexData[vertexID] = default;
                        vertexID++;
                        randomVertexData[vertexID] = default;
                        vertexID++;
                        randomVertexData[vertexID] = default;
                        vertexID++;
                    }
                }
            }
            
            for (var i = 0; i < indexBuffer.Length; i++)
            {
                indexBuffer[i] = (uint) i;
            }

            clusterMesh.SetVertexBufferData(randomVertexData, 0, 0, indexBuffer.Length, 0, MeshUpdateFlagsNone);
            clusterMesh.SetIndexBufferData(indexBuffer, 0, 0, indexBuffer.Length, MeshUpdateFlagsNone);

            clusterMesh.bounds = new Bounds {min = Vector3.zero, max = Vector3.one * ClusterLength};

            clusterMesh.SetSubMesh(0, new SubMeshDescriptor(0, 3), MeshUpdateFlagsNone);

            indexBuffer.Dispose();
            randomVertexData.Dispose();

            return new CClusterMesh {mesh = clusterMesh};
        }
    }
}