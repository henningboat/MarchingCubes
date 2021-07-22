using Code.CubeMarching.GeometryComponents;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Code.CubeMarching.Authoring
{
	public class TerrainCombiner : MonoBehaviour, ITerrainModifierEntitySource, IConvertGameObjectToEntity
	{
		#region Serialize Fields

		[SerializeField] private float _blendFactor;
		[SerializeField] private CombinerOperation _operation;

		#endregion

		#region Properties

		#endregion

		#region IConvertGameObjectToEntity Members

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			var children = dstManager.AddBuffer<CTerrainChunkCombinerChild>(entity);
			children.Clear();
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform childTransform = transform.GetChild(i);
				if (childTransform.GetComponent<ITerrainModifierEntitySource>() != null)
				{
					children.Add(new CTerrainChunkCombinerChild { SourceEntity = conversionSystem.GetPrimaryEntity(childTransform), });
				}
			}

			if ((transform.parent == null) || (transform.parent.GetComponent<TerrainCombiner>() == null))
			{
				dstManager.AddComponent<CTopLevelTerrainModifier>(entity);
			}
			
			
			if (transform.gameObject.isStatic)
			{
				dstManager.AddComponent<Static>(entity);
			}

			dstManager.AddComponent<CGeometryCombiner>(entity);
			dstManager.SetComponentData(entity, new CGeometryCombiner { Operation = _operation, BlendFactor = _blendFactor, });
		}

		#endregion
	}
}