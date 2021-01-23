using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Couchbase.Core.Exceptions;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.Core.IO.Operations;

namespace Couchbase.Utils
{
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentException(string message, string paramName) =>
            throw new ArgumentException(message, paramName);

        [DoesNotReturn]
        public static void ThrowArgumentNullException(string paramName) =>
            throw new ArgumentNullException(paramName);

        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException(string paramName) =>
            throw new ArgumentOutOfRangeException(paramName);

        [DoesNotReturn]
        public static void ThrowInvalidEnumArgumentException(string argumentName, int invalidValue, Type enumClass) =>
            throw new InvalidEnumArgumentException(argumentName, invalidValue, enumClass);

        [DoesNotReturn]
        public static void ThrowInvalidIndexException(string message) =>
            throw new InvalidIndexException(message);

        [DoesNotReturn]
        public static void ThrowInvalidOperationException(string message) =>
            throw new InvalidOperationException(message);

        [DoesNotReturn]
        public static void ThrowNotSupportedException(string message) =>
            throw new NotSupportedException(message);

        [DoesNotReturn]
        public static void ThrowObjectDisposedException(string objectName) =>
            throw new ObjectDisposedException(objectName);

        [DoesNotReturn]
        public static void ThrowOperationCanceledException() =>
            throw new OperationCanceledException();

        [DoesNotReturn]
        public static void ThrowSendQueueFullException() =>
            throw new SendQueueFullException();
            
        public static void ThrowTimeoutException(IOperation operation)
        {
            var message = $"The operation {operation.Opaque}/{operation.Opaque} timed out after {operation.Timeout}. " +
                          $"It was retried {operation.Attempts} times using {operation.RetryStrategy.GetType()}.";

            if (operation.IsSent && !operation.IsReadOnly)
            {
                throw new AmbiguousTimeoutException(message);
            }

            throw new UnambiguousTimeoutException(message);
        }
    }
}
