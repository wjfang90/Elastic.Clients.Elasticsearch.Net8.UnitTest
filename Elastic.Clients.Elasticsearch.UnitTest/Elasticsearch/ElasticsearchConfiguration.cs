using Elastic.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Clients.Elasticsearch.UnitTest.Elasticsearch {
    /// <summary>
    /// ES数据库初始化配置
    /// </summary>
    public static class ElasticsearchConfiguration {

        private static readonly string Host = "localhost";
        private static readonly string DefaultIndex = "chl";
        private static readonly int MaxConnections = 100;
        private static readonly string[] _hosts;

        private static readonly string[] _aihosts;

        private static Version _currentVersion;

        private static string CloudId => string.Empty;
        private static string ApiKey => string.Empty;

        //public static string EsHost => "192.168.0.160:9204";// es 8 正式
        public static string EsHost => "192.168.0.160:9601";//test es 8
        private static string EsAiHost => EsHost;
        private static string EsDefaultIndex => "chl";
        
        /// <summary>
        /// 是否https请求
        /// </summary>
        public static bool IsSearchGuard => false;
        private static bool IsCloudIdConnected => !string.IsNullOrWhiteSpace(CloudId) && !string.IsNullOrWhiteSpace(ApiKey);


        private static string Fingerprint => string.Empty;

        private static string EsUserName {
            get {
                var esUser = "elastic";
                return esUser;
            }
        }

        private static string EsPassword {
            get {
                var esPwd = "123456";
                return esPwd;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        static ElasticsearchConfiguration() {
            if (!string.IsNullOrWhiteSpace(EsHost)) {
                _hosts = EsHost.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else {
                _hosts = new[] { Host };
            }

            if (!string.IsNullOrWhiteSpace(EsAiHost)) {
                _aihosts = EsAiHost.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else {
                _aihosts = new[] { Host };
            }

            if (!string.IsNullOrWhiteSpace(EsDefaultIndex)) {
                DefaultIndex = EsDefaultIndex;
            }
        }

        /// <summary>
        /// 当前版本
        /// </summary>
        public static Version CurrentVersion {
            get {
                if (_currentVersion == null)
                    _currentVersion = GetCurrentVersion();

                return _currentVersion;
            }
        }

        /// <summary>
        /// 创建基uri
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Uri CreateBaseUri(string host = null, int? port = null) {
            host = host ?? _hosts.First().Split(':')[0];
            var schema = IsSearchGuard ? "https" : "http";
            var uri = new UriBuilder(schema, host, port.GetValueOrDefault(9200)).Uri;
            return uri;
        }


        private static NodePool CreateConnectionPool() {
            if (_hosts.Count() <= 1) {
                var hostport = _hosts.First().Split(':');

                return new SingleNodePool(CreateBaseUri(hostport[0], int.Parse(hostport[1])));
            }
            return new StaticNodePool(_hosts.Select(n => CreateBaseUri(n.Split(':')[0], int.Parse(n.Split(':')[1]))));
        }


        private static NodePool CreateAiConnectionPool() {
            if (_aihosts.Count() <= 1) {
                var hostport = _aihosts.First().Split(':');

                return new SingleNodePool(CreateBaseUri(hostport[0], int.Parse(hostport[1])));
            }
            //请求时随机请求各个正常节点，不请求异常节点,异常节点恢复后会重新被请求
            return new StaticNodePool(_aihosts.Select(n => CreateBaseUri(n.Split(':')[0], int.Parse(n.Split(':')[1]))));
        }




        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static ElasticsearchClientSettings Settings() {
            if (IsSearchGuard) {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句
            }

            var connectSetings = new ElasticsearchClientSettings(CreateConnectionPool())
               .ConnectionLimit(MaxConnections)
               .PrettyJson(false)
               .RequestTimeout(TimeSpan.FromMinutes(2))
               .EnableTcpKeepAlive(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1))
               .DefaultFieldNameInferrer(n => n)//注：nest默认字段名首字母小写，如果要设置为与Model中一致，在创建client时按如下设置。（强烈建议使用该设置，避免造成字段不一致）
               .EnableDebugMode()
               .DisableDirectStreaming(true)//调试               
               .ThrowExceptions(true);

            if (IsSearchGuard) {
                connectSetings = connectSetings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);//当您使用自签名证书或不受默认信任存储信任的证书时，如果不设置此回调（或错误地实现此回调），会导致 SSL/TLS 握手失败
            }

            if (!string.IsNullOrWhiteSpace(Fingerprint)) {
                connectSetings = connectSetings.CertificateFingerprint(Fingerprint);
            }

            if (!string.IsNullOrWhiteSpace(EsUserName) && !string.IsNullOrWhiteSpace(EsPassword)) {
                var basicAuth = new BasicAuthentication(EsUserName, EsPassword);
                connectSetings = connectSetings.Authentication(basicAuth);
            }
            if (!string.IsNullOrWhiteSpace(ApiKey)) {
                connectSetings = connectSetings.Authentication(new ApiKey(ApiKey));
            }

            return connectSetings;
        }

        /// <summary>
        /// 智能检索 设置
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static ElasticsearchClientSettings AiSettings() {
            if (IsSearchGuard) {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //加上这一句
            }

            var connectSetings = new ElasticsearchClientSettings(CreateConnectionPool())
               .ConnectionLimit(MaxConnections)
               .PrettyJson(false)
               .RequestTimeout(TimeSpan.FromMinutes(2))
               .EnableTcpKeepAlive(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(1))
               .DefaultFieldNameInferrer(n => n)//注：nest默认字段名首字母小写，如果要设置为与Model中一致，在创建client时按如下设置。（强烈建议使用该设置，避免造成字段不一致）
               .EnableDebugMode()
               .DisableDirectStreaming(true)//调试               
               .ThrowExceptions(true);

            if (IsSearchGuard) {
                connectSetings = connectSetings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);//当您使用自签名证书或不受默认信任存储信任的证书时，如果不设置此回调（或错误地实现此回调），会导致 SSL/TLS 握手失败
            }

            if (!string.IsNullOrWhiteSpace(Fingerprint)) {
                connectSetings = connectSetings.CertificateFingerprint(Fingerprint);
            }

            if (!string.IsNullOrWhiteSpace(EsUserName) && !string.IsNullOrWhiteSpace(EsPassword)) {
                var basicAuth = new BasicAuthentication(EsUserName, EsPassword);
                connectSetings = connectSetings.Authentication(basicAuth);
            }
            if (!string.IsNullOrWhiteSpace(ApiKey)) {
                connectSetings = connectSetings.Authentication(new ApiKey(ApiKey));
            }

            return connectSetings;
        }



        /// <summary>
        /// 客户端实例
        /// </summary>
        public static readonly Lazy<ElasticsearchClient> Client = new Lazy<ElasticsearchClient>(()
            => !IsCloudIdConnected ? new ElasticsearchClient(Settings()) : new ElasticsearchClient(CloudId, new ApiKey(ApiKey)));


        /// <summary>
        /// 智能检索客户端实例
        /// </summary>
        public static readonly Lazy<ElasticsearchClient> AiClient = new Lazy<ElasticsearchClient>(()
            => !IsCloudIdConnected ? new ElasticsearchClient(AiSettings()) : new ElasticsearchClient(CloudId, new ApiKey(ApiKey)));


        /// <summary>
        ///  客户端实例
        /// </summary>
        public static readonly Lazy<ElasticsearchClient> ClientNoRawResponse =
            new Lazy<ElasticsearchClient>(()
                => !IsCloudIdConnected ? new ElasticsearchClient(Settings().ThrowExceptions(false)) : new ElasticsearchClient(CloudId, new ApiKey(ApiKey)));

        /// <summary>
        ///  客户端实例
        /// </summary>
        public static readonly Lazy<ElasticsearchClient> ClientThatThrows =
            new Lazy<ElasticsearchClient>(()
                => !IsCloudIdConnected ? new ElasticsearchClient(Settings().ThrowExceptions(true)) : new ElasticsearchClient(CloudId, new ApiKey(ApiKey)));

        /// <summary>
        /// NewUniqueIndexName
        /// </summary>
        public static string NewUniqueIndexName() {
            return DefaultIndex + "_" + Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 当前版本
        /// </summary>
        public static Version GetCurrentVersion() {
            //GET: /?pretty=true&error_trace=true
            var versionString = Client.Value.InfoAsync().Result.Version.Number;
            if (versionString.Contains("Beta"))
                versionString = string.Join(".",
                    versionString.Split('.').Where(s => !s.StartsWith("Beta", StringComparison.OrdinalIgnoreCase)));
            var version = Version.Parse(versionString);

            return version;
        }
    }
}
