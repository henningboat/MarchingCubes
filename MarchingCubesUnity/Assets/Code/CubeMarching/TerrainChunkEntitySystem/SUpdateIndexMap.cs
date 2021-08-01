using Code.CubeMarching.Authoring;
using Code.CubeMarching.TerrainChunkSystem;
using Code.CubeMarching.Utils;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    [UpdateAfter(typeof(SBuildStaticGeometry))]
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    public class SUpdateIndexMap : SystemBase
    {
        #region Protected methods
    
        protected override void OnUpdate()
        {
            var indexMap = this.GetSingletonBuffer<TerrainChunkIndexMap>();

            var indexMapSize = GetSingleton<TotalClusterCounts>().Value * 8;
            
            Dependency = Entities.ForEach((CTerrainChunkStaticData staticDistanceField, CTerrainChunkDynamicData dynamicDistanceField, CTerrainEntityChunkPosition chunkPosition) =>
            {
                var indexInChunkMap = Utils.PositionToIndex(chunkPosition.positionGS, indexMapSize);

                var indexInDistanceFieldBuffer = 0;
                var hasData = false;
                if (dynamicDistanceField.DistanceFieldChunkData.HasData)
                {
                    indexInDistanceFieldBuffer = dynamicDistanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer;
                    hasData = true;
                }
                else if(staticDistanceField.DistanceFieldChunkData.HasData)
                {
                    indexInDistanceFieldBuffer = staticDistanceField.DistanceFieldChunkData.IndexInDistanceFieldBuffer;
                    hasData = true;
                }
    
                if (hasData)
                {
                    indexMap[indexInChunkMap] = new TerrainChunkIndexMap() {Index = indexInDistanceFieldBuffer};
                }
                else
                {
                    if (staticDistanceField.DistanceFieldChunkData.ChunkInsideTerrain == 0)
                    {
                        indexMap[indexInChunkMap] = new TerrainChunkIndexMap() {Index = 0};;
                    }
                    else
                    {
                        indexMap[indexInChunkMap] = new TerrainChunkIndexMap() {Index = 1};
                    }
                }
            }).Schedule(Dependency);
        }
    
        #endregion
    }
}