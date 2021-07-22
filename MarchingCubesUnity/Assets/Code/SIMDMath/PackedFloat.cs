using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Code.SIMDMath
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedFloat
    {
        public float4 PackedValues;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator +(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(a.PackedValues + b.PackedValues);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator +(PackedFloat a, float b)
        {
            return new PackedFloat(a.PackedValues + b);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator -(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(a.PackedValues - b.PackedValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator -(PackedFloat a)
        {
            return new PackedFloat(-a.PackedValues);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator -(PackedFloat a, float b)
        {
            return new PackedFloat(a.PackedValues - b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator *(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(a.PackedValues * b.PackedValues);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat operator /(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(a.PackedValues / b.PackedValues);
        }

        public bool Equals(PackedFloat other)
        {
            return PackedValues.Equals(other.PackedValues);
        }

        public override bool Equals(object obj)
        {
            return obj is PackedFloat other && Equals(other);
        }

        public override int GetHashCode()
        {
            return PackedValues.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat(float4 packedValues)
        {
            PackedValues = packedValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat(float a, float b, float c, float d)
        {
            PackedValues = new float4(a, b, c, d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator ==(PackedFloat a, PackedFloat b)
        {
            return a.PackedValues == b.PackedValues;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator !=(PackedFloat a, PackedFloat b)
        {
            return !(a == b);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PackedFloat(float a)
        {
            return new PackedFloat(a);
        }
    }
}