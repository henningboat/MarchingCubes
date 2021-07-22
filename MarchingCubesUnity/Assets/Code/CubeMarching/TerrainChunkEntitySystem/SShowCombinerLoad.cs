// using Unity.Collections;
// using Unity.Entities;
// using Unity.Mathematics;
//
// namespace Code.CubeMarching.TerrainChunkEntitySystem
// {
//     [UpdateAfter(typeof(SWriteTerrainInstructions))]
//     [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
//     public class SShowCombinerLoad : SystemBase
//     {
//         public static NativeList<GizmosVisualization> GizmosVisualizations;
//
//         protected override void OnCreate()
//         {
//             GizmosVisualizations = new NativeList<GizmosVisualization>(Allocator.Persistent);
//         }
//
//         protected override void OnDestroy()
//         {
//             GizmosVisualizations.Dispose();
//         }
//
//         protected override void OnUpdate()
//         {
//             NativeList<GizmosVisualization> visualizations = new NativeList<GizmosVisualization>(Allocator.TempJob);
//
//             var terrainChunkLength = SSpawnTerrainChunks.TerrainChunkLength;
//             Dependency = Entities.ForEach((in CTerrainEntityChunkPosition chunk, in CTerrainEntityChunkDistanceField distanceField) =>
//             {
//                 //todo reimplement
//                 // if (combinerDependencies.Length > 0)
//                 // {
//                 //     float3 position = (chunk.positionGS + new float3(0.5)) * terrainChunkLength;
//                 //     visualizations.Add(new GizmosVisualization() {transformation = float4x4.TRS(position, quaternion.identity, terrainChunkLength ),color = 0});
//                 //
//                 // }
//                 // if (distanceField.HasData)
//                 // {
//                 //     var mask = distanceField.InnerDataMask;
//                 //     for (int i = 0; i < 8; i++)
//                 //     {
//                 //         if ((mask & 1 << i) > 0)
//                 //         {
//                 //             float3 offset = new float3(i % 2, (i / 2) % 2, i / 4)*0.5f;
//                 //             float3 position = (chunk.positionGS + new float3(0.25f) + offset) * terrainChunkLength;
//                 //             visualizations.Add(new GizmosVisualization() {IsSubChunk = true, transformation = float4x4.TRS(position, quaternion.identity, terrainChunkLength * 0.5f),color = 1});
//                 //         }
//                 //     }
//                 // }
//             }).WithBurst().Schedule(Dependency);
//             var gizmosVisualizations = GizmosVisualizations;
//             Dependency = Job.WithCode(() =>
//             {
//                 gizmosVisualizations.Clear();
//                 gizmosVisualizations.CopyFrom(visualizations);
//             }).WithBurst().WithNativeDisableContainerSafetyRestriction(gizmosVisualizations).Schedule(Dependency);
//             visualizations.Dispose(Dependency);
//         }
//     }
// }