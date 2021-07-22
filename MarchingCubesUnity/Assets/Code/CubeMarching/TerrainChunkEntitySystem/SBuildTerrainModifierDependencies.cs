using Code.CubeMarching.Authoring;
using Code.CubeMarching.GeometryComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
    [ExecuteAlways]
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(UpdateTerrainSystemGroup), OrderFirst = true)]
    public class SBuildTerrainModifierDependencies : SystemBase
    {
        private static bool _editorEntitiesChanged;

        protected override void OnCreate()
        {
            var mainCombinerEntity = EntityManager.CreateEntity(typeof(CMainTerrainCombiner), typeof(CTerrainChunkCombinerChild), typeof(CGeometryCombiner));
#if UNITY_EDITOR
            EntityManager.SetName(mainCombinerEntity, "TerrainMainCombiner");
#endif
            EntityManager.SetComponentData(mainCombinerEntity, new CGeometryCombiner {Operation = CombinerOperation.Min});

            var staticTerrainCombiner = EntityManager.CreateEntity(typeof(CStaticTerrainCombiner), typeof(CTerrainChunkCombinerChild), typeof(CGeometryCombiner));
#if UNITY_EDITOR
            EntityManager.SetName(staticTerrainCombiner, "StaticTerrainCombiner");
#endif
            EntityManager.SetComponentData(staticTerrainCombiner, new CGeometryCombiner {Operation = CombinerOperation.Min});
        }

        protected override void OnUpdate()
        {
            _editorEntitiesChanged = false;

            var staticTerrainCombinerEntity = GetSingletonEntity<CStaticTerrainCombiner>();
            var mainStaticCombinerEntity = GetSingletonEntity<CMainTerrainCombiner>();

            var needsToRebuildStaticGeometry = true;

            var topLevelStaticEntities = GetEntityQuery(typeof(CTopLevelTerrainModifier), typeof(Static), ComponentType.Exclude<IgnoreGeometry>()).ToEntityArray(Allocator.Temp);
            var topLevelDynamicEntities = GetEntityQuery(ComponentType.ReadWrite<CTopLevelTerrainModifier>(), ComponentType.Exclude<Static>(), ComponentType.Exclude<IgnoreGeometry>()).ToEntityArray(Allocator.Temp);


            var staticCombinerChildren = EntityManager.GetBuffer<CTerrainChunkCombinerChild>(staticTerrainCombinerEntity);
            staticCombinerChildren.Clear();
            staticCombinerChildren.AddRange(topLevelStaticEntities.Reinterpret<CTerrainChunkCombinerChild>());

            var mainCombinerChildren = EntityManager.GetBuffer<CTerrainChunkCombinerChild>(mainStaticCombinerEntity);
            mainCombinerChildren.Clear();

            var topLevelCombiners = new NativeList<Entity>(Allocator.Temp);

            mainCombinerChildren.AddRange(topLevelDynamicEntities.Reinterpret<CTerrainChunkCombinerChild>());

            topLevelCombiners.AddRange(mainCombinerChildren.AsNativeArray().Reinterpret<Entity>());

            if (Application.isPlaying)
            {
                foreach (var entity in topLevelStaticEntities)
                {
                   // EntityManager.AddComponent<IgnoreGeometry>(entity);
                }
            }

            topLevelCombiners.Dispose();
        }
        
        protected override void OnDestroy()
        {
            EntityManager.DestroyEntity(GetSingletonEntity<CMainTerrainCombiner>());
        }

        public static void MarkEditorChange()
        {
            _editorEntitiesChanged = true;
        }
    }

    public struct IgnoreGeometry : IComponentData
    {
    }
}