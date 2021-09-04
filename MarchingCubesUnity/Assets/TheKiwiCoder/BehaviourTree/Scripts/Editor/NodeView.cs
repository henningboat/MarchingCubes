using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace TheKiwiCoder {

    public class NodeView : UnityEditor.Experimental.GraphView.Node {
        public Action<NodeView> OnNodeSelected;
        public GeometryNode node;
        public Port input;
        public Port output;

        public NodeView(GeometryNode node) : base(AssetDatabase.GetAssetPath(BehaviourTreeSettings.GetOrCreateSettings().nodeXml)) {
            this.node = node;
            this.node.name = node.GetType().Name;
            this.title = node.name.Replace("(Clone)", "").Replace("Node", "");
            this.viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();
        }

        private void SetupDataBinding() {
            Label descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(node));
        }

        private void SetupClasses() {
            //todo add something
            if (node is RootNode) {
                AddToClassList("root");
            }
        }

        private void CreateInputPorts()
        {
            if (node.InputPortCapacity.HasValue)
            {
                input = new NodePort(Direction.Input, node.InputPortCapacity.Value);

                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts() {
            
            if (node.OutputPortCapacity.HasValue)
            {
                output = new NodePort(Direction.Output, node.OutputPortCapacity.Value);

                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Behaviour Tree (Set Position");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected() {
            base.OnSelected();
            if (OnNodeSelected != null) {
                OnNodeSelected.Invoke(this);
            }
        }

        public void SortChildren() {
            //todo
            // if (node is CompositeNode composite) {
            //     composite.children.Sort(SortByHorizontalPosition);
            // }
        }

        private int SortByHorizontalPosition(GeometryNode left, GeometryNode right) {
            return left.position.x < right.position.x ? -1 : 1;
        }

        public void UpdateState() 
        {
            //todo find out how this actually works
            
            // RemoveFromClassList("running");
            // RemoveFromClassList("failure");
            // RemoveFromClassList("success");
            //
            // if (Application.isPlaying) {
            //     switch (node.state) {
            //         case Node.State.Running:
            //             if (node.started) {
            //                 AddToClassList("running");
            //             }
            //             break;
            //         case Node.State.Failure:
            //             AddToClassList("failure");
            //             break;
            //         case Node.State.Success:
            //             AddToClassList("success");
            //             break;
            //     }
            //}
        }
    }
}