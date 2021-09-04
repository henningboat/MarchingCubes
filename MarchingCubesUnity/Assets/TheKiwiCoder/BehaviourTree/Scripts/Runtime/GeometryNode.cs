using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TheKiwiCoder
{
    public abstract class GeometryNode : ScriptableObject
    {
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public Blackboard blackboard;
        [TextArea] public string description;
        public bool drawGizmos = false;

        [HideInInspector] [SerializeField] private List<GeometryNode> _inputs = new();


        public virtual Port.Capacity? InputPortCapacity => null;
        public virtual Port.Capacity? OutputPortCapacity => null;
        public List<GeometryNode> Inputs => _inputs;

        public void AddInputNode(GeometryNode child)
        {
            _inputs.Add(child);
        }

        public void RemoveInputNode(GeometryNode child)
        {
            _inputs.Remove(child);
        }
    }
}