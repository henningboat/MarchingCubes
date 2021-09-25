using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;
using UnityEditor.GraphToolsFoundation.Searcher;
using UnityEngine;
using UnityEngine.GraphToolsFoundation.Overdrive;

namespace UnityEditor.GraphToolsFoundation.Overdrive.Samples.MathBook
{
    internal class MathBookStencil : Stencil, ISearcherDatabaseProvider
    {
        private List<SearcherDatabaseBase> m_Databases = new();

        public override string ToolName => GraphName;

        public static string GraphName => "Math Book";

        public MathBookStencil()
        {
            SearcherItem MakeSearcherItem((Type t, string name) tuple)
            {
                return new GraphNodeModelSearcherItem(GraphModel, null, data => data.CreateNode(tuple.t), tuple.name);
            }

            var shapes = TypeCache.GetTypesDerivedFrom(typeof(ShapeNode<>)).Where(type => !type.IsAbstract).Select(typeInfo => MakeSearcherItem((typeInfo, typeInfo.Name))).ToList();
            var shapeItems = new SearcherItem("Combiners", "", shapes.ToList());
            
            var combiners = TypeCache.GetTypesDerivedFrom(typeof(GeometryCombinerNode)).Where(type => !type.IsAbstract).Select(typeInfo => MakeSearcherItem((typeInfo, typeInfo.Name))).ToList();
            var combinerItems = new SearcherItem("Combiners", "", combiners.ToList());
            
            
            var constants = new List<SearcherItem>
            {
                new GraphNodeModelSearcherItem(GraphModel, null,
                    t => t.GraphModel.CreateConstantNode(TypeHandle.Float, "", t.Position, t.Guid, null, t.SpawnFlags),
                    "Constant"),
                MakeSearcherItem((typeof(PIConstant), "PI"))
            };

            var constantsItem = new SearcherItem("Values", "", constants);

            var items = new List<SearcherItem> {combinerItems, shapeItems, constantsItem};

            var searcherDatabase = new SearcherDatabase(items);
            m_Databases.Add(searcherDatabase);
        }

        public override IToolbarProvider GetToolbarProvider()
        {
            return new GeometryGraphToolbarProvider();
        }

        public override void OnGraphProcessingStarted(IGraphModel graphModel)
        {
            Debug.Log("Graph processing started");
        }

        public override Type GetConstantNodeValueType(TypeHandle typeHandle)
        {
            return TypeToConstantMapper.GetConstantNodeType(typeHandle);
        }

        public override ISearcherDatabaseProvider GetSearcherDatabaseProvider()
        {
            return this;
        }

        List<SearcherDatabaseBase> ISearcherDatabaseProvider.GetGraphElementsSearcherDatabases(IGraphModel graphModel)
        {
            return m_Databases;
        }

        private List<SearcherDatabaseBase> m_EmptyList = new();

        List<SearcherDatabaseBase> ISearcherDatabaseProvider.GetVariableTypesSearcherDatabases()
        {
            return m_EmptyList;
        }

        List<SearcherDatabaseBase> ISearcherDatabaseProvider.GetGraphVariablesSearcherDatabases(IGraphModel graphModel)
        {
            return m_Databases;
        }

        List<SearcherDatabaseBase> ISearcherDatabaseProvider.GetDynamicSearcherDatabases(IPortModel portModel)
        {
            return m_Databases;
        }

        public List<SearcherDatabaseBase> GetDynamicSearcherDatabases(IEnumerable<IPortModel> portModel)
        {
            return m_Databases;
        }

        /// <inheritdoc />
        public override IBlackboardGraphModel CreateBlackboardGraphModel(IGraphAssetModel graphAssetModel)
        {
            return new BlackboardGraphModel(graphAssetModel);
        }

        public override void PopulateBlackboardCreateMenu(string sectionName, GenericMenu menu, CommandDispatcher commandDispatcher)
        {
            menu.AddItem(new GUIContent("Create Variable"), false, () =>
            {
                const string newItemName = "variable";
                var finalName = newItemName;
                var i = 0;
                while (commandDispatcher.State.WindowState.GraphModel.VariableDeclarations.Any(v => v.Title == finalName))
                    finalName = newItemName + i++;

                commandDispatcher.Dispatch(new CreateGraphVariableDeclarationCommand(finalName, true, TypeHandle.Float, typeof(MathBookVariableDeclarationModel)));
            });

            menu.AddItem(new GUIContent("Create Vector3"), false, () =>
            {
                const string newItemName = "variable";
                var finalName = newItemName;
                var i = 0;
                while (commandDispatcher.State.WindowState.GraphModel.VariableDeclarations.Any(v => v.Title == finalName))
                    finalName = newItemName + i++;

                commandDispatcher.Dispatch(new CreateGraphVariableDeclarationCommand(finalName, true, TypeHandle.Vector3, typeof(MathBookVariableDeclarationModel)));
            });
        }
    }

    internal class GeometryGraphToolbarProvider : IToolbarProvider
    {
        public bool ShowButton(string buttonName)
        {
            return true;
        }
    }
}