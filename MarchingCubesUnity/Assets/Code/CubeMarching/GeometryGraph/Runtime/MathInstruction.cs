using System;
using System.Reflection;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.GeometryNodes;
using Code.CubeMarching.GeometryGraph.Editor.DataModel.ShapeNodes;
using Code.CubeMarching.TerrainChunkEntitySystem;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

        public void Execute(DynamicBuffer<float> instancePropertyBuffer)
        {
            unsafe
            {
                switch (ResultType)
                {
                    case GeometryPropertyType.Float:
                        var floatResult = CalculateFloat(instancePropertyBuffer);
                        instancePropertyBuffer.Write(floatResult, ResultIndex);
                        break;
                    case GeometryPropertyType.Float3:
                        var float3Result = CalculateFloat3(instancePropertyBuffer);
                        instancePropertyBuffer.Write(float3Result, ResultIndex);
                        break;
                    case GeometryPropertyType.Float4X4:
                        float4x4 float4X4Result = CalculateFloat4X4(instancePropertyBuffer);
                        instancePropertyBuffer.Write(float4X4Result, ResultIndex);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private float3 CalculateFloat3(DynamicBuffer<float> instancePropertyBuffer)
        {
            throw new NotImplementedException();
        }

        private float4x4 CalculateFloat4X4(DynamicBuffer<float> instancePropertyBuffer)
        {
            switch (MathOperationType)
            {
                case MathOperatorType.Translate:
                    float4x4 transformation = instancePropertyBuffer.Read<float4x4>(InputAIndex);
                    float3 offset = instancePropertyBuffer.Read<float3>(InputBIndex);
                    
                    //kind of unintuitive, but the transformation we are calculating is actually worldToObject, 
                    //so we need to invert the offset
                    offset = -offset;
                    
                    return math.mul(transformation, float4x4.Translate(offset));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float CalculateFloat(DynamicBuffer<float> instancePropertyBuffer)
        {
            var inputA = instancePropertyBuffer.Read<float>(InputAIndex);
            var inputB = instancePropertyBuffer.Read<float>(InputBIndex);

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

    public static unsafe class DynamicBufferFloatExtensionMethods
    {
        public static T Read<T>(this DynamicBuffer<float> buffer, int index) where T : struct
        {
            return UnsafeUtility.ReadArrayElementWithStride<T>(buffer.GetUnsafePtr(), 1, sizeof(float) * index);
        }

        public static void Write<T>(this DynamicBuffer<float> buffer, T value, int index) where T : struct
        {
            UnsafeUtility.WriteArrayElementWithStride(buffer.GetUnsafeReadOnlyPtr(), 1, sizeof(float) * index, value);
        }
        
        public static T Read<T>(this NativeArray<float> buffer, int index) where T : struct
        {
            return UnsafeUtility.ReadArrayElementWithStride<T>(buffer.GetUnsafeReadOnlyPtr(), 1, sizeof(float) * index);
        }

        public static void Write<T>(this NativeArray<float> buffer, T value, int index) where T : struct
        {
            UnsafeUtility.WriteArrayElementWithStride(buffer.GetUnsafePtr(), 1, sizeof(float) * index, value);
        }
    }
}