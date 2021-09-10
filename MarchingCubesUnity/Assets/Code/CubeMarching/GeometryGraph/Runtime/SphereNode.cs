using System;
using Code.CubeMarching.GeometryComponents;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public class SphereNode : ShapeNode<CShapeSphere>
    {
        public override NodePort GetOutputPort()
        {
            throw new NotImplementedException();
        }
    }
}