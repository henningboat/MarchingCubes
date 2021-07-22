using Code.CubeMarching.Authoring;
using Unity.Entities;
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
            var getClusterPosition = GetComponentDataFromEntity<CClusterPosition>(true);

            //Static geometry 
            {
                //Write Instructions
                var writeJob = new TerrainInstructionWriter(this, GetSingletonEntity<CStaticTerrainCombiner>());
                Dependency = Entities.ForEach((ref DynamicBuffer<TerrainInstruction> terrainInstructions, ref CClusterPosition clusterPosition) =>
                {
                    writeJob.Execute(terrainInstructions, ref clusterPosition, isPlaying);
                }).WithName("WriteStaticTerrainInstructions").WithBurst().Schedule(Dependency);

                //Calculate Distance Fields
                var getTerrainInstructionBuffer = GetBufferFromEntity<TerrainInstruction>(true);

                Dependency = Entities.ForEach((ref CTerrainChunkStaticData distanceField, in CTerrainEntityChunkPosition chunkPosition) =>
                {
                    DistanceFieldResolver.CalculateDistanceFieldForChunk(terrainChunkBuffer, ref distanceField.DistanceFieldChunkData, chunkPosition, getTerrainInstructionBuffer,
                        getClusterPosition[distanceField.DistanceFieldChunkData.ClusterEntity],
                        distanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer, isPlaying);
                }).WithNativeDisableParallelForRestriction(terrainChunkBuffer).WithReadOnly(getTerrainInstructionBuffer).WithReadOnly(getClusterPosition).WithBurst().ScheduleParallel(Dependency);
            }


            //Dynamic Geometry
            {
                //Write Instructions
                var writeJob = new TerrainInstructionWriter(this, GetSingletonEntity<CMainTerrainCombiner>());
                Dependency = Entities.ForEach((ref DynamicBuffer<TerrainInstruction> terrainInstructions, ref CClusterPosition clusterPosition) =>
                {
                    writeJob.Execute(terrainInstructions, ref clusterPosition, true);
                }).WithName("WriteDynamicTerrainInstructions").WithBurst().Schedule(Dependency);

                //Calculate Distance Fields
                var getTerrainInstructionBuffer = GetBufferFromEntity<TerrainInstruction>(true);

                Dependency = Entities.ForEach((ref CTerrainChunkDynamicData distanceField, in CTerrainChunkStaticData staticData, in CTerrainEntityChunkPosition chunkPosition) =>
                {
                    int existingData;
                    if (staticData.DistanceFieldChunkData.HasData)
                    {
                        existingData = staticData.DistanceFieldChunkData.IndexInDistanceFieldBuffer;
                    }
                    else
                    {
                        if (staticData.DistanceFieldChunkData.InnerDataMask > 0)
                        {
                            existingData = 1;
                        }
                        else
                        {
                            existingData = 0;
                        }
                    }

                    DistanceFieldResolver.CalculateDistanceFieldForChunk(terrainChunkBuffer, ref distanceField.DistanceFieldChunkData, chunkPosition, getTerrainInstructionBuffer,
                        getClusterPosition[distanceField.DistanceFieldChunkData.ClusterEntity], existingData, true);
                }).WithNativeDisableParallelForRestriction(terrainChunkBuffer).WithReadOnly(getTerrainInstructionBuffer).WithReadOnly(getClusterPosition).WithBurst().ScheduleParallel(Dependency);
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