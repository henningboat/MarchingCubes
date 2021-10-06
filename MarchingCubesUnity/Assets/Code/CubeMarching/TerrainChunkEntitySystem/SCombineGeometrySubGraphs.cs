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
            var singleton = EntityManager.CreateEntity(typeof(CMainGraphGeometryInstruction), typeof(CMainGeometryGraphSingleton), typeof(CMainGeometryGraphPropertyValue));
            EntityManager.SetName(singleton, "MainGraphSingleton");
        }

        protected override void OnUpdate()
        {
            var globalGraphEntity = GetSingletonEntity<CMainGeometryGraphSingleton>();
            var globalGeometryInstructions = EntityManager.GetBuffer<CMainGraphGeometryInstruction>(globalGraphEntity).Reinterpret<GeometryInstruction>();
            var globalGraphPropertyValues = EntityManager.GetBuffer<CMainGeometryGraphPropertyValue>(globalGraphEntity).Reinterpret<float>();

            globalGeometryInstructions.Clear();
            globalGraphPropertyValues.Clear();

            Dependency = Entities.ForEach((DynamicBuffer<CSubGraphGeometryInstruction> subGraphGeometryInstructions, DynamicBuffer<CSubGeometryGraphPropertyValue> subGraphPropertyValues, in CGeometryGraphInstance _) =>
                {
                    var valueBufferOffset = globalGraphPropertyValues.Length;
                    globalGraphPropertyValues.AddRange(subGraphPropertyValues.Reinterpret<float>().AsNativeArray());

                    var castGeometryInstructions = subGraphGeometryInstructions.Reinterpret<GeometryInstruction>();

                    foreach (var instruction in castGeometryInstructions)
                    {
                        instruction.AddValueBufferOffset(valueBufferOffset);
                        globalGeometryInstructions.Add(instruction);
                    }
                }).WithBurst()
                .WithName("BuildMainGeometryGraph").Schedule(Dependency);
        }
    }
}