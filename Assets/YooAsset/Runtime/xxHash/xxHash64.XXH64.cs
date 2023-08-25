// ReSharper disable InconsistentNaming

namespace Standart.Hash.xxHash
{
    using System.Runtime.CompilerServices;

    public static partial class xxHash64
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong XXH64(byte* input, int len, ulong seed)
        {
            ulong h64;

            if (len >= 32)
            {
                byte* end = input + len;
                byte* limit = end - 31;

                ulong v1 = seed + XXH_PRIME64_1 + XXH_PRIME64_2;
                ulong v2 = seed + XXH_PRIME64_2;
                ulong v3 = seed + 0;
                ulong v4 = seed - XXH_PRIME64_1;

                do
                {
                    v1 = XXH64_round(v1, *(ulong*) input); input += 8;
                    v2 = XXH64_round(v2, *(ulong*) input); input += 8;
                    v3 = XXH64_round(v3, *(ulong*) input); input += 8;
                    v4 = XXH64_round(v4, *(ulong*) input); input += 8;
                } while (input < limit);

                h64 = XXH_rotl64(v1, 1) +
                      XXH_rotl64(v2, 7) +
                      XXH_rotl64(v3, 12) +
                      XXH_rotl64(v4, 18);  
                
                h64 = XXH64_mergeRound(h64, v1);
                h64 = XXH64_mergeRound(h64, v2);
                h64 = XXH64_mergeRound(h64, v3);
                h64 = XXH64_mergeRound(h64, v4);
            }
            else
            {
                h64 = seed + XXH_PRIME64_5;
            }

            h64 += (ulong)len;
            
            return XXH64_finalize(h64, input, len);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH64_round(ulong acc, ulong input)
        {
            acc += input * XXH_PRIME64_2;
            acc  = XXH_rotl64(acc, 31);
            acc *= XXH_PRIME64_1;
            return acc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH64_mergeRound(ulong acc, ulong val)
        {
            val  = XXH64_round(0, val);
            acc ^= val;
            acc  = acc * XXH_PRIME64_1 + XXH_PRIME64_4;
            return acc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH64_avalanche(ulong hash)
        {
            hash ^= hash >> 33;
            hash *= XXH_PRIME64_2;
            hash ^= hash >> 29;
            hash *= XXH_PRIME64_3;
            hash ^= hash >> 32;
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong XXH64_finalize(ulong hash, byte* ptr, int len)
        {
            len &= 31;
            while (len >= 8) {
                ulong k1 = XXH64_round(0, *(ulong*)ptr);
                ptr += 8;
                hash ^= k1;
                hash  = XXH_rotl64(hash,27) * XXH_PRIME64_1 + XXH_PRIME64_4;
                len -= 8;
            }
            if (len >= 4) {
                hash ^= *(uint*)ptr * XXH_PRIME64_1;
                ptr += 4;
                hash = XXH_rotl64(hash, 23) * XXH_PRIME64_2 + XXH_PRIME64_3;
                len -= 4;
            }
            while (len > 0) {
                hash ^= (*ptr++) * XXH_PRIME64_5;
                hash = XXH_rotl64(hash, 11) * XXH_PRIME64_1;
                --len;
            }
            return  XXH64_avalanche(hash);
        }
    }
}
