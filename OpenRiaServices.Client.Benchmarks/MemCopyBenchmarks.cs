using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using OpenRiaServices.Client.Benchmarks.Client.Cities;
using OpenRiaServices.Client;
using System.Runtime.CompilerServices;

namespace ClientBenchmarks
{
    //[RyuJitX86Job, RyuJitX64Job]
    public class MemCopyBenchmarks
    {
        readonly byte[] _src = new byte[1024 * 10 + 3];
        readonly byte[] _dest = new byte[1024 * 10 + 3];
        private static readonly bool Is64Bit = Environment.Is64BitProcess;

        [Params(/*4, 40,*/ 200, 500, 10*1024)]
        public int NumBytes { get; set; }

//        [Params(0,3)]
        public int DestOffset { get; set; }

        //[Params(0)]
        public int SrcOffset { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var rand = new Random();
            rand.NextBytes(_src);
        }

        [Benchmark(Baseline = true)]
        public void Buffer_BlockCopy()
        {
            Buffer.BlockCopy(_src, SrcOffset, _dest, DestOffset, NumBytes);
        }

        [Benchmark]
        public void Unsafe_Copy()
        {
            Unsafe.CopyBlockUnaligned(ref _dest[DestOffset], ref _src[SrcOffset], (uint)NumBytes);
        }

        [Benchmark]
        public void Span_Copy()
        {
            _src.AsSpan(SrcOffset, NumBytes)
                .CopyTo(_dest.AsSpan(DestOffset));
        }

        //[Benchmark()]
        unsafe public void Buffer_MemoryCopy()
        {
            fixed (byte* src = &_src[SrcOffset])
            fixed (byte* dst = &_dest[DestOffset])
                Buffer.MemoryCopy(src, dst, _dest.Length - DestOffset, NumBytes);
        }

        //[Benchmark()]
        unsafe public void Buffer_SmartCopy()
        {
            if (Is64Bit)
            {
                fixed (byte* src = &_src[SrcOffset])
                fixed (byte* dst = &_dest[DestOffset])
                    Buffer.MemoryCopy(src, dst, _dest.Length - DestOffset, NumBytes);
            }
            else
            {
                Buffer.BlockCopy(_src, SrcOffset, _dest, DestOffset, NumBytes);
            }
        }

        //     [Benchmark()]
        public void For_ByteCopy()
        {
            if (SrcOffset + NumBytes < _src.Length && DestOffset + NumBytes < _dest.Length)
            {
                for (int i = 0; i < NumBytes; ++i)
                    _dest[DestOffset + i] = _src[SrcOffset + i];
            }
        }

        //        [Benchmark()]
        public void For_ByteCopy_Revere()
        {
            for (int i = NumBytes; i >= 0; --i)
                _dest[i + DestOffset] = _src[i + SrcOffset];
        }

        //  [Benchmark()]
        public unsafe void FastCopy()
        {
            fixed (byte* srcStart = &_src[SrcOffset])
            fixed (byte* dstStart = &_dest[DestOffset])

            {
                int* srcInt = (int*)srcStart;
                int* dstInt = (int*)dstStart;
                uint toCopy = ((uint)NumBytes >> 2); // division by 4

                for (uint i = 0; i < toCopy; ++i)
                {
                    *(dstInt + i) = *(srcInt + i);
                }

                for (uint i = (uint)NumBytes & 0xfffffffc; i < NumBytes; ++i)
                {
                    *(dstStart + i) = *(srcStart + i);
                }
            }
        }

   //     [Benchmark()]
        public unsafe void FastCopy_Long()
        {
            uint count = (uint)NumBytes;
            fixed (byte* srcStart = &_src[SrcOffset])
            fixed (byte* dstStart = &_dest[DestOffset])
            {
                uint i;
                uint longCount = (count & 0xfffffff8); // modulo 8 (sizeof(long))
                for (i = 0; i < longCount; i += 8)
                {
                    *(long*)(dstStart + i) = *(long*)(srcStart + i);
                }

                if ((count & 0x04) != 0)
                {
                    *(int*)(dstStart + i) = *(int*)(srcStart + i);
                    i += 4;
                }

                for (; i < count; ++i)
                {
                    *(dstStart + i) = *(srcStart + i);
                }
            }
        }

        //        [Benchmark()]
        public unsafe void FastCopy_ByteOnly()
        {
            fixed (byte* srcStart = &_src[SrcOffset])
            fixed (byte* dstStart = &_dest[DestOffset])
            {
                for (uint i = 0; i < NumBytes; ++i)
                {
                    *(dstStart + i) = *(srcStart + i);
                }
            }
        }


        //      [Benchmark()]
        public unsafe void FastCopy_AlignDest()
        {
            fixed (byte* src = &_src[SrcOffset],
                dst = &_dest[DestOffset])
            {
                uint bytes = (uint)NumBytes;
                uint i;

                // TODO: if (NumBytes >= 4)
                uint byteToAlign = (4 - ((uint)dst & 0x3)) & 0x3;
                for (i = 0; i < byteToAlign; ++i)
                    dst[i] = src[i];

                byte* dstEnd = dst + bytes;
                uint byteToKeep = ((uint)dstEnd & 0x3);
                for (; i < bytes - byteToKeep; i += 4)
                    *(int*)(dst + i) = *(int*)(src + i);

                for (; i < bytes; ++i)
                    dst[i] = src[i];
            }
        }
    }
}
