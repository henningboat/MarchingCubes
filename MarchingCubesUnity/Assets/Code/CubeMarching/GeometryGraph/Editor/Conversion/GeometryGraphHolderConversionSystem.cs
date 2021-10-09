using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.GeometryGraph.Runtime;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    [UpdateInGroup(typeof(GameObjectDeclareReferencedObjectsGroup))]
    public class GeometryGraphHolderConversionReferenceSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((GeometryGraphInstance holder) =>
            {
                if (holder.Graph != null)
                {
                    DeclareAssetDependency(holder.gameObject, holder.Graph);
                }
            });
        }
    }

    public class GeometryGraphHolderConversionSystem : GameObjectConversionSystem
    {
        private Entity SpawnPropertyOverwriteProvider(GeometryGraphPropertyOverwrite propertyOverwrite, GeometryGraphInstance holder)
        {
            var entity = CreateAdditionalEntity(holder);

            Float16 value = default;
            for (var i = 0; i < propertyOverwrite.Value.Length; i++)
            {
                value[i] = propertyOverwrite.Value[i];
            }

            DstEntityManager.AddComponentData(entity, new CGeometryGraphPropertyValueProvider() {Value = value});
            DstEntityManager.SetName(entity, $"Property Overwrite {propertyOverwrite.PropertyGUID}");
            return entity;
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((GeometryGraphInstance graphInstance) =>
            {
                unsafe
                {
                    var entity = GetPrimaryEntity(graphInstance);

                    if (graphInstance.Graph == null)
                    {
                        return;
                    }

                    var resolvedGraph = graphInstance.Graph.ResolveGraph();

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

                    var propertyOverwrites = new List<CGeometryPropertyOverwrite>();

                    foreach (var propertyOverwrite in graphInstance.Overwrites)
                    {
                        if (propertyOverwrite.ProviderObject == null)
                        {
                            var selectedProperty = resolvedGraph.GetExposedVariableProperty(propertyOverwrite.PropertyGUID);
                            if (selectedProperty != null)
                            {
                                var overwritePropertyProvider = SpawnPropertyOverwriteProvider(propertyOverwrite, graphInstance);
                                propertyOverwrites.Add(new CGeometryPropertyOverwrite()
                                {
                                    PropertyType = selectedProperty.Type,
                                    TargetIndex = selectedProperty.Index,
                                    OverwritePropertyProvider = overwritePropertyProvider
                                });
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    propertyOverwrites.Add(new CGeometryPropertyOverwrite()
                        {PropertyType = GeometryPropertyType.Float4X4, TargetIndex = resolvedGraph.OriginTransformation.Index, OverwritePropertyProvider = entity});
                    
                    var overwriteBuffer = DstEntityManager.AddBuffer<CGeometryPropertyOverwrite>(entity);
                    overwriteBuffer.CopyFrom(propertyOverwrites.ToArray());

                    DstEntityManager.AddComponentData(entity, new CGeometryGraphInstance()
                    {
                        graph = blobBuilder.CreateBlobAssetReference<GeometryGraphBlob>(Allocator.Persistent)
                    });

                    DstEntityManager.AddBuffer<CGeometryGraphPropertyValue>(entity);
                    var instructionBuffer = DstEntityManager.AddBuffer<GeometryInstruction>(entity);
                    instructionBuffer.Reinterpret<GeometryInstruction>().CopyFrom(resolvedGraph.GeometryInstructionBuffer.ToArray());

                    DstEntityManager.AddComponent<CFloat4x4PropertyFromTransformation>(entity);
                    DstEntityManager.AddComponent<CGeometryGraphPropertyValueProvider>(entity);
                }
            });
        }
    }
}