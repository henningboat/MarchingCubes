using Code.CubeMarching.TerrainChunkSystem;
using Unity.Entities;
using UnityEngine;

namespace Code.CubeMarching
{
    //todo convert into more general TerrainShaper component?
    public struct CTerrainMaterial : IComponentData
    {
        #region Public Fields

        [HideInInspector] public PackedTerrainMaterial Material;

        #endregion
    }
}