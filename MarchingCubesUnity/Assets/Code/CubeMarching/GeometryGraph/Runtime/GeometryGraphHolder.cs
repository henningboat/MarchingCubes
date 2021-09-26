using Code.CubeMarching.GeometryGraph.Editor;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public class GeometryGraphHolder : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] private GeometryGraphAsset _geometryGraph;

        public GeometryGraphAsset Graph => _geometryGraph;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            unsafe
            {
                if (_geometryGraph == null)
                {
                    return;
                }

                var resolvedGraph = _geometryGraph.ResolveGraph();

                using var blobBuilder = new BlobBuilder(Allocator.Temp);

                ref var root = ref blobBuilder.ConstructRoot<GeometryGraphBlob>();

                var mathInstructionsBlobArray = blobBuilder.Allocate(ref root.mathInstructions, resolvedGraph.MathInstructionBuffer.Count);

                for (var i = 0; i < resolvedGraph.MathInstructionBuffer.Count; i++)
                {
                    mathInstructionsBlobArray[i] = resolvedGraph.MathInstructionBuffer[i];
                }

                var geometryInstructionsBlobArray = blobBuilder.Allocate(ref root.geometryInstructions, resolvedGraph.GeometryInstructionBuffer.Count);

                for (var i = 0; i < resolvedGraph.GeometryInstructionBuffer.Count; i++)
                {
                    geometryInstructionsBlobArray[i] = resolvedGraph.GeometryInstructionBuffer[i];
                }

                var valueBufferBlobArray = blobBuilder.Allocate(ref root.valueBuffer, resolvedGraph.PropertyValueBuffer.Count);

                for (var i = 0; i < resolvedGraph.PropertyValueBuffer.Count; i++)
                {
                    valueBufferBlobArray[i] = new CGeometryGraphPropertyValue() {Value = resolvedGraph.PropertyValueBuffer[i]};
                }

                dstManager.AddComponentData(entity, new CGeometryGraphInstance()
                {
                    graph = blobBuilder.CreateBlobAssetReference<GeometryGraphBlob>(Allocator.Persistent)
                });

                dstManager.AddBuffer<CGeometryGraphPropertyValue>(entity);
            }
        }
    }


    public struct CGeometryGraphInstance : IComponentData
    {
        public BlobAssetReference<GeometryGraphBlob> graph;
    }
}