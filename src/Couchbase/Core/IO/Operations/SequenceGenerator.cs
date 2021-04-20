using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Couchbase.Core.IO.Operations
{
    internal static class SequenceGenerator
    {
        private static readonly Random Random = new Random();
        private static int _sequenceId;

        public static uint GetNext()
        {
            var temp = Interlocked.Increment(ref _sequenceId);
            return (uint)temp;
        }

        public static void Reset()
        {
            Interlocked.Exchange(ref _sequenceId, 0);
        }

        public static ulong GetRandomLong()
        {
#if !SPAN_SUPPORT
            var bytes = new byte[8];
            lock (Random)
            {
                Random.NextBytes(bytes);
            }

            return BitConverter.ToUInt64(bytes, 0);
#else
            Span<byte> bytes = stackalloc byte[8];
            lock (Random)
            {
                Random.NextBytes(bytes);
            }

            return MemoryMarshal.Read<ulong>(bytes);
#endif
        }
    }
}

#region [ License information          ]

/* ************************************************************
 *
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2017 Couchbase, Inc.
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
