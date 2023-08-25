/*
* This is the auto generated code.
* All function calls are inlined in XXH64
* Please don't try to analyze it.
*/

using System.Runtime.CompilerServices;

namespace Standart.Hash.xxHash
{
    public partial class xxHash64
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong __inline__XXH64(byte* input, int len, ulong seed)
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
                    var reg1 = *((ulong*)(input + 0));
                    var reg2 = *((ulong*)(input + 8));
                    var reg3 = *((ulong*)(input + 16));
                    var reg4 = *((ulong*)(input + 24));

                    // XXH64_round
                    v1 += reg1 * XXH_PRIME64_2;
                    v1 = (v1 << 31) | (v1 >> (64 - 31));
                    v1 *= XXH_PRIME64_1;

                    // XXH64_round
                    v2 += reg2 * XXH_PRIME64_2;
                    v2 = (v2 << 31) | (v2 >> (64 - 31));
                    v2 *= XXH_PRIME64_1;

                    // XXH64_round
                    v3 += reg3 * XXH_PRIME64_2;
                    v3 = (v3 << 31) | (v3 >> (64 - 31));
                    v3 *= XXH_PRIME64_1;

                    // XXH64_round
                    v4 += reg4 * XXH_PRIME64_2;
                    v4 = (v4 << 31) | (v4 >> (64 - 31));
                    v4 *= XXH_PRIME64_1;
                    input += 32;
                } while (input < limit);

                h64 = ((v1 << 1) | (v1 >> (64 - 1))) +
                      ((v2 << 7) | (v2 >> (64 - 7))) +
                      ((v3 << 12) | (v3 >> (64 - 12))) +
                      ((v4 << 18) | (v4 >> (64 - 18)));

                // XXH64_mergeRound
                v1 *= XXH_PRIME64_2;
                v1 = (v1 << 31) | (v1 >> (64 - 31));
                v1 *= XXH_PRIME64_1;
                h64 ^= v1;
                h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;

                // XXH64_mergeRound
                v2 *= XXH_PRIME64_2;
                v2 = (v2 << 31) | (v2 >> (64 - 31));
                v2 *= XXH_PRIME64_1;
                h64 ^= v2;
                h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;

                // XXH64_mergeRound
                v3 *= XXH_PRIME64_2;
                v3 = (v3 << 31) | (v3 >> (64 - 31));
                v3 *= XXH_PRIME64_1;
                h64 ^= v3;
                h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;

                // XXH64_mergeRound
                v4 *= XXH_PRIME64_2;
                v4 = (v4 << 31) | (v4 >> (64 - 31));
                v4 *= XXH_PRIME64_1;
                h64 ^= v4;
                h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;
            }
            else
            {
                h64 = seed + XXH_PRIME64_5;
            }

            h64 += (ulong) len;

            // XXH64_finalize
            len &= 31;
            while (len >= 8) {
                ulong k1 = XXH64_round(0, *(ulong*)input);
                input += 8;
                h64 ^= k1;
                h64  = XXH_rotl64(h64,27) * XXH_PRIME64_1 + XXH_PRIME64_4;
                len -= 8;
            }
            if (len >= 4) {
                h64 ^= *(uint*)input * XXH_PRIME64_1;
                input += 4;
                h64 = XXH_rotl64(h64, 23) * XXH_PRIME64_2 + XXH_PRIME64_3;
                len -= 4;
            }
            while (len > 0) {
                h64 ^= (*input++) * XXH_PRIME64_5;
                h64 = XXH_rotl64(h64, 11) * XXH_PRIME64_1;
                --len;
            }

            // XXH64_avalanche
            h64 ^= h64 >> 33;
            h64 *= XXH_PRIME64_2;
            h64 ^= h64 >> 29;
            h64 *= XXH_PRIME64_3;
            h64 ^= h64 >> 32;

            return h64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void __inline__XXH64_stream_process(byte[] input, int len, ref ulong v1, ref ulong v2, ref ulong v3,
            ref ulong v4)
        {
            fixed (byte* pData = &input[0])
            {
                byte* ptr = pData;
                byte* limit = ptr + len;

                do
                {
                    var reg1 = *((ulong*)(ptr + 0));
                    var reg2 = *((ulong*)(ptr + 8));
                    var reg3 = *((ulong*)(ptr + 16));
                    var reg4 = *((ulong*)(ptr + 24));

                    // XXH64_round
                    v1 += reg1 * XXH_PRIME64_2;
                    v1 = (v1 << 31) | (v1 >> (64 - 31));
                    v1 *= XXH_PRIME64_1;

                    // XXH64_round
                    v2 += reg2 * XXH_PRIME64_2;
                    v2 = (v2 << 31) | (v2 >> (64 - 31));
                    v2 *= XXH_PRIME64_1;

                    // XXH64_round
                    v3 += reg3 * XXH_PRIME64_2;
                    v3 = (v3 << 31) | (v3 >> (64 - 31));
                    v3 *= XXH_PRIME64_1;

                    // XXH64_round
                    v4 += reg4 * XXH_PRIME64_2;
                    v4 = (v4 << 31) | (v4 >> (64 - 31));
                    v4 *= XXH_PRIME64_1;
                    ptr += 32;
                } while (ptr < limit);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong __inline__XXH64_stream_finalize(byte[] input, int len, ref ulong v1, ref ulong v2, ref ulong v3,
            ref ulong v4, long length, ulong seed)
        {
            fixed (byte* pData = &input[0])
            {
                byte* ptr = pData;
                byte* end = pData + len;
                ulong h64;

                if (length >= 32)
                {
                    h64 = ((v1 << 1) | (v1 >> (64 - 1))) +
                          ((v2 << 7) | (v2 >> (64 - 7))) +
                          ((v3 << 12) | (v3 >> (64 - 12))) +
                          ((v4 << 18) | (v4 >> (64 - 18)));

                    // XXH64_mergeRound
                    v1 *= XXH_PRIME64_2;
                    v1 = (v1 << 31) | (v1 >> (64 - 31));
                    v1 *= XXH_PRIME64_1;
                    h64 ^= v1;
                    h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;

                    // XXH64_mergeRound
                    v2 *= XXH_PRIME64_2;
                    v2 = (v2 << 31) | (v2 >> (64 - 31));
                    v2 *= XXH_PRIME64_1;
                    h64 ^= v2;
                    h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;

                    // XXH64_mergeRound
                    v3 *= XXH_PRIME64_2;
                    v3 = (v3 << 31) | (v3 >> (64 - 31));
                    v3 *= XXH_PRIME64_1;
                    h64 ^= v3;
                    h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;

                    // XXH64_mergeRound
                    v4 *= XXH_PRIME64_2;
                    v4 = (v4 << 31) | (v4 >> (64 - 31));
                    v4 *= XXH_PRIME64_1;
                    h64 ^= v4;
                    h64 = h64 * XXH_PRIME64_1 + XXH_PRIME64_4;
                }
                else
                {
                    h64 = seed + XXH_PRIME64_5;
                }

                h64 += (ulong) length;

                // XXH64_finalize
                len &= 31;
                while (len >= 8) {
                    ulong k1 = XXH64_round(0, *(ulong*)ptr);
                    ptr += 8;
                    h64 ^= k1;
                    h64  = XXH_rotl64(h64,27) * XXH_PRIME64_1 + XXH_PRIME64_4;
                    len -= 8;
                }
                if (len >= 4) {
                    h64 ^= *(uint*)ptr * XXH_PRIME64_1;
                    ptr += 4;
                    h64 = XXH_rotl64(h64, 23) * XXH_PRIME64_2 + XXH_PRIME64_3;
                    len -= 4;
                }
                while (len > 0) {
                    h64 ^= (*ptr++) * XXH_PRIME64_5;
                    h64 = XXH_rotl64(h64, 11) * XXH_PRIME64_1;
                    --len;
                }

                // XXH64_avalanche
                h64 ^= h64 >> 33;
                h64 *= XXH_PRIME64_2;
                h64 ^= h64 >> 29;
                h64 *= XXH_PRIME64_3;
                h64 ^= h64 >> 32;

                return h64;
            }
        }
    }
}