using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.Core.Reindex;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch.UnitTest.Elasticsearch;
using Elastic.Clients.Elasticsearch.UnitTest.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Clients.Elasticsearch.UnitTest {
    [TestClass]
    public class UnitTestElasticsearchDocumentManagement {
        private const string _library = "chl";
        private readonly ElasticsearchClient _client = ElasticsearchConfiguration.Client.Value;

        private static dynamic GetIndexObj(int start = 100, int count = 5, int index = default) {
            var list = Enumerable.Range(start, count).Select(t => new {
                Gid = t.ToString(),
                Library = _library,
                IssueDate = "2006.03.14",
                ImplementDate = "2006.03.14",
                TimelinessDic = "01",
                IsProcess = "0",
                Title = "fang test create 索引单条数据_" + t,
                Keywords = "全国人民代表大会,常务委员会,工作报告",
                Category = new[] {
                            "001",
                            "00106"
                        },
                EffectivenessDic = new[] {
                            "XA01",
                            "XA0105"
                        },
                UpdateTime = "2022.11.22 10:02:44",
                MaxTiao = 10,
                UplineTime = "1900/3/11 16:26:13",
                FullText = "fang test create 索引单条数据_" + t
            }).ToList(); ;

            return (index > count) ? list[0] : list[index];
        }

        private static List<Dictionary<string, object>> GetIndexObjList(int start = 100, int count = 5) {
            var list = Enumerable.Range(start, count).Select(t => new Dictionary<string, object>{
                { "Gid" , t.ToString() },
                { "Library", _library },
                { "IssueDate", "2006.03.14" },
                { "ImplementDate" , "2006.03.14" },
                { "TimelinessDic","01" },
                { "IsProcess", "0" },
                { "Title" , "fang test bulk 索引多条数据_" + t },
                { "Keywords" , "全国人民代表大会,常务委员会,工作报告" },
                { "Category" , new[] {"001","00106"} },
                { "EffectivenessDic" , new[] { "XA01","XA0105"} },
                { "UpdateTime" , "2022.11.22 10:02:44" },
                { "MaxTiao" , 10 },
                {"UplineTime" , "1900/3/11 16:26:13" },
                { "FullText" , "fang test bulk 索引多条数据_"+ t }
            }).ToList();

            return list;
        }

        private static Chl GetIndexChlModel(int start = 100, int count = 5, int index = default) {
            var list = GetIndexChlModelList(start, count);
            return (index > count) ? list[0] : list[index];
        }

        private static List<Chl> GetIndexChlModelList(int start = 100, int count = 5) {
            var list = Enumerable.Range(start, count).Select(t => new Chl {
                Gid = t.ToString(),
                Library = _library,
                IssueDate = "2006.03.14",
                Title = "fang test create 索引单条数据_" + t,
                Category = new List<string> {
                            "001",
                            "00106"
                        },
                EffectivenessDic = new List<string> {
                            "XA01",
                            "XA0105"
                        },
                UpdateTime = "2022.11.22 10:02:44",
                FullText = "fang test create 索引单条数据_" + t
            }).ToList();
            return list;
        }

        [TestMethod]
        public async Task TestGetDocument() {
            //GET: /chl/_doc/75580?pretty=true&error_trace=true
            var gid = "75580";


            var getRequest = new GetRequest(_library, gid);
            var response = await _client.GetAsync<object>(getRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Found);
            Assert.IsNotNull(response.Source);
            Assert.IsNotNull(response.Source?.ToString());
            Assert.IsTrue(response.Id == gid);


            response = await _client.GetAsync<object>(_library, gid);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Found);
            Assert.IsNotNull(response.Source);
            Assert.IsNotNull(response.Source?.ToString());
            Assert.IsTrue(response.Id == gid);


            var responseChl = await _client.GetAsync(new GetRequestDescriptor<Chl>(_library, gid));

            Assert.IsNotNull(responseChl);
            Assert.IsTrue(responseChl.IsSuccess());
            Assert.IsTrue(responseChl.IsValidResponse);
            Assert.IsTrue(responseChl.Found);
            Assert.IsNotNull(responseChl.Source);
            Assert.IsTrue(responseChl.Index == _library);
            Assert.IsTrue(responseChl.Id == gid);
            Assert.IsTrue(responseChl.Source.Gid == gid);

            response = await _client.GetAsync<object>(gid, t => t.Index(_library));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Found);
            Assert.IsNotNull(response.Source);
            Assert.IsNotNull(response.Source?.ToString());
            Assert.IsTrue(response.Id == gid);

            var chlDict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(response.Source.ToString());

            Assert.IsNotNull(chlDict);
            Assert.IsNotNull(chlDict["Gid"]);
            Assert.IsTrue(chlDict["Gid"] == gid);

        }

        [TestMethod]
        public async Task TestGetSource() {

            var gid = "75580";

            //GET: /chl/_source/75580?pretty=true&error_trace=true&_source_includes=Gid%2CTitle%2CCategory%2CFlagCategory%2CMaxTiao%2CIssueDate%2CUpdateTime%2CUplineTime
            var getSourceRequest = new GetSourceRequest(_library, gid) {
                SourceIncludes = new[] { "Gid", "Title", "Category", "FlagCategory", "MaxTiao", "IssueDate", "UpdateTime", "UplineTime" }
            };
            var response = await _client.GetSourceAsync<object>(getSourceRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Body);

            //GET: /chl/_source/75580?pretty=true&error_trace=true
            response = await _client.GetSourceAsync<object>(_library, gid);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Body);

            //GET: /chl/_source/75580?pretty=true&error_trace=true
            response = await _client.GetSourceAsync(new GetSourceRequestDescriptor<object>(_library, gid));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Body);

            //GET: /chl/_source/75580?pretty=true&error_trace=true
            response = await _client.GetSourceAsync<object>(gid, t => t.Index(_library));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Body);

            //GET: /chl/_source/75580?pretty=true&error_trace=true
            var responseChl = await _client.GetSourceAsync<Chl>(gid, t => t.Index(_library));

            Assert.IsNotNull(responseChl);
            Assert.IsTrue(responseChl.IsSuccess());
            Assert.IsTrue(responseChl.IsValidResponse);
            Assert.IsNotNull(responseChl.Body);
            Assert.IsTrue(responseChl.Body.Gid == gid);
        }

        [TestMethod]
        public async Task TestExistsDocument() {

            var gid = "75580";

            //HEAD: /chl/_doc/75580?pretty=true&error_trace=true
            var existsRequest = new ExistsRequest(_library, gid);
            var response = await _client.ExistsAsync(existsRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);
            Assert.IsNotNull(response);

            response = await _client.ExistsAsync((IndexName)_library, new Id(gid));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);
            Assert.IsNotNull(response);

            response = await _client.ExistsAsync(new ExistsRequestDescriptor(_library, gid));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);
            Assert.IsNotNull(response);

            response = await _client.ExistsAsync<Chl>(gid, t => t.Index(_library));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task TestCreateDocument() {

            /*
             PUT: /chl/_create/101?pretty=true&error_trace=true&refresh=true
            # Request:
            {
              "Gid": "101",
              "Library": "chl",
              "IssueDate": "2006.03.14",
              "ImplementDate": "2006.03.14",
              "TimelinessDic": "01",
              "IsProcess": "0",
              "Title": "fang test create \u7D22\u5F15\u5355\u6761\u6570\u636E",
              "Keywords": "\u5168\u56FD\u4EBA\u6C11\u4EE3\u8868\u5927\u4F1A,\u5E38\u52A1\u59D4\u5458\u4F1A,\u5DE5\u4F5C\u62A5\u544A",
              "Category": [
                "001",
                "00106"
              ],
              "EffectivenessDic": [
                "XA01",
                "XA0105"
              ],
              "UpdateTime": "2022.11.22 10:02:44",
              "MaxTiao": 10,
              "UplineTime": "1900/3/11 16:26:13",
              "FullText": "fang test create \u7D22\u5F15\u5355\u6761\u6570\u636E"
            }
             */
            object testChlObj1 = GetIndexObj(index: 1);
            object testChlObj2 = GetIndexObj(index: 2);
            object testChlObj3 = GetIndexObj(index: 3);
            object testChlObj4 = GetIndexObj(index: 4);

            string chlObjGid1 = GetIndexObj(index: 1).Gid.ToString();
            string chlObjGid2 = GetIndexObj(index: 2).Gid.ToString();
            string chlObjGid3 = GetIndexObj(index: 3).Gid.ToString();
            string chlObjGid4 = GetIndexObj(index: 4).Gid.ToString();


            Chl testChlModel1 = GetIndexChlModel(start: 110, index: 1);
            Chl testChlModel2 = GetIndexChlModel(start: 110, index: 2);
            Chl testChlModel3 = GetIndexChlModel(start: 110, index: 3);
            Chl testChlModel4 = GetIndexChlModel(start: 110, index: 4);

            string chlModelGid1 = GetIndexChlModel(start: 110, index: 1).Gid;
            string chlModelGid2 = GetIndexChlModel(start: 110, index: 2).Gid;
            string chlModelGid3 = GetIndexChlModel(start: 110, index: 3).Gid;
            string chlModelGid4 = GetIndexChlModel(start: 110, index: 4).Gid;

            var createRequest = new CreateRequest<dynamic>(_library, chlObjGid1) {
                Refresh = Refresh.True,
                Document = testChlObj1
            };

            var response = await _client.CreateAsync(createRequest);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created);


            response = await _client.CreateAsync(new CreateRequestDescriptor<object>(testChlObj2, _library, chlObjGid2));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created);


            response = await _client.CreateAsync(testChlObj3, _library, chlObjGid3);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created);

            response = await _client.CreateAsync(testChlObj4, t => t.Index(_library).Id(chlObjGid4));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created);

            response = await _client.CreateAsync(new CreateRequest<Chl>(testChlModel1, _library, chlModelGid1));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created);

            response = await _client.CreateAsync(new CreateRequestDescriptor<Chl>(testChlModel2, _library, chlModelGid2));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created);

            response = await _client.CreateAsync(testChlModel3, _library, chlModelGid3);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created);

            response = await _client.CreateAsync(testChlModel4, t => t.Index(_library).Id(chlModelGid4));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created);
        }

        [TestMethod]
        public async Task TestIndexDocument() {

            /*
             PUT: /chl/_doc/100?pretty=true&error_trace=true&refresh=true
                # Request:
                {
                  "Gid": "100",
                  "Library": "chl",
                  "IssueDate": "2006.03.14",                  
                  "TimelinessDic": "01",                  
                  "Title": "fang test create \u7D22\u5F15\u5355\u6761\u6570\u636E",
                  "Keywords": "\u5168\u56FD\u4EBA\u6C11\u4EE3\u8868\u5927\u4F1A,\u5E38\u52A1\u59D4\u5458\u4F1A,\u5DE5\u4F5C\u62A5\u544A",
                  "Category": [
                    "001",
                    "00106"
                  ],                  
                  "EffectivenessDic": [
                    "XA01",
                    "XA0105"
                  ],
                  "UpdateTime": "2022.11.22 10:02:44",
                  "MaxTiao": 10,
                  "UplineTime": "1900/3/11 16:26:13",
                  "FullText": "fang test create \u7D22\u5F15\u5355\u6761\u6570\u636E"
                }
             */

            string chlObjGid = GetIndexObj(index: 0).Gid.ToString();
            object testChlObj = GetIndexObj(index: 0);

            Chl testChlModel = GetIndexChlModel(index: 0);
            string chlModelGid = GetIndexChlModel(index: 0).Gid;

            var indexRequest = new IndexRequest<object>(_library, chlObjGid) {
                Refresh = Refresh.True,
                Document = testChlObj
            };

            var response = await _client.IndexAsync(indexRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created || response.Result == Result.Updated);

            response = await _client.IndexAsync(testChlObj, _library, chlObjGid);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created || response.Result == Result.Updated);

            response = await _client.IndexAsync(testChlObj, t => t.Index(_library).Id(chlObjGid));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created || response.Result == Result.Updated);


            response = await _client.IndexAsync(new IndexRequestDescriptor<Chl>(testChlModel, _library, chlModelGid));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created || response.Result == Result.Updated);


            response = await _client.IndexAsync(testChlModel, _library, new Id(chlModelGid));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created || response.Result == Result.Updated);

            response = await _client.IndexAsync(testChlModel, t => t.Index(_library).Id(chlModelGid));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Created || response.Result == Result.Updated);
        }       


        [TestMethod]
        public async Task TestUpdateDocument() {
            /*
             POST: /chl/_update/100?pretty=true&error_trace=true&refresh=true
            # Request:
            {              
              "doc": {
                "UpdateTime": "2025-11-03 15:13:01"
              }              
            }
             */

            var gid = "100";
            var updateRequest = new UpdateRequest<object, dynamic>(_library, gid) {
                Doc = new {
                    UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                },
                Refresh = Refresh.True
            };
            var response = await _client.UpdateAsync(updateRequest);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Updated || response.Result == Result.NoOp);


            /*
             POST: /chl/_update/100?pretty=true&error_trace=true&refresh=true
            # Request:
            {
                "script": {
                "lang": "painless",
                "source": "ctx._source.UpdateTime = params.updateTime",
                "params": {
                    "updateTime": "2025-11-14 09:38:02"
                }
                },
                "scripted_upsert": true
            }
             */

            var script = new Script(new InlineScript() {
                Language = ScriptLanguage.Painless,
                Source = "ctx._source.UpdateTime = params.updateTime",
                Params = new Dictionary<string, object>() {
                    { "updateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                }
            });
            updateRequest = new UpdateRequest<object, dynamic>(_library, gid) {
                ScriptedUpsert = true,
                Script = script,
                Refresh = Refresh.True
            };
            response = await _client.UpdateAsync(updateRequest);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Updated || response.Result == Result.NoOp);

            var descriptor = new UpdateRequestDescriptor<object, dynamic>(_library, gid)
                             .Doc(new { UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") })
                             .Refresh(Refresh.True);
            response = await _client.UpdateAsync(descriptor);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Updated || response.Result == Result.NoOp);


            response = await _client.UpdateAsync<object, object>(_library, gid, t =>
                                                                                    t.Doc(new { UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") })
                                                                                    .Refresh(Refresh.True));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Updated || response.Result == Result.NoOp);

            /*
            POST: /chl/_update/100?pretty=true&error_trace=true&refresh=true
            # Request:
            {
              "doc": {
                "UpdateTime": "2025-11-03 15:14:33",
                "BeReferencedNum": 0
              }
            } 
            */
            var updateRequestModel = new UpdateRequest<Chl, Chl>(_library, gid) {
                Doc = new Chl { UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                Refresh = Refresh.True
            };
            var responseModel = await _client.UpdateAsync(updateRequestModel);
            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsTrue(responseModel.Result == Result.Updated || responseModel.Result == Result.NoOp);


            var descriptorModel = new UpdateRequestDescriptor<Chl, Chl>(_library, gid)
                             .Doc(new Chl { UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") })
                             .Refresh(Refresh.True);
            responseModel = await _client.UpdateAsync(descriptorModel);
            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsTrue(responseModel.Result == Result.Updated || responseModel.Result == Result.NoOp);


            responseModel = await _client.UpdateAsync<Chl, Chl>(gid, t =>
                                                                        t.Index(_library)
                                                                         .Doc(new Chl { UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") })
                                                                         .Refresh(Refresh.True));
            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsTrue(responseModel.Result == Result.Updated || responseModel.Result == Result.NoOp);
        }


        [TestMethod]
        public async Task TestPutScript() {

            //注意：ctx._source：用于更新脚本，读写原始数据。
            //      ctx.doc：用于检索脚本，只读，不能用于更新
            /*
                PUT: /_scripts/scrpit_update_updatetime1?pretty=true&error_trace=true            
            # Request:
            {
                "script": {
                "lang": "painless",
                "source": "ctx._source.UpdateTime = params.updateTime;"
                }
            }
                */

            var id1 = "scrpit_update_updatetime1";
            var id2 = "scrpit_update_updatetime2";
            var id3 = "scrpit_update_maxtiao";
            var source1 = "ctx._source.UpdateTime = params.updateTime;";
            var source2 = "ctx._source['UpdateTime'] = params.updateTime;";
            var source3 = "ctx._source['MaxTiao'] = ctx._source.MaxTiao + params.maxTiao;";

            var putScriptRequest = new PutScriptRequest(id1) {
                Script = new StoredScript() {
                    Language = ScriptLanguage.Painless,
                    Source = source1
                }
            };
            var response = await _client.PutScriptAsync(putScriptRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);


            var putScriptRequestDescriptor = new PutScriptRequestDescriptor(id2)
                                            .Script(new StoredScriptDescriptor()
                                            .Language(ScriptLanguage.Painless)
                                            .Source(source2));

            response = await _client.PutScriptAsync(putScriptRequestDescriptor);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);


            /*
             PUT: /_scripts/scrpit_update_maxtiao?pretty=true&error_trace=true
            # Request:
            {
              "script": {
                "lang": "painless",
                "source": "ctx._source[\u0027MaxTiao\u0027] = ctx._source.MaxTiao \u002B params.maxTiao;"
              }
            }
             */

            response = await _client.PutScriptAsync(id3, t => t.Script(s =>
                                                                s.Language(ScriptLanguage.Painless)
                                                                 .Source(source3)));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);

        }

        [TestMethod]
        public async Task TestGetScript() {
            /*
             GET: /_scripts/scrpit_update_updatetime1?pretty=true&error_trace=true
                # Response:
                {
                  "_id" : "scrpit_update_updatetime1",
                  "found" : true,
                  "script" : {
                    "lang" : "painless",
                    "source": "ctx._source.UpdateTime = params.updateTime;"
                  }
                }
             */
            var id1 = "scrpit_update_updatetime1";
            var id2 = "scrpit_update_updatetime2";
            var id3 = "scrpit_update_maxtiao";

            var response = await _client.GetScriptAsync(new GetScriptRequest(id1));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Found);
            Assert.IsTrue(response.Id == id1);


            response = await _client.GetScriptAsync(new GetScriptRequestDescriptor(id2));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Found);
            Assert.IsTrue(response.Id == id2);


            /*
             GET: /_scripts/scrpit_update_maxtiao?pretty=true&error_trace=true
                # Response:
                {
                  "_id" : "scrpit_update_maxtiao",
                  "found" : true,
                  "script" : {
                    "lang" : "painless",
                    "source" : "ctx._source['MaxTiao'] = ctx._source.MaxTiao + params.maxTiao;"
                  }
                }
             */

            response = await _client.GetScriptAsync(id3);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Found);
            Assert.IsTrue(response.Id == id3);

        }

        [TestMethod]
        public async Task TestDeleteScript() {

            /*
             DELETE: /_scripts/scrpit_update_updatetime1?pretty=true&error_trace=true
             */

            var id1 = "scrpit_update_updatetime1";
            var id2 = "scrpit_update_updatetime2";
            var id3 = "scrpit_update_maxtiao";

            var response = await _client.DeleteScriptAsync(new DeleteScriptRequest(id1));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);


            response = await _client.DeleteScriptAsync(new DeleteScriptRequestDescriptor(id2));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);


            response = await _client.DeleteScriptAsync(id3);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);

        }

        [TestMethod]
        public async Task TestUpdateByQuery() {

            /*
             POST: /chl/_update_by_query?pretty=true&error_trace=true&refresh=true
            # Request:
            {
              "query": {
                "term": {
                  "Gid": {
                    "value": "100"
                  }
                }
              },
              "script": {
                "source": "ctx._source.UpdateTime = params.updateTime ; ctx._source.MaxTiao = ctx._source.MaxTiao \u002B params.maxTiao;",
                "params": {
                  "updateTime": "2025-11-14 14:20:24",
                  "maxTiao": 5
                }
              }
            }
             */

            var gid = "100";
            var termQuery = new TermQuery("Gid") {
                Value = gid
            };

            //直接拼接参数值方式
            //var script = new InlineScript() {
            //    Source = $"ctx._source.UpdateTime = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}';ctx._source.MaxTiao = ctx._source.MaxTiao + {5};" // ok 
            //    Source = $"ctx._source.UpdateTime = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}'" // ok                
            //};

            //使用内置params对象方式
            var script = new InlineScript() {
                Source = "ctx._source.UpdateTime = params.updateTime ; ctx._source.MaxTiao = ctx._source.MaxTiao + params.maxTiao;",
                Params = new Dictionary<string, object>() {
                    { "updateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                    { "maxTiao", 5 }
                }
            };

            var updateRequest = new UpdateByQueryRequest(_library) {
                Query = termQuery,
                Script = new Script(script),
                Refresh = true
            };

            var response = await _client.UpdateByQueryAsync(updateRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Total > 0);
            Assert.IsTrue(response.Updated > 0 || response.Noops > 0);


            /*
             POST: /chl/_update_by_query?pretty=true&error_trace=true&refresh=true
            # Request:
            {
              "query": {
                "term": {
                  "Gid": {
                    "value": "100"
                  }
                }
              },
              "script": {
                "id": "scrpit_update_updatetime1",
                "params": {
                  "updateTime": "2025-11-14 14:44:24"
                }
              }
            }
             */
            var scriptId1 = "scrpit_update_updatetime1";
            var storedScript = new Script(new StoredScriptId(scriptId1) {
                Params = new Dictionary<string, object>() {
                    { "updateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                }
            });

            var updateByQueryRequestDescriptor = new UpdateByQueryRequestDescriptor(_library)
                                                    .Query(termQuery)
                                                    .Refresh()
                                                    .Script(storedScript);

            response = await _client.UpdateByQueryAsync(updateByQueryRequestDescriptor);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Total > 0);
            Assert.IsTrue(response.Updated > 0 || response.Noops > 0);



            /*
             POST: /chl/_update_by_query?pretty=true&error_trace=true&refresh=true
            # Request:
            {
                "query": {
                "term": {
                    "Gid": {
                    "value": "100"
                    }
                }
                },
                "script": {
                "id": "scrpit_update_maxtiao",
                "params": {
                    "maxTiao": 5
                }
                }
            }
             */
            var scriptId3 = "scrpit_update_maxtiao";
            var storedScript3 = new Script(new StoredScriptId(scriptId3) {
                Params = new Dictionary<string, object>() {
                    { "maxTiao", 5 }
                }
            });

            response = await _client.UpdateByQueryAsync(_library, t => t.Query(termQuery)
                                                                        .Refresh()
                                                                        .Script(storedScript3));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Total > 0);
            Assert.IsTrue(response.Updated > 0 || response.Noops > 0);
        }


        [TestMethod]
        public async Task TestDeleteDocument() {

            string chlObjGid1 = "101";
            string chlObjGid2 = "102";
            string chlObjGid3 = "103";
            string chlObjGid4 = "104";

            //var gid = "100";
            //var response = await _client.DeleteAsync<object>(_library, gid); //ok

            var deleteRequest = new DeleteRequest(_library, chlObjGid1) {
                Refresh = Refresh.True,
                VersionType = VersionType.Internal
            };
            var response = await _client.DeleteAsync(deleteRequest);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Deleted || response.Result == Result.NoOp);


            response = await _client.DeleteAsync(new DeleteRequestDescriptor(_library, chlObjGid2));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Deleted || response.Result == Result.NoOp);


            response = await _client.DeleteAsync(Indices.Index(_library), new Id(chlObjGid3));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Deleted || response.Result == Result.NoOp);


            response = await _client.DeleteAsync<object>(chlObjGid4, t => t.Index(_library));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Result == Result.Deleted || response.Result == Result.NoOp);
        }

        [TestMethod]
        public async Task TestDeleteByQuery() {
            /*
             POST: /chl/_delete_by_query?pretty=true&error_trace=true&refresh=true
            # Request:
            {
              "query": {
                "terms": {
                  "Gid": [
                    "200",
                    "201",
                    "202"
                  ]
                }
              }
            }
             */
            var gids = "200,201,202";
            var termQuery = new TermsQuery() {
                Field = "Gid",
                Terms = new TermsQueryField(gids.Split(',').Select(t => FieldValue.String(t)).ToList())
            };

            var deleteRequest = new DeleteByQueryRequest(_library) {
                Query = termQuery,
                Refresh = true
            };

            var response = await _client.DeleteByQueryAsync(deleteRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Total >= 0);
            Assert.IsTrue(response.Deleted >= 0 || response.Noops >= 0);


            /*
             POST: /chl/_delete_by_query?pretty=true&error_trace=true
                # Request:
                {
                  "query": {
                    "ids": {
                      "values": [
                        "110",
                        "111"
                      ]
                    }
                  }
                }
             */
            var gids2 = "110,111";
            var deleteByQueryRequestDescriptor = new DeleteByQueryRequestDescriptor(_library)
                                            .Query(Query.Ids(new IdsQuery() {
                                                Values = new Ids(gids2)
                                            }));
            response = await _client.DeleteByQueryAsync(deleteByQueryRequestDescriptor);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Total >= 0);
            Assert.IsTrue(response.Deleted >= 0 || response.Noops >= 0);



            var gids3 = "112,113,114";
            response = await _client.DeleteByQueryAsync<object>(t =>
                                        t.Indices(_library)
                                         .Query(q => q.Ids(i => i.Values(gids3))));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Total >= 0);
            Assert.IsTrue(response.Deleted >= 0 || response.Noops >= 0);
        }


        [TestMethod]
        public async Task TestBulkCreateMany() {
            /*
             /_bulk?pretty=true&error_trace=true&refresh=true
            # Request:
            {"create":{"_id":"200","_index":"chl"}}
            {"Gid":"200","Title":"fang test create \u7D22\u5F15\u5355\u6761\u6570\u636E_200","Library":"chl","EffectivenessDic":["XA01","XA0105"],"Category":["001","00106"],"UpdateTime":"2022.11.22 10:02:44","IssueDate":"2006.03.14","BeReferencedNum":0,"FullText":"fang test create \u7D22\u5F15\u5355\u6761\u6570\u636E_200"}
            {"create":{"_id":"201","_index":"chl"}}
            {"Gid":"201","Title":"fang test create \u7D22\u5F15\u5355\u6761\u6570\u636E_201","Library":"chl","EffectivenessDic":["XA01","XA0105"],"Category":["001","00106"],"UpdateTime":"2022.11.22 10:02:44","IssueDate":"2006.03.14","BeReferencedNum":0,"FullText":"fang test create \u7D22\u5F15\u5355\u6761\u6570\u636E_201"}
             */
            var dataList1 = GetIndexChlModelList(200, 2);
            var dataList2 = GetIndexChlModelList(202, 2);
            var dataList3 = GetIndexChlModelList(204, 2);

            //BulkCreateOperation,BulkIndexOperation,BulkUpdateOperation,BulkDeletOperation
            var bulkIndexOptions = dataList1.Select(t => new BulkCreateOperation<Chl>(t) { Index = _library, Id = t.Gid });
            var bulkRequest = new BulkRequest() {
                Refresh = Refresh.True,
                Operations = new BulkOperationsCollection(bulkIndexOptions)
            };

            var response = await _client.BulkAsync(bulkRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);


            var bulkRequestDescriptor = new BulkRequestDescriptor(_library)
                                            .CreateMany(dataList2, (d, item) => d.Id(item.Gid))
                                            .Refresh(Refresh.True);
            response = await _client.BulkAsync(bulkRequestDescriptor);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);


            response = await _client.BulkAsync(b =>
                                        b.Index(_library)
                                         .CreateMany(dataList3, (d, item) => d.Id(item.Gid))
                                         .Refresh(Refresh.True));
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);

        }
        [TestMethod]
        public async Task TestBulkIndexMany() {
            /*
             POST: /chl/_bulk?pretty=true&error_trace=true&refresh=true
            # Request:
            {"index":{"_id":"200"}}
            {"Gid":"200","Library":"chl","IssueDate":"2006.03.14","ImplementDate":"2006.03.14","TimelinessDic":"01","IsProcess":"0","Title":"fang test bulk \u7D22\u5F15\u591A\u6761\u6570\u636E_200","Keywords":"\u5168\u56FD\u4EBA\u6C11\u4EE3\u8868\u5927\u4F1A,\u5E38\u52A1\u59D4\u5458\u4F1A,\u5DE5\u4F5C\u62A5\u544A","Category":["001","00106"],"EffectivenessDic":["XA01","XA0105"],"UpdateTime":"2022.11.22 10:02:44","MaxTiao":10,"UplineTime":"1900/3/11 16:26:13","FullText":"fang test bulk \u7D22\u5F15\u591A\u6761\u6570\u636E_200"}
            {"index":{"_id":"201"}}
            {"Gid":"201","Library":"chl","IssueDate":"2006.03.14","ImplementDate":"2006.03.14","TimelinessDic":"01","IsProcess":"0","Title":"fang test bulk \u7D22\u5F15\u591A\u6761\u6570\u636E_201","Keywords":"\u5168\u56FD\u4EBA\u6C11\u4EE3\u8868\u5927\u4F1A,\u5E38\u52A1\u59D4\u5458\u4F1A,\u5DE5\u4F5C\u62A5\u544A","Category":["001","00106"],"EffectivenessDic":["XA01","XA0105"],"UpdateTime":"2022.11.22 10:02:44","MaxTiao":10,"UplineTime":"1900/3/11 16:26:13","FullText":"fang test bulk \u7D22\u5F15\u591A\u6761\u6570\u636E_201"}
            {"index":{"_id":"202"}}
            {"Gid":"202","Library":"chl","IssueDate":"2006.03.14","ImplementDate":"2006.03.14","TimelinessDic":"01","IsProcess":"0","Title":"fang test bulk \u7D22\u5F15\u591A\u6761\u6570\u636E_202","Keywords":"\u5168\u56FD\u4EBA\u6C11\u4EE3\u8868\u5927\u4F1A,\u5E38\u52A1\u59D4\u5458\u4F1A,\u5DE5\u4F5C\u62A5\u544A","Category":["001","00106"],"EffectivenessDic":["XA01","XA0105"],"UpdateTime":"2022.11.22 10:02:44","MaxTiao":10,"UplineTime":"1900/3/11 16:26:13","FullText":"fang test bulk \u7D22\u5F15\u591A\u6761\u6570\u636E_202"}
             */

            var dataObjList = GetIndexObjList(start: 200, count: 3);

            //BulkCreateOperation,BulkIndexOperation,BulkUpdateOperation,BulkDeletOperation
            var bulkIndexOperationsObj = dataObjList.Select(t => new BulkIndexOperation<object>(t) { Id = t["Gid"].ToString() });

            var bulkRequestObj = new BulkRequest(_library) {
                Refresh = Refresh.True,
                Operations = new BulkOperationsCollection(bulkIndexOperationsObj)
            };

            var response = await _client.BulkAsync(bulkRequestObj);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);


            var bulkRequestDescriptorObj = new BulkRequestDescriptor(_library)
                                            .IndexMany(dataObjList, (d, item) => d.Id(item["Gid"].ToString()))
                                            .Refresh(Refresh.True);
            response = await _client.BulkAsync(bulkRequestDescriptorObj);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);


            response = await _client.BulkAsync(r =>
                                        r.Index(_library)
                                         //.IndexMany(dataList, (b,o) => b.Id(((Dictionary<string,object>)o)["Gid"].ToString()))
                                         //.IndexMany(dataList, (b, o) => b.Id(o.Gid)) //设置_id与gid值相同                                         
                                         .IndexMany(dataObjList, (b, o) => b.Id(o["Gid"].ToString()))
                                         .Refresh(Refresh.True)
                                      );

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);


            var dataModelList = GetIndexChlModelList(start: 200, count: 3);
            var bulkIndexOperationsModel = dataModelList.Select(t => new BulkIndexOperation<Chl>(t) { Id = t.Gid });
            var bulkRequestModel = new BulkRequest(_library) {
                Refresh = Refresh.True,
                Operations = new BulkOperationsCollection(bulkIndexOperationsModel)
            };
            var responseModel = await _client.BulkAsync(bulkRequestModel);
            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsNotNull(responseModel.Items);
            Assert.IsFalse(responseModel.Errors);


            var BulkRequestDescriptorModel = new BulkRequestDescriptor(_library)
                                            .IndexMany(dataModelList, (d, item) => d.Id(item.Gid))
                                            .Refresh(Refresh.True);
            responseModel = await _client.BulkAsync(BulkRequestDescriptorModel);
            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsNotNull(responseModel.Items);
            Assert.IsFalse(responseModel.Errors);


            responseModel = await _client.BulkAsync(r =>
                                        r.Index(_library)
                                         .IndexMany(dataModelList, (b, o) => b.Id(o.Gid))
                                         .Refresh(Refresh.True)
                                      );

            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsNotNull(responseModel.Items);
            Assert.IsFalse(responseModel.Errors);


        }

        [TestMethod]
        public async Task TestBulkUpdateMany() {
            /*
             POST: /chl/_bulk?pretty=true&error_trace=true&refresh=true
                # Request:
                {"update":{"_id":"200"}}
                {"doc":{"Gid":"200","UpdateTime":"2025-11-18 11:38:18"}}
                {"update":{"_id":"201"}}
                {"doc":{"Gid":"201","UpdateTime":"2025-11-18 11:38:18"}}
                {"update":{"_id":"202"}}
                {"doc":{"Gid":"202","UpdateTime":"2025-11-18 11:38:18"}}
             */

            var dataList = Enumerable.Range(200, 3)
                                     .Select(t =>
                                        new Chl {
                                            Gid = t.ToString(),
                                            UpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                        }).ToList();

            var dataDict = Enumerable.Range(200, 3)
                                     .Select(t =>
                                         new Dictionary<string, object> {
                                            { "Gid" , t.ToString() },
                                            { "UpdateTime" , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                                         }).ToList();



            var bulkUpdateOperationsObj = dataDict.Select(t => new BulkUpdateOperation<object, object>(t["Gid"].ToString()) {
                Doc = t
            });
            var bulkRequestObj = new BulkRequest(_library) {
                Refresh = Refresh.True,
                Operations = new BulkOperationsCollection(bulkUpdateOperationsObj)
            };

            var responseObj = await _client.BulkAsync(bulkRequestObj);
            Assert.IsNotNull(responseObj);
            Assert.IsTrue(responseObj.IsSuccess());
            Assert.IsTrue(responseObj.IsValidResponse);
            Assert.IsNotNull(responseObj.Items);
            Assert.IsFalse(responseObj.Errors);


            var bulkRequestDescriptorObj = new BulkRequestDescriptor(_library)
                                        .UpdateMany(dataDict, (d, item) => d.Doc(item).Id(item["Gid"].ToString()))
                                        .Refresh(Refresh.True);

            responseObj = await _client.BulkAsync(bulkRequestDescriptorObj);
            Assert.IsNotNull(responseObj);
            Assert.IsTrue(responseObj.IsSuccess());
            Assert.IsTrue(responseObj.IsValidResponse);
            Assert.IsNotNull(responseObj.Items);
            Assert.IsFalse(responseObj.Errors);


            responseObj = await _client.BulkAsync(r =>
                                        r.Index(_library)
                                         .UpdateMany(dataDict, (b, o) => b.Doc(o).Id(o["Gid"].ToString()))
                                         .Refresh(Refresh.True)
                                      );

            Assert.IsNotNull(responseObj);
            Assert.IsTrue(responseObj.IsSuccess());
            Assert.IsTrue(responseObj.IsValidResponse);
            Assert.IsNotNull(responseObj.Items);
            Assert.IsFalse(responseObj.Errors);


            /*
             POST: /chl/_bulk?pretty=true&error_trace=true&refresh=true
                # Request:
                {"update":{"_id":"200"}}
                {"doc":{"Gid":"200","UpdateTime":"2025-11-18 11:38:18","BeReferencedNum":0}}
                {"update":{"_id":"201"}}
                {"doc":{"Gid":"201","UpdateTime":"2025-11-18 11:38:18","BeReferencedNum":0}}
                {"update":{"_id":"202"}}
                {"doc":{"Gid":"202","UpdateTime":"2025-11-18 11:38:18","BeReferencedNum":0}}
             */
            var bulkUpdateOperationsModel = dataList.Select(t => new BulkUpdateOperation<Chl, Chl>(t.Gid) {
                Doc = t
            });
            var bulkRequestModel = new BulkRequest(_library) {
                Refresh = Refresh.True,
                Operations = new BulkOperationsCollection(bulkUpdateOperationsModel)
            };

            var responseModel = await _client.BulkAsync(bulkRequestModel);
            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsNotNull(responseModel.Items);
            Assert.IsFalse(responseModel.Errors);


            var bulkRequestDescriptorModel = new BulkRequestDescriptor(_library)
                                       .UpdateMany(dataList, (d, item) => d.Doc(item).Id(item.Gid))
                                       .Refresh(Refresh.True);

            responseModel = await _client.BulkAsync(bulkRequestDescriptorModel);
            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsNotNull(responseModel.Items);
            Assert.IsFalse(responseModel.Errors);


            responseModel = await _client.BulkAsync(r =>
                                        r.Index(_library)
                                         .UpdateMany(dataList, (b, o) => b.Doc(o).Id(o.Gid)) //设置_id与gid值相同，UpdateMany doc 不能使用匿名类型，只能使用强类型或字典类型
                                         .Refresh(Refresh.True)
                                      );
            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsNotNull(responseModel.Items);
            Assert.IsFalse(responseModel.Errors);


            /*
             POST: /chl/_bulk?pretty=true&error_trace=true&refresh=true
                # Request:
                {"update":{"_id":"200"}}
                {"script":{"lang":"painless","params":{"maxTiao":5},"source":"ctx._source.MaxTiao = (ctx._source.containsKey(\u0027MaxTiao\u0027) \u0026\u0026 ctx._source.MaxTiao != null ? ctx._source.MaxTiao : 0) \u002B params.maxTiao"},"scripted_upsert":true}
                {"update":{"_id":"201"}}
                {"script":{"lang":"painless","params":{"maxTiao":5},"source":"ctx._source.MaxTiao = (ctx._source.containsKey(\u0027MaxTiao\u0027) \u0026\u0026 ctx._source.MaxTiao != null ? ctx._source.MaxTiao : 0) \u002B params.maxTiao"},"scripted_upsert":true}
                {"update":{"_id":"202"}}
                {"script":{"lang":"painless","params":{"maxTiao":5},"source":"ctx._source.MaxTiao = (ctx._source.containsKey(\u0027MaxTiao\u0027) \u0026\u0026 ctx._source.MaxTiao != null ? ctx._source.MaxTiao : 0) \u002B params.maxTiao"},"scripted_upsert":true}
             */
            responseModel = await _client.BulkAsync(r =>
                                        r.Index(_library)
                                         .UpdateMany(dataList, (b, o) => b.ScriptedUpsert()
                                                                          .Script(s => s.Source("ctx._source.MaxTiao = (ctx._source.containsKey('MaxTiao') && ctx._source.MaxTiao != null ? ctx._source.MaxTiao : 0) + params.maxTiao")
                                                                                        .Params(p => p.Add("maxTiao", 5))
                                                                                        .Language(ScriptLanguage.Painless))
                                                                          .Id(o.Gid))

                                         .Refresh(Refresh.True)
                                      );
            Assert.IsNotNull(responseModel);
            Assert.IsTrue(responseModel.IsSuccess());
            Assert.IsTrue(responseModel.IsValidResponse);
            Assert.IsNotNull(responseModel.Items);
            Assert.IsFalse(responseModel.Errors);
        }

        [TestMethod]
        public async Task TestBulkDeleteMany() {

            /*
             POST: /chl/_bulk?pretty=true&error_trace=true&refresh=true
            # Request:
            {"delete":{"_id":"200"}}
            {"delete":{"_id":"201"}}
             */

            var dataDict1 = Enumerable.Range(200, 2)
                                     .Select(t =>
                                        new {
                                            Gid = t.ToString()
                                        }).ToList();

            var dataDict2 = Enumerable.Range(202, 2)
                                     .Select(t =>
                                        new {
                                            Gid = t.ToString()
                                        }).ToList();
            var dataDict3 = Enumerable.Range(204, 2)
                                     .Select(t =>
                                        new {
                                            Gid = t.ToString()
                                        }).ToList();

            var dataDict4 = Enumerable.Range(206, 3)
                                     .Select(t =>
                                        new {
                                            Gid = t.ToString()
                                        }).ToList();

            var bulkDeleteOperationsObj = dataDict1.Select(t => new BulkDeleteOperation(t.Gid));
            var bulkRequestObj = new BulkRequest(_library) {
                Refresh = Refresh.True,
                Operations = new BulkOperationsCollection(bulkDeleteOperationsObj)
            };

            var response = await _client.BulkAsync(bulkRequestObj);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);



            var bulkRequestDescriptorObj = new BulkRequestDescriptor(_library)
                                        .DeleteMany(dataDict2, (d, item) => d.Id(item.Gid))
                                        .Refresh(Refresh.True);

            response = await _client.BulkAsync(bulkRequestDescriptorObj);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);


            response = await _client.BulkAsync(r =>
                                        r.Index(_library)
                                         .DeleteMany(dataDict3, (b, o) => b.Id(o.Gid))
                                         .Refresh(Refresh.True)
                                      );

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);


            /*
             POST: /_bulk?pretty=true&error_trace=true&refresh=true
            # Request:
            {"delete":{"_id":"206","_index":"chl"}}
            {"delete":{"_id":"207","_index":"chl"}}
            {"delete":{"_id":"208","_index":"chl"}}
             */
            response = await _client.BulkAsync(r =>
                                        r.DeleteMany(_library, dataDict4.Select(t => new Id(t.Gid)))
                                         .Refresh(Refresh.True)
                                      );

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);
        }



        [TestMethod]
        public async Task TestBulkDelete() {

            var dataList = Enumerable.Range(200, 6)
                                     .Select(t =>
                                        new {
                                            Gid = t.ToString()
                                        }).ToList();
            /*
            POST: /_bulk?pretty=true&error_trace=true
            # Request:
            {"delete":{"_id":"200","_index":"chl"}}
            {"delete":{"_id":"201","_index":"chl"}}
            {"delete":{"_id":"202","_index":"chl"}}
             
             */
            //var bulkDeleteOptions = dataList.Select(t => new BulkDeleteOperation(t.Gid) { Index = _library });
            //var bulkRequest = new BulkRequest() {
            //    Operations= new BulkOperationsCollection(bulkDeleteOptions)
            //};

            /*
             POST: /chl/_bulk?pretty=true&error_trace=true            
            # Request:
            {"delete":{"_id":"200"}}
            {"delete":{"_id":"201"}}
            {"delete":{"_id":"202"}} 

             */
            var bulkDeleteOptions = dataList.Select(t => new BulkDeleteOperation(t.Gid));
            var bulkRequest = new BulkRequest(_library) {
                Operations = new BulkOperationsCollection(bulkDeleteOptions)
            };
            var response = await _client.BulkAsync(bulkRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Items);
            Assert.IsFalse(response.Errors);
        }


        [TestMethod]
        public async Task TestReIndex() {
            /*
             * 测试数据
             PUT /test/_doc/100?pretty=true&error_trace=true&refresh=true
             {
              "Gid": "100",
              "Library": "test",
              "Date2": "2006.03.14",
              "Category2": [
                "01"
              ],
              "Phrase1": "测试发字2025 第2号",
              "Number1": 1,
              "Title": "fang test create 索引单条数据",
              "Category4": [
                "001",
                "00106"
              ],
              "Category3": [
                "XA01",
                "XA0105"
              ],
              "UpdateTime": "2022.11.22 10:02:44",
              "CreateTime": "1900.03.11 16:26:13",
              "FullText": "fang test reindex 索引数据"
            }


            POST: /_reindex?pretty=true&error_trace=true&wait_for_completion=true
                # Request:
                {
                  "conflicts": "proceed",
                  "dest": {
                    "index": "testa"
                  },
                  "script": {
                    "lang": "painless",
                    "source": "ctx._source.UpdateTime = params.updateTime",
                    "params": {
                      "updateTime": "2025.11.18 15:17:12"
                    }
                  },
                  "source": {
                    "index": "test",
                    "query": {
                      "term": {
                        "Gid": {
                          "value": "100"
                        }
                      }
                    },	
                    "_source": [
                      "Gid",
                      "Title",
                      "FullText",
                      "Category2",
                      "Phrase1",
                      "CreateTime"
                    ]
                  }
                }
             */

            var destIndex = "testa";
            var sourceIndex = "test";
            var sourceGid = "100";
            var keyField = "Gid";

            var sourceFields = new[] { "Gid", "Title", "FullText", "Category2", "Phrase1", "CreateTime" }.Select(t => new Field(t)).ToArray();
            var query = Query.Term(new TermQuery(keyField) { Value = sourceGid });
            var script = new Script(new InlineScript() {
                Language = ScriptLanguage.Painless,
                Source = "ctx._source.UpdateTime = params.updateTime",
                Params = new Dictionary<string, object>() {
                        { "updateTime", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss") }
                    }
            });
            var reindexRequest = new ReindexRequest() {
                Dest = new Destination() {
                    Index = destIndex
                    //,
                    //Pipeline= "pipeline_name"//如果需要在 Reindex 时对字段进行转换或重命名，可以使用 Ingest Pipeline
                },
                Source = new Source() {
                    Indices = sourceIndex,
                    SourceFields = sourceFields,
                    Query = query
                },
                Conflicts = Conflicts.Proceed,
                Script = script,
                //RequireAlias = true,//强制要求目标索引必须是别名
                WaitForCompletion = true
            };
            var response = await _client.ReindexAsync(reindexRequest);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Batches >= 0);
            Assert.IsTrue(response.Total >= 0);


            var reindexRequestDescriptor = new ReindexRequestDescriptor()
                                            .Source(new SourceDescriptor()
                                                        .Indices(sourceIndex)
                                                        .Query(query)
                                                        .SourceFields(sourceFields))
                                            .Dest(new DestinationDescriptor()
                                                    .Index(destIndex))
                                            .Conflicts(Conflicts.Proceed)
                                            .Script(script)
                                            .WaitForCompletion();
            response = await _client.ReindexAsync(reindexRequestDescriptor);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Batches >= 0);
            Assert.IsTrue(response.Total >= 0);


            response = await _client.ReindexAsync(t => t.Source(s => s.Indices(sourceIndex)
                                                                      .SourceFields(sourceFields)
                                                                      .Query(q => q.Term<object>(tq => tq.Field(keyField).Value(sourceGid))))
                                                        .Dest(d => d.Index(destIndex))
                                                        .Conflicts(Conflicts.Proceed)
                                                        .Script(script)
                                                        .WaitForCompletion());
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Batches >= 0);
            Assert.IsTrue(response.Total >= 0);

        }
    }
}
