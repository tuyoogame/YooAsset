// ReSharper disable InconsistentNaming

using System.Runtime.CompilerServices;
namespace Standart.Hash.xxHash
{
    public static partial class xxHash128
    {
        private static readonly ulong XXH_PRIME64_1 = 11400714785074694791UL;
        private static readonly ulong XXH_PRIME64_2 = 14029467366897019727UL;
        private static readonly ulong XXH_PRIME64_3 = 1609587929392839161UL;
        private static readonly ulong XXH_PRIME64_4 = 9650029242287828579UL;
        private static readonly ulong XXH_PRIME64_5 = 2870177450012600261UL;

        private static readonly uint XXH_PRIME32_1 = 2654435761U;
        private static readonly uint XXH_PRIME32_2 = 2246822519U;
        private static readonly uint XXH_PRIME32_3 = 3266489917U;
        private static readonly uint XXH_PRIME32_4 = 668265263U;
        private static readonly uint XXH_PRIME32_5 = 374761393U;

        private static readonly int XXH_STRIPE_LEN = 64;
        private static readonly int XXH_ACC_NB = 8;
        private static readonly int XXH_SECRET_CONSUME_RATE = 8;
        private static readonly int XXH_SECRET_MERGEACCS_START = 11;
        private static readonly int XXH_SECRET_DEFAULT_SIZE = 192;
        private static readonly int XXH_SECRET_LASTACC_START = 7;

        private static readonly byte MM_SHUFFLE_0_3_0_1 = 0b0011_0001;
        private static readonly byte MM_SHUFFLE_1_0_3_2 = 0b0100_1110;

    


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint XXH_rotl32(uint x, int r)
        {
            return (x << r) | (x >> (32 - r));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH_rotl64(ulong x, int r)
        {
            return (x << r) | (x >> (64 - r));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong XXH_readLE64(byte* ptr)
        {
            return *(ulong*) ptr;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint XXH_readLE32(byte* ptr)
        {
            return *(uint*) ptr;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH_xorshift64(ulong v64, int shift)
        {
            return v64 ^ (v64 >> shift);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint XXH_swap32(uint x)
        {
            return ((x << 24) & 0xff000000 ) | 
                   ((x <<  8) & 0x00ff0000 ) | 
                   ((x >>  8) & 0x0000ff00 ) | 
                   ((x >> 24) & 0x000000ff );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH_swap64(ulong x)
        {
            return ((x << 56) & 0xff00000000000000UL) | 
                   ((x << 40) & 0x00ff000000000000UL) | 
                   ((x << 24) & 0x0000ff0000000000UL) | 
                   ((x << 8)  & 0x000000ff00000000UL) | 
                   ((x >> 8)  & 0x00000000ff000000UL) | 
                   ((x >> 24) & 0x0000000000ff0000UL) | 
                   ((x >> 40) & 0x000000000000ff00UL) | 
                   ((x >> 56) & 0x00000000000000ffUL);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH_mult32to64(ulong x, ulong y)
        {
            return (ulong)(uint)(x) * (ulong)(uint)(y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint128 XXH_mult64to128(ulong lhs, ulong rhs)
        {
         
            
            return XXH_mult64to128_scalar(lhs, rhs);
        }

 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint128 XXH_mult64to128_scalar(ulong lhs, ulong rhs)
        {
            ulong lo_lo = XXH_mult32to64(lhs & 0xFFFFFFFF, rhs & 0xFFFFFFFF);
            ulong hi_lo = XXH_mult32to64(lhs >> 32,        rhs & 0xFFFFFFFF);
            ulong lo_hi = XXH_mult32to64(lhs & 0xFFFFFFFF, rhs >> 32);
            ulong hi_hi = XXH_mult32to64(lhs >> 32,        rhs >> 32);
            
            ulong cross = (lo_lo >> 32) + (hi_lo & 0xFFFFFFFF) + lo_hi;
            ulong upper = (hi_lo >> 32) + (cross >> 32)        + hi_hi;
            ulong lower = (cross << 32) | (lo_lo & 0xFFFFFFFF);

            uint128 r128;
            r128.low64  = lower;
            r128.high64 = upper;
            return r128;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH_writeLE64(byte* dst, ulong v64)
        {
            *(ulong*) dst = v64;
        }
    }
}