using System;
using System.Linq;
using Code.CubeMarching.GeometryGraph.Runtime;
using UnityEditor;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    [CustomEditor(typeof(GeometryGraphInstance))]
    public class GeometryGraphInstanceEditor : UnityEditor.Editor
    {
        private SerializedProperty _geometryGraphProperty;
        private SerializedProperty _overwritesProperty;

        private void OnEnable()
        {
            _geometryGraphProperty = serializedObject.FindProperty("_geometryGraph");
        }

        public override void OnInspectorGUI()
        {
            var graph = _geometryGraphProperty.objectReferenceValue as GeometryGraphAsset;

            EditorGUILayout.PropertyField(_geometryGraphProperty);
            if (graph == null)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.LabelField(graph.name);

            GUILayout.Space(10);

            var geometryGraphInstance = (GeometryGraphInstance) target;
            var currentOverwrites = geometryGraphInstance.GetOverwrites();

            foreach (var variableDeclarationModel in graph.GraphModel.VariableDeclarations)
            {
                EditorGUILayout.BeginHorizontal();

                var currentOverwrite = currentOverwrites.FirstOrDefault(overwrite => overwrite.PropertyGUID == variableDeclarationModel.Guid);
                if (currentOverwrite == null)
                {
                    currentOverwrite = new GeometryGraphPropertyOverwrite(variableDeclarationModel.Guid);
                    currentOverwrites.Add(currentOverwrite);

                    ResetProperty(variableDeclarationModel, currentOverwrite);
                }

                if (variableDeclarationModel.DataType == TypeHandle.Float)
                {
                    currentOverwrite.SetValueCapacity(1);
                    currentOverwrite.Value[0] = EditorGUILayout.FloatField(variableDeclarationModel.GetVariableName(), currentOverwrite.Value[0]);
                }

                if (variableDeclarationModel.DataType == TypeHandle.Vector3)
                {
                    currentOverwrite.SetValueCapacity(3);

                    var currentValue = new Vector3(currentOverwrite.Value[0], currentOverwrite.Value[1], currentOverwrite.Value[2]);
                    currentValue = EditorGUILayout.Vector3Field(variableDeclarationModel.GetVariableName(), currentValue);

                    currentOverwrite.Value[0] = currentValue.x;
                    currentOverwrite.Value[1] = currentValue.y;
                    currentOverwrite.Value[2] = currentValue.z;
                }

                if (GUILayout.Button("Reset"))
                {
                    ResetProperty(variableDeclarationModel, currentOverwrite);
                }

                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                geometryGraphInstance.SetOverwrites(currentOverwrites);
                EditorUtility.SetDirty(geometryGraphInstance);
            }
        }

        private void ResetProperty(IVariableDeclarationModel variableDeclarationModel, GeometryGraphPropertyOverwrite currentOverwrite)
        {
            if (variableDeclarationModel.DataType == TypeHandle.Float)
            {
                currentOverwrite.SetValueCapacity(1);
                currentOverwrite.Value[0] = (float) variableDeclarationModel.InitializationModel.ObjectValue;
            }

            else if (variableDeclarationModel.DataType == TypeHandle.Vector3)
            {
                currentOverwrite.SetValueCapacity(3);

                var newValue = (Vector3) variableDeclarationModel.InitializationModel.ObjectValue;

                currentOverwrite.Value[0] = newValue.x;
                currentOverwrite.Value[1] = newValue.y;
                currentOverwrite.Value[2] = newValue.z;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}