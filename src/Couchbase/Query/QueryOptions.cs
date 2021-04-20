using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.Retry;
using Couchbase.Utils;
using Newtonsoft.Json;

#nullable enable

namespace Couchbase.Query
{
    /// <summary>
    /// Options to control execution of a N1QL query.
    /// </summary>
    public class QueryOptions
    {
        private readonly List<object> _arguments = new List<object>();
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly Dictionary<string, object> _rawParameters = new Dictionary<string, object>();
        private bool _autoExecute;
        private bool? _includeMetrics;
        private int? _maxServerParallelism;
        private int? _pipelineBatch;
        private int? _pipelineCapacity;
        private QueryPlan? _preparedPayload;
        private QueryProfile _profile = QueryProfile.Off;
        private bool? _readOnly;
        private int? _scanCapacity;
        private QueryScanConsistencyInternal? _scanConsistency;
        private Dictionary<string, Dictionary<string, List<object>>>? _scanVectors;
        private TimeSpan? _scanWait;
        private string? _statement;
        private TimeSpan? _timeOut;
        private bool _flexIndex;

        /// <summary>
        /// Creates a new QueryOptions object.
        /// </summary>
        public QueryOptions()
        {
        }

        /// <summary>
        /// Creates a new QueryOptions object with a N1QL query statement.
        /// </summary>
        /// <param name="statement">A N1QL query statement.</param>
        public QueryOptions(string statement) : this()
        {
            _statement = statement;
            _preparedPayload = null;
            IsPrepared = false;
        }

        /// <summary>
        /// Creates a new QueryOptions object with an existing <see cref="QueryPlan"/>.
        /// </summary>
        /// <param name="plan">The <see cref="QueryPlan"/>.</param>
        /// <param name="originalStatement">The original N1QL query statement used to generate the plan.</param>
        public QueryOptions(QueryPlan plan, string originalStatement) : this()
        {
            _statement = originalStatement;
            _preparedPayload = plan;
            IsPrepared = true;
        }

        /// <summary>
        /// The bucket name for tracing.
        /// </summary>
        /// <remarks>For internal use only</remarks>
        internal string? BucketName { get; set; }

        /// <summary>
        /// The bucket name for tracing.
        /// </summary>
        /// <remarks>For internal use only</remarks>
        internal string? ScopeName { get; set; }

        internal IRetryStrategy? RetryStrategyValue { get; set; }

        /// <summary>
        /// Overrides the global <see cref="IRetryStrategy"/> defined in <see cref="ClusterOptions"/> for a request.
        /// </summary>
        /// <param name="retryStrategy">The <see cref="IRetryStrategy"/> to use for a single request.</param>
        /// <returns>The options.</returns>
        public QueryOptions RetryStrategy(IRetryStrategy retryStrategy)
        {
            RetryStrategyValue = retryStrategy;
            return this;
        }

        internal CancellationToken Token { get; set; } = System.Threading.CancellationToken.None;

        internal TimeSpan? TimeoutValue
        {
            get => _timeOut;
            set => _timeOut = value;
        }

        internal string? StatementValue => _statement;

        internal string GetAllParametersAsJson()
        {
            var allParameters = new
            {
                Named = _parameters,
                Raw = _rawParameters,
                Positional = _arguments
            };

            return JsonConvert.SerializeObject(allParameters);
        }

        /// <summary>
        ///     Returns true if the request is a prepared statement
        /// </summary>
        public bool IsPrepared { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this query statement is to executed in an ad-hoc manner.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is ad-hoc; otherwise, <c>false</c>.
        /// </value>
        public bool IsAdHoc { get; private set; } = true;

        /// <summary>
        ///     Gets the context identifier for the N1QL query request/response. Useful for debugging.
        /// </summary>
        /// <remarks>This value changes for every request./></remarks>
        /// <value>
        ///     The context identifier.
        /// </value>
        public string? CurrentContextId { get; private set; }

        /// <summary>
        ///     Custom <see cref="ITypeSerializer" /> to use when deserializing query results.
        /// </summary>
        /// <remarks>Null will use the default <see cref="ITypeSerializer" />.</remarks>
        public ITypeSerializer? Serializer { get; set; }

        internal bool IsReadOnly => _readOnly.HasValue && _readOnly.Value;

        /// <summary>
        ///     Provides a means of ensuring "read your own writes" or RYOW consistency on the current query.
        /// </summary>
        /// <remarks>Note: <see cref="ScanConsistency" /> will be overwritten to <see cref="QueryScanConsistencyInternal.AtPlus" />.</remarks>
        /// <param name="mutationState">State of the mutation.</param>
        /// <returns>A reference to the current <see cref="QueryOptions" /> for method chaining.</returns>
        public QueryOptions ConsistentWith(MutationState mutationState)
        {
#pragma warning disable 618
            ScanConsistencyInternal(QueryScanConsistencyInternal.AtPlus);
#pragma warning restore 618
            _scanVectors = new Dictionary<string, Dictionary<string, List<object>>>();
            foreach (var token in mutationState)
                if (_scanVectors.TryGetValue(token.BucketRef, out var vector))
                {
                    var bucketId = token.VBucketId.ToString();
                    if (vector.TryGetValue(bucketId, out var bucketRef))
                    {
                        if ((long) bucketRef.First() < token.SequenceNumber)
                            vector[bucketId] = new List<object>
                            {
                                token.SequenceNumber,
                                token.VBucketUuid.ToString()
                            };
                    }
                    else
                    {
                        vector.Add(token.VBucketId.ToString(),
                            new List<object>
                            {
                                token.SequenceNumber,
                                token.VBucketUuid.ToString()
                            });
                    }
                }
                else
                {
                    _scanVectors.Add(token.BucketRef, new Dictionary<string, List<object>>
                    {
                        {
                            token.VBucketId.ToString(),
                            new List<object>
                            {
                                token.SequenceNumber,
                                token.VBucketUuid.ToString()
                            }
                        }
                    });
                }

            return this;
        }

        /// <summary>
        ///     Specifies the maximum parallelism for the query. A zero or negative value means the number of logical
        ///     cpus will be used as the parallelism for the query. There is also a server wide max_parallelism parameter
        ///     which defaults to 1. If a request includes max_parallelism, it will be capped by the server max_parallelism.
        ///     If a request does not include max_parallelism, the server wide max_parallelism will be used.
        /// </summary>
        /// <param name="parallelism"></param>
        /// <returns></returns>
        /// <value>
        ///     The maximum server parallelism.
        /// </value>
        public QueryOptions MaxServerParallelism(int parallelism)
        {
            _maxServerParallelism = parallelism;
            return this;
        }

        /// <summary>
        ///     If set to false, the client will try to perform optimizations
        ///     transparently based on the server capabilities, like preparing the statement and
        ///     then executing a query plan instead of the raw query.
        /// </summary>
        /// <param name="adHoc">if set to <c>false</c> the query will be optimized if possible.</param>
        /// <returns></returns>
        /// <remarks>
        ///     The default is <c>true</c>; the query will executed in an ad-hoc manner,
        ///     without special optimizations.
        /// </remarks>
        public QueryOptions AdHoc(bool adHoc)
        {
            IsAdHoc = adHoc;
            return this;
        }

        /// <summary>
        ///     Sets a N1QL statement to be executed in an optimized way using the given queryPlan.
        /// </summary>
        /// <param name="preparedPlan">The <see cref="Query.QueryPlan" /> that was prepared beforehand.</param>
        /// <param name="originalStatement">The original statement (eg. SELECT * FROM default) that the user attempted to optimize</param>
        /// <returns>A reference to the current <see cref="QueryOptions" /> for method chaining.</returns>
        /// <remarks>Required if statement not provided, will erase a previously set Statement.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="preparedPlan" /> is <see langword="null" />.</exception>
        public QueryOptions Prepared(QueryPlan preparedPlan, string originalStatement)
        {
            if (string.IsNullOrWhiteSpace(originalStatement))
                throw new ArgumentNullException(nameof(originalStatement));

            _statement = originalStatement;
            _preparedPayload = preparedPlan ?? throw new ArgumentNullException(nameof(preparedPlan));
            IsPrepared = true;
            return this;
        }

        /// <summary>
        ///     Sets a N1QL statement to be executed.
        /// </summary>
        /// <param name="statement">
        ///     Any valid N1QL statement for a POST request, or a read-only N1QL statement (SELECT, EXPLAIN)
        ///     for a GET request.
        /// </param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">statement</exception>
        /// <remarks>
        ///     Will erase a previous optimization of a statement using Prepared.
        /// </remarks>
        internal QueryOptions Statement(string statement)
        {
            if (string.IsNullOrWhiteSpace(statement)) throw new ArgumentNullException(nameof(statement));
            _statement = statement;
            _preparedPayload = null;
            IsPrepared = false;
            return this;
        }

        /// <summary>
        ///     Sets the maximum time to spend on the request.
        /// </summary>
        /// <param name="timeOut">Maximum time to spend on the request</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <remarks>
        ///     Optional - the default is 0ms, which means the request runs for as long as it takes.
        /// </remarks>
        public QueryOptions Timeout(TimeSpan timeOut)
        {
            _timeOut = timeOut;
            return this;
        }

        /// <summary>
        ///     If a GET request, this will always be true otherwise false.
        /// </summary>
        /// <param name="readOnly">True for get requests.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <remarks>
        ///     Any value set here will be overridden by the type of request sent.
        /// </remarks>
        [Obsolete("Use QueryOptions.Readonly property instead.")]
        public QueryOptions ReadOnly(bool readOnly)
        {
            _readOnly = readOnly;
            return this;
        }

        /// <summary>
        ///     If a GET request, this will always be true otherwise false.
        /// </summary>
        /// <param name="readOnly">True for get requests.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <remarks>
        ///     Any value set here will be overridden by the type of request sent.
        /// </remarks>
        public QueryOptions Readonly(bool readOnly)
        {
            _readOnly = readOnly;
            return this;
        }

        /// <summary>
        ///     Specifies that metrics should be returned with query results.
        /// </summary>
        /// <param name="includeMetrics">True to return query metrics.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <remarks>
        ///     Optional.
        /// </remarks>
            public QueryOptions Metrics(bool includeMetrics)
        {
            _includeMetrics = includeMetrics;
            return this;
        }

        /// <summary>
        ///     Adds a named parameter to the parameters to the statement or prepared statement.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <remarks>
        ///     Optional.
        /// </remarks>
        public QueryOptions Parameter(string name, object value)
        {
            _parameters.Add(name, value);
            return this;
        }

        /// <summary>
        ///     Adds a positional parameter to the parameters to the statement or prepared statement.
        /// </summary>
        /// <param name="value">The value of the positional parameter.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <remarks>
        ///     Optional.
        /// </remarks>
        public QueryOptions Parameter(object value)
        {
            _arguments.Add(value);
            return this;
        }

        /// <summary>
        ///     Adds a collection of named parameters to the parameters to the statement or prepared statement.
        /// </summary>
        /// <param name="parameters">A list of <see cref="KeyValuePair{K, V}" /> to be sent.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <remarks>
        ///     Optional.
        /// </remarks>
        public QueryOptions Parameter(params KeyValuePair<string, object>[] parameters)
        {
            if (_arguments.Any()) throw new ArgumentException("Cannot combine positional and named query parameters.");

            foreach (var parameter in parameters) _parameters.Add(parameter.Key, parameter.Value);

            return this;
        }

        /// <summary>
        ///     Adds a list of positional parameters to the statement or prepared statement.
        /// </summary>
        /// <param name="parameters">A list of positional parameters.</param>
        /// <returns></returns>
        public QueryOptions Parameter(params object[] parameters)
        {
            if (_parameters.Any()) throw new ArgumentException("Cannot combine positional and named query parameters.");

            foreach (var parameter in parameters) _arguments.Add(parameter);

            return this;
        }

        /// <summary>
        ///     Specifies the consistency guarantee/constraint for index scanning.
        /// </summary>
        /// <param name="scanConsistency">Specify the consistency guarantee/constraint for index scanning.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <remarks>
        ///     Optional.
        /// </remarks>
        public QueryOptions ScanConsistency(QueryScanConsistency scanConsistency) =>
            ScanConsistencyInternal((QueryScanConsistencyInternal) scanConsistency);

        /// <summary>
        ///     Specifies the consistency guarantee/constraint for index scanning.
        /// </summary>
        /// <param name="scanConsistency">Specify the consistency guarantee/constraint for index scanning.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <exception cref="InvalidEnumArgumentException">Invalid <paramref name="scanConsistency"/>.</exception>
        /// <remarks>
        ///     Used internally to allow <see cref="ConsistentWith"/> to set the consistency to <see cref="QueryScanConsistencyInternal.AtPlus"/>.
        /// </remarks>
        internal QueryOptions ScanConsistencyInternal(QueryScanConsistencyInternal scanConsistency)
        {
            if (scanConsistency < QueryScanConsistencyInternal.NotBounded ||
                scanConsistency > QueryScanConsistencyInternal.AtPlus)
            {
                throw new InvalidEnumArgumentException(nameof(scanConsistency), (int) scanConsistency, typeof(QueryScanConsistencyInternal));
            }

            _scanConsistency = scanConsistency;
            return this;
        }

        /// <summary>
        ///     Specifies the maximum time the client is willing to wait for an index to catch up to the vector timestamp in the
        ///     request. If an index has to catch up, and the <see cref="ScanWait" /> time is exceed doing so, an error is
        ///     returned.
        /// </summary>
        /// <param name="scanWait">The maximum time the client is willing to wait for index to catch up to the vector timestamp.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        /// <remarks>
        ///     Optional.
        /// </remarks>
        public QueryOptions ScanWait(TimeSpan scanWait)
        {
            _scanWait = scanWait;
            return this;
        }

        /// <summary>
        ///     Client Context ID.
        ///     If no client context ID is provided on this option, a UUID is generated and sent
        ///     automatically so by default it is always possible to identify a query when debugging.
        /// </summary>
        /// <param name="clientContextId">The client context identifier.</param>
        /// <returns>A reference to the current <see cref="QueryOptions" /> for method chaining.</returns>
        public QueryOptions ClientContextId(string clientContextId)
        {
            //this is seeded in the ctor
            if (clientContextId != null) CurrentContextId = clientContextId;

            return this;
        }

        /// <summary>
        ///     Adds a raw query parameter and value to the query.
        ///     NOTE: This is uncommitted and may change in the future.
        /// </summary>
        /// <param name="name">The paramter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>A reference to the current <see cref="QueryOptions" /> for method chaining.</returns>
        public QueryOptions Raw(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter name cannot be null or empty.");

            _rawParameters.Add(name, value);
            return this;
        }

        /// <summary>
        ///     Sets maximum buffered channel size between the indexer client
        ///     and the query service for index scans.
        ///     This parameter controls when to use scan backfill.
        ///     Use 0 or a negative number to disable.
        /// </summary>
        /// <param name="capacity">The maximum number of channels.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        public QueryOptions ScanCap(int capacity)
        {
            _scanCapacity = capacity;
            return this;
        }

        /// <summary>
        ///     Sets the number of items execution operators can batch for
        ///     fetch from the KV.
        /// </summary>
        /// <param name="batchSize">The maximum number of items.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        public QueryOptions PipelineBatch(int batchSize)
        {
            _pipelineBatch = batchSize;
            return this;
        }

        /// <summary>
        ///     Sets maximum number of items each execution operator can buffer
        ///     between various operators.
        /// </summary>
        /// <param name="capacity">The maximum number of items.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        public QueryOptions PipelineCap(int capacity)
        {
            _pipelineCapacity = capacity;
            return this;
        }

        /// <summary>
        ///     Set the <see cref="QueryProfile"/> information to be returned along with the query results.
        /// </summary>
        /// <param name="profile">The <see cref="QueryProfile"/>.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        public QueryOptions Profile(QueryProfile profile)
        {
            _profile = profile;
            return this;
        }

        /// <summary>
        ///     Set the <see cref="CancellationToken"/> which will cancel the query if it is incomplete.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        ///     A reference to the current <see cref="QueryOptions" /> for method chaining.
        /// </returns>
        public QueryOptions CancellationToken(CancellationToken cancellationToken)
        {
            Token = cancellationToken;
            return this;
        }

        /// <summary>
        /// Tells the query engine to use a flex index (utilizing the search service).
        /// </summary>
        /// <param name="flexIndex">true if a flex index should be used, false is the default</param>
        /// <returns>A reference to the current <see cref="QueryOptions" /> for method chaining.</returns>
        /// <remarks>This feature is Uncommitted and may change in the future.</remarks>
        public QueryOptions FlexIndex(bool flexIndex)
        {
            _flexIndex = flexIndex;
            return this;
        }

        /// <summary>
        ///The alias for the namespace:bucket:scope:collection
        /// </summary>
        /// <returns></returns>
        internal string? QueryContext { get; set; }

        internal QueryOptions QueryPlan(QueryPlan queryPlan)
        {
            _preparedPayload = queryPlan;
            return this;
        }

        internal QueryOptions AutoExecute(bool autoExecute)
        {
            _autoExecute = autoExecute;
            return this;
        }

        /// <summary>
        ///     Gets a <see cref="IDictionary{K, V}" /> of the name/value pairs to be POSTed to the service.
        /// </summary>
        /// <returns>
        ///     The <see cref="IDictionary{K, V}" /> of the name/value pairs to be POSTed to the service.
        /// </returns>
        /// <exception cref="System.ArgumentException">A statement or prepared plan must be provided.</exception>
        /// <remarks>
        ///     Since values will be POSTed as JSON, here we deal with unencoded typed values
        ///     (like ints, Lists, etc...) rather than only strings.
        /// </remarks>
        public IDictionary<string, object> GetFormValues()
        {
            if (string.IsNullOrWhiteSpace(_statement) ||
                IsPrepared && _preparedPayload == null)
                throw new ArgumentException("A statement or prepared plan must be provided.");

            //build the map of request parameters
            IDictionary<string, object> formValues = new Dictionary<string, object>();

            if (_maxServerParallelism.HasValue)
                formValues.Add(QueryParameters.MaxServerParallelism, _maxServerParallelism.Value.ToString());

            if (IsPrepared)
            {
                formValues.Add(QueryParameters.Prepared, _preparedPayload!.Name);

                // don't include empty plan
                if (!string.IsNullOrEmpty(_preparedPayload.EncodedPlan))
                    formValues.Add(QueryParameters.PreparedEncoded, _preparedPayload.EncodedPlan);
            }
            else
            {
                formValues.Add(QueryParameters.Statement, _statement!);
            }

            if (_timeOut.HasValue)
            {
                formValues.Add(QueryParameters.Timeout, (uint) _timeOut.Value.TotalMilliseconds + "ms");
            }

            if (_readOnly.HasValue) formValues.Add(QueryParameters.Readonly, _readOnly.Value);

            if (_includeMetrics.HasValue) formValues.Add(QueryParameters.Metrics, _includeMetrics);

            if (_parameters.Count > 0)
                foreach (var parameter in _parameters)
                    formValues.Add(
                        parameter.Key.Contains("$") ? parameter.Key : "$" + parameter.Key,
                        parameter.Value);

            if (_arguments.Count > 0) formValues.Add(QueryParameters.Args, _arguments);

            if (_scanConsistency.HasValue)
                formValues.Add(QueryParameters.ScanConsistency, _scanConsistency.GetDescription());

            if (_scanVectors != null)
            {
#pragma warning disable 618
                if (_scanConsistency != QueryScanConsistencyInternal.AtPlus)
#pragma warning restore 618
                    throw new ArgumentException("Only ScanConsistency.AtPlus is supported for this query request.");

                formValues.Add(QueryParameters.ScanVectors, _scanVectors);
            }

            if (_scanWait.HasValue)
                formValues.Add(QueryParameters.ScanWait, $"{(uint) _scanWait.Value.TotalMilliseconds}ms");

            if (_scanCapacity.HasValue) formValues.Add(QueryParameters.ScanCapacity, _scanCapacity.Value.ToString());

            if (_pipelineBatch.HasValue) formValues.Add(QueryParameters.PipelineBatch, _pipelineBatch.Value.ToString());

            if (_pipelineCapacity.HasValue)
                formValues.Add(QueryParameters.PipelineCapacity, _pipelineCapacity.Value.ToString());

            if (_profile != QueryProfile.Off)
                formValues.Add(QueryParameters.Profile, _profile.ToString().ToLowerInvariant());

            foreach (var parameter in _rawParameters) formValues.Add(parameter.Key, parameter.Value);

            if (_autoExecute) formValues.Add(QueryParameters.AutoExecute, true);

            if (CurrentContextId != null) formValues.Add(QueryParameters.ClientContextId, CurrentContextId);

            if (QueryContext != null) formValues.Add(QueryParameters.QueryContext, QueryContext);

            if(_flexIndex) formValues.Add(QueryParameters.FlexIndex, _flexIndex);

            return formValues;
        }

        /// <summary>
        /// Gets the JSON representation of this query for execution in a POST.
        /// </summary>
        /// <returns>The form values as a JSON object.</returns>
        public string GetFormValuesAsJson()
        {
            var formValues = GetFormValues();
            return JsonConvert.SerializeObject(formValues);
        }

        /// <summary>
        ///     Creates a new <see cref="QueryOptions" /> object.
        /// </summary>
        /// <returns></returns>
        public static QueryOptions Create()
        {
            return new QueryOptions();
        }

        /// <summary>
        ///     Creates a new <see cref="QueryOptions" /> object with the specified statement.
        /// </summary>
        /// <param name="statement">The statement.</param>
        /// <returns></returns>
        public static QueryOptions Create(string statement)
        {
            return new QueryOptions(statement);
        }

        /// <summary>
        ///     Creates a query using the given plan as an optimization for the originalStatement.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="originalStatement">The original statement, unoptimized.</param>
        /// <returns></returns>
        public static QueryOptions Create(QueryPlan plan, string originalStatement)
        {
            return new QueryOptions(plan, originalStatement);
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string request;
            try
            {
                request = "[" + GetFormValuesAsJson() + "]";
            }
            catch
            {
                request = string.Empty;
            }

            return request;
        }

        private struct QueryParameters
        {
            public const string Statement = "statement";
            public const string PreparedEncoded = "encoded_plan";
            public const string Prepared = "prepared";
            public const string Timeout = "timeout";
            public const string Readonly = "readonly";
            public const string Metrics = "metrics";
            public const string Args = "args";

            // ReSharper disable once UnusedMember.Local
            public const string BatchArgs = "batch_args";

            // ReSharper disable once UnusedMember.Local
            public const string BatchNamedArgs = "batch_named_args";
            public const string Format = "format";
            public const string Encoding = "encoding";
            public const string Compression = "compression";
            public const string Signature = "signature";
            public const string ScanConsistency = "scan_consistency";
            public const string ScanVectors = "scan_vectors";
            public const string ScanWait = "scan_wait";
            public const string Pretty = "pretty";
            public const string Creds = "creds";
            public const string ClientContextId = "client_context_id";
            public const string MaxServerParallelism = "max_parallelism";
            public const string ScanCapacity = "scan_cap";
            public const string PipelineBatch = "pipeline_batch";
            public const string PipelineCapacity = "pipeline_cap";
            public const string Profile = "profile";
            public const string AutoExecute = "auto_execute";
            public const string QueryContext = "query_context";
            public const string FlexIndex = "use_fts";
        }
    }
}
