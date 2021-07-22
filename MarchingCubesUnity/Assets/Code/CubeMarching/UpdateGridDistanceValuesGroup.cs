using Code.CubeMarching.Authoring;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Entities;
using UnityEngine;

namespace Code.CubeMarching
{
    [ExecuteAlways]
    [UpdateInGroup(typeof(UpdateTerrainSystemGroup))]
    [UpdateAfter(typeof(SBuildTerrainModifierDependencies))]
    public class UpdateGridDistanceValuesGroup : ComponentSystemGroup
    {
    }
}