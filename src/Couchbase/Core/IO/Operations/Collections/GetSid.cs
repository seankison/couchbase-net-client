using System;

namespace Couchbase.Core.IO.Operations.Collections
{
    internal class GetSid : OperationBase<uint?>
    {
        public override bool RequiresVBucketId => false;

        public override OpCode OpCode => OpCode.GetSidByName;

        public override uint? GetValue()
        {
            if (Data.Length > 0)
            {
                try
                {
                    var buffer = Data;
                    ReadExtras(buffer.Span);
                    return Converters.ByteConverter.ToUInt32(buffer.Span.Slice(Header.ExtrasOffset + 8, 4));
                }
                catch (Exception e)
                {
                    Exception = e;
                    HandleClientError(e.Message, ResponseStatus.ClientFailure);
                }
            }

            return 0u;
        }

        protected override void WriteExtras(OperationBuilder builder)
        {
        }

        protected override void WriteBody(OperationBuilder builder)
        {
        }
    }
}
