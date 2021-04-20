using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Couchbase.Core.CircuitBreakers;
using Couchbase.Core.Compatibility;
using Couchbase.Core.DI;
using Couchbase.Core.Diagnostics.Tracing;
using Couchbase.Core.Diagnostics.Tracing.ThresholdTracing;
using Couchbase.Core.IO.Authentication.X509;
using Couchbase.Core.IO.Compression;
using Couchbase.Core.IO.Connections;
using Couchbase.Core.IO.Connections.Channels;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.IO.Transcoders;
using Couchbase.Core.Logging;
using Couchbase.Core.Retry;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using IRequestTracer = Couchbase.Core.Diagnostics.Tracing.IRequestTracer;

// ReSharper disable UnusedMember.Global

#nullable enable

namespace Couchbase
{
    /// <summary>
    /// Options controlling the connection to the Couchbase cluster.
    /// </summary>
    public sealed class ClusterOptions
    {
        public ClusterOptions()
        {
            HttpCertificateCallbackValidation  = (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (HttpIgnoreRemoteCertificateMismatch)
                {
                    // mask out the name mismatch error, and the chain error that comes along with it
                    sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateNameMismatch;
                    sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateChainErrors;
                }

                return sslPolicyErrors == SslPolicyErrors.None;
            };

            KvCertificateCallbackValidation = (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (KvIgnoreRemoteCertificateNameMismatch)
                {
                    // mask out the name mismatch error, and the chain error that comes along with it
                    sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateNameMismatch;
                    sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateChainErrors;
                }

                return sslPolicyErrors == SslPolicyErrors.None;
            };
        }
        internal ConnectionString? ConnectionStringValue { get; set; }

        /// <summary>
        /// The connection string for the cluster.
        /// </summary>
        public string? ConnectionString
        {
            get => ConnectionStringValue?.ToString();
            set
            {
                ConnectionStringValue = value != null ? Couchbase.ConnectionString.Parse(value) : null;
                if (!string.IsNullOrWhiteSpace(ConnectionStringValue?.Username))
                {
                    UserName = ConnectionStringValue!.Username;
                }

                if (ConnectionStringValue != null)
                {
                    if (ConnectionStringValue.TryGetParameter(CStringParams.KvTimeout, out TimeSpan kvTimeout))
                    {
                        KvTimeout = kvTimeout;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.AnalyticsTimeout, out TimeSpan analyticsTimeout))
                    {
                        AnalyticsTimeout = analyticsTimeout;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.ConfigIdleRedialTimeout, out TimeSpan configIdleRedialTimeout))
                    {
                        ConfigIdleRedialTimeout = configIdleRedialTimeout;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.ConfigPollFloorInterval, out TimeSpan configPollFloorInterval))
                    {
                        ConfigPollFloorInterval = configPollFloorInterval;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.ConfigPollInterval, out TimeSpan configPollInterval))
                    {
                        ConfigPollInterval = configPollInterval;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.EnableMutationTokens, out bool enableMutationTokens))
                    {
                        EnableMutationTokens = enableMutationTokens;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.EnableTcpKeepAlives, out bool enableTcpAlives))
                    {
                        EnableTcpKeepAlives = enableTcpAlives;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.EnableTls, out bool enableTls))
                    {
                        EnableTls = enableTls;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.ForceIpv4, out bool forceIp4))
                    {
                        ForceIPv4 = forceIp4;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.KvConnectTimeout, out TimeSpan kvConnectTimeout))
                    {
                        KvConnectTimeout = kvConnectTimeout;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.KvDurableTimeout, out TimeSpan kvDurableTimeout))
                    {
                        KvDurabilityTimeout = kvDurableTimeout;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.QueryTimeout, out TimeSpan queryTimeout))
                    {
                        QueryTimeout = queryTimeout;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.ManagementTimeout, out TimeSpan managementTimeout))
                    {
                        ManagementTimeout = managementTimeout;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.MaxHttpConnections, out int maxHttpConnections))
                    {
                        MaxHttpConnections = maxHttpConnections;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.NumKvConnections, out int numKvConnections))
                    {
                        NumKvConnections = numKvConnections;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.ViewTimeout, out TimeSpan viewTimeout))
                    {
                        ViewTimeout = viewTimeout;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.SearchTimeout, out TimeSpan searchTimeout))
                    {
                        SearchTimeout = searchTimeout;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.TcpKeepAliveTime, out TimeSpan tcpKeepAliveTime))
                    {
                        TcpKeepAliveTime = tcpKeepAliveTime;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.TcpKeepAliveInterval, out TimeSpan tcpKeepAliveInterval))
                    {
                        TcpKeepAliveInterval = tcpKeepAliveInterval;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.MaxKvConnections, out int maxKvConnections))
                    {
                        MaxKvConnections = maxKvConnections;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.Compression, out bool compression))
                    {
                        Compression = compression;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.CompressionMinSize, out int compressionMinSize))
                    {
                        CompressionMinSize = compressionMinSize;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.CompressionMinRatio, out float compressionMinRatio))
                    {
                        CompressionMinRatio = compressionMinRatio;
                    }
                    if (ConnectionStringValue.TryGetParameter(CStringParams.NetworkResolution, out string networkResolution))
                    {
                        NetworkResolution = networkResolution;
                    }
                }
            }
        }

        /// <summary>
        /// Set the connection string for the cluster.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>
        /// A reference to this <see cref="ClusterOptions"/> object for method chaining.
        /// </returns>
        public ClusterOptions WithConnectionString(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            return this;
        }

        /// <summary>
        /// The buckets to be used in the cluster.
        /// </summary>
        public IList<string> Buckets { get; set; } = new List<string>();

        /// <summary>
        /// Set the buckets to be used in the cluster.
        /// </summary>
        /// <param name="bucketNames">The names of the buckets.</param>
        /// <returns>
        /// A reference to this <see cref="ClusterOptions"/> object for method chaining.
        /// </returns>
        public ClusterOptions WithBuckets(params string[] bucketNames)
        {
            if (!bucketNames?.Any() ?? true)
            {
                throw new ArgumentException($"{nameof(bucketNames)} cannot be null or empty.");
            }

            //just the name of the bucket for now - later make and actual cluster
            Buckets = new List<string>(bucketNames!);
            return this;
        }

        /// <summary>
        /// Set credentials used for authentication.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// A reference to this <see cref="ClusterOptions"/> object for method chaining.
        /// </returns>
        public ClusterOptions WithCredentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException($"{nameof(username)} cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"{nameof(password)} cannot be null or empty.");
            }

            UserName = username;
            Password = password;
            return this;
        }

        /// <summary>
        /// The <see cref="ILoggerFactory"/> to use for logging.
        /// </summary>
        public ILoggerFactory? Logging { get; set; }

        /// <summary>
        /// Set the <see cref="ILoggerFactory"/> to use for logging.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <returns>
        /// A reference to this <see cref="ClusterOptions"/> object for method chaining.
        /// </returns>
        public ClusterOptions WithLogging(ILoggerFactory? loggerFactory = null)
        {
            Logging = loggerFactory;

            return this;
        }

        /// <summary>
        /// Provide a custom <see cref="ITypeSerializer"/>.
        /// </summary>
        public ITypeSerializer? Serializer { get; set; }

        /// <summary>
        /// Provide a custom <see cref="ITypeSerializer"/>.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <returns>
        /// A reference to this <see cref="ClusterOptions"/> object for method chaining.
        /// </returns>
        public ClusterOptions WithSerializer(ITypeSerializer serializer)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            return this;
        }

        /// <summary>
        /// Provide a custom <see cref="ITypeTranscoder"/>.
        /// </summary>
        public ITypeTranscoder? Transcoder { get; set; }

        /// <summary>
        /// Provide a custom <see cref="ITypeTranscoder"/>.
        /// </summary>
        /// <param name="transcoder">Transcoder to use.</param>
        /// <returns>
        /// A reference to this <see cref="ClusterOptions"/> object for method chaining.
        /// </returns>
        public ClusterOptions WithTranscoder(ITypeTranscoder transcoder)
        {
            Transcoder = transcoder ?? throw new ArgumentNullException(nameof(transcoder));

            return this;
        }

        /// <summary>
        /// Provide a custom <see cref="IDnsResolver"/> for DNS SRV resolution.
        /// </summary>
        public IDnsResolver? DnsResolver { get; set; }

        /// <summary>
        /// Provide a custom <see cref="IDnsResolver"/> for DNS SRV resolution.
        /// </summary>
        /// <param name="dnsResolver">DNS resolver to use.</param>
        /// <returns>
        /// A reference to this <see cref="ClusterOptions"/> object for method chaining.
        /// </returns>
        public ClusterOptions WithDnsResolver(IDnsResolver dnsResolver)
        {
            DnsResolver = dnsResolver ?? throw new ArgumentNullException(nameof(dnsResolver));

            return this;
        }

        /// <summary>
        /// Provide a custom <see cref="ICompressionAlgorithm"/> for key/value body compression.
        /// </summary>
        /// <param name="compressionAlgorithm">Compression algorithm to use.</param>
        /// <returns>
        /// A reference to this <see cref="ClusterOptions"/> object for method chaining.
        /// </returns>
        [InterfaceStability(Level.Volatile)]
        public ClusterOptions WithCompressionAlgorithm(ICompressionAlgorithm compressionAlgorithm) =>
            this.AddClusterService(compressionAlgorithm);

        /// <summary>
        /// Provide a custom <see cref="ICompressionAlgorithm"/> for key/value body compression.
        /// </summary>
        /// <typeparam name="TImplementation">Compression algorithm to use.</typeparam>
        /// <returns>
        /// A reference to this <see cref="ClusterOptions"/> object for method chaining.
        /// </returns>
        [InterfaceStability(Level.Volatile)]
        public ClusterOptions WithCompressionAlgorithm<TImplementation>()
            where TImplementation : class, ICompressionAlgorithm
        {
            _services[typeof(ICompressionAlgorithm)] = new SingletonServiceFactory(typeof(TImplementation));

            return this;
        }

        [Obsolete("Use WithThresholdTracing instead.")]
        public IRequestTracer? RequestTracer { get; set; }

        [Obsolete("Use WithThresholdTracing instead.")]
        public ClusterOptions WithRequestTracer(IRequestTracer requestTracer)
        {
            RequestTracer = requestTracer;
            return this;
        }

        public ThresholdOptions ThresholdOptions { get; set; } = new();

        public ClusterOptions WithThresholdTracing(ThresholdOptions options)
        {
            ThresholdOptions = options;
            return this;
        }

        public ClusterOptions WithThresholdTracing(Action<ThresholdOptions> configure)
        {
            var opts = new ThresholdOptions();
            configure(opts);
            return WithThresholdTracing(opts);
        }

        /// <summary>
        /// The <see cref="IRetryStrategy"/> for operation retries. Applies to all services: K/V, Query, etc.
        /// </summary>
        /// <param name="retryStrategy">The custom <see cref="RetryStrategy"/>.</param>
        /// <returns></returns>
        public ClusterOptions WithRetryStrategy(IRetryStrategy retryStrategy)
        {
            RetryStrategy = retryStrategy;
            return this;
        }

        /// <summary>
        /// The <see cref="IRetryStrategy"/> for operation retries. Applies to all services: K/V, Query, etc.
        /// </summary>
        public IRetryStrategy? RetryStrategy { get; set; } = new BestEffortRetryStrategy();

        public string? UserName { get; set; }
        public string? Password { get; set; }

        //Foundation RFC conformance
        public TimeSpan KvConnectTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan KvTimeout { get; set; } = TimeSpan.FromSeconds(2.5);
        public TimeSpan KvDurabilityTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan ViewTimeout { get; set; } = TimeSpan.FromSeconds(75);
        public TimeSpan QueryTimeout { get; set; } = TimeSpan.FromSeconds(75);
        public TimeSpan AnalyticsTimeout { get; set; } = TimeSpan.FromSeconds(75);
        public TimeSpan SearchTimeout { get; set; } = TimeSpan.FromSeconds(75);
        public TimeSpan ManagementTimeout { get; set; } = TimeSpan.FromSeconds(75);

        /// <summary>
        /// Gets or sets the maximum number of operations that will be queued for processing per node.
        /// If this value is exceeded, any additional operations will be put into  the retry loop.
        /// </summary>
        /// <remarks>Defaults to 1024 operations.</remarks>
        [InterfaceStability(Level.Volatile)]
        public uint KvSendQueueCapacity { get; set; } = 1024;

        /// <summary>
        /// Overrides the TLS behavior in <see cref="ConnectionString"/>, enabling or
        /// disabling TLS.
        /// </summary>
        public bool? EnableTls { get; set; }

        public bool EnableMutationTokens { get; set; } = true;
        ////public ITracer Tracer = new ThresholdLoggingTracer();
        public TimeSpan TcpKeepAliveTime { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan TcpKeepAliveInterval { get; set; } = TimeSpan.FromSeconds(1);
        public bool ForceIPv4 { get; set; }
        public TimeSpan ConfigPollInterval { get; set; } = TimeSpan.FromSeconds(2.5);
        public TimeSpan ConfigPollFloorInterval { get; set; } = TimeSpan.FromMilliseconds(50);
        public TimeSpan ConfigIdleRedialTimeout { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Minimum number of connections per key/value node.
        /// </summary>
        public int NumKvConnections { get; set; } = 2;

        /// <summary>
        /// Maximum number of connections per key/value node.
        /// </summary>
        public int MaxKvConnections { get; set; } = 5;

        /// <summary>
        /// Amount of time with no activity before a key/value connection is considered idle.
        /// </summary>
        public TimeSpan IdleKvConnectionTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public int MaxHttpConnections { get; set; } = 0;

        [Obsolete("Not supported in .NET, uses system defaults.")]
        public TimeSpan IdleHttpConnectionTimeout { get; set; }

        public CircuitBreakerConfiguration? CircuitBreakerConfiguration { get; set; } =
            CircuitBreakerConfiguration.Default;

        public bool EnableOperationDurationTracing { get; set; } = true;

        public RedactionLevel RedactionLevel { get; set; } = RedactionLevel.None;

        /// <summary>
        /// Port used for HTTP bootstrapping fallback if other bootstrap methods are not available.
        /// </summary>
        public int BootstrapHttpPort { get; set; } = 8091;

        /// <summary>
        /// Port used for TLS HTTP bootstrapping fallback if other bootstrap methods are not available.
        /// </summary>
        public int BootstrapHttpPortTls { get; set; } = 18091;

        /// <summary>
        /// Used for checking that the SDK has bootstrapped and potentially retrying if not.
        /// </summary>
        public TimeSpan BootstrapPollInterval { get; set; } = TimeSpan.FromSeconds(2.5);

        //Volatile or obsolete options
        public bool EnableExpect100Continue { get; set; }

        [Obsolete("This property is ignored; set the ClusterOptions.X509CertificateFactory property to a "
                  +" ICertificateFactory instance - Couchbase.Core.IO.Authentication.X509.CertificateStoreFactory for example.")]
        public bool EnableCertificateAuthentication { get; set; }
        public bool EnableCertificateRevocation { get; set; }

        /// <summary>
        /// Ignore CertificateNameMismatch and CertificateChainMismatch, since they happen together.
        /// </summary>
        [Obsolete("Use KvIgnoreRemoteCertificateNameMismatch and/or HttpIgnoreRemoteCertificateMismatch instead of this property.")]
        public bool IgnoreRemoteCertificateNameMismatch
        {
            get => KvIgnoreRemoteCertificateNameMismatch && HttpIgnoreRemoteCertificateMismatch;
            set => KvIgnoreRemoteCertificateNameMismatch = HttpIgnoreRemoteCertificateMismatch = value;
        }

        private bool _enableOrphanedResponseLogging;
        public bool EnableOrphanedResponseLogging
        {
            get => _enableOrphanedResponseLogging;
            set
            {
                if (value != _enableOrphanedResponseLogging)
                {
                    _enableOrphanedResponseLogging = value;

                    /*if (value)
                    {
                        this.AddClusterService<Couchbase.Core.Diagnostics.Tracing.IOrphanedResponseLogger, Couchbase.Core.Diagnostics.Tracing.OrphanedResponseLogger>();//TODO temp
                    }
                    else
                    {
                        this.AddClusterService<Couchbase.Core.Diagnostics.Tracing.IOrphanedResponseLogger, Couchbase.Core.Diagnostics.Tracing.NullOrphanedResponseLogger>();
                    }*/
                }
            }
        }

        public bool EnableConfigPolling { get; set; } = true;
        public bool EnableTcpKeepAlives { get; set; } = true;
        public bool EnableDnsSrvResolution { get; set; } = true;
        public string NetworkResolution { get; set; } = Couchbase.NetworkResolution.Auto;
        [CanBeNull] internal string? EffectiveNetworkResolution { get; set; }
        internal bool HasNetworkResolution => !string.IsNullOrWhiteSpace(EffectiveNetworkResolution);

        /// <summary>
        /// Enables compression for key/value operations.
        /// </summary>
        /// <remarks>
        /// The value is ignored if no compression algorithm is supplied via <see cref="WithCompressionAlgorithm"/>.
        /// Defaults to true.
        /// </remarks>
        public bool Compression { get; set; } = true;

        /// <summary>
        /// If compression is enabled, the minimum document size considered for compression (in bytes).
        /// Documents smaller than this size are always sent to the server uncompressed.
        /// </summary>
        public int CompressionMinSize { get; set; } = 32;

        /// <summary>
        /// If compression is enabled, the minimum compression ratio to accept when sending documents to the server.
        /// Documents which don't achieve this compression ratio are sent to the server uncompressed.
        /// </summary>
        /// <remarks>
        /// 1.0 means no compression was achieved. A value of 0.75 would result in documents which compress to at least
        /// 75% of their original size to be transmitted compressed. The default is 0.83 (83%).
        /// </remarks>
        public float CompressionMinRatio { get; set; } = 0.83f;

        /// <inheritdoc cref="TuningOptions"/>
        public TuningOptions Tuning { get; set; } = new();

        /// <inheritdoc cref="ExperimentalOptions"/>
        public ExperimentalOptions Experiments { get; set; } = new();

        /// <summary>
        /// Provides a default implementation of <see cref="ClusterOptions"/>.
        /// </summary>
        public static ClusterOptions Default => new ClusterOptions();

        /// <summary>
        /// Effective value for TLS, should be used instead of <see cref="EnableTls"/> internally within the SDK.
        /// </summary>
        internal bool EffectiveEnableTls => EnableTls ?? ConnectionStringValue?.Scheme == Scheme.Couchbases;

        /// <summary>
        /// Ignore CertificateNameMismatch and CertificateChainMismatch for Key/Value operations, since they happen together.
        /// </summary>
        public bool KvIgnoreRemoteCertificateNameMismatch { get; set; }

        /// <summary>
        /// The default RemoteCertificateValidationCallback called by .NET to validate the TLS/SSL certificates being used for
        /// Key/Value operations. To ignore RemoteCertificateNameMismatch and RemoteCertificateChainErrors errors caused when the
        /// subject and subject alternative name do not match the requesting DNS name, set ClusterOptions.KvCertificateCallbackValidation
        /// to true.
        /// </summary>
        public RemoteCertificateValidationCallback KvCertificateCallbackValidation { get; set; }

        /// <summary>
        /// Ignore CertificateNameMismatch and CertificateChainMismatch for HTTP services (Query, FTS, Analytics, etc), since they happen together.
        /// </summary>
        public bool HttpIgnoreRemoteCertificateMismatch { get; set; }

        /// <summary>
        /// The default RemoteCertificateValidationCallback called by .NET to validate the TLS/SSL certificates being used for
        /// HTTP services (Query, FTS, Analytics, etc). To ignore RemoteCertificateNameMismatch and RemoteCertificateChainErrors
        /// errors caused when the subject and subject alternative name do not match the requesting DNS name, set
        /// ClusterOptions.KvCertificateCallbackValidation to true.
        /// </summary>
        public RemoteCertificateValidationCallback HttpCertificateCallbackValidation { get; set; }

        public ICertificateFactory? X509CertificateFactory { get; set; }

        public ClusterOptions WithX509CertificateFactory(ICertificateFactory certificateFactory)
        {
            X509CertificateFactory = certificateFactory ?? throw new NullReferenceException(nameof(certificateFactory));
            EnableTls = true;
            return this;
        }

        public bool UnorderedExecutionEnabled { get; set; } = false;

        #region DI

        private readonly IDictionary<Type, IServiceFactory> _services = DefaultServices.GetDefaultServices();

        /// <summary>
        /// Build a <see cref="IServiceProvider"/> from the currently registered services.
        /// </summary>
        /// <returns>The new <see cref="IServiceProvider"/>.</returns>
        internal IServiceProvider BuildServiceProvider()
        {
            this.AddClusterService(this);
            this.AddClusterService(Logging ??= new NullLoggerFactory());
            if (ThresholdOptions.Enabled)
            {
                //No custom logger has been registered, so create a default logger
                if (ThresholdOptions.RequestTracer == null)
                {
                    var thresholdTracer = new ThresholdRequestTracer(ThresholdOptions, Logging);
                    thresholdTracer.Start(new ThresholdTraceListener(ThresholdOptions));
                    ThresholdOptions.RequestTracer = thresholdTracer;
                }

                this.AddClusterService(ThresholdOptions.RequestTracer);
            }
            else
            {
                this.AddClusterService(NoopRequestTracer.Instance);
            }

            if (Experiments.ChannelConnectionPools)
            {
                this.AddClusterService<IConnectionPoolFactory, ChannelConnectionPoolFactory>();
            }

            if (Serializer != null)
            {
                this.AddClusterService(Serializer);
            }

            if (Transcoder != null)
            {
                this.AddClusterService(Transcoder);
            }

            if (DnsResolver != null)
            {
                this.AddClusterService(DnsResolver);
            }

            if (CircuitBreakerConfiguration != null)
            {
                this.AddClusterService(CircuitBreakerConfiguration);
            }

            if (RetryStrategy != null)
            {
                this.AddClusterService(RetryStrategy);
            }

            return new CouchbaseServiceProvider(_services);
        }

        /// <summary>
        /// Register a service with the cluster's <see cref="ICluster.ClusterServices"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service which will be requested.</typeparam>
        /// <typeparam name="TImplementation">The type of the service implementation which is returned.</typeparam>
        /// <param name="factory">Factory which will create the service.</param>
        /// <param name="lifetime">Lifetime of the service.</param>
        /// <returns>The <see cref="ClusterOptions"/>.</returns>
        public ClusterOptions AddService<TService, TImplementation>(
            Func<IServiceProvider, TImplementation> factory,
            ClusterServiceLifetime lifetime)
            where TImplementation : notnull, TService
        {
            _services[typeof(TService)] = lifetime switch
            {
                ClusterServiceLifetime.Transient => new TransientServiceFactory(serviceProvider => factory(serviceProvider)),
                ClusterServiceLifetime.Cluster => new SingletonServiceFactory(serviceProvider => factory(serviceProvider)),
                _ => throw new InvalidEnumArgumentException(nameof(lifetime), (int) lifetime,
                    typeof(ClusterServiceLifetime))
            };

            return this;
        }

        /// <summary>
        /// Register a service with the cluster's <see cref="ICluster.ClusterServices"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service which will be requested.</typeparam>
        /// <typeparam name="TImplementation">The type of the service implementation which is returned.</typeparam>
        /// <param name="lifetime">Lifetime of the service.</param>
        /// <returns>The <see cref="ClusterOptions"/>.</returns>
        public ClusterOptions AddService<TService, TImplementation>(
            ClusterServiceLifetime lifetime)
            where TImplementation : TService
        {
            _services[typeof(TService)] = lifetime switch
            {
                ClusterServiceLifetime.Transient => new TransientServiceFactory(typeof(TImplementation)),
                ClusterServiceLifetime.Cluster => new SingletonServiceFactory(typeof(TImplementation)),
                _ => throw new InvalidEnumArgumentException(nameof(lifetime), (int) lifetime,
                    typeof(ClusterServiceLifetime))
            };

            return this;
        }

        #endregion
    }

    [Obsolete("Use Couchbase.NetworkResolution")]
    public static class NetworkTypes
    {
        public const string Auto = "auto";
        public const string Default = "default";
        public const string External = "external";
    }
}
