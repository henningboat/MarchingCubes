using System;
using System.Collections.Generic;
using Code.CubeMarching.Authoring;
using Code.CubeMarching.GeometryComponents;
using Code.CubeMarching.GeometryGraph.Editor.DataModel;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Code.CubeMarching.Utils;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Code.CubeMarching.GeometryGraph.Editor
{
    public class GeometryGraphResolverContext
    {
        internal abstract class GeometryGraphInstruction
        {
            public readonly int Depth;

            protected GeometryGraphInstruction(int depth)
            {
                Depth = depth;
            }

            public abstract GeometryInstruction GetInstruction();
        }

        internal class ShapeInstruction : GeometryGraphInstruction
        {
            public readonly ShapeType ShapeType;
            public readonly GeometryGraphProperty Position;
            public readonly List<GeometryGraphProperty> ShapeProperties;
            private CGeometryCombiner _combiner;

            public ShapeInstruction(ShapeType shapeType, GeometryGraphProperty position, List<GeometryGraphProperty> shapeProperties, int currentCombinerDepth,
                CGeometryCombiner combiner) : base(currentCombinerDepth)
            {
                _combiner = combiner;
                ShapeType = shapeType;
                Position = position;
                ShapeProperties = shapeProperties;
            }

            public override GeometryInstruction GetInstruction()
            {
                return new()
                {
                    CombinerDepth = Depth,
                    CoverageMask = BitArray512.AllBitsTrue,
                    DependencyIndex = default,Combiner = _combiner,
                    TerrainShape = new GeometryShapeTranslationTuple()
                    {
                        Translation = new CGeometryTransformation(float3.zero),
                        TerrainMaterial = default,
                        TerrainModifier = BuildGenericTerrainModifier()
                    },
                    TerrainInstructionType = TerrainInstructionType.Shape
                };
            }

            private CGenericTerrainModifier BuildGenericTerrainModifier()
            {
                unsafe
                {
                    var shape = new CGenericTerrainModifier();
                    shape.ShapeType = ShapeType;
                    if (ShapeProperties.Count > 16)
                    {
                        throw new ArgumentOutOfRangeException("There's no support for more than 16 properties");
                    }

                    for (var i = 0; i < ShapeProperties.Count; i++)
                    {
                        UnsafeUtility.WriteArrayElement((int*) UnsafeUtility.AddressOf(ref shape), i, ShapeProperties[i].Index);
                    }

                    return shape;
                }
            }
        }

        internal class CombinerInstruction : GeometryGraphInstruction
        {
            public readonly CombinerOperation Operation;
            public readonly GeometryGraphProperty Property;

            public CombinerInstruction(CombinerOperation operation, GeometryGraphProperty property, int currentCombinerDepth) : base(currentCombinerDepth)
            {
                Operation = operation;
                Property = property;
            }

            public override GeometryInstruction GetInstruction()
            {
                return new()
                {
                    CombinerDepth = Depth,
                    CoverageMask = BitArray512.AllBitsTrue,
                    DependencyIndex = Depth+1,
                    TerrainShape = default,
                    TerrainInstructionType = TerrainInstructionType.Combiner,
                    Combiner = new CGeometryCombiner()
                    {
                        Operation = Operation,
                        BlendFactor = 1
                    }
                };
            }
        }

        private List<GeometryGraphProperty> _properties = new();
        private List<GeometryGraphInstruction> _instructions = new();
        private CGeometryCombiner _currentCombiner;

        private int currentCombinerDepth;
        private List<float> _propertyValueBuffer;
        private List<GeometryInstruction> _instructionBuffer;

        public List<float> PropertyValueBuffer => _propertyValueBuffer;

        public List<GeometryInstruction> InstructionBuffer => _instructionBuffer;

        public void BeginWriteCombiner(CGeometryCombiner combiner)
        {
            _currentCombiner = combiner;
            currentCombinerDepth++;
        }

        public void FinishWritingCombiner(CombinerOperation operation, GeometryGraphProperty property)
        {
            RegisterProperty(property);
            currentCombinerDepth--;
            _instructions.Add(new CombinerInstruction(operation, property, currentCombinerDepth));
        }

        public void WriteShape(ShapeType shapeType, GeometryGraphProperty positionProperty, List<GeometryGraphProperty> getProperties)
        {
            _instructions.Add(new ShapeInstruction(shapeType, positionProperty, getProperties, currentCombinerDepth, _currentCombiner));
            RegisterProperty(positionProperty);
            getProperties.ForEach(RegisterProperty);

            _currentCombiner = default;
        }

        private void RegisterProperty(GeometryGraphProperty property)
        {
            if (!_properties.Contains(property))
            {
                _properties.Add(property);
            }
        }

        public void BuildBuffers()
        {
            _propertyValueBuffer = new List<float>();
            foreach (var property in _properties)
            {
                property.Index = PropertyValueBuffer.Count;
                switch (property.Type)
                {
                    case GeometryPropertyType.Float:
                        var constantFloatValue = (float) ((GeometryGraphConstant) property).ConstantValue;
                        PropertyValueBuffer.Add(constantFloatValue);
                        break;
                    case GeometryPropertyType.Float3:
                        var constantFloat3Value = (Vector3) ((GeometryGraphConstant) property).ConstantValue;
                        PropertyValueBuffer.Add(constantFloat3Value.x);
                        PropertyValueBuffer.Add(constantFloat3Value.y);
                        PropertyValueBuffer.Add(constantFloat3Value.z);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _instructionBuffer = new List<GeometryInstruction>();
            for (var i = 0; i < _instructions.Count; i++)
            {
                InstructionBuffer.Add(_instructions[i].GetInstruction());
            }
        }
    }
}