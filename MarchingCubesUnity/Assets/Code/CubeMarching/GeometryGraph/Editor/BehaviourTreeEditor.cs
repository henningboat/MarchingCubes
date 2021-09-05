using System;
using System.Collections;
using System.Collections.Generic;
using Code.CubeMarching.GeometryGraph.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;

namespace TheKiwiCoder
{
    public class BehaviourTreeEditor : EditorWindow
    {
        private BehaviourTreeView treeView;
        private BehaviourTree tree;
        private InspectorView inspectorView;
        private IMGUIContainer blackboardView;
        private ToolbarMenu toolbarMenu;
        private TextField treeNameField;
        private TextField locationPathField;
        private Button createNewTreeButton;
        private VisualElement overlay;
        private BehaviourTreeSettings settings;

        private SerializedObject treeObject;
        private SerializedProperty blackboardProperty;

        [MenuItem("TheKiwiCoder/BehaviourTreeEditor ...")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
            wnd.minSize = new Vector2(800, 600);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTree)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        private List<T> LoadAssets<T>() where T : UnityEngine.Object
        {
            var assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assets = new List<T>();
            foreach (var assetId in assetIds)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetId);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }

            return assets;
        }

        public void CreateGUI()
        {
            settings = BehaviourTreeSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;

            // Import UXML
            var visualTree = settings.behaviourTreeXml;
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = settings.behaviourTreeStyle;
            root.styleSheets.Add(styleSheet);

            // Main treeview
            treeView = root.Q<BehaviourTreeView>();
            treeView.OnNodeSelected = OnNodeSelectionChanged;

            // Inspector View
            inspectorView = root.Q<InspectorView>();

            // Blackboard view
            blackboardView = root.Q<IMGUIContainer>();
            blackboardView.onGUIHandler = () =>
            {
                if (treeObject != null && treeObject.targetObject != null)
                {
                    treeObject.Update();
                    EditorGUILayout.PropertyField(blackboardProperty);
                    treeObject.ApplyModifiedProperties();
                }
            };

            // Toolbar assets menu
            toolbarMenu = root.Q<ToolbarMenu>();
            var behaviourTrees = LoadAssets<BehaviourTree>();
            behaviourTrees.ForEach(tree => { toolbarMenu.menu.AppendAction($"{tree.name}", (a) => { Selection.activeObject = tree; }); });
            toolbarMenu.menu.AppendSeparator();
            toolbarMenu.menu.AppendAction("New Tree...", (a) => CreateNewTree("NewBehaviourTree"));

            // New Tree Dialog
            treeNameField = root.Q<TextField>("TreeName");
            locationPathField = root.Q<TextField>("LocationPath");
            overlay = root.Q<VisualElement>("Overlay");
            createNewTreeButton = root.Q<Button>("CreateButton");
            createNewTreeButton.clicked += () => CreateNewTree(treeNameField.value);

            if (tree == null)
            {
                OnSelectionChange();
            }
            else
            {
                SelectTree(tree);
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void OnSelectionChange()
        {
            EditorApplication.delayCall += () =>
            {
                var tree = Selection.activeObject as BehaviourTree;

                SelectTree(tree);
            };
        }

        private void SelectTree(BehaviourTree newTree)
        {
            if (treeView == null)
            {
                return;
            }

            if (!newTree)
            {
                return;
            }

            tree = newTree;

            overlay.style.visibility = Visibility.Hidden;

            if (Application.isPlaying)
            {
                treeView.PopulateView(tree);
            }
            else
            {
                treeView.PopulateView(tree);
            }


            treeObject = new SerializedObject(tree);
            blackboardProperty = treeObject.FindProperty("blackboard");

            EditorApplication.delayCall += () => { treeView.FrameAll(); };
        }

        private void OnNodeSelectionChanged(NodeView node)
        {
            inspectorView.UpdateSelection(node);
        }

        private void OnInspectorUpdate()
        {
            treeView?.UpdateNodeStates();
        }

        private void CreateNewTree(string assetName)
        {
            var path = System.IO.Path.Combine(locationPathField.value, $"{assetName}.asset");
            var tree = CreateInstance<BehaviourTree>();
            tree.name = treeNameField.ToString();
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree;
            EditorGUIUtility.PingObject(tree);
        }
    }
}