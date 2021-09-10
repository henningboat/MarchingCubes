using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public abstract class ShapeNode<T> : GeometryNode, IShapeNode where T : ITerrainModifierShape
    {
        [HideInInspector] [SerializeField] private GeometryNodePort _geometryOutput;
        [SerializeField] private T _shape;


        public override List<GeometryNodePortDescription> GetPortInfo()
        {
            var serializedObject = new SerializedObject(this);
            return new List<GeometryNodePortDescription>()
            {
                new(serializedObject, nameof(_geometryOutput), "", Direction.Output, Port.Capacity.Multi)
            };
        }
    }

    public interface IShapeNode
    {
    }
}