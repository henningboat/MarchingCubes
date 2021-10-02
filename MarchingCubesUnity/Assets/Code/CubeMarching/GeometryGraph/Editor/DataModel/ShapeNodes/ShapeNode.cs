using System;
using System.Collections.Generic;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes
{
    [Serializable]
    public abstract class ShapeNode<T> : NodeModel, IGeometryNode where T : struct, ITerrainModifierShape
    {
        public IPortModel GeometryOut { get; set; }

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            GeometryOut = this.AddDataOutputPort<DistanceFieldValue>(nameof(GeometryOut));

            Color = new Color(0.4f, 0, 0);
        }

        public void WriteGeometryInstruction(ref GeometryInstruction instruction)
        {
        }

        protected abstract ShapeType GetShapeType();

        public abstract List<GeometryGraphProperty> GetProperties(GeometryGraphResolverContext context);

        public void Resolve(GeometryGraphResolverContext context, GeometryTransformationInstruction parent)
        {
            context.WriteShape(GetShapeType(), parent, GetProperties(context));
        }
    }
}