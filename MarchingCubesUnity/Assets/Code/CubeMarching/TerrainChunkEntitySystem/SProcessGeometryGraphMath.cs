using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Runtime;
using Unity.Entities;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateAfter(typeof(SBuildStaticGeometry))]
    public class SProcessGeometryGraphMath : SystemBase
    {
        protected override void OnUpdate()
        {
            var getOverwritePropertyFromEntity = GetComponentDataFromEntity<CGeometryGraphPropertyOverwriteProvider>(true);

            Dependency = Entities.ForEach((CGeometryGraphInstance graphInstance, DynamicBuffer<CGeometryGraphPropertyValue> instancePropertyBuffer,
                DynamicBuffer<CGeometryPropertyOverwrite> overwriteProperties) =>
            {
                ref var blobPropertyBuffer = ref graphInstance.graph.Value.valueBuffer;
                ref var mathInstructions = ref graphInstance.graph.Value.mathInstructions;

                if (instancePropertyBuffer.Length != blobPropertyBuffer.Length)
                {
                    instancePropertyBuffer.ResizeUninitialized(blobPropertyBuffer.Length);
                }
                
                for (var i = 0; i < instancePropertyBuffer.Length; i++)
                {
                    instancePropertyBuffer[i] = blobPropertyBuffer[i];
                }

                for (var i = 0; i < overwriteProperties.Length; i++)
                {
                    var overwrite = getOverwritePropertyFromEntity[overwriteProperties[i].OverwritePropertyProvider];

                    instancePropertyBuffer[overwriteProperties[i].TargetIndex] = new CGeometryGraphPropertyValue() {Value = overwrite.Value[0]};
                }
                
                for (var i = 0; i < mathInstructions.Length; i++)
                {
                    MathInstruction instruction = mathInstructions[i];
                    instruction.Execute(instancePropertyBuffer);
                }
            }).WithBurst().WithReadOnly(getOverwritePropertyFromEntity).ScheduleParallel(Dependency);
        }
    }
}