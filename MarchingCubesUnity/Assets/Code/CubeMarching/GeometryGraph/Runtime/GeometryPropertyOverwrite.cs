﻿using System;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Unity.Entities;

namespace Code.CubeMarching.GeometryGraph.Runtime
{
    [Serializable]
    public struct GeometryPropertyOverwrite
    {
        public Entity OverwritePropertyProvider;
        public GeometryPropertyType PropertyType;
        public int TargetIndex;
    }
}