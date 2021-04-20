//#nullable enable

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Couchbase.Core.Diagnostics.Tracing.ThresholdTracing
{
    /// <summary>
    /// Provides a summary for Threshold Request Logging for slow operations.
    /// </summary>
    internal struct ThresholdSummary
    {
        /// <summary>
        /// The duration of the outer request span.
        /// </summary>
        public ulong? total_duration_us { get; set; }

        /// <summary>
        /// The duration of the encode span, if present.
        /// </summary>
        public ulong? encode_duration_us { get; set; }

        /// <summary>
        /// The duration of the last dispatch span if present.
        /// </summary>
        public ulong? last_dispatch_duration_us { get; set; }

        /// <summary>
        /// The duration of all dispatch spans, summed up.
        /// </summary>
        public ulong? total_dispatch_duration_us { get; set; }

        /// <summary>
        /// The server duration attribute of the last dispatch span, if present.
        /// </summary>
        public ulong? last_server_duration_us { get; set; }

        /// <summary>
        /// The total duration of  all server duration spans, if present.
        /// </summary>
        public ulong? total_server_duration_us { get; set; }

        /// <summary>
        /// The name of the outer request span.
        /// </summary>
        public string operation_name { get; set; }

        /// <summary>
        /// The local_id from the last dispatch span, if present.
        /// </summary>
        public string last_local_id { get; set; }

        /// <summary>
        /// The operation_id from the outer request span, if present.
        /// </summary>
        public string operation_id { get; set; }

        /// <summary>
        /// The local_address from the last dispatch span, if present. Should combine the host and port into a  “host:port” format.
        /// </summary>
        public string last_local_socket { get; set; }

        /// <summary>
        /// The remote_address from the last dispatch span, if present. Should combine the host and port into a  “host:port” format.
        /// </summary>
        public string last_remote_socket { get; set; }

        public static ThresholdSummary FromActivity(Activity activity)
        {
            return new()
            {
                //ThresholdSummary specific spans
                total_duration_us = LastMicroseconds(activity, ThresholdTags.TotalDuration),
                encode_duration_us = SumMicroseconds(activity, ThresholdTags.EncodeDuration),
                last_dispatch_duration_us = LastMicroseconds(activity, ThresholdTags.DispatchDuration),
                total_dispatch_duration_us = SumMicroseconds(activity, ThresholdTags.DispatchDuration),

                //Basic OT tags
                total_server_duration_us = LastMicroseconds(activity, InnerRequestSpans.DispatchSpan.Attributes.ServerDuration),
                last_server_duration_us = LastMicroseconds(activity, InnerRequestSpans.DispatchSpan.Attributes.ServerDuration),
                operation_name = LastValueOrNull(activity, OuterRequestSpans.Attributes.Operation),
                operation_id = LastValueOrNull(activity, InnerRequestSpans.DispatchSpan.Attributes.OperationId),
                last_local_id = LastValueOrNull(activity, InnerRequestSpans.DispatchSpan.Attributes.LocalId),
                last_local_socket = $"{LastValueOrNull(activity, InnerRequestSpans.DispatchSpan.Attributes.LocalHostname)}:" +
                                    $"{LastValueOrNull(activity, InnerRequestSpans.DispatchSpan.Attributes.LocalPort)}",
                last_remote_socket = $"{LastValueOrNull(activity, InnerRequestSpans.DispatchSpan.Attributes.LocalHostname)}:" +
                                     $"{LastValueOrNull(activity, InnerRequestSpans.DispatchSpan.Attributes.LocalPort)}"
            };
        }

        private static readonly KeyValuePair<string, string> DefaultKvp = default;

        private static string LastValueOrNull(Activity activity, string keyName)
        {
            var last = activity.Tags.LastOrDefault(tag => tag.Key == keyName);
            if ((last.Key, last.Value) == (DefaultKvp.Key, DefaultKvp.Value))
            {
                return null;
            }

            return last.Value;
        }

        private static ulong? SumMicroseconds(Activity activity, string keyName)
        {
            ulong? sum = null;
            foreach (var tagValue in activity.Tags.Where(tag => tag.Key == keyName).Select(tag => tag.Value))
            {
                if (ulong.TryParse(tagValue, out var parsed))
                {
                    sum = (sum ?? 0) + parsed;
                }
            }

            return sum;
        }

        private static ulong? LastMicroseconds(Activity activity, string keyName)
        {
            ulong? result = null;
            foreach (var tag in activity.Tags)
            {
                if (tag.Key == keyName && ulong.TryParse(tag.Value, out var parsed))
                {
                    result = parsed;
                }
            }

            return result;
        }
    }
}
