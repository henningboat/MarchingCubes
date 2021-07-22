using System;
using System.Runtime.CompilerServices;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.TerrainChunkSystem;
using Code.CubeMarching.Utils;
using Code.SIMDMath;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Code.CubeMarching.TerrainChunkEntitySystem
{
   
    //todo re-implement this functionality
    public struct CTerrainShapeCoverageMask : IBufferElementData
    {
        #region Public Fields

        public byte CoverageMask;

        #endregion
    }
}