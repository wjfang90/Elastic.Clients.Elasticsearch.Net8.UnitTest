using Elastic.Clients.Elasticsearch.UnitTest.Elasticsearch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Elastic.Clients.Elasticsearch.UnitTest {
    [TestClass]
    public class UnitTestElasticsearchSettings {

        [TestMethod]
        public void TestElasticserchSettings() {
            var settings = ElasticsearchConfiguration.Settings();
            Assert.IsNotNull(settings);
        }

        [TestMethod]
        public void TestElasticsearchClient() {
            var client = ElasticsearchConfiguration.Client.Value;
            Assert.IsNotNull(client);
        }

        [TestMethod]
        public void TestElasticsearchAiClient() {
            var client = ElasticsearchConfiguration.AiClient.Value;
            Assert.IsNotNull(client);
        }

        [TestMethod]
        public void TestCurrentVersion() {
            var versionInfo = ElasticsearchConfiguration.CurrentVersion;
            Assert.IsNotNull(versionInfo);
            Assert.IsTrue(versionInfo.Major == 8);
            Assert.IsTrue(versionInfo.Minor == 11);
        }
    }
}
