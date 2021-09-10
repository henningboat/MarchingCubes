using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif


namespace Code.CubeMarching.GeometryGraph.Runtime
{
    [CreateAssetMenu()]
    public class GeometryTree : ScriptableObject
    {
        public GeometryNode rootNode;
        public List<GeometryNode> nodes = new();
        public Blackboard blackboard = new();


        #region Editor Compatibility

#if UNITY_EDITOR

        public GeometryNode CreateNode(System.Type type)
        {
            var node = CreateInstance(type) as GeometryNode;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            node.GetPortInfo().ForEach(portInfo => { portInfo.InitializeGUID(); });

            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(GeometryNode node)
        {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

#endif

        #endregion Editor Compatibility

        public OutputNode GetRoot()
        {
            return nodes.FirstOrDefault(node => node is OutputNode) as OutputNode;
        }

        public GeometryNode GetNodeByOutputPortGUID(string guid)
        {
            return nodes.FirstOrDefault(node => node.GetOutputPort().viewDataKey == guid);
        }
    }
}