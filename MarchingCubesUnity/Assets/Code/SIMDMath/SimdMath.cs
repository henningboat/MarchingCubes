using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Code.SIMDMath
{
    public static class SimdMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat abs(PackedFloat a)
        {
            return new PackedFloat(math.abs(a.PackedValues));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 abs(PackedFloat3 a)
        {
            return new PackedFloat3(math.abs(a.x.PackedValues), math.abs(a.y.PackedValues), math.abs(a.z.PackedValues));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat min(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(math.min(a.PackedValues, b.PackedValues));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 min(PackedFloat3 a, PackedFloat3 b)
        {
            return new PackedFloat3(min(a.x, b.x), min(a.y, b.y), min(a.z, b.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 min(PackedFloat3 a, PackedFloat b)
        {
            return new PackedFloat3(min(a.x, b), min(a.y, b), min(a.z, b));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat max(PackedFloat a, PackedFloat b)
        {
            return new PackedFloat(math.max(a.PackedValues, b.PackedValues));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 max(PackedFloat3 a, PackedFloat b)
        {
            return new PackedFloat3(max(a.x, b), max(a.y, b), max(a.z, b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 max(PackedFloat3 a, PackedFloat3 b)
        {
            return new PackedFloat3(max(a.x, b.x), max(a.y, b.y), max(a.z, b.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat lerp(PackedFloat a, PackedFloat b, PackedFloat t)
        {
            return new PackedFloat(math.lerp(a.PackedValues, b.PackedValues, t.PackedValues));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 lerp(PackedFloat3 a, PackedFloat3 b, PackedFloat t)
        {
            return new PackedFloat3(lerp(a.x, b.x, t), lerp(a.y, b.y, t), lerp(a.z, b.z, t));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat dot(PackedFloat3 a, PackedFloat3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat dot(PackedFloat2 a, PackedFloat2 b)
        {
            return a.x * b.x + a.y * b.y;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat sqrt(PackedFloat a)
        {
            a.PackedValues = math.sqrt(a.PackedValues);
            return a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat length(PackedFloat3 a)
        {
            return sqrt(dot(a, a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat length(PackedFloat2 a)
        {
            return sqrt(dot(a, a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat clamp(PackedFloat a, PackedFloat min, PackedFloat max)
        {
            return new PackedFloat(math.clamp(a.PackedValues, min.PackedValues, max.PackedValues));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat clamp(PackedFloat a, float min, float max)
        {
            return new PackedFloat(math.clamp(a.PackedValues, min, max));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat3 PackedFloat3(PackedFloat x, PackedFloat y, PackedFloat z)
        {
            return new PackedFloat3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat2 PackedFloat2(PackedFloat x, PackedFloat y)
        {
            return new PackedFloat2(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat acos(PackedFloat value)
        {
            return new PackedFloat {PackedValues = math.acos(value.PackedValues)};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat sin(PackedFloat value)
        {
            return new PackedFloat {PackedValues = math.sin(value.PackedValues)};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat cos(PackedFloat value)
        {
            return new PackedFloat {PackedValues = math.cos(value.PackedValues)};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat atan(PackedFloat value)
        {
            return new PackedFloat {PackedValues = math.atan(value.PackedValues)};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat pow(PackedFloat x, PackedFloat y)
        {
            return new PackedFloat {PackedValues = math.pow(x.PackedValues, y.PackedValues)};
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PackedFloat log(PackedFloat x)
        {
            return new PackedFloat {PackedValues = math.log(x.PackedValues)};
        }
    }
}