using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

        public virtual List<GeometryNodePortDescription> GetPortInfo() => new();
    }

    public  class GeometryNodePortDescription
    {
        private SerializedProperty Target;
        public readonly string PorpertyName;
        public readonly Direction Direction;
        public Port.Capacity Capacity;

        public GeometryNodePortDescription(SerializedObject serializedObject,string porpertyName, Direction direction, Port.Capacity capacity)
        {
            PorpertyName = porpertyName;
            Direction = direction;
            Capacity = capacity;
            Target = serializedObject.FindProperty(porpertyName);
        }
    }
    
    [Serializable]
    public class GeometryNodePort
    {
        private List<GeometryNodePort> _connection;
        public List<GeometryNodePort> Connection => _connection;
    }
}