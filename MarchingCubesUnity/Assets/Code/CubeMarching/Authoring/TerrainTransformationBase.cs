using Code.CubeMarching.GeometryComponents;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Code.CubeMarching.Authoring
{
    public abstract class TerrainTransformationBase<T> : MonoBehaviour, ITerrainModifierEntitySource, IConvertGameObjectToEntity where T : struct, ITerrainTransformation
    {
        #region IConvertGameObjectToEntity Members

        public unsafe void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var componentData = GetComponentData();

            CGenericTerrainTransformation genericComponentData = default;
            genericComponentData.TerrainTransformationType = componentData.TerrainTransformationType;

            var ptr = UnsafeUtility.AddressOf(ref genericComponentData.TerrainModifierDataA);
            UnsafeUtility.CopyStructureToPtr(ref componentData, ptr);

            dstManager.AddComponent<CGenericTerrainTransformation>(entity);
            dstManager.SetComponentData(entity, genericComponentData);
        }

        #endregion

        #region Protected methods

        protected abstract T GetComponentData();

        #endregion
    }

    //todo allow rotation of mirror axis
}