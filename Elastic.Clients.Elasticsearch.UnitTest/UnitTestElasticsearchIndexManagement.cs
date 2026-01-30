using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
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
    public class UnitTestElasticsearchIndexManagement {

        private const string _library = "chl";
        private const string _testIndex = "test";
        private const string _testa = "testa";
        private const string _testb = "testb";
        private const string _testc = "testc";

        private const string _testAlias1 = "test1";
        private const string _testAlias2 = "test2";
        private const string _testAlias3 = "test3";
        private const string _testAlias4 = "test4";
        private readonly ElasticsearchClient _client = ElasticsearchConfiguration.Client.Value;

        private Query aliasFilter = Query.Range(
                                new RangeQuery(
                                    new DateRangeQuery("UpdateTime") {
                                        From = "2025-01-01"
                                    }));

        #region 索引管理

        private IDictionary<PropertyName, IProperty> GetCustomProperties() {
            var defaultTextProperty = new TextProperty() {
                Store = true,
                TermVector = TermVectorOption.WithPositionsOffsets,
                Analyzer = "standard"
            };

            var defaultDateProperty = new DateProperty() {
                Store = true,
                Format = "yyyy||yyyyMM||yyyy.MM||yyyy/MM||yyyy-MM||yyyyMMdd||yyyy.MM.dd||yyyy/MM/dd||yyyy-MM-dd||yyyy-MM-dd HH:mm:ss||yyyy.MM.dd HH:mm:ss||yyyy/MM/dd HH:mm:ss||yyyy-MM-dd HH:mm:ss.SSS||yyyy.MM.dd HH:mm:ss.SSS||yyyy/MM/dd HH:mm:ss.SSS"
            };


            var categoryProperties = Enumerable.Range(1, 10)
                                               .Select(t => new KeyValuePair<PropertyName, IProperty>("Category" + t, new KeywordProperty() { Store = true }))
                                               .ToList();
            var dateProperties = Enumerable.Range(1, 5)
                                            .Select(t => new KeyValuePair<PropertyName, IProperty>("Date" + t, defaultDateProperty))
                                            .ToList();
            var fultextProperties = Enumerable.Range(1, 5)
                                              .Select(t => new KeyValuePair<PropertyName, IProperty>("FullText" + t, defaultTextProperty))
                                              .ToList();

            var phraseProperties = Enumerable.Range(1, 5)
                                              .Select(t => new KeyValuePair<PropertyName, IProperty>("Phrase" + t, defaultTextProperty))
                                              .ToList();

            var numberProperties = Enumerable.Range(1, 5)
                                             .Select(t => new KeyValuePair<PropertyName, IProperty>("Number" + t, new IntegerNumberProperty()))
                                             .ToList();

            var properties = new List<KeyValuePair<PropertyName, IProperty>>() {
                new KeyValuePair<PropertyName, IProperty>("Gid",new KeywordProperty()),
                new KeyValuePair<PropertyName, IProperty>("Title",defaultTextProperty),
                new KeyValuePair<PropertyName, IProperty>("FullText",new TextProperty() {
                        Index = false
                    }),
                new KeyValuePair<PropertyName, IProperty>("CheckFullText",new TextProperty() {
                        TermVector = TermVectorOption.WithPositionsOffsets,
                        Analyzer = "v6standardV3"
                    }),
                new KeyValuePair<PropertyName, IProperty>("CreateTime",defaultDateProperty),
                new KeyValuePair<PropertyName, IProperty>("UpdateTime",defaultDateProperty),
                new KeyValuePair<PropertyName, IProperty>("NavCatalog",new ObjectProperty())
            };

            properties.AddRange(categoryProperties);
            properties.AddRange(dateProperties);
            properties.AddRange(fultextProperties);
            properties.AddRange(phraseProperties);
            properties.AddRange(numberProperties);

            return properties.ToDictionary(t => t.Key, v => v.Value);
        }

        [TestMethod]
        public async Task TestCreateIndex() {

            #region es语句

            /*PUT /test
             {
              "aliases": {},
              "mappings": {
                "dynamic": "false",
                "_source": {
                  "excludes": [
                    "CheckFullText"
                  ]
                },
                "properties": {
                  "Category1": {
                    "type": "keyword",
                    "store": true
                  },
                  "Category10": {
                    "type": "keyword",
                    "store": true
                  },
                  "Category2": {
                    "type": "keyword",
                    "store": true
                  },
                  "Category3": {
                    "type": "keyword",
                    "store": true
                  },
                  "Category4": {
                    "type": "keyword",
                    "store": true
                  },
                  "Category5": {
                    "type": "keyword",
                    "store": true
                  },
                  "Category6": {
                    "type": "keyword",
                    "store": true
                  },
                  "Category7": {
                    "type": "keyword",
                    "store": true
                  },
                  "Category8": {
                    "type": "keyword",
                    "store": true
                  },
                  "Category9": {
                    "type": "keyword",
                    "store": true
                  },
                  "CheckFullText": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "v6standardV3"
                  },
                  "CreateTime": {
                    "type": "date",
                    "store": true,
                    "format": "yyyy||yyyyMM||yyyy.MM||yyyy/MM||yyyy-MM||yyyyMMdd||yyyy.MM.dd||yyyy/MM/dd||yyyy-MM-dd||yyyy-MM-dd HH:mm:ss||yyyy.MM.dd HH:mm:ss||yyyy/MM/dd HH:mm:ss||yyyy-MM-dd HH:mm:ss.SSS||yyyy.MM.dd HH:mm:ss.SSS||yyyy/MM/dd HH:mm:ss.SSS"
                  },
                  "Date1": {
                    "type": "date",
                    "store": true,
                    "format": "yyyy||yyyyMM||yyyy.MM||yyyy/MM||yyyy-MM||yyyyMMdd||yyyy.MM.dd||yyyy/MM/dd||yyyy-MM-dd||yyyy-MM-dd HH:mm:ss||yyyy.MM.dd HH:mm:ss||yyyy/MM/dd HH:mm:ss||yyyy-MM-dd HH:mm:ss.SSS||yyyy.MM.dd HH:mm:ss.SSS||yyyy/MM/dd HH:mm:ss.SSS"
                  },
                  "Date2": {
                    "type": "date",
                    "store": true,
                    "format": "yyyy||yyyyMM||yyyy.MM||yyyy/MM||yyyy-MM||yyyyMMdd||yyyy.MM.dd||yyyy/MM/dd||yyyy-MM-dd||yyyy-MM-dd HH:mm:ss||yyyy.MM.dd HH:mm:ss||yyyy/MM/dd HH:mm:ss||yyyy-MM-dd HH:mm:ss.SSS||yyyy.MM.dd HH:mm:ss.SSS||yyyy/MM/dd HH:mm:ss.SSS"
                  },
                  "Date3": {
                    "type": "date",
                    "store": true,
                    "format": "yyyy||yyyyMM||yyyy.MM||yyyy/MM||yyyy-MM||yyyyMMdd||yyyy.MM.dd||yyyy/MM/dd||yyyy-MM-dd||yyyy-MM-dd HH:mm:ss||yyyy.MM.dd HH:mm:ss||yyyy/MM/dd HH:mm:ss||yyyy-MM-dd HH:mm:ss.SSS||yyyy.MM.dd HH:mm:ss.SSS||yyyy/MM/dd HH:mm:ss.SSS"
                  },
                  "Date4": {
                    "type": "date",
                    "store": true,
                    "format": "yyyy||yyyyMM||yyyy.MM||yyyy/MM||yyyy-MM||yyyyMMdd||yyyy.MM.dd||yyyy/MM/dd||yyyy-MM-dd||yyyy-MM-dd HH:mm:ss||yyyy.MM.dd HH:mm:ss||yyyy/MM/dd HH:mm:ss||yyyy-MM-dd HH:mm:ss.SSS||yyyy.MM.dd HH:mm:ss.SSS||yyyy/MM/dd HH:mm:ss.SSS"
                  },
                  "Date5": {
                    "type": "date",
                    "store": true,
                    "format": "yyyy||yyyyMM||yyyy.MM||yyyy/MM||yyyy-MM||yyyyMMdd||yyyy.MM.dd||yyyy/MM/dd||yyyy-MM-dd||yyyy-MM-dd HH:mm:ss||yyyy.MM.dd HH:mm:ss||yyyy/MM/dd HH:mm:ss||yyyy-MM-dd HH:mm:ss.SSS||yyyy.MM.dd HH:mm:ss.SSS||yyyy/MM/dd HH:mm:ss.SSS"
                  },
                  "FullText": {
                    "type": "text",
                    "index": false
                  },
                  "FullText1": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "FullText2": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "FullText3": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "FullText4": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "FullText5": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "Gid": {
                    "type": "keyword"
                  },
                  "Number1": {
                    "type": "integer"
                  },
                  "Number2": {
                    "type": "integer"
                  },
                  "Number3": {
                    "type": "integer"
                  },
                  "Number4": {
                    "type": "integer"
                  },
                  "Number5": {
                    "type": "integer"
                  },
                  "Phrase1": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "Phrase2": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "Phrase3": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "Phrase4": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "Phrase5": {
                    "type": "text",
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "Title": {
                    "type": "text",
                    "store": true,
                    "term_vector": "with_positions_offsets",
                    "analyzer": "standard"
                  },
                  "UpdateTime": {
                    "type": "date",
                    "store": true,
                    "format": "yyyy||yyyyMM||yyyy.MM||yyyy/MM||yyyy-MM||yyyyMMdd||yyyy.MM.dd||yyyy/MM/dd||yyyy-MM-dd||yyyy-MM-dd HH:mm:ss||yyyy.MM.dd HH:mm:ss||yyyy/MM/dd HH:mm:ss||yyyy-MM-dd HH:mm:ss.SSS||yyyy.MM.dd HH:mm:ss.SSS||yyyy/MM/dd HH:mm:ss.SSS"
                  }
                }
              },
              "settings": {
                "index": {
                  "number_of_shards": "2",
                  "number_of_replicas": "1"
                }
              }
            }
             */

            #endregion

            var indexSettings = new IndexSettings() {
                Index = new IndexSettings() {
                    NumberOfShards = 1,
                    NumberOfReplicas = 2
                }
            };

            var propertyDict = GetCustomProperties();

            var indexMappings = new TypeMapping() {
                Dynamic = DynamicMapping.False,
                Source = new SourceField() {
                    Excludes = new[] { "CheckFullText" }
                },
                Properties = new Properties(propertyDict)
            };


            var createIndexRequest = new CreateIndexRequest(_testIndex) {
                Pretty = true,
                Aliases = new Dictionary<Name, Alias>() {
                    {
                        _testAlias1,
                        new Alias() {
                            Filter = aliasFilter
                        }
                    }
                },
                Settings = indexSettings,
                Mappings = indexMappings
            };

            var response = await _client.Indices.CreateAsync(createIndexRequest);
            await _client.Indices.RefreshAsync();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsNotNull(response.Index);
            Assert.IsTrue(response.Index.Equals(_testIndex));




            response = await _client.Indices.CreateAsync(_testa, t =>
                                                                t.Aliases(a => a.Add(_testAlias1, ad => ad.Filter(aliasFilter)))
                                                                .Settings(indexSettings)
                                                                .Mappings(indexMappings));

            await _client.Indices.RefreshAsync();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsNotNull(response.Index);
            Assert.IsTrue(response.Index.Equals(_testa));



            var createIndexRequestDescriptor = new CreateIndexRequestDescriptor(_testb)
                                                    .Aliases(a => a.Add(_testAlias1, new AliasDescriptor().Filter(aliasFilter)))
                                                    .Settings(indexSettings)
                                                    .Mappings(indexMappings);


            response = await _client.Indices.CreateAsync(createIndexRequestDescriptor);
            await _client.Indices.RefreshAsync();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsNotNull(response.Index);
            Assert.IsTrue(response.Index.Equals(_testb));



            response = await _client.Indices.CreateAsync<object>(t => t.Index(_testc)
                                                                       .Aliases(a => a.Add(_testAlias1, f => f.Filter(aliasFilter)))
                                                                       .Settings(s => s.Index(i => i.NumberOfShards(1).NumberOfReplicas(2)))
                                                                       .Mappings(m => m.Dynamic(DynamicMapping.False)
                                                                                       .Source(s => s.Excludes(new[] { "CheckFullText" }))
                                                                                       .Properties(new Properties(propertyDict))));
            await _client.Indices.RefreshAsync();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsNotNull(response.Index);
            Assert.IsTrue(response.Index.Equals(_testc));
        }

        [TestMethod]
        public async Task TestIndexExists() {
            //HEAD: /chl?pretty=true&error_trace=true
            var indexExistsRequest = new IndexManagement.ExistsRequest(_library);
            var response = await _client.Indices.ExistsAsync(indexExistsRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);
        }

        [TestMethod]
        public async Task TestGetIndex() {
            //GET: /chl?pretty=true&error_trace=true
            var getIndexRequest = new GetIndexRequest(_library);
            var response = await _client.Indices.GetAsync(getIndexRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Indices.ContainsKey(_library));
            Assert.IsNotNull(response.Indices.Values.FirstOrDefault());
            Assert.IsTrue(response.Indices.Count == 1);

            response = await _client.Indices.GetAsync(_library);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Indices.ContainsKey(_library));
            Assert.IsNotNull(response.Indices.Values.FirstOrDefault());
            Assert.IsTrue(response.Indices.Count == 1);

            response = await _client.Indices.GetAsync(new GetIndexRequestDescriptor(_library));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Indices.ContainsKey(_library));
            Assert.IsNotNull(response.Indices.Values.FirstOrDefault());
            Assert.IsTrue(response.Indices.Count == 1);

            response = await _client.Indices.GetAsync<object>(t => t.Indices(_library));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Indices.ContainsKey(_library));
            Assert.IsNotNull(response.Indices.Values.FirstOrDefault());
            Assert.IsTrue(response.Indices.Count == 1);
        }

        [TestMethod]
        public async Task TestGetIndexSettings() {
            //GET: /chl/_settings?pretty=true&error_trace=true
            var getIndexSettingsRequest = new GetIndicesSettingsRequest(Indices.Index(_library));
            var response = await _client.Indices.GetSettingsAsync(getIndexSettingsRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);

            //GET: /_settings?pretty=true&error_trace=true
            response = await _client.Indices.GetSettingsAsync();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);

            //GET: /chl/_settings?pretty=true&error_trace=true
            response = await _client.Indices.GetSettingsAsync(Indices.Index(_library), null);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);

            //GET: /chl/_settings?pretty=true&error_trace=true
            response = await _client.Indices.GetSettingsAsync(t => t.Indices(_library));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);

        }

        [TestMethod]
        public async Task TestGetMapping() {
            //GET: /_mapping?pretty=true&error_trace=true
            var response = await _client.Indices.GetMappingAsync();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Indices.Count > 0);

            //GET: /chl/_mapping?pretty=true&error_trace=true
            response = await _client.Indices.GetMappingAsync(t => t.Indices(_library));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Indices.Count > 0);


            //GET: /chl/_mapping?pretty=true&error_trace=true&human=true
            var chlMapping = new GetMappingRequest(_library) {
                Human = true,
                Pretty = true
            };
            var chlResponse = await _client.Indices.GetMappingAsync(chlMapping);
            Assert.IsNotNull(chlResponse);
            Assert.IsTrue(chlResponse.IsSuccess());
            Assert.IsTrue(chlResponse.IsValidResponse);

            var chlMappingRecord = chlResponse.Indices[_library];
            Assert.IsNotNull(chlMappingRecord);

            var properties = chlMappingRecord.Mappings.Properties;
            Assert.IsNotNull(properties);
            Assert.IsTrue(properties.Count() > 0);

            var source = chlMappingRecord.Mappings.Source;
            Assert.IsNotNull(source);
            Assert.IsTrue(source.Excludes.Count > 0);
            Assert.IsTrue(source.Excludes.Contains("CheckFullText"));

            var titleProperty = chlMappingRecord.Mappings.Properties["Title"] as TextProperty;

            chlMappingRecord.Mappings.Properties.TryGetProperty("Title", out var titlePro);
            var titlePro1 = titlePro as TextProperty;

            Assert.IsNotNull(titleProperty);
            Assert.IsNotNull(titleProperty.Type);
            Assert.IsTrue(titleProperty.Type == "text");

            Assert.IsNotNull(titleProperty.TermVector);
            Assert.IsTrue(titleProperty.TermVector == TermVectorOption.WithPositionsOffsets);

            Assert.IsNotNull(titleProperty.Analyzer);
            Assert.IsTrue(titleProperty.Analyzer == "standard");

        }

        [TestMethod]
        public async Task TestGetFieldMapping() {
            //%2C 解码后是逗号
            //GET: /chl/_mapping/field/Gid%2CTitle%2CFlagCategory%2CCategory%2CUpdateTime%2CIssueDate%2CBeReferencedNum?pretty=true&error_trace=true&human=true
            var fields = new string[] { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum" };
            var getFieldMappingRequest = new GetFieldMappingRequest(_library, fields) {
                Human = true,
                Pretty = true
            };
            var response = await _client.Indices.GetFieldMappingAsync(getFieldMappingRequest);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsNotNull(response.FieldMappings);

            var categoryProperty = response.GetProperty(_library, "Category") as KeywordProperty;
            Assert.IsNotNull(categoryProperty);
            Assert.IsNotNull(categoryProperty.Type);
            Assert.IsTrue(categoryProperty.Type == "keyword");

            var issueDateProperty = response.GetProperty(_library, "IssueDate") as DateProperty;

            Assert.IsNotNull(issueDateProperty);
            Assert.IsNotNull(issueDateProperty.Type);
            Assert.IsTrue(issueDateProperty.Type == "date");
            Assert.IsNotNull(issueDateProperty.Format);


            //Mapping.IntegerNumberProperty
            //Mapping.IntegerRangeProperty
            //Mapping.LongNumberProperty
            //Mapping.LongRangeProperty
            //Mapping.BooleanProperty
            //Mapping.ByteNumberProperty
            //Mapping.DateRangeProperty
            //Mapping.BinaryProperty
            //Mapping.NestedProperty
            //Mapping.ObjectProperty
            //Mapping.PointProperty
            //Mapping.WildcardProperty
            //Mapping.IpProperty
            //Mapping.IpRangeProperty
        }

        [TestMethod]
        public async Task TestDeleteIndex() {
            //DELETE: /test?pretty=true&error_trace=true

            var deleteIndexRequest = new DeleteIndexRequest(_testIndex);
            var response = await _client.Indices.DeleteAsync(deleteIndexRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);


            response = await _client.Indices.DeleteAsync(_testa);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);

            response = await _client.Indices.DeleteAsync(new DeleteIndexRequestDescriptor(_testb));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);


            response = await _client.Indices.DeleteAsync<object>(t => t.Indices(_testc));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);
        }

        #endregion

        #region 别名管理

        [TestMethod]
        public async Task TestPutAlias() {

            #region es 8 语句
            /*
             PUT: /test/_alias/test1?pretty=true&error_trace=true
             PUT: /test,testa/_alias/test1?pretty=true&error_trace=true
            
            # Request:
            {
                "filter": {
                "range": {
                    "UpdateTime": {
                        "from": "2025-01-01"
                    }
                }
                }
            }
             */

            #endregion 

            var putAliasRequest = new PutAliasRequest(_testIndex, _testAlias1) {
                Filter = aliasFilter
            };
            var response = await _client.Indices.PutAliasAsync(putAliasRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);

            response = await _client.Indices.PutAliasAsync(_testIndex, _testAlias2, d => d.Filter(aliasFilter));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);

            response = await _client.Indices.PutAliasAsync(new PutAliasRequestDescriptor(_testIndex, _testAlias3).Filter(aliasFilter));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);

            response = await _client.Indices.PutAliasAsync<object>(_testIndex, _testAlias4, t => t.Filter(aliasFilter));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);

        }

        [TestMethod]
        public async Task TestUpdateAlias() {

            #region es 8 语句
            /*
             POST: /_aliases?pretty=true&error_trace=true
                # Request:
                {
                  "actions": [
                    {
                      "remove": {
                        "aliases": [
                          "test1",
                          "test2"
                        ],
                        "indices": "test"
                      }
                    },
                    {
                      "add": {
                        "aliases": [
                          "test1",
                          "test2"
                        ],
                        "filter": {
                          "range": {
                            "UpdateTime": {
                              "from": "2025-01-01"
                            }
                          }
                        },
                        "indices": [
                            "test",
                            "testa"
                        ]
                      }
                    }
                  ]
                }
             */
            #endregion

            var indexAilasList = new List<IndexAlias> { _testAlias1, _testAlias2, _testAlias3, _testAlias4 };
            var removeAction = new RemoveAction() {
                Indices = _testIndex,
                Aliases = indexAilasList
            };

            var addAction = new AddAction() {
                Indices = _testIndex,
                Aliases = indexAilasList,
                Filter = aliasFilter
            };

            var actions = new List<IndexUpdateAliasesAction>() { removeAction, addAction };


            var updateAliasRequest = new UpdateAliasesRequest() { Actions = actions };
            var response = await _client.Indices.UpdateAliasesAsync(updateAliasRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);



            var updateAliasRequestDescriptor = new UpdateAliasesRequestDescriptor().Actions(actions);
            response = await _client.Indices.UpdateAliasesAsync(updateAliasRequestDescriptor);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);

            response = await _client.Indices.UpdateAliasesAsync(d =>
                                                    d.Actions(t => t.Remove(r => r.Indices(_testIndex).Aliases(indexAilasList)),
                                                              a => a.Add<object>(s => s.Indices(_testIndex).Aliases(indexAilasList).Filter(aliasFilter)))
                                                    );

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);
        }

        [TestMethod]
        public async Task TestAliasExists() {

            //HEAD: /_alias/test1?pretty=true&error_trace=true
            var aliasExistsRequest = new ExistsAliasRequest(_testAlias1);
            var response = await _client.Indices.ExistsAliasAsync(aliasExistsRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);

            //HEAD: /_alias/test1?pretty=true&error_trace=true
            response = await _client.Indices.ExistsAliasAsync(_testAlias1);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);

            //HEAD: /test/_alias/test1?pretty=true&error_trace=true
            response = await _client.Indices.ExistsAliasAsync(_testIndex, _testAlias1);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);

            //HEAD: /_alias/test1?pretty=true&error_trace=true
            response = await _client.Indices.ExistsAliasAsync(new ExistsAliasRequestDescriptor(_testAlias1));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);

            //HEAD: /test/_alias/test1?pretty=true&error_trace=true
            response = await _client.Indices.ExistsAliasAsync(new ExistsAliasRequestDescriptor<object>(_testIndex, _testAlias1));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.Exists);
        }

        [TestMethod]
        public async Task TestGetAlias() {

            //GET: /test/_alias/test1?pretty=true&error_trace=true
            var response = await _client.Indices.GetAliasAsync(new GetAliasRequest(_testIndex, _testAlias1));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Aliases);
            Assert.IsNotNull(response.Aliases.ContainsKey(_testAlias1));

            //GET: /test/_alias/test1?pretty=true&error_trace=true
            response = await _client.Indices.GetAliasAsync(new GetAliasRequestDescriptor(_testIndex, _testAlias1));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Aliases);
            Assert.IsNotNull(response.Aliases.ContainsKey(_testAlias1));

            //GET: /test/_alias/test1?pretty=true&error_trace=true
            response = await _client.Indices.GetAliasAsync(_testIndex, _testAlias1);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Aliases);
            Assert.IsNotNull(response.Aliases.ContainsKey(_testAlias1));

            //GET: /test/_alias/test1?pretty=true&error_trace=true
            response = await _client.Indices.GetAliasAsync(_testIndex, t => t.Name(_testAlias1));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Aliases);
            Assert.IsNotNull(response.Aliases.ContainsKey(_testAlias1));

            //GET: /test/_alias/test1?pretty=true&error_trace=true
            response = await _client.Indices.GetAliasAsync(t => t.Indices(_testIndex).Name(_testAlias1));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Aliases);
            Assert.IsNotNull(response.Aliases.ContainsKey(_testAlias1));

            //GET: /_alias/test1?pretty=true&error_trace=true
            response = await _client.Indices.GetAliasAsync(t => t.Name(_testAlias1));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Aliases);
            Assert.IsNotNull(response.Aliases.ContainsKey(_testAlias1));

            //GET: /test/_alias?pretty=true&error_trace=true
            response = await _client.Indices.GetAliasAsync(t => t.Indices(_testIndex));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Aliases);
            Assert.IsNotNull(response.Aliases.ContainsKey(_testAlias1));

            //GET: /_alias?pretty=true&error_trace=true
            response = await _client.Indices.GetAliasAsync();

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsNotNull(response.Aliases);
            Assert.IsNotNull(response.Aliases.ContainsKey(_testAlias1));
        }

        [TestMethod]
        public async Task TestDeleteAlias() {
            //DELETE: /test/_alias/test1?pretty=true&error_trace=true

            var deleteAliasRequest = new DeleteAliasRequest(_testIndex, new Names(new[] { _testAlias1 }));
            var response = await _client.Indices.DeleteAliasAsync(deleteAliasRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);


            response = await _client.Indices.DeleteAliasAsync(_testIndex, _testAlias2);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);


            response = await _client.Indices.DeleteAliasAsync(new DeleteAliasRequestDescriptor(_testIndex, _testAlias3));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);

            response = await _client.Indices.DeleteAliasAsync<object>(_testAlias4, t => t.Indices(_testIndex));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Acknowledged);
        }

        #endregion

        [TestMethod]
        public async Task TestAnalyze() {

            var text = new[] { "中华人民共和国" };
            var analyzer = "ik_max_word";
            var analyzeRequest = new AnalyzeIndexRequest() {
                Text = text,
                Analyzer = analyzer
            };

            /*
             POST: /_analyze?pretty=true&error_trace=true            
            # Request:
            {
              "analyzer": "ik_max_word",
              "text": "\u4E2D\u534E\u4EBA\u6C11\u5171\u548C\u56FD"
            }             
             */
            var response = await _client.Indices.AnalyzeAsync(analyzeRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsNotNull(response.Tokens);
            Assert.IsTrue(response.Tokens.Count > 0);

            response = await _client.Indices.AnalyzeAsync(new AnalyzeIndexRequestDescriptor()
                                                                .Text(text)
                                                                .Analyzer(analyzer));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsNotNull(response.Tokens);
            Assert.IsTrue(response.Tokens.Count > 0);

            /*
            POST: /chl/_analyze?pretty=true&error_trace=true            
               # Request:
               {
                 "analyzer": "ik_max_word",
                 "text": "\u4E2D\u534E\u4EBA\u6C11\u5171\u548C\u56FD"
               }             
            */
            response = await _client.Indices.AnalyzeAsync(t => t.Index(_library)
                                                                .Text(text)
                                                                .Analyzer(analyzer));

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsNotNull(response.Tokens);
            Assert.IsTrue(response.Tokens.Count > 0);
        }
    }
}
