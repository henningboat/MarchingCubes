using System;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Code.CubeMarching.GeometryGraph.Editor.Conversion
{
    public struct MathInstruction
    {
        public MathOperatorType MathOperationType;
        public int InputAIndex;
        public GeometryPropertyType InputAType;
        public int InputBIndex;
        public GeometryPropertyType InputBType;
        public int ResultIndex;
        public GeometryPropertyType ResultType;

        public void Execute(DynamicBuffer<CGeometryGraphPropertyValue> instancePropertyBuffer)
        {
            switch (ResultType)
            {
                case GeometryPropertyType.Float:
                    var floatResult = CalculateFloat(instancePropertyBuffer);
                    instancePropertyBuffer[ResultIndex] = new CGeometryGraphPropertyValue() {Value = floatResult};
                    break;
                case GeometryPropertyType.Float3:
                    var float3Result = CalculateFloat3(instancePropertyBuffer);
                    instancePropertyBuffer[ResultIndex + 0] = new CGeometryGraphPropertyValue() {Value = float3Result.x};
                    instancePropertyBuffer[ResultIndex + 1] = new CGeometryGraphPropertyValue() {Value = float3Result.y};
                    instancePropertyBuffer[ResultIndex + 2] = new CGeometryGraphPropertyValue() {Value = float3Result.z};
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float3 CalculateFloat3(DynamicBuffer<CGeometryGraphPropertyValue> instancePropertyBuffer)
        {
            throw new NotImplementedException();
        }

        private float CalculateFloat(DynamicBuffer<CGeometryGraphPropertyValue> instancePropertyBuffer)
        {
            var inputA = instancePropertyBuffer[InputAIndex].Value;
            var inputB = instancePropertyBuffer[InputBIndex].Value;

            switch (MathOperationType)
            {
                case MathOperatorType.Addition:
                    return inputA + inputB;
                    break;
                case MathOperatorType.Subtraction:
                    return inputA - inputB;
                    break;
                case MathOperatorType.Multiplication:
                    return inputA * inputB;
                    break;
                case MathOperatorType.Division:
                    return inputA / inputB;
                case MathOperatorType.Min:
                    return math.min(inputA, inputB);
                    break;
                case MathOperatorType.Max:
                    return math.max(inputA, inputB);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}