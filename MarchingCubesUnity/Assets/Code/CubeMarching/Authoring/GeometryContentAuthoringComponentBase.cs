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
using Unity.Mathematics;
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
            genericComponentData.ShapeType = componentData.Type;

            var ptr = UnsafeUtility.AddressOf(ref genericComponentData.PropertyIndexesA);
            UnsafeUtility.CopyStructureToPtr(ref componentData, ptr);

            entityManager.AddComponent<CGenericTerrainModifier>(entity);
            entityManager.AddComponent<CTerrainMaterial>(entity);
            entityManager.AddComponent<Translation>(entity);
            entityManager.AddComponent<CTerrainModifierBounds>(entity);
            entityManager.AddComponent<CGeometryTransformation>(entity);
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
            entityManager.SetComponentData(entity, CGeometryTransformation.GetFromTransform(transform));

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

        public int4 PropertyIndexesA;
        public int4 PropertyIndexesB;
        public int4 PropertyIndexesC;
        public int4 PropertyIndexesD;
        public ShapeType ShapeType;

        #endregion

        #region Public methods

        public PackedFloat GetSurfaceDistance(PackedFloat3 positionOS, NativeArray<float> valueBuffer)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref PropertyIndexesA);
                switch (ShapeType)
                {
                    case ShapeType.Sphere:
                        return ((CShapeSphere*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    case ShapeType.BoundingBox:
                        return ((CShapeBoundingBox*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    case ShapeType.Torus:
                        return ((CShapeTorus*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    case ShapeType.Noise:
                        return ((CShapeNoise*) ptr)->GetSurfaceDistance(positionOS, valueBuffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public TerrainBounds CalculateBounds(Translation translation, NativeArray<float> valueBuffer)
        {
            unsafe
            {
                var ptr = UnsafeUtility.AddressOf(ref PropertyIndexesA);
                switch (ShapeType)
                {
                    case ShapeType.Sphere:
                        return ((CShapeSphere*) ptr)->CalculateBounds(translation, valueBuffer);
                        break;
                    case ShapeType.BoundingBox:
                        return ((CShapeBoundingBox*) ptr)->CalculateBounds(translation, valueBuffer);
                        break;
                    case ShapeType.Torus:
                        return ((CShapeTorus*) ptr)->CalculateBounds(translation, valueBuffer);
                        break;
                    case ShapeType.Noise:
                        return ((CShapeNoise*) ptr)->CalculateBounds(translation, valueBuffer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public unsafe uint CalculateHash()
        {
            var ptr = UnsafeUtility.AddressOf(ref PropertyIndexesA);
            switch (ShapeType)
            {
                case ShapeType.Sphere:
                    return ((CShapeSphere*) ptr)->CalculateHash();
                    break;
                case ShapeType.BoundingBox:
                    return ((CShapeBoundingBox*) ptr)->CalculateHash();
                    break;
                case ShapeType.Torus:
                    return ((CShapeTorus*) ptr)->CalculateHash();
                    break;
                case ShapeType.Noise:
                    return ((CShapeNoise*) ptr)->CalculateHash();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}