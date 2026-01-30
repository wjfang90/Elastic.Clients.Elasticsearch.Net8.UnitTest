using Elastic.Clients.Elasticsearch.QueryDsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Clients.Elasticsearch.UnitTest.Elasticsearch {
    /// <summary>
    /// ES操作客户端基类
    /// </summary>
    public class ElasticsearchBase {
        /// <summary>
        /// 客户端
        /// </summary>
        protected ElasticsearchClient Client { get { return ElasticsearchConfiguration.Client.Value; } }


        /// <summary>
        /// 智能检索 客户端
        /// </summary>
        protected ElasticsearchClient AiClient { get { return ElasticsearchConfiguration.AiClient.Value; } }


        /// <summary>
        /// 
        /// </summary>
        protected ElasticsearchClient ClientThatThrows { get { return ElasticsearchConfiguration.ClientThatThrows.Value; } }
        /// <summary>
        /// 
        /// </summary>
        protected ElasticsearchClient ClientNoRawResponse { get { return ElasticsearchConfiguration.ClientNoRawResponse.Value; } }
        /// <summary>
        /// 设置
        /// </summary>
        protected ElasticsearchClientSettings Settings { get { return ElasticsearchConfiguration.Settings(); } }

        /// <summary>
        /// 检索方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        protected async Task<SearchResponse<SearchResponse<T>>> SearchRawAsync<T>(string query) where T : class {
            var index = this.Client.Infer.IndexName<T>();

            SearchRequest searchRequest = new SearchRequest(index) {
                Query = Query.RawJson(new RawJsonQuery(query))
            };
            var response = await this.Client.SearchAsync<SearchResponse<T>>(searchRequest);

            return response;
        }
    }
}
