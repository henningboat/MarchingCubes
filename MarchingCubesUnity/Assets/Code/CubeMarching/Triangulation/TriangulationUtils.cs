using System.Runtime.InteropServices;
using Code.CubeMarching.Rendering;
using Code.CubeMarching.TerrainChunkSystem;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace Code.CubeMarching.Triangulation
{
    public static unsafe class TriangulationUtils
    {
        private const int terrainChunkLength = 8;
        private const int numPointsPerAxis = terrainChunkLength;

        private const float isoLevel = 0;
        private static readonly int3 _TerrainMapSize = 8;


        private static int indexFromCoordAndGridSize(int3 position, int3 gridSize)
        {
            return position.z * gridSize.y * gridSize.x + position.y * gridSize.x + position.x;
        }

        private static int indexFromCoord(int3 position, int sizeOfSubChunk = 8)
        {
            return position.z * sizeOfSubChunk * sizeOfSubChunk + position.y * sizeOfSubChunk + position.x;
        }

        private static int GetPointPositionInIndexMap(int3 position)
        {
            int3 positionInIndexMap = 0;
            positionInIndexMap.x = position.x / 8;
            positionInIndexMap.y = position.y / 8;
            positionInIndexMap.z = position.z / 8;
            var indexInTerrainIndexMap = indexFromCoordAndGridSize(positionInIndexMap, _TerrainMapSize);
            return indexInTerrainIndexMap;
        }

        private static float4 GetPointPosition(int3 position, ref TerrainCollisionDataAccessor terrain)
        {
            var positionInIndexMap = GetPointPositionInIndexMap(position);
            var terrainChunkCapacity = 512;
            var baseIndexOfTerrainChunk = terrain.AccessDetailBufferMapping(positionInIndexMap) * terrainChunkCapacity;

            var positionWithinTerrainChunk = (position + 8) % 8;

            int subChunkIndex = indexFromCoord(positionWithinTerrainChunk / 4, 2);
            int indexWithinSubChunk = indexFromCoord(positionWithinTerrainChunk % 4, 4);
            var indexWithinTerrainChunk = subChunkIndex * 64 + indexWithinSubChunk;

            var indexInTerrainBuffer = baseIndexOfTerrainChunk + indexWithinTerrainChunk;

            float surfaceDistance;

            if (position.x <= 0 || position.y <= 0 || position.z <= 0 ||
                position.x > _TerrainMapSize.x * 8 - 1 ||
                position.y > _TerrainMapSize.y * 8 - 1 ||
                position.z > _TerrainMapSize.z * 8 - 1)
            {
                surfaceDistance = 0.1f;
            }
            else
            {
                surfaceDistance = terrain.AccessDetailBuffer(indexInTerrainBuffer);
            }


            return new float4(position.x, position.y, position.z, surfaceDistance);
        }


        private static float3 interpolateVerts(float4 v1, float4 v2)
        {
            var t = (isoLevel - v1.w) / (v2.w - v1.w);
            return v1.xyz + t * (v2.xyz - v1.xyz);
        }

        public struct TriangleMarcher
        {
            private TerrainBounds _terrainBounds;
            private int _cubeIndex;
            private bool needsNewTriangle;
            public int _triangleIndex;

            public TriangleMarcher(TerrainBounds terrainBounds)
            {
                _terrainBounds = terrainBounds;
                _cubeIndex = 0;
                needsNewTriangle = true;
                _triangleIndex = 0;
            }
 
            public bool TryGetTriangleMarch(int3 positionWS, BlobAssetReference<TriangulationTables.TriangulationDataBlobAsset> triangulationData,  TerrainCollisionDataAccessor terrain,
                out float3 a, out float3 b, out float3 c, out uint collisionKey)
            {
                //todo cache this
                var _cubeCorners = (float4*) UnsafeUtility.Malloc(8, 4 * 4, Allocator.Temp);

                _cubeCorners[0] = GetPointPosition(new int3(positionWS.x, positionWS.y, positionWS.z), ref terrain);
                _cubeCorners[1] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y, positionWS.z), ref terrain);
                _cubeCorners[2] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y, positionWS.z + 1), ref terrain);
                _cubeCorners[3] = GetPointPosition(new int3(positionWS.x, positionWS.y, positionWS.z + 1), ref terrain);
                _cubeCorners[4] = GetPointPosition(new int3(positionWS.x, positionWS.y + 1, positionWS.z), ref terrain);
                _cubeCorners[5] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y + 1, positionWS.z), ref terrain);
                _cubeCorners[6] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y + 1, positionWS.z + 1), ref terrain);
                _cubeCorners[7] = GetPointPosition(new int3(positionWS.x, positionWS.y + 1, positionWS.z + 1), ref terrain);

                // Calculate unique index for each cube configuration.
                // There are 256 possible values
                // A value of 0 means cube is entirely inside surface; 255 entirely outside.
                // The value is used to look up the edge table, which indicates which edges of the cube are cut by the isosurface.
                _cubeIndex = 0;
                if (_cubeCorners[0].w > isoLevel)
                {
                    _cubeIndex |= 1;
                }

                if (_cubeCorners[1].w > isoLevel)
                {
                    _cubeIndex |= 2;
                }

                if (_cubeCorners[2].w > isoLevel)
                {
                    _cubeIndex |= 4;
                }

                if (_cubeCorners[3].w > isoLevel)
                {
                    _cubeIndex |= 8;
                }

                if (_cubeCorners[4].w > isoLevel)
                {
                    _cubeIndex |= 16;
                }

                if (_cubeCorners[5].w > isoLevel)
                {
                    _cubeIndex |= 32;
                }

                if (_cubeCorners[6].w > isoLevel)
                {
                    _cubeIndex |= 64;
                }

                if (_cubeCorners[7].w > isoLevel)
                {
                    _cubeIndex |= 128;
                }

                ref var triangulation = ref triangulationData.Value.triangulation;
                var hasTriangle = triangulation[_cubeIndex * 16 + _triangleIndex] >= 0;
                if (hasTriangle)
                {
                    FetchTriangle(positionWS, triangulationData, _cubeCorners, ref _triangleIndex, out a, out b, out c, out collisionKey);
                }
                else
                {
                    a = 0;
                    b = 0;
                    c = 0;
                    collisionKey = 0;
                }

                UnsafeUtility.Free(_cubeCorners, Allocator.Temp);

                return hasTriangle;
            }


            public void FetchTriangle(int3 positionWS, BlobAssetReference<TriangulationTables.TriangulationDataBlobAsset> triangulationData, float4* cubeCorners, ref int triangleIndex, out float3 a,
                out float3 b, out float3 c, out uint collisionKey)
            {
                ref var triangulation = ref triangulationData.Value.triangulation;

                ref var cornerIndexAFromEdge = ref triangulationData.Value.cornerIndexAFromEdge;
                ref var cornerIndexBFromEdge = ref triangulationData.Value.cornerIndexBFromEdge;

                // Get indices of corner points A and B for each of the three edges
                // of the cube that need to be joined to form the triangle.
                var a0 = cornerIndexAFromEdge[triangulation[_cubeIndex * 16 + triangleIndex]];
                var b0 = cornerIndexBFromEdge[triangulation[_cubeIndex * 16 + triangleIndex]];

                var a1 = cornerIndexAFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 1]];
                var b1 = cornerIndexBFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 1]];

                var a2 = cornerIndexAFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 2]];
                var b2 = cornerIndexBFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 2]];


                a = interpolateVerts(cubeCorners[a0], cubeCorners[b0]);
                b = interpolateVerts(cubeCorners[a1], cubeCorners[b1]);
                c = interpolateVerts(cubeCorners[a2], cubeCorners[b2]);

                collisionKey = PositionAndTriangleIndexToCollisionKey(positionWS, triangleIndex);

                triangleIndex += 3;
            }

            private static uint PositionAndTriangleIndexToCollisionKey(int3 positionWS, int triangleIndex)
            {
                var key = 0;
                key += positionWS.x;
                key += positionWS.y * 64;
                key += positionWS.z * 64 * 64;
                key *= 10;
                key += triangleIndex / 3;
                return (uint) key;
            }

            private static void CollisionKeyToPositionAndTriangleIndex(uint key, out int3 positionWS, out int triangleIndex)
            {
                triangleIndex = (int) key % 10 * 3;
                key /= 10;
                positionWS = new int3((int) key % 64, (int) (key / 64) % 64, (int) (key / 4096) % 64);
            }

            //todo unify code with above
            public static void GetTriangleFromColliderKey(uint index, BlobAssetReference<TriangulationTables.TriangulationDataBlobAsset> triangulationData, TerrainCollisionDataAccessor terrain,
                out float3 a, out float3 b, out float3 c)
            {
                // uint triangleIndex = index % 3 * 3;
                // int positionIndex = (int)index / 3;
                // var positionWS = new int3(positionIndex % 3, (positionIndex / 8) % 8, (positionIndex / 64) % 8);

                CollisionKeyToPositionAndTriangleIndex(index, out var positionWS, out var triangleIndex);

                //todo cache this
                var _cubeCorners = (float4*) UnsafeUtility.Malloc(8, 4 * 4, Allocator.Temp);

                _cubeCorners[0] = GetPointPosition(new int3(positionWS.x, positionWS.y, positionWS.z), ref terrain);
                _cubeCorners[1] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y, positionWS.z), ref terrain);
                _cubeCorners[2] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y, positionWS.z + 1), ref terrain);
                _cubeCorners[3] = GetPointPosition(new int3(positionWS.x, positionWS.y, positionWS.z + 1), ref terrain);
                _cubeCorners[4] = GetPointPosition(new int3(positionWS.x, positionWS.y + 1, positionWS.z), ref terrain);
                _cubeCorners[5] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y + 1, positionWS.z), ref terrain);
                _cubeCorners[6] = GetPointPosition(new int3(positionWS.x + 1, positionWS.y + 1, positionWS.z + 1), ref terrain);
                _cubeCorners[7] = GetPointPosition(new int3(positionWS.x, positionWS.y + 1, positionWS.z + 1), ref terrain);

                // Calculate unique index for each cube configuration.
                // There are 256 possible values
                // A value of 0 means cube is entirely inside surface; 255 entirely outside.
                // The value is used to look up the edge table, which indicates which edges of the cube are cut by the isosurface.
                var _cubeIndex = 0;
                if (_cubeCorners[0].w > isoLevel)
                {
                    _cubeIndex |= 1;
                }

                if (_cubeCorners[1].w > isoLevel)
                {
                    _cubeIndex |= 2;
                }

                if (_cubeCorners[2].w > isoLevel)
                {
                    _cubeIndex |= 4;
                }

                if (_cubeCorners[3].w > isoLevel)
                {
                    _cubeIndex |= 8;
                }

                if (_cubeCorners[4].w > isoLevel)
                {
                    _cubeIndex |= 16;
                }

                if (_cubeCorners[5].w > isoLevel)
                {
                    _cubeIndex |= 32;
                }

                if (_cubeCorners[6].w > isoLevel)
                {
                    _cubeIndex |= 64;
                }

                if (_cubeCorners[7].w > isoLevel)
                {
                    _cubeIndex |= 128;
                }

                ref var triangulation = ref triangulationData.Value.triangulation;


                ref var cornerIndexAFromEdge = ref triangulationData.Value.cornerIndexAFromEdge;
                ref var cornerIndexBFromEdge = ref triangulationData.Value.cornerIndexBFromEdge;

                // Get indices of corner points A and B for each of the three edges
                // of the cube that need to be joined to form the triangle.
                var firstEdge = triangulation[_cubeIndex * 16 + triangleIndex];
                var a0 = cornerIndexAFromEdge[firstEdge];
                var b0 = cornerIndexBFromEdge[firstEdge];

                var a1 = cornerIndexAFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 1]];
                var b1 = cornerIndexBFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 1]];

                var a2 = cornerIndexAFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 2]];
                var b2 = cornerIndexBFromEdge[triangulation[_cubeIndex * 16 + triangleIndex + 2]];


                a = interpolateVerts(_cubeCorners[a0], _cubeCorners[b0]);
                b = interpolateVerts(_cubeCorners[a1], _cubeCorners[b1]);
                c = interpolateVerts(_cubeCorners[a2], _cubeCorners[b2]);

                UnsafeUtility.Free(_cubeCorners, Allocator.Temp);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct TerrainCollisionDataAccessor
        {
            private int* DetailBufferMapping;
            private PackedTerrainData* DetailBuffer;

            public TerrainCollisionDataAccessor(int* detailBufferMapping, PackedTerrainData* detailBuffer)
            {
                DetailBufferMapping = detailBufferMapping;
                DetailBuffer = detailBuffer;
            }

            public int AccessDetailBufferMapping(int index)
            {
                unsafe
                {
                    return DetailBufferMapping[index];
                }
            }

            public float AccessDetailBuffer(int index)
            {
                unsafe
                {
                    int packedIndex = index / 4;
                    PackedTerrainData packedData = DetailBuffer[packedIndex];
                    return packedData.SurfaceDistance.PackedValues[index % 4];
                }
            }
        
        }
    }
}