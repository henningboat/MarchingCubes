using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    public abstract class GeometryNode : ScriptableObject
    {
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public Blackboard blackboard;

        public virtual List<GeometryNodePortDescription> GetPortInfo()
        {
            return new();
        }

        public abstract NodePort GetOutputPort();
    }

    public class GeometryNodePortDescription
    {
        public SerializedProperty Target;
        public readonly string PorpertyName;
        public readonly Direction Direction;
        public Port.Capacity Capacity;
        public string DisplayName;
        public SerializedObject SerializedObject;

        public GeometryNodePortDescription(SerializedObject serializedObject, string porpertyName, string displayName, Direction direction, Port.Capacity capacity)
        {
            SerializedObject = serializedObject;
            DisplayName = displayName;
            PorpertyName = porpertyName;
            Direction = direction;
            Capacity = capacity;
            Target = serializedObject.FindProperty(porpertyName);
        }

        public string GUID => Target.FindPropertyRelative("_guid").stringValue;

        public void GetGUIDAndConnections(out string selfGUID, out string[] connectionGUIDs)
        {
            selfGUID = Target.FindPropertyRelative("_guid").stringValue;

            var connectionsValue = Target.FindPropertyRelative("_connections");
            connectionGUIDs = new string[connectionsValue.arraySize];
            for (var i = 0; i < connectionsValue.arraySize; i++)
            {
                connectionGUIDs[i] = connectionsValue.GetArrayElementAtIndex(i).stringValue;
            }
        }

        public void InitializeGUID()
        {
            Target.FindPropertyRelative("_guid").stringValue = UnityEditor.GUID.Generate().ToString();
            SerializedObject.ApplyModifiedProperties();
        }

        public void AddInput(string connectionNode)
        {
            var arrayProperty = Target.FindPropertyRelative("_connections");
            if (Capacity == Port.Capacity.Single)
            {
                arrayProperty.arraySize = 1;
                arrayProperty.GetArrayElementAtIndex(0).stringValue = connectionNode;
            }

            if (Capacity == Port.Capacity.Multi)
            {
                arrayProperty.arraySize++;
                arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1).stringValue = connectionNode;
            }

            SerializedObject.ApplyModifiedProperties();
        }

        public void RemoveInput(string portDescriptionGUID)
        {
            var arrayProperty = Target.FindPropertyRelative("_connections");
            if (Capacity == Port.Capacity.Single)
            {
                arrayProperty.arraySize = 0;
            }
            else
            {
                for (var i = 0; i < arrayProperty.arraySize; i++)
                {
                    if (arrayProperty.GetArrayElementAtIndex(i).stringValue == portDescriptionGUID)
                    {
                        arrayProperty.DeleteArrayElementAtIndex(i);
                        i--;
                    }
                }
            }

            SerializedObject.ApplyModifiedProperties();
        }
    }

    [Serializable]
    public class GeometryNodePort
    {
        [SerializeField] private string _guid;
        [SerializeField] private List<string> _connections;
        public List<string> Connections => _connections;
        public string Guid => _guid;

        public GeometryNode GetFirstConnection(GeometryTree geometryTree)
        {
            if (_connections.Count == 0)
            {
                throw new ArgumentOutOfRangeException("_connections.Count == 0");
            }

            return geometryTree.GetNodeByOutputPortGUID(_connections[0]);
        }
    }
}