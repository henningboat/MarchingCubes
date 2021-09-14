using System;
using System.Runtime.InteropServices;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Code.SIMDMath.SimdMath;


namespace Code.CubeMarching.GeometryComponents
{
    public class ShapeSphere : GeometryContentAuthoringComponentBase<CShapeSphere>
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;

            Gizmos.DrawWireSphere(transform.position, transform.lossyScale.x);

            Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.01f);
            Gizmos.DrawSphere(Vector3.zero, 1);
        }

        protected override CShapeSphere GetShape()
        {
            var radius = transform.lossyScale.x;
            return new CShapeSphere {radius = radius};
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
    [Serializable]
    public struct CShapeSphere : IComponentData, ITerrainModifierShape
    {
        public bool Equals(CShapeSphere other)
        {
            return radius.Equals(other.radius);
        }

        public override bool Equals(object obj)
        {
            return obj is CShapeSphere other && Equals(other);
        }

        public override int GetHashCode()
        {
            return radius.GetHashCode();
        }

        #region ActualData

        [FieldOffset(0)] public float radius;

        #endregion

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            return length(positionOS) - radius;
        }

        public TerrainBounds CalculateBounds(Translation translation)
        {
            var center = translation.Value;
            return new TerrainBounds
            {
                min = center - radius, max = center + radius
            };
        }

        public uint CalculateHash()
        {
            return math.asuint(radius);
        }

        public TerrainModifierType Type => TerrainModifierType.Sphere;
    }
}