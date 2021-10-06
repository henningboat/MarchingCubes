using Code.CubeMarching.GeometryGraph;
using Code.CubeMarching.GeometryGraph.Runtime;
using Code.CubeMarching.Rendering;
using Code.CubeMarching.Utils;
using Unity.Entities;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    public struct RuntimeGeometryGraphBuilder
    {
        private Entity _graphEntity;
        public ComponentDataFromEntity<CGeometryGraphInstance> GetGeometryGraphInstanceFromEntity;
        private readonly BufferFromEntity<CMainGraphGeometryInstruction> _instructionBuffer;
        private readonly BufferFromEntity<CMainGeometryGraphPropertyValue> _valueBuffer;


        public RuntimeGeometryGraphBuilder(SystemBase system, Entity graphEntity)
        {
            _graphEntity = graphEntity;
            GetGeometryGraphInstanceFromEntity = system.GetComponentDataFromEntity<CGeometryGraphInstance>(false);
            _instructionBuffer = system.GetBufferFromEntity<CMainGraphGeometryInstruction>();
            _valueBuffer = system.GetBufferFromEntity<CMainGeometryGraphPropertyValue>();
        }

        public void Execute(DynamicBuffer<CSubGraphGeometryInstruction> subGraphGeometryInstructions, ref CClusterParameters clusterParameters, in CClusterPosition clusterPosition, bool isPlaying,
            DynamicBuffer<CSubGeometryGraphPropertyValue> valueBuffer)
        {
            clusterParameters.WriteMask = BitArray512.AllBitsTrue;
            
            var geometryInstructionsCast = subGraphGeometryInstructions.Reinterpret<GeometryInstruction>();
            geometryInstructionsCast.Clear();

            var mainGraphInstructionBuffer = _instructionBuffer[_graphEntity].Reinterpret<GeometryInstruction>();

            for (var i = 0; i <mainGraphInstructionBuffer.Length; i++)
            {
                geometryInstructionsCast.Add(mainGraphInstructionBuffer[i]);
            }

            var mainGraphValueBuffer = _valueBuffer[_graphEntity].Reinterpret<float>();
            var valueBufferCast = valueBuffer.Reinterpret<float>();
            valueBufferCast.Clear();
            for (var i = 0; i < mainGraphValueBuffer.Length; i++)
            {
                valueBufferCast.Add(mainGraphValueBuffer[i]);
            }
        }
    }
}