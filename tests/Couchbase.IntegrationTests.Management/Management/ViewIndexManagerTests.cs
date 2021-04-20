using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.IntegrationTests.Fixtures;
using Couchbase.Management.Views;
using Couchbase.Views;
using Xunit;

namespace Couchbase.IntegrationTests.Management
{
    [Collection("NonParallel")]
    public class ViewIndexManagerTests : IClassFixture<ClusterFixture>
    {
        private readonly ClusterFixture _fixture;

        public ViewIndexManagerTests(ClusterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task TestIndexManager()
        {
            var cluster = _fixture.Cluster;
            var bucket = await _fixture.GetDefaultBucket().ConfigureAwait(false);
            var manager = bucket.ViewIndexes;

            var designDoc = new DesignDocument
            {
                Name = "test_ddoc",
                Views = new Dictionary<string, View>
                {
                    {
                        "test_view",
                        new View
                        {
                            Map = "function (doc, meta) { emit(meta.id, null); }",
                            Reduce = "_count"
                        }
                    }
                }
            };

            try
            {
                // upsert
                await manager.UpsertDesignDocumentAsync(designDoc, DesignDocumentNamespace.Development).ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

                // get
                var getResult = await manager.GetDesignDocumentAsync(designDoc.Name, DesignDocumentNamespace.Development).ConfigureAwait(false);
                VerifyDesignDoc(designDoc, getResult);

                // publish
                await manager.PublishDesignDocumentAsync(designDoc.Name).ConfigureAwait(false);

                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

                // get all
                var getAllResult =
                    (await manager.GetAllDesignDocumentsAsync(DesignDocumentNamespace.Production).ConfigureAwait(false)).ToList();
                var result = getAllResult.First(p => p.Name == "test_ddoc");
                VerifyDesignDoc(designDoc, result);
            }
            finally
            {
                // drop
                await manager.DropDesignDocumentAsync(designDoc.Name, DesignDocumentNamespace.Production).ConfigureAwait(false);
            }
        }

        private void VerifyDesignDoc(DesignDocument expected, DesignDocument result)
        {
            Assert.Equal("test_ddoc", result.Name);
            Assert.Single(result.Views);
            Assert.Equal(expected.Views.First().Value.Map, result.Views.First().Value.Map);
            Assert.Equal(expected.Views.First().Value.Reduce, result.Views.First().Value.Reduce);
        }
    }
}
