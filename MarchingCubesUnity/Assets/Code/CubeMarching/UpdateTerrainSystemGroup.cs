using Unity.Entities;
using UnityEngine;

namespace Code.CubeMarching
{
    [ExecuteAlways]
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = false)]
    public class UpdateTerrainSystemGroup : ComponentSystemGroup
    {
    }
}