using System.Collections.Generic;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook;
using UnityEngine;
using CGenericTerrainModifier = Code.CubeMarching.Authoring.CGenericTerrainModifier;

namespace Code.CubeMarching.GeometryGraph
{
    [ExecuteInEditMode]
    [ExecuteAlways]
    public class GeometryGraphTestSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((CGeometryGraphInstance geometryGraphInstance) =>
            {
                if (geometryGraphInstance.graph.IsCreated)
                {
                    if (geometryGraphInstance.graph.Value.instructions.Length > 0)
                    {
                        Debug.Log(geometryGraphInstance.graph.Value.instructions[0].TerrainInstructionType);
                    }
                }
                else
                {
                    Debug.Log("no instructions");
                }
            }).Run();
        }
    }

    public class GeometryGraphHolder : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] private MathBookAsset _geometryGraph;

        public MathBookAsset Graph => _geometryGraph;

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

                
                var instructionsBlobArray = blobBuilder.Allocate(ref root.instructions, resolvedGraph.InstructionBuffer.Count);
                
                for (int i = 0; i < resolvedGraph.InstructionBuffer.Count; i++)
                {
                    instructionsBlobArray[i] = resolvedGraph.InstructionBuffer[i];
                }

                var valueBufferBlobArray = blobBuilder.Allocate(ref root.valueBuffer, resolvedGraph.PropertyValueBuffer.Count);
                
                for (int i = 0; i < resolvedGraph.PropertyValueBuffer.Count; i++)
                {
                    valueBufferBlobArray[i] = resolvedGraph.PropertyValueBuffer[i];
                }

                dstManager.AddComponentData(entity, new CGeometryGraphInstance()
                {
                    graph = blobBuilder.CreateBlobAssetReference<GeometryGraphBlob>(Allocator.Persistent)
                });
            }
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
        public BlobArray<GeometryInstruction> instructions;
        public BlobArray<float> valueBuffer;
    }
}