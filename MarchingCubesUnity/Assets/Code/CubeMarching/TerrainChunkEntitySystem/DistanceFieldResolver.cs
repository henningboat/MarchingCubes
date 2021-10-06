using System;
using System.Runtime.CompilerServices;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.Rendering;
using Code.CubeMarching.TerrainChunkSystem;
using Code.CubeMarching.Utils;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    public static class DistanceFieldResolver
    {
        public static void CalculateDistanceFieldForChunk(DynamicBuffer<TerrainChunkDataBuffer> terrainChunkBuffer, ref DistanceFieldChunkData distanceField, CTerrainEntityChunkPosition chunkPosition,
            BufferFromEntity<CSubGraphGeometryInstruction> getTerrainInstructionBuffer, Entity clusterEntity, int backgroundDataIndex, bool ignoreBackgroundData, CClusterParameters clusterParameters,
            NativeArray<float> valueBuffer)
        {
            TerrainChunkData terrainChunk = default;
            var existingData = terrainChunkBuffer[backgroundDataIndex].Value;

            if (clusterParameters.WriteMask[chunkPosition.indexInCluster])
            {
                //todo make the masks work again to reduce amount of computations
                var positionGS = chunkPosition.positionGS;

                var positionsToCheck = new NativeArray<PackedFloat3>(2, Allocator.Temp);
                for (var i = 0; i < 2; i++)
                {
                    var offsetInChunk = new PackedFloat3(new float4(2, 6, 2, 6), new float4(2, 2, 6, 6), new float4(i) * 4 + 2);
                    positionsToCheck[i] = new float3(positionGS.x, positionGS.y, positionGS.z) * SSpawnTerrainChunks.TerrainChunkLength + offsetInChunk;
                }

                var iterator = new TerrainInstructionIterator(positionsToCheck, getTerrainInstructionBuffer[clusterEntity].Reinterpret<GeometryInstruction>(), chunkPosition.indexInCluster, existingData, valueBuffer);

                iterator.CalculateTerrainData();

                GetCoverageAndFillMaskFromSurfaceDistance(iterator._terrainDataBuffer, out var mask, out var insideTerrainMask);

                distanceField.InnerDataMask = mask;
                distanceField.ChunkInsideTerrain = insideTerrainMask;

                positionsToCheck.Dispose();
            }
            else
            {
                distanceField.InnerDataMask = 0;
                if (ignoreBackgroundData)
                {
                    return;
                }
            }


            if (distanceField.InnerDataMask != 0)
            {
                CreatePositionsArray(distanceField, chunkPosition, out var positions);

                var iterator = new TerrainInstructionIterator(positions, getTerrainInstructionBuffer[clusterEntity].Reinterpret<GeometryInstruction>(), chunkPosition.indexInCluster, existingData, valueBuffer);
                iterator.CalculateTerrainData();

                terrainChunk = CopyResultsBackToBuffer(distanceField, terrainChunk, iterator);

                positions.Dispose();
                iterator.Dispose();
            }
            else
            {
                if (distanceField.ChunkInsideTerrain == 0)
                {
                    terrainChunk = TerrainChunkData.Outside;
                }
                else
                {
                    terrainChunk = TerrainChunkData.Inside;
                }
            }

            terrainChunkBuffer[distanceField.IndexInDistanceFieldBuffer] = new TerrainChunkDataBuffer {Value = terrainChunk};
        }

        private static TerrainChunkData CopyResultsBackToBuffer(DistanceFieldChunkData distanceField, TerrainChunkData terrainChunk, TerrainInstructionIterator iterator)
        {
            var readDataOffset = 0;
            for (var subChunkIndex = 0; subChunkIndex < 8; subChunkIndex++)
            {
                if (distanceField.InnerDataMask.GetBit(subChunkIndex))
                {
                    for (var i = 0; i < 16; i++)
                    {
                        terrainChunk[subChunkIndex * 16 + i] = iterator._terrainDataBuffer[readDataOffset + i];
                    }

                    readDataOffset += 16;
                }
                else
                {
                    for (var indexInSubChunk = 0; indexInSubChunk < 16; indexInSubChunk++)
                    {
                        //todo use a reasonable value instead of the hard coded 2
                        if (distanceField.ChunkInsideTerrain.GetBit(subChunkIndex))
                        {
                            terrainChunk[subChunkIndex * 16 + indexInSubChunk] = new PackedTerrainData(-2);
                        }
                        else
                        {
                            terrainChunk[subChunkIndex * 16 + indexInSubChunk] = new PackedTerrainData(2);
                        }
                    }
                }
            }

            return terrainChunk;
        }

        private static void GetCoverageAndFillMaskFromSurfaceDistance(NativeArray<PackedTerrainData> result, out byte mask, out byte insideTerrainMask)
        {
            mask = 0;
            insideTerrainMask = 0;
            for (var i = 0; i < 2; i++)
            {
                var surfaceDistance = result[i].SurfaceDistance;

                for (var j = 0; j < 4; j++)
                {
                    //it should be possible to set this tolerance smaller
                    if (surfaceDistance.PackedValues[j] < 5 && surfaceDistance.PackedValues[j] > -5)
                    {
                        mask |= (byte) (1 << (i * 4 + j));
                    }

                    if (surfaceDistance.PackedValues[j] < 0)
                    {
                        insideTerrainMask |= (byte) (1 << (i * 4 + j));
                    }
                }
            }
        }

        // Returns a temp array of the positions that actually need to be computed
        private static void CreatePositionsArray(DistanceFieldChunkData distanceField, CTerrainEntityChunkPosition chunk, out NativeArray<PackedFloat3> positions)
        {
            var countBits = math.countbits((int) distanceField.InnerDataMask);

            positions = new NativeArray<PackedFloat3>(16 * countBits, Allocator.Temp);
            new NativeArray<int>(16 * countBits, Allocator.Temp);

            var writtenPositionCount = 0;
            for (var subChunkIndex = 0; subChunkIndex < 8; subChunkIndex++)
            {
                if (distanceField.InnerDataMask.GetBit(subChunkIndex))
                {
                    for (var indexInSubChunk = 0; indexInSubChunk < 16; indexInSubChunk++)
                    {
                        var positionWS = IndexToPositionWSPacked(subChunkIndex, indexInSubChunk, chunk.positionGS);
                        positions[writtenPositionCount + indexInSubChunk] = positionWS;
                    }

                    writtenPositionCount += 16;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PackedFloat3 IndexToPositionWSPacked(int subChunkIndex, int indexInSubChunk, in float3 positionGS)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (subChunkIndex < 0 || subChunkIndex >= 8)
            {
                throw new ArgumentOutOfRangeException($"subChunkIndex is {subChunkIndex}, must be between 0 and 7");
            }

            if (indexInSubChunk < 0 || indexInSubChunk >= 16)
            {
                throw new ArgumentOutOfRangeException($"indexInSubChunk is {indexInSubChunk}, must be between 0 and 15");
            }
#endif

            var x = new float4(0, 1, 2, 3);
            float4 y = indexInSubChunk % 4;
            float4 z = indexInSubChunk / 4;

            x += subChunkIndex % 2 * 4;
            y += subChunkIndex / 2 % 2 * 4;
            z += subChunkIndex / 4 * 4;

            return new PackedFloat3(x, y, z) + positionGS * SSpawnTerrainChunks.TerrainChunkLength;
        }
    }
}