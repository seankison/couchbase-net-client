using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Core.Diagnostics.Tracing;
using Couchbase.Core.IO;
using Couchbase.Core.IO.Connections;
using Couchbase.Core.IO.Operations;
using Couchbase.Core.IO.Operations.Errors;

namespace Couchbase.LoadTests.Helpers
{
    internal class MockConnection : IConnection
    {
        public void Dispose()
        {
        }

        public Socket Socket { get; set; }
        public Guid Identity { get; set; }
        public ulong ConnectionId { get; set; }
        public string ContextId { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsSecure { get; set; }
        public bool IsConnected { get; set; }
        public EndPoint EndPoint { get; set; }
        public EndPoint LocalEndPoint { get; set; }
        public bool IsDead { get; set; }
        public TimeSpan IdleTime { get; set; }
        public ServerFeatureSet ServerFeatures { get; set; } = ServerFeatureSet.Empty;

        public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, IOperation operation, ErrorMap errorMap = null,
            CancellationToken cancellationToken = default)
        {
            operation.HandleOperationCompleted(AsyncState.BuildErrorResponse(0, ResponseStatus.KeyNotFound));
            return default;
        }

        public bool InUse { get; set; }
        public void MarkUsed(bool isUsed)
        {
        }

        public bool IsDisposed { get; set; }
        public bool HasShutdown { get; set; }
        public void Authenticate()
        {
        }

        public bool CheckedForEnhancedAuthentication { get; set; }
        public bool MustEnableServerFeatures { get; set; }
        public ValueTask CloseAsync(TimeSpan timeout)
        {
            Dispose();
            return default;
        }

        public void AddTags(IInternalSpan span)
        {
        }
    }
}
