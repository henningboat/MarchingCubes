using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
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
            var getOverwritePropertyFromEntity = GetComponentDataFromEntity<CGeometryGraphPropertyValueProvider>(true);

            Dependency = Entities.ForEach((CGeometryGraphInstance graphInstance, DynamicBuffer<CSubGeometryGraphPropertyValue> instancePropertyBuffer,
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
                    var overwriteProperty = overwriteProperties[i];
                    instancePropertyBuffer = ApplyOverwrite(instancePropertyBuffer, overwriteProperty, overwrite);
                }

                for (var i = 0; i < mathInstructions.Length; i++)
                {
                    var instruction = mathInstructions[i];
                    instruction.Execute(instancePropertyBuffer.Reinterpret<float>());
                }
            }).WithBurst().WithReadOnly(getOverwritePropertyFromEntity).ScheduleParallel(Dependency);
        }

        private static DynamicBuffer<CSubGeometryGraphPropertyValue> ApplyOverwrite(DynamicBuffer<CSubGeometryGraphPropertyValue> instancePropertyBuffer, CGeometryPropertyOverwrite overwriteProperty,
            CGeometryGraphPropertyValueProvider overwrite)
        {
            int componentCount;
            switch (overwriteProperty.PropertyType)
            {
                case GeometryPropertyType.Float:
                    componentCount = 1;
                    break;
                case GeometryPropertyType.Float3:
                    componentCount = 3;
                    break;
                case GeometryPropertyType.Float4X4:
                    componentCount = 16;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (var i = 0; i < componentCount; i++)
            {
                instancePropertyBuffer[overwriteProperty.TargetIndex + i] = new CSubGeometryGraphPropertyValue() {Value = overwrite.Value[i]};
            }

            return instancePropertyBuffer;
        }
    }

    public struct CMainGeometryGraphSingleton:IComponentData
    {
    }
}