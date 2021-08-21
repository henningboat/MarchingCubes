using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Code.CubeMarching.Rendering
{
    internal struct ClusterMeshGPUBuffers : ISharedComponentData, IEquatable<ClusterMeshGPUBuffers>
    {
        private ComputeShader _computeShader;
        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _trianglePositionBuffer;
        private ComputeBuffer _trianglePositionCountBuffer;
        private ComputeBuffer _chunksToTriangulize;
        private ComputeBuffer _triangleCountPerSubChunk;
        private ComputeBuffer _indexBufferCounter;

        public static ClusterMeshGPUBuffers CreateGPUData()
        {
            ClusterMeshGPUBuffers result = default;
            result._computeShader = DynamicCubeMarchingSettingsHolder.Instance.Compute;
            result._argsBuffer = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);
            result._argsBuffer.SetData(new[] {3, 0, 0, 0});
            result._trianglePositionCountBuffer = new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);

            result._triangleCountPerSubChunk = new ComputeBuffer(512 * 8, 4);


            result._chunksToTriangulize = new ComputeBuffer(10000, 4 * 4, ComputeBufferType.Default);
            result._indexBufferCounter = new ComputeBuffer(2, 4, default);

            result._triangleCountPerSubChunk.SetData(new[] {result._triangleCountPerSubChunk.count});

            return result;
        }

        public void UpdateWithSurfaceData(ComputeBuffer globalTerrainBuffer, ComputeBuffer globalTerrainIndexMap, DynamicBuffer<CTriangulationInstruction> triangulationInstructions,
            DynamicBuffer<CSubChunkWithTrianglesIndex> cSubChunkWithTrianglesIndices,
            int3 clusterCounts, int materialIDFilter, Mesh mesh, int3 clusterPositionWS)
        {
            var chunkCounts = 8 * clusterCounts;
            
            const int maxTrianglesPerSubChunk = 4 * 4 * 4 * 5;
            cSubChunkWithTrianglesIndices.ResizeUninitialized(4096);
            var indexCount = math.min(mesh.vertexCount, cSubChunkWithTrianglesIndices.Length * maxTrianglesPerSubChunk * 3);

            _indexBufferCounter.SetData(new int[] {0, 0});
            
            //todo
           // fromome reason, the mesh vertex buffer get's overwritten instead of cached. I wonder why
            if (triangulationInstructions.Length > 0)
            {
                var trianbgleByteSize = (3 + 3 + 4) * 4;
                var requiredTriangleCapacity = triangulationInstructions.Length * 4 * 4 * 4 * 5;
                if (_trianglePositionBuffer == null || _trianglePositionBuffer.count < requiredTriangleCapacity)
                {
                    if (_trianglePositionBuffer != null)
                    {
                        _trianglePositionBuffer.Dispose();
                    }

                    _trianglePositionBuffer = new ComputeBuffer(requiredTriangleCapacity, 8 * 4, ComputeBufferType.Append);
                }

                _chunksToTriangulize.SetData(triangulationInstructions.AsNativeArray());

                int[] dataReadback = new int[512 * 8];
                _trianglePositionCountBuffer.SetData(new[] {1, 1, 1, 1, 1});

                //_triangleCountPerSubChunk.SetData(dataReadback);

                //Fine positions in the grid that contain triangles
                var getPositionKernel = _computeShader.FindKernel("GetTrianglePositions");
                _computeShader.SetInt("numPointsPerAxis", ChunkLength);
                _computeShader.SetInt("_MaterialIDFilter", materialIDFilter);
                _computeShader.SetInts("_TerrainMapSize", chunkCounts.x, chunkCounts.y, chunkCounts.z);
                _computeShader.SetBuffer(getPositionKernel, "_TerrainChunkBasePosition", _chunksToTriangulize);
                _computeShader.SetBuffer(getPositionKernel, "_ValidTrianglePositions", _trianglePositionBuffer);
                _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainBuffer", globalTerrainBuffer);
                _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainIndexMap", globalTerrainIndexMap);
                _computeShader.SetBuffer(getPositionKernel, "_TriangleCountPerSubChunk", _triangleCountPerSubChunk);
                _trianglePositionBuffer.SetCounterValue(0);
                _computeShader.Dispatch(getPositionKernel, triangulationInstructions.Length, 1, 1);
                ComputeBuffer.CopyCount(_trianglePositionBuffer, _trianglePositionCountBuffer, 0);

                var meshVertexBuffer = mesh.GetVertexBuffer(0);

                var clearVertexData = _computeShader.FindKernel("ClearVertexData");
                _computeShader.SetBuffer(clearVertexData, "triangles", meshVertexBuffer);
                _computeShader.Dispatch(clearVertexData, mesh.vertexCount / 512, 1, 1);

                var calculateTriangulationThreadGroupSizeKernel = _computeShader.FindKernel("CalculateTriangulationThreadGroupSizeKernel");
                _computeShader.SetBuffer(calculateTriangulationThreadGroupSizeKernel, "_ArgsBuffer", _trianglePositionCountBuffer);
                _computeShader.Dispatch(calculateTriangulationThreadGroupSizeKernel, 1, 1, 1);

                var triangulationKernel = _computeShader.FindKernel("Triangulation");
                _computeShader.SetInt("numPointsPerAxis", ChunkLength);
                _computeShader.SetInt("_MaterialIDFilter", materialIDFilter);
                _computeShader.SetInts("_TerrainMapSize", chunkCounts.x, chunkCounts.y, chunkCounts.z);

                _computeShader.SetBuffer(triangulationKernel, "triangles", meshVertexBuffer);
                _computeShader.SetBuffer(triangulationKernel, "_GlobalTerrainBuffer", globalTerrainBuffer);
                _computeShader.SetBuffer(triangulationKernel, "_GlobalTerrainIndexMap", globalTerrainIndexMap);
                _computeShader.SetBuffer(triangulationKernel, "_ValidTrianglePositionResults", _trianglePositionBuffer);
                _computeShader.SetBuffer(triangulationKernel, "_ArgsBuffer", _trianglePositionCountBuffer);
                _computeShader.SetInts("_ClusterPositionWS", clusterPositionWS.x, clusterPositionWS.y, clusterPositionWS.z);
                _computeShader.DispatchIndirect(triangulationKernel, _trianglePositionCountBuffer, 4);
                meshVertexBuffer.Dispose();
            }

            var meshIndexBuffer = mesh.GetIndexBuffer();


            _chunksToTriangulize.SetData(cSubChunkWithTrianglesIndices.AsNativeArray());


            var indexBufferKernel = _computeShader.FindKernel("BuildIndexBuffer");
            _computeShader.SetBuffer(indexBufferKernel, "_TerrainChunkBasePosition", _chunksToTriangulize);
            _computeShader.SetBuffer(indexBufferKernel, "_TriangleCountPerSubChunkResult", _triangleCountPerSubChunk);
            _computeShader.SetBuffer(indexBufferKernel, "_IndexBufferCounter", _indexBufferCounter);
            _computeShader.SetBuffer(indexBufferKernel, "_ClusterMeshIndexBuffer", meshIndexBuffer);
            _computeShader.SetInt("_IndexBufferSize", indexCount);
            _computeShader.SetInts("_TerrainMapSize", chunkCounts.x, chunkCounts.y, chunkCounts.z);
            _computeShader.Dispatch(indexBufferKernel, cSubChunkWithTrianglesIndices.Length, 1, 1);

            meshIndexBuffer.Dispose();

            //todo asyncly read back exact vertex count, in the meantime we use an approximation

            // _triangleCountPerSubChunk.GetData(dataReadback);
            // int totdalIndexCount = 0;
            // for (int i = 0; i < dataReadback.Length; i++)
            // {
            //     totdalIndexCount += dataReadback[i];
            // }
            //
            // if (totdalIndexCount >= mesh.vertexCount)
            // {
            //     throw new OutOfMemoryException($"Index count {totdalIndexCount}");
            // }

            //mesh.SetSubMeshes(new[] {new SubMeshDescriptor(0, totdalIndexCount)}, MeshGeneratorBuilder.MeshUpdateFlagsNone);
            mesh.SetSubMeshes(new[] {new SubMeshDescriptor(0, indexCount)}, MeshGeneratorBuilder.MeshUpdateFlagsNone);
        }

        public const int ChunkLength = 8;


        public void Dispose()
        {
            _argsBuffer.Dispose();
            _trianglePositionBuffer.Dispose();
            _chunksToTriangulize.Dispose();
            _trianglePositionCountBuffer.Dispose();
            _triangleCountPerSubChunk.Dispose();
            _indexBufferCounter.Dispose();
        }

        public bool Equals(ClusterMeshGPUBuffers other)
        {
            return Equals(_computeShader, other._computeShader) && Equals(_argsBuffer, other._argsBuffer) && Equals(_trianglePositionBuffer, other._trianglePositionBuffer) &&
                   Equals(_trianglePositionCountBuffer, other._trianglePositionCountBuffer) && Equals(_chunksToTriangulize, other._chunksToTriangulize) &&
                   Equals(_triangleCountPerSubChunk, other._triangleCountPerSubChunk) && Equals(_indexBufferCounter, other._indexBufferCounter);
        }

        public override bool Equals(object obj)
        {
            return obj is ClusterMeshGPUBuffers other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_computeShader, _argsBuffer, _trianglePositionBuffer, _trianglePositionCountBuffer, _chunksToTriangulize, _triangleCountPerSubChunk, _indexBufferCounter);
        }
    }
}