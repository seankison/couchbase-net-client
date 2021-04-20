using System;

namespace Couchbase.Core.IO.Operations.SubDocument
{
    /// <summary>
    /// Represents one more fragments of a document that is returned by the sub-document API.
    /// </summary>
    /// <typeparam name="TDocument">The document</typeparam>
    [Obsolete("This interface is not required and will be removed in a future release.")] // Delete
    public interface IDocumentFragment<out TDocument> : IOperationResult<TDocument>, IDocumentFragment
    {
        /// <summary>
        /// The time-to-live or TTL for the document before it's evicted from disk in milliseconds.
        /// </summary>
        /// <remarks>Setting this to zero or less will give the document infinite lifetime</remarks>
        uint Expiry { get; set; }
    }
}
