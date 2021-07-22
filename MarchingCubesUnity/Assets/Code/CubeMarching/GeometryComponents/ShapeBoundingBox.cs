﻿using System.Runtime.InteropServices;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.Rendering;
using Code.SIMDMath;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Code.SIMDMath.SimdMath;

namespace Code.CubeMarching.GeometryComponents
{
	public class ShapeBoundingBox : GeometryContentAuthoringComponentBase<CShapeBoundingBox>
	{
		#region Serialize Fields

		[SerializeField] private float _boundsWidth = 1;

		#endregion

		#region Unity methods

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireCube(transform.position, transform.lossyScale);
		}

		#endregion

		#region Public methods

		protected override CShapeBoundingBox GetShape()
		{
			return new CShapeBoundingBox
			       {
				       extends = transform.lossyScale / 2f,
				       boundWidth = _boundsWidth,
			       };
		}

		#endregion
	}

	[StructLayout(LayoutKind.Explicit, Size = 4 * 16)]
	public struct CShapeBoundingBox : IComponentData, ITerrainModifierShape
	{
		#region Static Stuff

		//SDF code from
		//https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
		public static PackedFloat ComputeBoundingBoxDistance(PackedFloat3 p, PackedFloat3 b, PackedFloat e)
		{
			p = abs(p) - b;
			var q = abs(p + e) - e;
			return min(min(
					length(max(PackedFloat3(p.x, q.y, q.z), 0.0f)) + min(max(p.x, max(q.y, q.z)), 0.0f),
					length(max(PackedFloat3(q.x, p.y, q.z), 0.0f)) + min(max(q.x, max(p.y, q.z)), 0.0f)),
				length(max(PackedFloat3(q.x, q.y, p.z), 0.0f)) + min(max(q.x, max(q.y, p.z)), 0.0f));
		}

		#endregion

		#region Public Fields

		[FieldOffset(0)] public float boundWidth;
		[FieldOffset(4)] public float3 extends;

		#endregion

		#region ITerrainModifierShape Members

		public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
		{
			return ComputeBoundingBoxDistance(positionOS, extends, new PackedFloat(boundWidth, boundWidth, boundWidth, boundWidth));
		}

		public TerrainBounds CalculateBounds(Translation translation)
		{
			int3 center = (int3) math.round(translation.Value);
			int3 boundsExtends = (int3) math.ceil(extends);
			return new TerrainBounds
			       {
				       min = center - 1 - boundsExtends, max = center + boundsExtends,
			       };
		}

		public TerrainModifierType Type => TerrainModifierType.BoundingBox;

		#endregion
	}
}