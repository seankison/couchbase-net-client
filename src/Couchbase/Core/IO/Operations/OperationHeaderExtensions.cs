using System;
using System.Collections.Generic;
using System.IO;
using Couchbase.Core.IO.Converters;
using Couchbase.Core.IO.Operations.Errors;


namespace Couchbase.Core.IO.Operations
{
    internal static class OperationHeaderExtensions
    {
        private static readonly HashSet<ResponseStatus> ValidResponseStatuses =
            new HashSet<ResponseStatus>((ResponseStatus[]) Enum.GetValues(typeof(ResponseStatus)));

        internal static OperationHeader CreateHeader(this Span<byte> buffer)
        {
            // This overload is necessary because the compiler won't apply implicit casting when finding extension methods,
            // so it avoids the need for explicit casting to find the extension method below.

            return CreateHeader((ReadOnlySpan<byte>) buffer);
        }

        internal static OperationHeader CreateHeader(this ReadOnlySpan<byte> buffer)
        {
            if (buffer == null || buffer.Length < OperationHeader.Length)
            {
                return new OperationHeader {Status = ResponseStatus.None};
            }

            int keyLength, framingExtrasLength;
            var magic = (Magic) buffer[HeaderOffsets.Magic];
            if (magic == Magic.AltResponse)
            {
                framingExtrasLength = buffer[HeaderOffsets.FramingExtras];
                keyLength = buffer[HeaderOffsets.AltKeyLength];
            }
            else
            {
                framingExtrasLength = 0;
                keyLength = ByteConverter.ToInt16(buffer.Slice(HeaderOffsets.KeyLength));
            }

            var statusCode = ByteConverter.ToInt16(buffer.Slice(HeaderOffsets.Status));
            var status = GetResponseStatus(statusCode);

            return new OperationHeader
            {
                Magic = (byte) magic,
                OpCode = buffer[HeaderOffsets.Opcode].ToOpCode(),
                FramingExtrasLength = framingExtrasLength,
                KeyLength = keyLength,
                ExtrasLength = buffer[HeaderOffsets.ExtrasLength],
                DataType = (DataType) buffer[HeaderOffsets.Datatype],
                Status = status,
                BodyLength = ByteConverter.ToInt32(buffer.Slice(HeaderOffsets.Body)),
                Opaque = ByteConverter.ToUInt32(buffer.Slice(HeaderOffsets.Opaque)),
                Cas = ByteConverter.ToUInt64(buffer.Slice(HeaderOffsets.Cas))
            };
        }

        internal static ResponseStatus GetResponseStatus(short code)
        {
            var status = (ResponseStatus) code;

            // Is it a known response status?
            if (!ValidResponseStatuses.Contains(status))
            {
                status = ResponseStatus.UnknownError;
            }

            return status;
        }

        internal static long? GetServerDuration(this OperationHeader header, MemoryStream stream)
        {
            if (header.FramingExtrasLength <= 0)
            {
                return null;
            }

            // copy framing extra bytes then reset steam position
            var bytes = new byte[header.FramingExtrasLength];
            stream.Position = OperationHeader.Length;
            stream.Read(bytes, 0, header.FramingExtrasLength);
            stream.Position = 0;

            return GetServerDuration(bytes);
        }

        internal static long? GetServerDuration(this OperationHeader header, ReadOnlySpan<byte> buffer)
        {
            if (header.FramingExtrasLength <= 0)
            {
                return null;
            }

            // copy framing extra bytes
            Span<byte> bytes = new byte[header.FramingExtrasLength];
            buffer.Slice(OperationHeader.Length, header.FramingExtrasLength).CopyTo(bytes);

            return GetServerDuration(bytes);
        }

        internal static long? GetServerDuration(ReadOnlySpan<byte> buffer)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                var control = buffer[offset++];
                var type = (ResponseFramingExtraType) (control & 0xF0); // first 4 bits
                var length = control & 0x0F; // last 4 bits

                if (type == ResponseFramingExtraType.ServerDuration)
                {
                    // read encoded two byte server duration
                    var encoded = ByteConverter.ToUInt16(buffer.Slice(offset));
                    if (encoded > 0)
                    {
                        // decode into microseconds
                        return (long) Math.Pow(encoded, 1.74) / 2;
                    }
                }

                offset += length;
            }

            return null;
        }
    }
}

#region [ License information ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2018 Couchbase, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *        http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *
 * ************************************************************/

#endregion
