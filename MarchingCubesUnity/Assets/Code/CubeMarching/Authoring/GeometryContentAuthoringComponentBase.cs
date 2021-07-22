using System;
using System.Runtime.InteropServices;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.Rendering;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.TerrainChunkSystem;
using Code.SIMDMath;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Code.CubeMarching.Authoring
{
    public abstract class GeometryContentAuthoringComponentBase<T> : MonoBehaviour, ITerrainModifierEntitySource, IConvertGameObjectToEntity where T : struct, IComponentData, ITerrainModifierShape
    {
        #region Serialize Fields

        [SerializeField] private TerrainMaterial _material;

        #endregion

        #region Properties

        public Transform Transform => transform;

        #endregion

        #region IConvertGameObjectToEntity Members

        public unsafe void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
        {
            entityManager.AddComponent<CGenericTerrainModifier>(entity);
            var componentData = GetShape();

            CGenericTerrainModifier genericComponentData = default;
            genericComponentData.TerrainModifierType = componentData.Type;

            var ptr = UnsafeUtility.AddressOf(ref genericComponentData.TerrainModifierDataA);
            UnsafeUtility.CopyStructureToPtr(ref componentData, ptr);

            entityManager.AddComponent<CGenericTerrainModifier>(entity);
            entityManager.AddComponent<CTerrainMaterial>(entity);
            entityManager.AddComponent<Translation>(entity);
            entityManager.AddComponent<CTerrainModifierBounds>(entity);
            entityManager.AddComponent<CTerrainModifierTransformation>(entity);
            entityManager.AddComponent<WorldToLocal>(entity);
            if (transform.parent == null || transform.parent.GetComponent<TerrainCombiner>() == null)
            {
                entityManager.AddComponent<CTopLevelTerrainModifier>(entity);
            }

            if (transform.gameObject.isStatic)
            {
                entityManager.AddComponent<Static>(entity);
            }

            entityManager.SetComponentData(entity, genericComponentData);
            entityManager.SetComponentData(entity, new Translation {Value = transform.position});
            entityManager.SetComponentData(entity, CTerrainModifierTransformation.GetFromTransform(transform));

            var terrainMaterial = new PackedTerrainMaterial(_material);
            entityManager.SetComponentData(entity, new CTerrainMaterial {Material = terrainMaterial});
        }

        #endregion

        #region Public methods

        protected abstract T GetShape();

        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CGenericTerrainModifier : IComponentData
    {
        #region Public Fields

        public Bytes16 TerrainModifierDataA;
        public Bytes16 TerrainModifierDataB;
        public Bytes16 TerrainModifierDataC;
        public Bytes16 TerrainModifierDataD;
        public TerrainModifierType TerrainModifierType;

        #endregion

        #region Public methods

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref TerrainModifierDataA);
                switch (TerrainModifierType)
                {
                    case TerrainModifierType.Sphere:
                        return ((CShapeSphere*) ptr)->GetSurfaceDistance(positionOS);
                        break;
                    case TerrainModifierType.BoundingBox:
                        return ((CShapeBoundingBox*) ptr)->GetSurfaceDistance(positionOS);
                        break;
                    case TerrainModifierType.Torus:
                        return ((CShapeTorus*) ptr)->GetSurfaceDistance(positionOS);
                        break;
                    case TerrainModifierType.Noise:
                        return ((CShapeNoise*) ptr)->GetSurfaceDistance(positionOS);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public TerrainBounds CalculateBounds(Translation translation)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref TerrainModifierDataA);
                switch (TerrainModifierType)
                {
                    case TerrainModifierType.Sphere:
                        return ((CShapeSphere*) ptr)->CalculateBounds(translation);
                        break;
                    case TerrainModifierType.BoundingBox:
                        return ((CShapeBoundingBox*) ptr)->CalculateBounds(translation);
                        break;
                    case TerrainModifierType.Torus:
                        return ((CShapeTorus*) ptr)->CalculateBounds(translation);
                        break;
                    case TerrainModifierType.Noise:
                        return ((CShapeNoise*) ptr)->CalculateBounds(translation);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion
    }
}