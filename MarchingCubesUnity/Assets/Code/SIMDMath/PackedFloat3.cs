using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Code.SIMDMath
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PackedFloat3
    {
        public bool Equals(PackedFloat3 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }

        public override bool Equals(object obj)
        {
            return obj is PackedFloat3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                hashCode = (hashCode * 397) ^ z.GetHashCode();
                return hashCode;
            }
        }

        public PackedFloat x;
        public PackedFloat y;
        public PackedFloat z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat3(float4 packedX, float4 packedY, float4 packedZ)
        {
            x = new PackedFloat(packedX);
            y = new PackedFloat(packedY);
            z = new PackedFloat(packedZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat3(PackedFloat x, PackedFloat y, PackedFloat z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat3(float3 a, float3 b, float3 c, float3 d)
        {
            x = new PackedFloat(new float4(a.x, b.x, c.x, d.x));
            y = new PackedFloat(new float4(a.y, b.y, c.y, d.y));
            z = new PackedFloat(new float4(a.z, b.z, c.z, d.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator +(PackedFloat3 a, PackedFloat3 b)
        {
            return new PackedFloat3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator +(PackedFloat3 a, PackedFloat b)
        {
            return new PackedFloat3(a.x + b, a.y + b, a.z + b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator -(PackedFloat3 a, PackedFloat3 b)
        {
            return new PackedFloat3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator -(PackedFloat3 a, PackedFloat b)
        {
            return new PackedFloat3(a.x - b, a.y - b, a.z - b);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator -(PackedFloat a, PackedFloat3 b)
        {
            return new PackedFloat3(b.x - a, b.y - a, b.z - a);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator *(PackedFloat3 a, PackedFloat3 b)
        {
            return new PackedFloat3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator *(PackedFloat3 a, PackedFloat b)
        {
            return new PackedFloat3(a.x * b, a.y * b, a.z * b);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 operator *(PackedFloat a, PackedFloat3 b)
        {
            return new PackedFloat3(a* b.x, a* b.y, a* b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator ==(PackedFloat3 a, PackedFloat3 b)
        {
            return (a.x == b.x) & (a.y == b.y) & (a.z == b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator !=(PackedFloat3 a, PackedFloat3 b)
        {
            return !(a == b);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PackedFloat3(float3 a)
        {
            return new PackedFloat3((PackedFloat) a.x, a.y, a.z);
        }
    }
    
    
    
     [StructLayout(LayoutKind.Sequential)]
    public struct PackedFloat2
    {
        public bool Equals(PackedFloat2 other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is PackedFloat2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                return hashCode;
            }
        }

        public PackedFloat x;
        public PackedFloat y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat2(float4 packedX, float4 packedY)
        {
            x = new PackedFloat(packedX);
            y = new PackedFloat(packedY);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat2(PackedFloat x, PackedFloat y)
        {
            this.x = x;
            this.y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PackedFloat2(float2 a, float2 b, float2 c, float2 d)
        {
            x = new PackedFloat(new float4(a.x, b.x, c.x, d.x));
            y = new PackedFloat(new float4(a.y, b.y, c.y, d.y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 operator +(PackedFloat2 a, PackedFloat2 b)
        {
            return new PackedFloat2(a.x + b.x, a.y + b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 operator +(PackedFloat2 a, PackedFloat b)
        {
            return new PackedFloat2(a.x + b, a.y + b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 operator -(PackedFloat2 a, PackedFloat2 b)
        {
            return new PackedFloat2(a.x - b.x, a.y - b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 operator -(PackedFloat2 a, PackedFloat b)
        {
            return new PackedFloat2(a.x - b, a.y - b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator ==(PackedFloat2 a, PackedFloat2 b)
        {
            return (a.x == b.x) & (a.y == b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool4 operator !=(PackedFloat2 a, PackedFloat2 b)
        {
            return !(a == b);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator PackedFloat2(float2 a)
        {
            return new PackedFloat2((PackedFloat) a.x, a.y);
        }
    }
}