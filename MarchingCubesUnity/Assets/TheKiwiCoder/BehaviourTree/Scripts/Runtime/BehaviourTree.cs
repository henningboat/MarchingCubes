using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace TheKiwiCoder {
    [CreateAssetMenu()]
    public class BehaviourTree : ScriptableObject {
        public GeometryNode rootNode;
        public List<GeometryNode> nodes = new List<GeometryNode>();
        public Blackboard blackboard = new Blackboard();


        #region Editor Compatibility
#if UNITY_EDITOR

        public GeometryNode CreateNode(System.Type type) {
            GeometryNode node = ScriptableObject.CreateInstance(type) as GeometryNode;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            node.GetPortInfo().ForEach((portInfo =>
            {
                portInfo.InitializeGUID();
            }));
            
            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            nodes.Add(node);

            if (!Application.isPlaying) {
                AssetDatabase.AddObjectToAsset(node, this);
            }

            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.SaveAssets();
            return node;
        }

        public void DeleteNode(GeometryNode node) {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

#endif
        #endregion Editor Compatibility
    }
}