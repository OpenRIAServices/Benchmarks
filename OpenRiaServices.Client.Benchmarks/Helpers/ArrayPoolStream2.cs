using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OpenRiaServices.Hosting.AspNetCore.Serialization
{
    /// <summary>
    /// Stream optimized for usage by <see cref="System.Xml.XmlDictionaryWriter"/> without unneccessary 
    /// allocations on LOH.
    /// It writes directly to memory pooled by a <see cref="BufferManager"/> in order to 
    /// avoid allocations and be able to return memory directly without additional copies 
    /// (for small messages).
    /// </summary>
    internal sealed class ArrayPoolStream2 : Stream
    {
        private readonly int _maxSize;
        // number of bytes written to _buffer, used as offset into _buffer where we write next time
        private int _bufferWritten;
        // "Current" buffer where the next write should go
        private Memory<byte> _buffer;
        private PipeWriter _writer;
        private ValueTask<FlushResult> _flushResult;

        // String "position" (total size so far)
        private int _position;

        public ArrayPoolStream2(int maxBlockSize)
        {
            if (maxBlockSize < 0)
                throw new ArgumentOutOfRangeException(nameof(maxBlockSize), maxBlockSize, "Max size can not have a negative value");

            _maxSize = maxBlockSize;
        }

        public void Reset(PipeWriter pipeWriter, int size)
        {
            _bufferWritten = 0;
            _position = 0;
            _buffer = pipeWriter.GetMemory(Math.Min(size, _maxSize));
            _writer = pipeWriter;
            _flushResult = default;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _position;

        public override long Position { get => _position; set => throw new NotImplementedException(); }

        public override void Flush()
        {
            // Nothing to do
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            // Note: BinaryXml buffers up to 512 bytesso we should expect most writes to be around 
            // 500+ bytes (smaller if the next write is a long string or byte array)
            do
            {
                EnsureBufferCapacity();

                // Write up to count bytes, but never more than the rest of the buffer
                int toCopy = Math.Min(buffer.Length, _buffer.Length - _bufferWritten);
                FastCopy(buffer, _buffer, _bufferWritten, toCopy);
                _position += toCopy;
                _bufferWritten += toCopy;
                buffer = buffer.Slice(toCopy);
            } while (buffer.Length > 0);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer.AsSpan(offset, count));
        }

        /// <summary>
        /// Copies bytes from <paramref name="src"/> to <paramref name="dest"/>
        /// </summary>
        private static unsafe void FastCopy(byte[] src, int srcOffset, byte[] dest, int destOffset, int count)
        {
            Unsafe.CopyBlockUnaligned(destination: ref dest[destOffset], source: ref src[srcOffset], (uint)count);
        }


        /// <summary>
        /// Copies bytes from <paramref name="src"/> to <paramref name="dest"/>
        /// </summary>
        private static unsafe void FastCopy(ReadOnlySpan<byte> src, Memory<byte> dest, int destOffset, int count)
        {
            Unsafe.CopyBlockUnaligned(destination: ref dest.Span[destOffset], source: ref MemoryMarshal.GetReference(src), (uint)count);
        }

        /// <summary>
        /// Allocate more space if buffer is full.
        /// Ensures _buffer is non null and has space to write more bytes
        /// </summary>
        private void EnsureBufferCapacity()
        {
            // There is space left
            if (_bufferWritten < _buffer.Length)
                return;

            // Save current buffer in list before allocating a new buffer
            _writer.Advance(_buffer.Length);
            // Ensure we never return buffer twice in case TakeBuffer below throws
            _buffer = null;

            if (_flushResult == default)
            {
                _flushResult = _writer.FlushAsync();
            }
            else if (_flushResult.IsCompleted) 
            {
                _flushResult.GetAwaiter().GetResult();
                _flushResult = _writer.FlushAsync();
            }

            int nextSize = _position * 2;
            // If the size is >1GB the next size might be larger than int.MaxValue
            // which means it will become negative
            if (nextSize < 0)
            {
                if (_position > 0 && _position < int.MaxValue)
                {
                    //This is the space left before we hit max int 
                    nextSize = int.MaxValue - _position;
                }
                else
                {
                    throw new InsufficientMemoryException();
                }
            }

            nextSize = Math.Min(nextSize, _maxSize);
            _buffer = _writer.GetMemory(nextSize);
            _bufferWritten = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clear();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        ///  Returns all memory to the current BufferManager
        /// </summary>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        public new ValueTask<FlushResult> FlushAsync()
        {
            _writer.Advance(_bufferWritten);
            _bufferWritten = 0;
            return _writer.FlushAsync();
        }

        internal async Task Finish(HttpResponse response)
        {
            _writer.Advance(_bufferWritten);
            _bufferWritten = 0;

            if (_flushResult != default)
            {
                await _flushResult;
            }

            await _writer.FlushAsync();
            await response.CompleteAsync();
        }
    }
}
