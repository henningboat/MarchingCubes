struct PackedTerrainMaterial{
    int4 data;
};

struct TerrainData4{
    float4 surfaceDistance;
    PackedTerrainMaterial terrainMaterial;
};

StructuredBuffer<TerrainData4> _GlobalTerrainBuffer;
StructuredBuffer<int> _GlobalTerrainIndexMap;


