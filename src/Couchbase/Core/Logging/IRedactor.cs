using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Couchbase.Core.Logging
{
    /// <summary>
    /// An interface used for redacting specific log information.
    /// </summary>
    public interface IRedactor
    {
        /// <summary>
        /// Redact user data like username, statements, etc
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("message")]
        object? UserData(object? message);

        /// <summary>
        /// Redact meta data like bucket names, etc
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("message")]
        object? MetaData(object? message);

        /// <summary>
        /// Redact system data like hostnames, etc.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull("message")]
        object? SystemData(object? message);
    }
}
