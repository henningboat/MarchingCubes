using Code.CubeMarching.Authoring;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.TerrainChunkSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Code.CubeMarching.Rendering
{
    [ExecuteAlways]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class RenderCubeMarchingSystem : SystemBase
    {
        public const int ChunkLength = 8;

        public ComputeBuffer _globalTerrainBuffer;
        public ComputeBuffer _globalTerrainIndexMap;
        private TerrainChunkGPUData[] _gpuDataPerMaterial;
        private int chunkCount = 10;
        private int[] materialIDsToRender = {0, 1};

        protected override void OnCreate()
        {
            _gpuDataPerMaterial = new TerrainChunkGPUData[DynamicCubeMarchingSettingsHolder.Instance.Materials.Length];
            for (var i = 0; i < _gpuDataPerMaterial.Length; i++)
            {
                _gpuDataPerMaterial[i] = new TerrainChunkGPUData();
            }

            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            Dependency.Complete();

            _globalTerrainBuffer?.Dispose();
            _globalTerrainIndexMap?.Dispose();

            //todo rename
            var newSystem = World.GetExistingSystem<SPrepareGPUData>();

            _globalTerrainBuffer = new ComputeBuffer(newSystem.TerrainChunkData.Length * TerrainChunkData.UnPackedCapacity, TerrainChunkData.UnpackedElementSize);
            _globalTerrainBuffer.SetData(newSystem.TerrainChunkData.AsArray());

            var terrainSize = newSystem.IndexMapSize;

            var indexMap = this.GetSingletonBuffer<TerrainChunkIndexMap>().Reinterpret<int>();

            _globalTerrainIndexMap = new ComputeBuffer(indexMap.Length, 4);
            _globalTerrainIndexMap.SetData(indexMap.AsNativeArray());

            var terrainChunkPositionsToRender = new NativeList<int3>(Allocator.Temp);

            for (var materialID = 0; materialID < _gpuDataPerMaterial.Length; materialID++)
            {
                for (var i = 0; i < indexMap.Length; i++)
                {
                    if (indexMap[i] < 2)
                    {
                        continue;
                    }

                    var positionGS = GetGridPositionFromIndex(i, terrainSize);
                    var gridPositionWS = positionGS * 8;
                    terrainChunkPositionsToRender.Add(gridPositionWS);
                }


                if (terrainChunkPositionsToRender.Length > 0)
                {
                    _gpuDataPerMaterial[materialID].UpdateWithSurfaceData(_globalTerrainBuffer, _globalTerrainIndexMap, terrainChunkPositionsToRender, terrainSize, materialID + 1);
                    _gpuDataPerMaterial[materialID].Draw(DynamicCubeMarchingSettingsHolder.Instance.Materials[materialID]);
                }
            }

            terrainChunkPositionsToRender.Dispose();
        }

        protected override void OnDestroy()
        {
            _globalTerrainBuffer?.Dispose();
            _globalTerrainIndexMap?.Dispose();

            for (var i = 0; i < _gpuDataPerMaterial.Length; i++)
            {
                _gpuDataPerMaterial[i].Dispose();
            }
        }

        public static int3 GetGridPositionFromIndex(int index, int3 gridSize)
        {
            int3 position;
            position.x = index % gridSize.x;
            position.y = index / gridSize.x % gridSize.y;
            position.z = index / (gridSize.x * gridSize.y);
            return position;
        }
    }
}