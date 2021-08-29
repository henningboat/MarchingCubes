using Code.CubeMarching.Authoring;
using Code.CubeMarching.Rendering;
using Code.CubeMarching.StateHashing;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    [UpdateAfter(typeof(SCalculateSphereBounds))]
    public class SBuildStaticGeometry : SystemBase
    {
        #region Protected methods

        protected override void OnUpdate()
        {
            var terrainChunkBuffer = this.GetSingletonBuffer<TerrainChunkDataBuffer>();
            var isPlaying = Application.isPlaying && UnityEngine.Time.frameCount > 1;
            var getClusterParameters = GetComponentDataFromEntity<CClusterParameters>(true);
            GeometryInstructionsHasher hasher = new GeometryInstructionsHasher(this);

            int frameCount = GetSingleton<CFrameCount>().Value;
            
            //Static geometry 
            {
                //Write Instructions
                var writeJob = new TerrainInstructionWriter(this, GetSingletonEntity<CStaticTerrainCombiner>());
                Dependency = Entities.ForEach((ref DynamicBuffer<TerrainInstruction> terrainInstructions, ref CClusterParameters clusterParameters, in CClusterPosition clusterPosition) =>
                {
                    writeJob.Execute(terrainInstructions, ref clusterParameters, clusterPosition, isPlaying);
                }).WithName("WriteStaticTerrainInstructions").WithBurst().Schedule(Dependency);

                //Calculate Distance Fields
                var getTerrainInstructionBuffer = GetBufferFromEntity<TerrainInstruction>(true);

                Dependency = Entities.ForEach((ref CTerrainChunkStaticData distanceField, in ClusterChild clusterChild, in CTerrainEntityChunkPosition chunkPosition) =>
                {
                    var clusterParameters = getClusterParameters[clusterChild.ClusterEntity];
                    hasher.Execute(ref distanceField.DistanceFieldChunkData, chunkPosition, clusterParameters, clusterChild, frameCount);

                    if (!distanceField.DistanceFieldChunkData.InstructionsChangedSinceLastFrame)
                        return;

                    DistanceFieldResolver.CalculateDistanceFieldForChunk(terrainChunkBuffer, ref distanceField.DistanceFieldChunkData, chunkPosition, getTerrainInstructionBuffer,
                        clusterChild.ClusterEntity, distanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer, isPlaying, clusterParameters);
                }).WithNativeDisableParallelForRestriction(terrainChunkBuffer).WithReadOnly(getTerrainInstructionBuffer).WithReadOnly(getClusterParameters).WithBurst().ScheduleParallel(Dependency);
            }


            //Dynamic Geometry
            {
                //Write Instructions
                var writeJob = new TerrainInstructionWriter(this, GetSingletonEntity<CMainTerrainCombiner>());
                Dependency = Entities.ForEach((ref DynamicBuffer<TerrainInstruction> terrainInstructions, ref CClusterParameters clusterParameters, in CClusterPosition clusterPosition) =>
                {
                    writeJob.Execute(terrainInstructions, ref clusterParameters, clusterPosition, true);
                }).WithName("WriteDynamicTerrainInstructions").WithBurst().Schedule(Dependency);

                //Calculate Distance Fields
                var getTerrainInstructionBuffer = GetBufferFromEntity<TerrainInstruction>(true);

                Dependency = Entities.ForEach((ref CTerrainChunkDynamicData distanceField, in CTerrainChunkStaticData staticDistanceField, in CTerrainEntityChunkPosition chunkPosition,
                    in ClusterChild clusterChild) =>
                {
                    var clusterParameters = getClusterParameters[clusterChild.ClusterEntity];
                    hasher.Execute(ref distanceField.DistanceFieldChunkData, chunkPosition, clusterParameters, clusterChild, frameCount);

                    if (!distanceField.DistanceFieldChunkData.InstructionsChangedSinceLastFrame)
                        return;

                    int existingData;
                    if (staticDistanceField.DistanceFieldChunkData.HasData)
                    {
                        existingData = staticDistanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer;
                    }
                    else
                    {
                        if (staticDistanceField.DistanceFieldChunkData.InnerDataMask > 0)
                        {
                            existingData = 1;
                        }
                        else
                        {
                            existingData = 0; 
                        }
                    }

                    DistanceFieldResolver.CalculateDistanceFieldForChunk(terrainChunkBuffer, ref distanceField.DistanceFieldChunkData, chunkPosition, getTerrainInstructionBuffer,
                        clusterChild.ClusterEntity, existingData, true, clusterParameters);
                }).WithNativeDisableParallelForRestriction(terrainChunkBuffer).WithReadOnly(getTerrainInstructionBuffer).WithReadOnly(getClusterParameters).WithBurst().ScheduleParallel(Dependency);
            }
        }

        #endregion
    }

    public static class Extensions
    {
        public static DynamicBuffer<T> GetSingletonBuffer<T>(this SystemBase system) where T : struct, IBufferElementData
        {
            var singletonEntity = system.GetSingletonEntity<T>();
            return system.GetBuffer<T>(singletonEntity);
        }
    }
}