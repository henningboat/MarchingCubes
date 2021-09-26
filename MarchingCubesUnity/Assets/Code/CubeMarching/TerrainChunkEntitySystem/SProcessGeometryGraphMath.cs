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

            Dependency = Entities.ForEach((CGeometryGraphInstance graphInstance, DynamicBuffer<CGeometryGraphPropertyValue> instancePropertyBuffer) =>
            {
                ref var blobPropertyBuffer = ref graphInstance.graph.Value.valueBuffer;
                ref var mathInstructions = ref graphInstance.graph.Value.mathInstructions;
                ref var overwritesBuffer = ref graphInstance.graph.Value.propertyOverwrites;

                if (instancePropertyBuffer.Length != blobPropertyBuffer.Length)
                {
                    instancePropertyBuffer.ResizeUninitialized(blobPropertyBuffer.Length);
                }

                for (var i = 0; i < instancePropertyBuffer.Length; i++)
                {
                    instancePropertyBuffer[i] = blobPropertyBuffer[i];
                }

                for (var i = 0; i < overwritesBuffer.Length; i++)
                {
                    var overwrite = getOverwritePropertyFromEntity[graphInstance.OverwriteEntity];

                    instancePropertyBuffer[overwritesBuffer[i].TargetIndex] = new CGeometryGraphPropertyValue() {Value = overwrite.Value[0].x};
                }

                for (var i = 0; i < mathInstructions.Length; i++)
                {
                    var instruction = mathInstructions[i];
                    instruction.Execute(instancePropertyBuffer);
                }
            }).WithBurst().WithReadOnly(getOverwritePropertyFromEntity).ScheduleParallel(Dependency);
        }
    }
}