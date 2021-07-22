struct PerVertexData{
    float3 positionWS;  
    float3 normalWS;
    float4 color;
};

StructuredBuffer<PerVertexData> _Triangles;
StructuredBuffer<int> _ArgsBuffer;

float3 _BasePosition;

void GetWorldPositionFromTriangleBuffer(uint vertexID, uint instanceID, out float3 positionWS, out float3 normalWS, out float4 color)
{
        positionWS = _Triangles[vertexID + instanceID*3].positionWS + _BasePosition;
        normalWS = _Triangles[vertexID + instanceID*3].normalWS;
        color= _Triangles[vertexID + instanceID*3].color;
}
 