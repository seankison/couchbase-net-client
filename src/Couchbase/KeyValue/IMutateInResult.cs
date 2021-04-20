using Couchbase.Core.Compatibility;

#nullable enable

namespace Couchbase.KeyValue
{
    /// <summary>
    /// Result of a sub document MutateIn operation.
    /// </summary>
    public interface IMutateInResult : IMutationResult
    {
        /// <summary>
        /// Gets the content of a mutation as the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the content</typeparam>
        /// <param name="index">The spec index.</param>
        /// <returns>The content, if the operation was an Increment or Decrement, otherwise <c>default(T)</c>.</returns>
        T ContentAs<T>(int index);

        /// <summary>
        /// Returns the index of a particular path.
        /// </summary>
        /// <param name="path">Path to find.</param>
        /// <returns>The index of the path, or -1 if not found.</returns>
        [InterfaceStability(Level.Volatile)]
        int IndexOf(string path);
    }
}
