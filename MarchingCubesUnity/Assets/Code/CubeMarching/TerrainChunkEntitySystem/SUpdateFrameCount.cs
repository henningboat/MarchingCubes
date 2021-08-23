using Unity.Entities;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    [UpdateAfter(typeof(SSpawnTerrainChunks))]
    [WorldSystemFilter(WorldSystemFilterFlags.Editor | WorldSystemFilterFlags.Default)]
    public class SUpdateFrameCount : SystemBase
    {
        protected override void OnCreate()
        {
            EntityManager.CreateEntity(typeof(CFrameCount));
        }

        protected override void OnUpdate()
        {
            var frameCount = GetSingleton<CFrameCount>();
            frameCount.Value++;
            SetSingleton(frameCount);
        }

        protected override void OnDestroy()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<CFrameCount>());
        }
    }
}