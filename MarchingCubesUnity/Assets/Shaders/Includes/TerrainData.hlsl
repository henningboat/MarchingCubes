struct PackedTerrainMaterial{
    int4 data;
};

struct TerrainData4{
    float4 surfaceDistance;
    PackedTerrainMaterial terrainMaterial;
};