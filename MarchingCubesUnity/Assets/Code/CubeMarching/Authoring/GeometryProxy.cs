using System.Linq;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Code.CubeMarching.Authoring
{
    public class GeometryProxy : MonoBehaviour, ITerrainModifierEntitySource, IConvertGameObjectToEntity
    {
        #region Serialize Fields

        [SerializeField] private GameObject _reference;

        #endregion

        #region IConvertGameObjectToEntity Members

        public unsafe void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            Entity referenceEntity;
            if (CheckSelfReference(this))
            {
                Debug.LogWarning("GeometryProxy references itself");
                referenceEntity = Entity.Null;
            }
            else
            {
                referenceEntity = conversionSystem.GetPrimaryEntity(_reference);
            }

            if (transform.parent == null || transform.parent.GetComponent<TerrainCombiner>() == null)
            {
                dstManager.AddComponent<CTopLevelTerrainModifier>(entity);
            }

            if (transform.gameObject.isStatic)
            {
                dstManager.AddComponent<Static>(entity);
            }

            var componentData = CTerrainModifierTransformation.GetFromTransform(transform);

            CGenericTerrainTransformation genericComponentData = default;
            genericComponentData.TerrainTransformationType = componentData.TerrainTransformationType;

            var ptr = UnsafeUtility.AddressOf(ref genericComponentData.TerrainModifierDataA);
            UnsafeUtility.CopyStructureToPtr(ref componentData, ptr);

            dstManager.AddComponent<CGenericTerrainTransformation>(entity);
            dstManager.SetComponentData(entity, genericComponentData);

            dstManager.AddComponent<CGeometryCombiner>(entity);
            dstManager.SetComponentData(entity, new CGeometryCombiner {Operation = CombinerOperation.Min});

            dstManager.AddBuffer<CTerrainChunkCombinerChild>(entity).Add(new CTerrainChunkCombinerChild {SourceEntity = referenceEntity});

            SBuildTerrainModifierDependencies.MarkEditorChange();
        }

        #endregion

        #region Private methods

        private bool CheckSelfReference(GeometryProxy geometryProxy)
        {
            //todo implement
            return false;
            var children = _reference.GetComponentsInChildren<GeometryProxy>().Contains(this);
        }

        #endregion
    }
}