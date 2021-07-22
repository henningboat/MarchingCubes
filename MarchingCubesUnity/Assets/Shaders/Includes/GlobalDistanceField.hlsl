int numPointsPerAxis;


uint3 _TerrainMapSize;
int _MaterialIDFilter;


static const int terrainChunkLength = 8;


int indexFromCoordAndGridSize(int3 position, int3 gridSize)
{
    return position.z * gridSize.y * gridSize.x + position.y * gridSize.x + position.x;
}

int indexFromCoord(int x, int y, int z)
{
    return z * numPointsPerAxis * numPointsPerAxis + y * numPointsPerAxis + x;
}

int GetPointPositionInIndexMap(uint3 position)
{
    uint3 positionInIndexMap = 0;
    positionInIndexMap.x = position.x / 8;
    positionInIndexMap.y = position.y / 8;
    positionInIndexMap.z = position.z / 8;
    uint indexInTerrainIndexMap = indexFromCoordAndGridSize(positionInIndexMap, _TerrainMapSize);
    return indexInTerrainIndexMap;
}

float4 GetPointPosition(uint3 position)
{
    uint positionInIndexMap = GetPointPositionInIndexMap(position);
    uint terrainChunkCapacity = 512;
    uint baseIndexOfTerrainChunk = _GlobalTerrainIndexMap[positionInIndexMap] * terrainChunkCapacity;

    uint3 positionWithinTerrainChunk = ((position + 8) % 8);
    uint indexWithinTerrainChunk = indexFromCoord(positionWithinTerrainChunk.x, positionWithinTerrainChunk.y, positionWithinTerrainChunk.z);

    uint indexInTerrainBuffer = baseIndexOfTerrainChunk + indexWithinTerrainChunk;

    float surfaceDistance = _GlobalTerrainBuffer[indexInTerrainBuffer / 4].surfaceDistance[indexInTerrainBuffer % 4];
    if (_GlobalTerrainBuffer[indexInTerrainBuffer / 4].terrainMaterial.data[indexInTerrainBuffer % 4] != _MaterialIDFilter)
    {
        if(surfaceDistance<0)
        {
            if(_MaterialIDFilter==0){
                surfaceDistance = 0.000001;
            }else
            {
                surfaceDistance = -0.000001;
            }
        }
    }

    if (position.x <= 0 || position.y <= 0 || position.z <= 0 ||
        position.x > _TerrainMapSize.x * 8 - 1 ||
        position.y > _TerrainMapSize.y * 8 - 1 ||
        position.z > _TerrainMapSize.z * 8 - 1)
    {
        surfaceDistance = 0.1f;
    }

    return float4(position.x, position.y, position.z, surfaceDistance);
}

float4 GetColorAtPosition(int3 position)
{
    int positionInIndexMap = GetPointPositionInIndexMap(position);
    int terrainChunkCapacity = 512;
    int baseIndexOfTerrainChunk = _GlobalTerrainIndexMap[positionInIndexMap] * terrainChunkCapacity;

    int3 positionWithinTerrainChunk = ((position + 8) % 8);
    int indexWithinTerrainChunk = indexFromCoord(positionWithinTerrainChunk.x, positionWithinTerrainChunk.y, positionWithinTerrainChunk.z);

    int indexInTerrainBuffer = baseIndexOfTerrainChunk + indexWithinTerrainChunk;

    float4 color = _GlobalTerrainBuffer[indexInTerrainBuffer / 4].terrainMaterial.data[indexInTerrainBuffer % 4] == 0 ? float4(1, 0, 0, 1) : float4(0, 1, 1, 1);

    return color;
}

float GetSurfaceDistanceInterpolated(float3 positionWS)
{
    int3 id = floor(positionWS);


    float3 positinWithinCube = frac(positionWS);

    float4 bottonPlane = lerp(lerp(GetPointPosition(int3(id.x, id.y, id.z)), GetPointPosition(int3(id.x + 1, id.y, id.z)), positinWithinCube.x),
                              lerp(GetPointPosition(int3(id.x, id.y, id.z + 1)), GetPointPosition(int3(id.x + 1, id.y, id.z + 1)), positinWithinCube.x), positinWithinCube.z);

    float4 topPlane = lerp(lerp(GetPointPosition(int3(id.x, id.y + 1, id.z)), GetPointPosition(int3(id.x + 1, id.y + 1, id.z)), positinWithinCube.x),
                           lerp(GetPointPosition(int3(id.x, id.y + 1, id.z + 1)), GetPointPosition(int3(id.x + 1, id.y + 1, id.z + 1)), positinWithinCube.x), positinWithinCube.z);

    float4 result = lerp(bottonPlane, topPlane, positinWithinCube.y);
    return result.w;
}

float rand(float3 co)
{
    return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 22.2830))) * 43758.5453);
}

// float RayMarchThickness(float3 positionWS, float3 viewDir)
// {
//     viewDir += rand(positionWS) * 0.1f;
//     float3 origin = positionWS;
//     positionWS += viewDir;
//
//
//     float maxDistance = 0;
//     float totalThickness = 0;
//
//     //first travel inside the object
//
//     float surfaceDistance = 0;
//     bool reachedOutside = false;
//
//     for (int i = 0; i < 30; i++)
//     {
//         surfaceDistance = GetSurfaceDistanceInterpolated(positionWS);
//         if (surfaceDistance > 0)
//         {
//             viewDir = 0;
//             reachedOutside = true;
//         }
//         else
//         {
//             totalThickness++;
//         }
//         viewDir += rand((positionWS) + i) * 0.01f;
//         positionWS += viewDir * 0.5;
//     }
//
//     if (reachedOutside == false)
//     {
//         return 1;
//     }
//
//     float3 sunDirection = float3(0, 1, 0);
//     positionWS += sunDirection;
//
//     bool hitSomething = false;
//
//     for (int i = 0; i < 30; i++)
//     {
//         surfaceDistance = GetSurfaceDistanceInterpolated(positionWS);
//         if (surfaceDistance < 0)
//         {
//             hitSomething = true;
//         }
//         positionWS += sunDirection;
//     }
//
//     if (hitSomething)
//     {
//         return 1;
//     }
//
//     //from the other side of the object
//     return 1;
//     return totalThickness / 30;
// }
