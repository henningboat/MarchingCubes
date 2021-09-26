﻿using System;
using System.Collections.Generic;
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

            float4x4 value = default;
            for (var i = 0; i < propertyOverwrite.Value.Length; i++)
            {
                value[i] = propertyOverwrite.Value[i];
            }

            DstEntityManager.AddComponentData(entity, new CGeometryGraphPropertyOverwriteProvider() {Value = value});
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

                    var propertyOverwrites = new List<GeometryPropertyOverwrite>();

                    var testOverwrite = Entity.Null;
                    foreach (var propertyOverwrite in graphInstance.Overwrites)
                    {
                        if (propertyOverwrite.ProviderObject == null)
                        {
                            var selectedProperty = resolvedGraph.GetExposedVariableProperty(propertyOverwrite.PropertyGUID);
                            if (selectedProperty != null)
                            {
                                var overwritePropertyProvider = SpawnPropertyOverwriteProvider(propertyOverwrite, graphInstance);
                                propertyOverwrites.Add(new GeometryPropertyOverwrite()
                                {
                                    PropertyType = selectedProperty.Type,
                                    TargetIndex = selectedProperty.Index,
                                    OverwritePropertyProvider = overwritePropertyProvider
                                });
                                testOverwrite = overwritePropertyProvider;
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }

                    var propertyOverwriteBlobArray = blobBuilder.Allocate(ref root.propertyOverwrites, propertyOverwrites.Count);
                    for (var i = 0; i < propertyOverwrites.Count; i++)
                    {
                        propertyOverwriteBlobArray[i] = propertyOverwrites[i];
                    }

                    DstEntityManager.AddComponentData(entity, new CGeometryGraphInstance()
                    {
                        graph = blobBuilder.CreateBlobAssetReference<GeometryGraphBlob>(Allocator.Persistent),
                        OverwriteEntity = testOverwrite
                    });

                    DstEntityManager.AddBuffer<CGeometryGraphPropertyValue>(entity);
                }
            });
        }
    }
}