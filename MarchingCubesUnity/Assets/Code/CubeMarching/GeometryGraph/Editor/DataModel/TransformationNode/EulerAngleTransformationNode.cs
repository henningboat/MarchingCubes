using System;
using Code.CubeMarching.GeometryGraph.Editor.Conversion;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor.DataModel.TransformationNode
{
    public class EulerAngleTransformationNode : TransformationNode, IGeometryNode
    {
        public IPortModel InEulerAngles;

        protected override void OnDefineNode()
        {
            base.OnDefineNode();
            
            InEulerAngles = this.AddDataInputPort<Vector3>("Rotation", nameof(InEulerAngles));
        }

        protected override GeometryGraphProperty GetTransformationInstruction(GeometryGraphResolverContext context, GeometryGraphProperty parent)
        {
            throw new NotImplementedException();
            //return context.PushEulerRotationInstruction(InEulerAngles.ResolvePropertyInput(context, GeometryPropertyType.Float3), parent);
        }
    }
}