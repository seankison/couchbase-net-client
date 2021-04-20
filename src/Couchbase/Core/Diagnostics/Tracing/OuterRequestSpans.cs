using System.Collections.Generic;
using Couchbase.Core.Compatibility;

namespace Couchbase.Core.Diagnostics.Tracing
{
    [InterfaceStability(Level.Volatile)]
    internal static class OuterRequestSpans
    {
        /// <summary>
        /// Span attributes for the outer-request span
        /// </summary>
        internal static class Attributes
        {
            /// <summary>
            ///     This attribute is a standard OpenTelemetry attribute and should be placed on all spans to uniquely identify them
            ///     for couchbase.
            /// </summary>
            public static KeyValuePair<string, string> System = new("db.system", "couchbase");

            /// <summary>
            ///     Each outer request should set an attribute that classifies the service
            /// </summary>
            public static string Service = "db.couchbase.service";

            /// <summary>
            ///     This attribute is a standard OpenTelemetry attribute and should be placed on all operations which are at the bucket
            ///     level or below.
            /// </summary>
            public static string BucketName = "db.name";

            /// <summary>
            ///     Should be placed on all operations which are at the scope level or below and on manager operations that touch a
            ///     single scope (especially on the CollectionManager).
            /// </summary>
            public static string ScopeName = "db.couchbase.scope";

            /// <summary>
            ///     Should be placed on all operations which are at the collection level.
            /// </summary>
            public static string CollectionName = "db.couchbase.collection";

            /// <summary>
            ///     This attribute is a standard OpenTelemetry attribute and should be placed on N1QL and analytics operations.
            /// </summary>
            public static string Statement = "db.statement";

            /// <summary>
            ///     This attribute is a standard OpenTelemetry attribute and should be placed on all operations which do NOT have the
            ///     db.statement set.
            /// </summary>
            public static string Operation = "db.operation";
        }

        /// <summary>
        /// Public API Name for the outer-request span
        /// </summary>
        internal static class ServiceSpan
        {
            public const string ViewQuery = "views";
            // ReSharper disable once InconsistentNaming
            public const string N1QLQuery = "query";
            public const string AnalyticsQuery = "analytics";
            public const string SearchQuery = "search";

            internal static class Kv
            {
                public const string Name = "kv";
                public const string Management = "management";
                public const string AddInsert = "insert";
                public const string Append = "append";
                public const string Decrement = "decrement";
                public const string DeleteRemove = "remove";
                public const string Get = "get";
                public const string GetAndLock = "get_and_lock";
                public const string GetMetaExists = "exists";
                public const string GetAndTouch = "get_and_touch";
                public const string Increment = "increment";
                public const string Prepend = "prepend";
                public const string Replace = "replace";
                public const string ReplicaRead = "get_replica";
                public const string SetUpsert = "upsert";
                public const string LookupIn = "lookup_in";
                public const string MutateIn = "mutate_in";
                public const string Touch = "touch";
                public const string Unlock = "unlock";
                public const string Observe = "observe";
                public const string GetAllReplicas = "get_all_replicas";
                public const string GetAnyReplica = "get_any_replica";
                public const string Ping = "ping";
            }

            internal static class Internal
            {
                public const string Hello = "hello";
                public const string GetManifest = "get_manifest";
                public const string GetClusterMap = "get_cluster_map";
                public const string GetCid = "get_cid";
                public const string GetErrorMap = "get_error_map";
                public const string SelectBucket = "select_bucket";
                public const string Prepare = "prepare";
                public const string PrepareAndExecute = "prepare_and_execute";
                public const string AuthenticateScramSha = "authn_scramsha";
                public const string AuthenticatePlain = "authn_plain";
                public const string SaslStart = "sasl_start";
                public const string SaslStep = "sasl_step";
            }
        }

        /// <summary>
        /// Manager API outer-request spans
        /// </summary>
        internal static class ManagerSpan
        {
            internal static class Analytics
            {
                public const string ConnectLink = "manager_analytics_connect_link";
                public const string CreateDataset = "manager_analytics_create_dataset";
                public const string CreateDataverse = "manager_analytics_create_dataverse";
                public const string CreateIndex = "manager_analytics_create_index";
                public const string DisconnectLink = "manager_analytics_disconnect_link";
                public const string DropDataset = "manager_analytics_drop_dataset";
                public const string DropDataverse = "manager_analytics_drop_dataverse";
                public const string DropIndex = " manager_analytics_drop_index";
                public const string GetAllDatasets = "manager_analytics_get_all_datasets";
                public const string GetPendingMutations = "manager_analytics_get_pending_mutations";
                public const string GetAllIndexes = " manager_analytics_get_all_indexes";
                public const string GetAllDataverses = " manager_analytics_get_all_dataverses";
            }

            internal static class Query
            {
                public const string BuildDeferredIndexes = "manager_query_build_deferred_indexes";
                public const string CreateIndex = "manager_query_create_index";
                public const string CreatePrimaryIndex = "manager_query_create_primary_index";
                public const string DropIndex = "manager_query_drop_index";
                public const string DropPrimaryIndex = "manager_query_drop_primary_index";
                public const string GetAllIndexes = "manager_query_get_all_indexes";
                public const string WatchIndexes = "manager_query_watch_indexes";
            }

            internal static class Bucket
            {
                public const string CreateBucket = "manager_buckets_create_bucket";
                public const string DropBucket = "manager_buckets_drop_bucket";
                public const string FlushBucket = "manager_buckets_flush_bucket";
                public const string GetAllBuckets = "manager_buckets_get_all_buckets";
                public const string GetBucket = "manager_buckets_get_bucket";
                public const string UpdateBucket = "manager_buckets_update_bucket";
            }

            internal static class Collections
            {
                public const string CreateCollection = "manager_collections_create_collection";
                public const string CreateScope = "manager_collections_create_scope";
                public const string DropCollection = "manager_collections_drop_collection";
                public const string DropScope = "manager_collections_drop_scope";
                public const string GetAllScopes = "manager_collections_get_all_scopes";
            }

            internal static class Search
            {
                public const string AllowQuerying = "manager_search_allow_querying";
                public const string AnalyzeDocument = "manager_search_analyze_document";
                public const string DisallowQuerying = "manager_search_disallow_querying";
                public const string DropIndex = "manager_search_drop_index";
                public const string FreezePlan = "manager_search_freeze_plan";
                public const string GetAllIndexes = "manager_search_get_all_indexes";
                public const string GetIndex = "manager_search_get_index";
                public const string GetIndexedDocumentsCount = "manager_search_get_indexed_documents_count";
                public const string PauseIngest = "manager_search_pause_ingest";
                public const string ResumeIngest = "manager_search_resume_ingest";
                public const string UnfreezePlan = "manager_search_unfreeze_plan";
                public const string UpsertIndex = "manager_search_upsert_index";
            }

            internal static class Users
            {
                public const string DropGroup = "manager_users_drop_group";
                public const string DropUser = "manager_users_drop_user";
                public const string GetAllGroups = "manager_users_get_all_groups";
                public const string GetAllUsers = "manager_users_get_all_users";
                public const string GetGroup = "manager_users_get_group";
                public const string GetRoles = "manager_users_get_roles";
                public const string GetUser = "manager_users_get_user";
                public const string UpsertGroup = "manager_users_upsert_group";
                public const string UpsertUser = "manager_users_upsert_user";
            }

            internal static class Views
            {
                public const string DropDesignDocument = "manager_views_drop_design_document";
                public const string GetAllDesignDocuments = "manager_views_get_all_design_documents";
                public const string GetDesignDocument = "manager_views_get_design_document";
                public const string PublishDesignDocument = "manager_views_publish_design_document";
                public const string UpsertDesignDocument = "manager_views_upsert_design_document";
            }
        }
    }
}
