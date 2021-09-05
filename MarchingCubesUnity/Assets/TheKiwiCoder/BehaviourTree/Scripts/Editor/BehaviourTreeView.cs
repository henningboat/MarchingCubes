using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace TheKiwiCoder {
    public class BehaviourTreeView : GraphView {

        public Action<NodeView> OnNodeSelected;
        public new class UxmlFactory : UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> { }
        BehaviourTree tree;
        BehaviourTreeSettings settings;

        public struct ScriptTemplate {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        public BehaviourTreeView() {
            settings = BehaviourTreeSettings.GetOrCreateSettings();

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var styleSheet = settings.behaviourTreeStyle;
            styleSheets.Add(styleSheet);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo() {
            PopulateView(tree);
            AssetDatabase.SaveAssets();
        }

        public NodeView FindNodeView(GeometryNode node)
        {
            return GetNodeByGuid(node.guid) as NodeView;
        }

        internal void PopulateView(BehaviourTree tree) {
            this.tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (tree.rootNode == null) {
                tree.rootNode = tree.CreateNode(typeof(OutputNode)) as OutputNode;
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            // Creates node view
            tree.nodes.ForEach(n => CreateNodeView(n));
            
            // Create edges
            tree.nodes.ForEach(node =>
            {
                var inputs = node.GetPortInfo().Where(description => description.Direction == Direction.Input).ToList();
                
                inputs.ForEach(inputNode =>
                {
                    inputNode.GetGUIDAndConnections(out string selfGUID, out string[] connectionGUIDs);

                    foreach (var connectionPortGUIDs in connectionGUIDs)
                    {
                        var portByGuid = GetPortByGuid(connectionPortGUIDs);
                        if (portByGuid != null)
                        {
                            var edge = GetPortByGuid(selfGUID).ConnectTo(portByGuid);
                            AddElement(edge);
                        }
                    }
                });
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            if (graphViewChange.elementsToRemove != null) {
                graphViewChange.elementsToRemove.ForEach(elem => {
                    NodeView nodeView = elem as NodeView;
                    if (nodeView != null) {
                        tree.DeleteNode(nodeView.node);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null) {
                        NodePort inputPort = GetPortByGuid(edge.input.viewDataKey) as NodePort;
                        NodePort outputPort = GetPortByGuid(edge.output.viewDataKey) as NodePort;

                        outputPort.PortDescription.RemoveInput(inputPort.PortDescription.GUID);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    NodePort inputPort = GetPortByGuid(edge.input.viewDataKey) as NodePort;
                    NodePort outputPort = GetPortByGuid(edge.output.viewDataKey) as NodePort;
                    
                    inputPort.PortDescription.AddInput(outputPort.PortDescription.GUID);
                });
            }

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {

            //base.BuildContextualMenu(evt);

            // New script functions
            // evt.menu.AppendAction($"Create Script.../New Action Node", (a) => CreateNewScript(scriptFileAssets[0]));
            // evt.menu.AppendAction($"Create Script.../New Composite Node", (a) => CreateNewScript(scriptFileAssets[1]));
            // evt.menu.AppendAction($"Create Script.../New Decorator Node", (a) => CreateNewScript(scriptFileAssets[2]));
            // evt.menu.AppendSeparator();

            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            {
                {
                    var types = TypeCache.GetTypesDerivedFrom<ShapeNode>();
                    foreach (var type in types)
                    {
                        evt.menu.AppendAction($"[Shape]/{type.Name}", (a) => CreateNode(type, nodePosition));
                    }
                }
                {
                    var types = TypeCache.GetTypesDerivedFrom<CombinerNode>();
                    foreach (var type in types)
                    {
                        evt.menu.AppendAction($"[Combiner]/{type.Name}", (a) => CreateNode(type, nodePosition));
                    }
                }
            }
        }

        void SelectFolder(string path) {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            // Check the path has no '/' at the end, if it does remove it,
            // Obviously in this example it doesn't but it might
            // if your getting the path some other way.

            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

        void CreateNode(System.Type type, Vector2 position) {
            GeometryNode node = tree.CreateNode(type);
            node.position = position;
            CreateNodeView(node);
        }

        void CreateNodeView(GeometryNode node) {
            NodeView nodeView = new NodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        public void UpdateNodeStates() {
            nodes.ForEach(n => {
                NodeView view = n as NodeView;
                view.UpdateState();
            });
        }
    }
}