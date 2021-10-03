using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using JetBrains.Annotations;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    [UsedImplicitly]
    public class TranslationNode : TransformationNode, IGeometryNode
    {
        private IPortModel _inTranslation;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();

            _inTranslation = this.AddDataInputPort<Vector3>("Translation", nameof(_inTranslation));
        }

        protected override GeometryGraphProperty GetTransformationInstruction(GeometryGraphResolverContext context, GeometryGraphProperty parent)
        {
            var translationProperty = _inTranslation.ResolvePropertyInput(context, GeometryPropertyType.Float3);
            var transformationProperty = new GeometryGraphMathOperatorProperty(context, GeometryPropertyType.Float4X4, MathOperatorType.Translate, parent, translationProperty, "Translation");
            return context.GetOrCreateProperty(Guid, transformationProperty);
        }
    }
}