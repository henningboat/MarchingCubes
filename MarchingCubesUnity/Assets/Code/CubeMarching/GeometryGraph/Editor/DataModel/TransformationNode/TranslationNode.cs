using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public class TranslationNode : TransformationNode, IGeometryNode
    {
        private IPortModel _inTranslation;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            _inTranslation = this.AddDataInputPort<Vector3>("Translation", nameof(_inTranslation));
        }

        protected override GeometryTransformationInstruction GetTransformationInstruction(GeometryGraphResolverContext context, GeometryTransformationInstruction parent)
        {
            return context.PushTranslation(_inTranslation.ResolvePropertyInput(context, GeometryPropertyType.Float3), parent);
        }
    }
}