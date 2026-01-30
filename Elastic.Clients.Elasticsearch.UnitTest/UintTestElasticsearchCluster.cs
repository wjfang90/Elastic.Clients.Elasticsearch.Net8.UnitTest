using Elastic.Clients.Elasticsearch.UnitTest.Elasticsearch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Clients.Elasticsearch.UnitTest {
    [TestClass]
    public class UintTestElasticsearchCluster {

        private readonly ElasticsearchClient _client = ElasticsearchConfiguration.Client.Value;
        [TestMethod]
        public async Task TestGetClusterSettings() {
            //GET: /_cluster/settings?pretty=true&error_trace=true
            var clusterSettings = await _client.Cluster.GetSettingsAsync();

            Assert.IsNotNull(clusterSettings);
            Assert.IsTrue(clusterSettings.IsValidResponse);
            Assert.IsTrue(clusterSettings.IsSuccess());
        }

        [TestMethod]
        public async Task TestClusterHealthy() {
            //GET: /_cluster/health?pretty=true&error_trace=true
            var healthy = await _client.Cluster.HealthAsync();

            Assert.IsNotNull(healthy);
            Assert.IsTrue(healthy.IsValidResponse);
            Assert.IsTrue(healthy.IsSuccess());

            Assert.IsNotNull(healthy.ClusterName);
            Assert.IsTrue(healthy.Status == HealthStatus.Yellow);
            Assert.IsTrue(healthy.NumberOfDataNodes > 0);
            Assert.IsTrue(healthy.NumberOfNodes > 0);
            Assert.IsTrue(healthy.ActiveShards > 0);
            Assert.IsTrue(healthy.ActivePrimaryShards > 0);
        }

        [TestMethod]
        public async Task TestClusterStats() {
            //GET: /_cluster/stats?pretty=true&error_trace=true
            var stats = await _client.Cluster.StatsAsync();

            Assert.IsNotNull(stats);
            Assert.IsTrue(stats.IsValidResponse);
            Assert.IsTrue(stats.IsSuccess());

            Assert.IsNotNull(stats.ClusterName);
            Assert.IsNotNull(stats.ClusterUuid);
            Assert.IsNotNull(stats.NodeStats);

            Assert.IsNotNull(stats.Nodes.Count);
            Assert.IsTrue(stats.Nodes.Plugins.Count > 0);
            Assert.IsTrue(stats.Nodes.Plugins.Any(t => t.Name == "analysis-V6standard-v3"));
            Assert.IsNotNull(stats.Nodes.Os);
            Assert.IsNotNull(stats.Nodes.Os.Mem);
            Assert.IsTrue(stats.Nodes.Os.Mem.UsedPercent > 0);
            Assert.IsTrue(stats.Nodes.Os.Mem.TotalInBytes > 0);
            Assert.IsTrue(stats.Nodes.Os.Mem.UsedInBytes > 0);
            Assert.IsNotNull(stats.Nodes.Jvm);
            Assert.IsNotNull(stats.Nodes.Jvm.Mem);
            Assert.IsTrue(stats.Nodes.Jvm.Mem.HeapMaxInBytes > 0);
            Assert.IsTrue(stats.Nodes.Jvm.Mem.HeapUsedInBytes > 0);

            Assert.IsNotNull(stats.Indices);
            Assert.IsTrue(stats.Indices.Count > 0);
            Assert.IsNotNull(stats.Indices.Docs);
            Assert.IsTrue(stats.Indices.Docs.Count > 0);
            Assert.IsTrue(stats.Indices.Analysis.BuiltInAnalyzers.Count > 0);
            Assert.IsTrue(stats.Indices.Mappings.FieldTypes.Count > 0);
            Assert.IsTrue(stats.Indices.Mappings.TotalFieldCount > 0);
            Assert.IsTrue(stats.Indices.Segments.Count > 0);

            Assert.IsTrue(stats.Status == HealthStatus.Yellow);
        }
    }   
}
