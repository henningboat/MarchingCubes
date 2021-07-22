using System;
using Code.CubeMarching.TerrainChunkSystem;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.CubeMarching.Rendering
{
    internal class TerrainChunkGPUData
    {
        private readonly ComputeShader _computeShader;
        private ComputeBuffer _argsBuffer;
        private ComputeBuffer _trianglePositionCountBuffer;
        private ComputeBuffer _terrainIndexMap;
        private ComputeBuffer _triangleBuffer;
        private ComputeBuffer _trianglePositionBuffer;

        public TerrainChunkGPUData()
        {
            _computeShader = DynamicCubeMarchingSettingsHolder.Instance.Compute;
            _argsBuffer = new ComputeBuffer(4, 4, ComputeBufferType.IndirectArguments);
            _argsBuffer.SetData(new[] {3, 0, 0, 0});
            _trianglePositionCountBuffer = new ComputeBuffer(5, 4, ComputeBufferType.IndirectArguments);
            _trianglePositionCountBuffer.SetData(new[] {1, 1, 1, 1, 1});
            //todo make this properly resize
            _terrainIndexMap = new ComputeBuffer(TerrainChunkData.UnPackedCapacity * 100, 4 * 3, ComputeBufferType.Default);
        }
 
        public void UpdateWithSurfaceData(ComputeBuffer globalTerrainBuffer, ComputeBuffer globalTerrainIndexMap, NativeList<int3> chunkPositionsToRender, int3 terrainMapSize, int materialIDFilter)
        {
            if (chunkPositionsToRender.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkPositionsToRender), chunkPositionsToRender.Length, "chunkPositionsToRender list was empty");
            }

            var trianbgleByteSize = (3 + 3 + 4) * 4;
            var requiredTriangleCapacity = TerrainChunkData.UnPackedCapacity * chunkPositionsToRender.Length * 5;
            if (_triangleBuffer == null || _triangleBuffer.count < requiredTriangleCapacity)
            {
                if(_triangleBuffer!=null)
                {
                    _trianglePositionBuffer.Dispose();
                    _triangleBuffer.Dispose();
                }
                _trianglePositionBuffer = new ComputeBuffer(requiredTriangleCapacity, 4*4, ComputeBufferType.Append);
                _triangleBuffer = new ComputeBuffer(requiredTriangleCapacity, trianbgleByteSize, ComputeBufferType.Append);
            }

            _terrainIndexMap.SetData(chunkPositionsToRender.AsArray());

            Shader.SetGlobalInt("numPointsPerAxis", RenderCubeMarchingSystem.ChunkLength);
            Shader.SetGlobalBuffer("_GlobalTerrainBuffer", globalTerrainBuffer);
            Shader.SetGlobalBuffer("_GlobalTerrainIndexMap", globalTerrainIndexMap);
            Shader.SetGlobalVector("_TerrainMapSize", new Vector3(terrainMapSize.x, terrainMapSize.y, terrainMapSize.z));

            
            //Fine positions in the grid that contain triangles
            var getPositionKernel = _computeShader.FindKernel("GetTrianglePositions");
            _computeShader.SetInt("numPointsPerAxis", RenderCubeMarchingSystem.ChunkLength);
            _computeShader.SetInt("_MaterialIDFilter", materialIDFilter);
            _computeShader.SetInts("_TerrainMapSize", terrainMapSize.x, terrainMapSize.y, terrainMapSize.z);
            _computeShader.SetBuffer(getPositionKernel, "_TerrainChunkBasePosition", _terrainIndexMap);
            _computeShader.SetBuffer(getPositionKernel, "_ValidTrianglePositions", _trianglePositionBuffer);
            _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainBuffer", globalTerrainBuffer);
            _computeShader.SetBuffer(getPositionKernel, "_GlobalTerrainIndexMap", globalTerrainIndexMap);
            _trianglePositionBuffer.SetCounterValue(0);
            _computeShader.Dispatch(getPositionKernel, chunkPositionsToRender.Length, 1, 1);
            ComputeBuffer.CopyCount(_trianglePositionBuffer, _trianglePositionCountBuffer, 0);
            
            var calculateTriangulationThreadGroupSizeKernel = _computeShader.FindKernel("CalculateTriangulationThreadGroupSizeKernel");
            _computeShader.SetBuffer(calculateTriangulationThreadGroupSizeKernel, "_ArgsBuffer", _trianglePositionCountBuffer);
            _computeShader.Dispatch(calculateTriangulationThreadGroupSizeKernel, 1, 1, 1);

            var triangulationKernel = _computeShader.FindKernel("Triangulation");
            _computeShader.SetInt("numPointsPerAxis", RenderCubeMarchingSystem.ChunkLength);
            _computeShader.SetInt("_MaterialIDFilter", materialIDFilter);
            _computeShader.SetInts("_TerrainMapSize", terrainMapSize.x, terrainMapSize.y, terrainMapSize.z);
            _computeShader.SetBuffer(triangulationKernel, "_TerrainChunkBasePosition", _terrainIndexMap);
            _computeShader.SetBuffer(triangulationKernel, "triangles", _triangleBuffer);
            _computeShader.SetBuffer(triangulationKernel, "_GlobalTerrainBuffer", globalTerrainBuffer);
            _computeShader.SetBuffer(triangulationKernel, "_GlobalTerrainIndexMap", globalTerrainIndexMap);
            _computeShader.SetBuffer(triangulationKernel, "_ValidTrianglePositionResults", _trianglePositionBuffer);
            _computeShader.SetBuffer(triangulationKernel, "_ArgsBuffer", _trianglePositionCountBuffer);
            _triangleBuffer.SetCounterValue(0);
            _computeShader.DispatchIndirect(triangulationKernel, _trianglePositionCountBuffer, 4);
            _argsBuffer.SetData(new[] {3}, 0, 0, 1);
            ComputeBuffer.CopyCount(_triangleBuffer, _argsBuffer, 4);
        }

        public void Draw(Material material)
        {
            var block = new MaterialPropertyBlock();
            block.SetBuffer("_Triangles", _triangleBuffer);
            block.SetVector("_BasePosition", Vector4.zero);

            Graphics.DrawProceduralIndirect(material, new Bounds(Vector3.zero, Vector3.one * 10000), MeshTopology.Triangles, _argsBuffer, 0, Camera.main, block);
        }

        public void Dispose()
        {
            _argsBuffer.Dispose();
            _triangleBuffer.Dispose();
            _trianglePositionBuffer.Dispose();
            _terrainIndexMap.Dispose();
            _trianglePositionCountBuffer.Dispose();
        }
    }
}