using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Entities;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph
{
    public class GeometryGraphHolder : MonoBehaviour,IConvertGameObjectToEntity
    {
        [SerializeField] private MathBookAsset _geometryGraph;
        
        public MathBookAsset Graph => _geometryGraph;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (_geometryGraph == null)
            {
                return;
            }

            using var blobBuilder = new BlobBuilder(Allocator.Temp);

            ref var root = ref blobBuilder.ConstructRoot<GeometryGraphBlob>();
            root.name = _geometryGraph.name;

            var instructions = blobBuilder.Allocate(ref root.instructions, 10);
            for (int i = 0; i < instructions.Length; i++)
            {
                instructions[i] = new GeometryInstruction() {TerrainInstructionType = TerrainInstructionType.Shape};
            }
            
            dstManager.AddComponentData(entity, new CGeometryGraphInstance()
            {
                graph = blobBuilder.CreateBlobAssetReference<GeometryGraphBlob>(Allocator.Persistent)
            });
        }
    }
    
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class GeometryGraphConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((GeometryGraphHolder holder) =>
            {
                if (holder.Graph != null)
                {
                    DeclareAssetDependency(holder.gameObject, holder.Graph);
                }
            });
        }
    }

    public struct CGeometryGraphInstance : IComponentData
    {
        public BlobAssetReference<GeometryGraphBlob> graph;
    }

    public struct GeometryGraphBlob
    {
        public FixedString32 name;
        public BlobArray<GeometryInstruction> instructions;
    }
}
