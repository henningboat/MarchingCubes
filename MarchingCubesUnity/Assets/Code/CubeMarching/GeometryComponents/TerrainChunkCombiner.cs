using Unity.Entities;

namespace Code.CubeMarching
{
	public enum CombinerOperation : byte
	{
		Min = 0,
		Max = 1,
		SmoothMin = 2,
		SmoothSubtract = 3,
		Add = 4,
		Replace = 5,
		ReplaceMaterial = 6,
	}

	[InternalBufferCapacity(128)]
	public struct CTerrainChunkCombinerChild : IBufferElementData
	{
		#region Public Fields

		public Entity SourceEntity;

		#endregion
	}
}