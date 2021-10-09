using Code.CubeMarching.GeometryGraph.Runtime;
using Unity.Entities;
using UnityEngine;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateAfter(typeof(SProcessGeometryGraphMath))]
    public class SCombineGeometrySubGraphs : SystemBase
    {
        protected override void OnCreate()
        {
            var singleton = EntityManager.CreateEntity(typeof(GeometryInstruction), typeof(CMainGeometryGraphSingleton), typeof(CGeometryGraphPropertyValue));
            EntityManager.SetName(singleton, "MainGraphSingleton");
        }

        protected override void OnUpdate()
        {
            var globalGraphEntity = GetSingletonEntity<CMainGeometryGraphSingleton>();
            var globalInstructionBuffer = EntityManager.GetBuffer<GeometryInstruction>(globalGraphEntity);
            var globalValueBuffer = EntityManager.GetBuffer<CGeometryGraphPropertyValue>(globalGraphEntity);

            globalInstructionBuffer.Clear();
            globalValueBuffer.Clear();
            
            var getGeometryInstructions = GetBufferFromEntity<GeometryInstruction>();
            var getGraphValuesInstructions = GetBufferFromEntity<CGeometryGraphPropertyValue>();
            
            
            var readableInstructions = GetBufferFromEntity<GeometryInstruction>(true);
            var readableValues = GetBufferFromEntity<CGeometryGraphPropertyValue>(true);
            //
            // globalInstructionBuffer.Clear();
            // globalValueBuffer.Clear();

            Dependency = Entities.WithAll<GeometryInstruction,CGeometryGraphPropertyValue,CGeometryGraphInstance>().ForEach((Entity graphInstanceEntity) =>
                {
                     var globalInstructionBuffer = getGeometryInstructions[globalGraphEntity];
                     var globalValueBuffer = getGraphValuesInstructions[globalGraphEntity];
                    
                    var graphInstanceGeometryInstructions = readableInstructions[graphInstanceEntity];
                    var graphInstanceGraphValueBuffer = readableValues[graphInstanceEntity];
                    
                    var valueBufferOffset = globalValueBuffer.Length;
                    globalValueBuffer.AddRange(graphInstanceGraphValueBuffer.AsNativeArray());

                    foreach (var instruction in graphInstanceGeometryInstructions)
                    {
                        instruction.AddValueBufferOffset(valueBufferOffset);
                        globalInstructionBuffer.Add(instruction);
                    }
                }).WithBurst().WithReadOnly(readableInstructions).WithReadOnly(readableValues).WithNativeDisableContainerSafetyRestriction(readableInstructions).WithNativeDisableContainerSafetyRestriction(readableValues).
                WithNativeDisableContainerSafetyRestriction(getGeometryInstructions).WithNativeDisableContainerSafetyRestriction(getGraphValuesInstructions)
                .WithName("BuildMainGeometryGraph").Schedule(Dependency);
        }
    }
}