using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch.UnitTest.Elasticsearch;
using Elastic.Clients.Elasticsearch.UnitTest.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Elastic.Clients.Elasticsearch.UnitTest {
    [TestClass]
    public class UnitTestElasticsearchSearch {

        private const string _library = "chl";
        private readonly ElasticsearchClient _client = ElasticsearchConfiguration.Client.Value;
        private readonly string[] HIT_WORD_START = new[] { "<span class='hit'>" };
        private readonly string[] HIT_WORD_END = new[] { "</span>" };


        [TestMethod]
        public async Task TestIdsQuery() {

            #region es语句
            /*
             POST: /chl/_search?pretty=true&error_trace=true
            # Request:
            {
              "from": 0,
              "query": {
                "ids": {
                  "values": [
                    "42361",
                    "75932",
                    "98563"
                  ]
                }
              },
              "size": 10,
              "sort": [
                {
                  "Sort": {
                    "missing": "_last",
                    "numeric_type": "long",
                    "order": "desc"
                  }
                },
                {
                  "UpdateTime": {
                    "missing": "_last",
                    "numeric_type": "date",
                    "order": "desc"
                  }
                }
              ],
              "_source": {
                "includes": [
                  "Gid",
                  "Title",
                  "FlagCategory",
                  "Category",
                  "UpdateTime",
                  "IssueDate",
                  "BeReferencedNum"
                ]
              }
            }
             */
            #endregion

            object value = "42361,75932,98563";

            var queryValue = value.ToString().Split(',');

            var query = new IdsQuery() {
                Values = queryValue
            };

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);
        }

        [TestMethod]
        public async Task TestTermQuery() {
            #region es 语句
            /*
             POST: /chl/_search?pretty=true&error_trace=true
                # Request:
                {
                  "from": 0,
                  "query": {
                    "term": {
                      "FlagCategory": {
                        "value": "015"
                      }
                    }
                  },
                  "size": 10,
                  "sort": [
                    {
                      "Sort": {
                        "missing": "_last",
                        "numeric_type": "long",
                        "order": "desc"
                      }
                    },
                    {
                      "UpdateTime": {
                        "missing": "_last",
                        "numeric_type": "date",
                        "order": "desc"
                      }
                    }
                  ],
                  "_source": {
                    "includes": [
                      "Gid",
                      "Title",
                      "FlagCategory",
                      "Category",
                      "UpdateTime",
                      "IssueDate",
                      "BeReferencedNum",
                      "Sort",
                      "NavCatalog"
                    ]
                  },
                  "track_total_hits": true
                }
             */

            #endregion

            var field = "FlagCategory";
            object value = "015";

            var queryValue = value.ToString();

            var query = new TermQuery(new Field(field)) {
                Value = queryValue
            };

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum", "Sort", "NavCatalog" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);

            var firstDataJsonStr = response.Documents.FirstOrDefault()?.ToString();
            Console.WriteLine(firstDataJsonStr);

            var firstData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(firstDataJsonStr);
            var flagCategoryList = firstData["FlagCategory"] as Newtonsoft.Json.Linq.JArray;

            Assert.IsTrue(flagCategoryList.Values<string>().Contains(value.ToString()));

            GenerateList(response);
        }

        [TestMethod]
        public async Task TestTermsQuery() {
            #region es语句
            /*
             POST: /chl/_search?pretty=true&error_trace=true
            # Request:
            {
              "from": 0,
              "query": {
                "terms": {
                  "FlagCategory": [
                    "019",
                    "015",
                    "016"
                  ]
                }
              },
              "size": 10,
              "sort": [
                {
                  "Sort": {
                    "missing": "_last",
                    "numeric_type": "long",
                    "order": "desc"
                  }
                },
                {
                  "UpdateTime": {
                    "missing": "_last",
                    "numeric_type": "date",
                    "order": "desc"
                  }
                }
              ],
              "_source": {
                "includes": [
                  "Gid",
                  "Title",
                  "FlagCategory",
                  "Category",
                  "UpdateTime",
                  "IssueDate",
                  "BeReferencedNum"
                ]
              },
              "track_total_hits": true
            }
             */
            #endregion

            var field = "FlagCategory";
            object value = "019,015,016";

            var queryValue = value.ToString().Split(',').Select(t => FieldValue.String(t)).ToList();

            var query = new TermsQuery() {
                Field = field,
                Terms = new TermsQueryField(queryValue)
            };

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);
        }

        [TestMethod]
        public async Task TestMatchPhraseQueryAndHighlight() {

            #region es语句
            /*
             POST: /chl/_search?pretty=true&error_trace=true
            # Request:
            {
              "from": 0,
              "highlight": {
                "fields": {
                  "Title": {
                    "matched_fields": "Title"
                  },
                  "DocumentNO": {
                    "matched_fields": "DocumentNO"
                  },
                  "CheckFullText": {
                    "matched_fields": "CheckFullText"
                  }
                },
                "fragment_size": 100,
                "number_of_fragments": 0,
                "post_tags": [
                  "\u003C/span\u003E"
                ],
                "pre_tags": [
                  "\u003Cspan class=\u0027hit\u0027\u003E"
                ],
                "require_field_match": true,
                "type": "fvh"
              },
              "query": {
                "match_phrase": {
                  "Title": {
                    "query": "\u6D77\u5173"
                  }
                }
              },
              "size": 10,
              "sort": [
                {
                  "Sort": {
                    "missing": "_last",
                    "numeric_type": "long",
                    "order": "desc"
                  }
                },
                {
                  "UpdateTime": {
                    "missing": "_last",
                    "numeric_type": "date",
                    "order": "desc"
                  }
                }
              ],
              "_source": {
                "includes": [
                  "Gid",
                  "Title",
                  "FlagCategory",
                  "Category",
                  "UpdateTime",
                  "IssueDate",
                  "BeReferencedNum",
                  "Sort"
                ]
              },
              "track_total_hits": true
            }
             */
            #endregion

            var field = "Title";
            object value = "海关";

            var queryValue = value.ToString();

            var query = new MatchPhraseQuery(new Field(field)) {
                Query = queryValue
            };

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum", "Sort" };
            var highlightFields = new List<string>() { "Title", "DocumentNO", "CheckFullText" };
            var highlight = new Highlight() {
                Fields = highlightFields.Select(t => new HighlightField() { MatchedFields = t })
                                        .ToDictionary(f => f.MatchedFields.FirstOrDefault()),
                PreTags = HIT_WORD_START,
                PostTags = HIT_WORD_END,
                FragmentSize = 100,
                NumberOfFragments = 0,
                RequireFieldMatch = true,
                Type = HighlighterType.FastVector

            };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts,
                Highlight = highlight
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Hits.Count > 0);

            var highlightValue = SetHighlightValue(response, field);
            Assert.IsNotNull(highlightValue);

            var highlightQuery = $"{HIT_WORD_START.FirstOrDefault()}{queryValue}{HIT_WORD_END.FirstOrDefault()}";
            Assert.IsTrue(highlightValue.Contains(highlightQuery));
        }

        [TestMethod]
        public async Task TestWildcardQuery() {
            /*
             POST: /chl/_search?pretty=true&error_trace=true
                # Request:
                {
                  "from": 0,
                  "query": {
                    "wildcard": {
                      "ReferenceArticleGidTiaoNum": {
                        "case_insensitive": true,
                        "rewrite": "constant_score",
                        "value": "*Chl#17010*"
                      }
                    }
                  },
                  "size": 10,
                  "sort": [
                    {
                      "Sort": {
                        "missing": "_last",
                        "numeric_type": "long",
                        "order": "desc"
                      }
                    },
                    {
                      "UpdateTime": {
                        "missing": "_last",
                        "numeric_type": "date",
                        "order": "desc"
                      }
                    }
                  ],
                  "_source": {
                    "includes": [
                      "Gid",
                      "Title",
                      "FlagCategory",
                      "Category",
                      "UpdateTime",
                      "IssueDate",
                      "BeReferencedNum",
                      "Sort",
                      "ReferenceArticleGidTiaoNum"
                    ]
                  },
                  "track_total_hits": true
                }
             */
            var field = "ReferenceArticleGidTiaoNum";
            object value = "17010";

            var queryValue = value.ToString();

            var query = new WildcardQuery(new Field(field)) {
                Value = $"*Chl#{queryValue}*",
                Rewrite = "constant_score",//默认constant_score_blended
                CaseInsensitive = true
            };

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum", "Sort", "ReferenceArticleGidTiaoNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
        }

        [TestMethod]
        public async Task TestPrefixQuery() {
            /*
             POST: /chl/_search?pretty=true&error_trace=true
            # Request:
            {
              "from": 0,
              "query": {
                "prefix": {
                  "ReferenceArticleGidTiaoNum": {
                    "case_insensitive": true,
                    "value": "Chl#17010"
                  }
                }
              },
              "size": 10,
              "sort": [
                {
                  "Sort": {
                    "missing": "_last",
                    "numeric_type": "long",
                    "order": "desc"
                  }
                },
                {
                  "UpdateTime": {
                    "missing": "_last",
                    "numeric_type": "date",
                    "order": "desc"
                  }
                }
              ],
              "_source": {
                "includes": [
                  "Gid",
                  "Title",
                  "FlagCategory",
                  "Category",
                  "UpdateTime",
                  "IssueDate",
                  "BeReferencedNum",
                  "Sort",
                  "ReferenceArticleGidTiaoNum"
                ]
              }
            }
             */

            var field = "ReferenceArticleGidTiaoNum";
            object value = "17010";

            var queryValue = value.ToString();

            var query = new PrefixQuery(new Field(field)) {
                Value = $"Chl#{queryValue}",
                CaseInsensitive = true
            };

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum", "Sort", "ReferenceArticleGidTiaoNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
        }

        [TestMethod]
        public async Task TestNumberRangeQuery() {

            /*
             POST: /chl/_search?pretty=true&error_trace=true
            # Request:
            {
              "from": 0,
              "query": {
                "range": {
                  "BeReferencedNum": {
                    "from": 0,
                    "to": 1
                  }
                }
              },
              "size": 10,
              "sort": [
                {
                  "Sort": {
                    "missing": "_last",
                    "numeric_type": "long",
                    "order": "desc"
                  }
                },
                {
                  "UpdateTime": {
                    "missing": "_last",
                    "numeric_type": "date",
                    "order": "desc"
                  }
                }
              ],
              "_source": {
                "includes": [
                  "Gid",
                  "Title",
                  "FlagCategory",
                  "Category",
                  "UpdateTime",
                  "IssueDate",
                  "BeReferencedNum",
                  "Sort",
                  "ReferenceArticleGidTiaoNum"
                ]
              },
              "track_total_hits": true
            }
             */

            var field = "BeReferencedNum";
            object value = "1";

            var queryValue = value.ToString();

            var query = new NumberRangeQuery(new Field(field)) {
                From = Double.Parse(queryValue) - 1,
                To = Double.Parse(queryValue)
            };

            /*
             
            //from to 相当于 Gte Lte
            var query = new NumberRangeQuery(new Field(field)) {
                Gte = Double.Parse(queryValue) - 1,
                Lte = Double.Parse(queryValue)
            };
            */

            /*
             //还可以 from Lte 混合写
            var query = new NumberRangeQuery(new Field(field)) {
                From = Double.Parse(queryValue) - 1,
                Lte = Double.Parse(queryValue)
            };

             var query = new NumberRangeQuery(new Field(field)) {
                Gte = Double.Parse(queryValue) - 1,
                Lte = Double.Parse(queryValue)
            };
             */

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum", "Sort", "ReferenceArticleGidTiaoNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
        }

        [TestMethod]
        public async Task TestDateRangeQuery() {
            /*
             POST: /chl/_search?pretty=true&error_trace=true
                # Request:
                {
                  "from": 0,
                  "query": {
                    "range": {
                      "UpdateTime": {
                        "from": "2024.01.01",
                        "to": "2024.02.01"
                      }
                    }
                  },
                  "size": 10,
                  "sort": [
                    {
                      "Sort": {
                        "missing": "_last",
                        "numeric_type": "long",
                        "order": "desc"
                      }
                    },
                    {
                      "UpdateTime": {
                        "missing": "_last",
                        "numeric_type": "date",
                        "order": "desc"
                      }
                    }
                  ],
                  "_source": {
                    "includes": [
                      "Gid",
                      "Title",
                      "FlagCategory",
                      "Category",
                      "UpdateTime",
                      "IssueDate",
                      "BeReferencedNum"
                    ]
                  },
                  "track_total_hits": true
                }
             */

            var field = "UpdateTime";
            object value = "2024.01.01";

            var queryValue = value.ToString();

            var query = new DateRangeQuery(new Field(field)) {
                From = queryValue,
                To = DateTime.Parse(queryValue).AddMonths(1).ToString("yyyy.MM.dd")
            };

            /*
             
            //from to 相当于 Gte Lte
            var query = new NumberRangeQuery(new Field(field)) {
                Gte = queryValue,
                Lte = DateTime.Parse(queryValue).AddMonths(1).ToString("yyyy.MM.dd")
            };
            */

            /*
             //还可以 from Lte 混合写
            var query = new NumberRangeQuery(new Field(field)) {
                From = queryValue,
                Lte = DateTime.Parse(queryValue).AddMonths(1).ToString("yyyy.MM.dd")
            };

             var query = new NumberRangeQuery(new Field(field)) {
                Gte = queryValue,
                Lte = DateTime.Parse(queryValue).AddMonths(1).ToString("yyyy.MM.dd")
            };
             */

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

        }


        [TestMethod]
        public async Task TestSpanTermQuery() {

            /*
             POST: /chl/_search?pretty=true&error_trace=true
                # Request:
                {
                  "from": 0,
                  "query": {
                    "span_term": {
                      "DocumentNO": {
                        "value": "\u8D22"
                      }
                    }
                  },
                  "size": 10,
                  "sort": [
                    {
                      "Sort": {
                        "missing": "_last",
                        "numeric_type": "long",
                        "order": "desc"
                      }
                    },
                    {
                      "UpdateTime": {
                        "missing": "_last",
                        "numeric_type": "date",
                        "order": "desc"
                      }
                    }
                  ],
                  "_source": {
                    "includes": [
                      "Gid",
                      "Title",
                      "FlagCategory",
                      "DocumentNO",
                      "Category",
                      "UpdateTime",
                      "IssueDate",
                      "BeReferencedNum"
                    ]
                  },
                  "track_total_hits": true
                }
             */

            var field = "DocumentNO";
            object value = "财";

            var queryValue = value.ToString();

            var query = new SpanTermQuery(new Field(field)) {
                Value = queryValue
            };

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "DocumentNO", "Category", "UpdateTime", "IssueDate", "BeReferencedNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
        }



        [TestMethod]
        public async Task TestSpanNearQuery() {

            /*
             POST: /chl/_search?pretty=true&error_trace=true
            # Request:
            {
              "from": 0,
              "query": {
                "span_near": {
                  "clauses": [
                    {
                      "span_term": {
                        "DocumentNO": {
                          "value": "\u8D22"
                        }
                      }
                    },
                    {
                      "span_term": {
                        "DocumentNO": {
                          "value": "\u653F"
                        }
                      }
                    }
                  ],
                  "in_order": false,
                  "slop": 2
                }
              },
              "size": 10,
              "sort": [
                {
                  "Sort": {
                    "missing": "_last",
                    "numeric_type": "long",
                    "order": "desc"
                  }
                },
                {
                  "UpdateTime": {
                    "missing": "_last",
                    "numeric_type": "date",
                    "order": "desc"
                  }
                }
              ],
              "_source": {
                "includes": [
                  "Gid",
                  "Title",
                  "FlagCategory",
                  "DocumentNO",
                  "Category",
                  "UpdateTime",
                  "IssueDate",
                  "BeReferencedNum"
                ]
              },
              "track_total_hits": true
            }
             */

            var field = "DocumentNO";
            object value1 = "财";
            object value2 = "政";

            var queryValue1 = value1.ToString();
            var queryValue2 = value2.ToString();

            var spanTermQuery1 = new SpanTermQuery(new Field(field)) {
                Value = queryValue1
            };
            var spanTermQuery2 = new SpanTermQuery(new Field(field)) {
                Value = queryValue2
            };

            var spanNearQuery = new SpanNearQuery() {
                Clauses = new List<SpanQuery>() {
                    SpanQuery.SpanTerm(spanTermQuery1),
                    SpanQuery.SpanTerm(spanTermQuery2)
                },
                Slop = 2,
                InOrder = false
            };

            var query = Query.SpanNear(spanNearQuery);

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "DocumentNO", "Category", "UpdateTime", "IssueDate", "BeReferencedNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
        }

        [TestMethod]
        public async Task TestBoolQuery() {

            /*
             POST: /chl/_search?pretty=true&error_trace=true
                # Request:
                {
                  "from": 0,
                  "query": {
                    "bool": {
                      "must": [
                        {
                          "range": {
                            "UpdateTime": {
                              "from": "2024.01.01"
                            }
                          }
                        },
                        {
                          "term": {
                            "FlagCategory": {
                              "value": "015"
                            }
                          }
                        }
                      ]
                    }
                  },
                  "size": 10,
                  "sort": [
                    {
                      "Sort": {
                        "missing": "_last",
                        "numeric_type": "long",
                        "order": "desc"
                      }
                    },
                    {
                      "UpdateTime": {
                        "missing": "_last",
                        "numeric_type": "date",
                        "order": "desc"
                      }
                    }
                  ],
                  "_source": {
                    "includes": [
                      "Gid",
                      "Title",
                      "FlagCategory",
                      "Category",
                      "UpdateTime",
                      "IssueDate",
                      "BeReferencedNum"
                    ]
                  },
                  "track_total_hits": true
                }
             */

            var field1 = "UpdateTime";
            var field2 = "FlagCategory";
            object value1 = "2024.01.01";
            object value2 = "015";

            var queryValue1 = value1.ToString();
            var queryValue2 = value2.ToString();

            var query1 = new DateRangeQuery(field1) {
                From = queryValue1
            };

            var query2 = new TermQuery(field2) {
                Value = queryValue2
            };

            var query = new BoolQuery() {
                Must = new List<Query>() { query1, query2 }
            };

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);
        }

        [TestMethod]
        public async Task TestBoolAndExistsQuery() {

            /*
             POST: /chl/_search?pretty=true&error_trace=true
            # Request:
            {
              "from": 0,
              "query": {
                "bool": {
                  "must": [
                    {
                      "term": {
                        "FlagCategory": {
                          "value": "015"
                        }
                      }
                    },
                    {
                      "exists": {
                        "field": "FlagCategory"
                      }
                    }
                  ]
                }
              },
              "size": 10,
              "sort": [
                {
                  "Sort": {
                    "missing": "_last",
                    "numeric_type": "long",
                    "order": "desc"
                  }
                },
                {
                  "UpdateTime": {
                    "missing": "_last",
                    "numeric_type": "date",
                    "order": "desc"
                  }
                }
              ],
              "_source": {
                "includes": [
                  "Gid",
                  "Title",
                  "FlagCategory",
                  "Category",
                  "UpdateTime",
                  "IssueDate",
                  "BeReferencedNum"
                ]
              },
              "track_total_hits": true
            }
             */

            var field = "FlagCategory";
            object value = "015";

            var queryValue = value.ToString();

            var query1 = new TermQuery(field) {
                Value = queryValue
            };

            var query2 = new ExistsQuery() {
                Field = field
            };
            var query = Query.Term(query1) & Query.Exists(query2);

            var includes = new List<string>() { "Gid", "Title", "FlagCategory", "Category", "UpdateTime", "IssueDate", "BeReferencedNum" };

            var sorts = new List<SortOptions>() {
                    SortOptions.Field("Sort", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Long,
                        Missing ="_last"
                    }),
                    SortOptions.Field("UpdateTime", new FieldSort() {
                        Order = SortOrder.Desc,
                        NumericType = FieldSortNumericType.Date,
                        Missing ="_last"
                    })
                };

            var searchRequest = new SearchRequest(_library) {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Source = new SourceConfig(new SourceFilter() { Includes = includes.ToArray() }),
                Sort = sorts
            };

            var response = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);
        }

        [TestMethod]
        public async Task TestScriptQuery() {
            /*
             POST: /chl/_search?pretty=true&error_trace=true
                # Request:
                {
                  "query": {
                    "script": {
                      "script": {
                        "lang": "painless",
                        "source": "doc[\u0027Gid\u0027].size() \u003E 0 \u0026\u0026 doc[\u0027Gid\u0027].value == params.Gid",
                        "params": {
                          "Gid": "100"
                        }
                      }
                    }
                  }
                }
             */
            var gid = "100";
            var script = new Script(new InlineScript() {
                Language = ScriptLanguage.Painless,
                Source = "doc['Gid'].size() > 0 && doc['Gid'].value == params.Gid",// Gid字段存在并且Gid==100
                Params = new Dictionary<string, object> { { "Gid", gid } }
            });
            var searchRequestDescriptor = new SearchRequestDescriptor<object>()
                                               .Indices(_library)
                                               .Query(Query.Script(new ScriptQuery() {
                                                   Script = script
                                               }));
            var response = await _client.SearchAsync(searchRequestDescriptor);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsTrue(response.Total > 0);
        }


        [TestMethod]
        public async Task TestScrollSearch() {

            /*
             POST: /_search?pretty=true&error_trace=true&scroll=2m
                # Request:
                {
                  "from": 0,
                  "query": {
                    "bool": {
                      "must": [
                        {
                          "term": {
                            "FlagCategory": {
                              "value": "015"
                            }
                          }
                        },
                        {
                          "range": {
                            "IssueDate": {
                              "gte": "2010.01.01",
                              "lte": "2010.09.01"
                            }
                          }
                        }
                      ]
                    }
                  },
                  "size": 10,
                  "track_total_hits": true
                }

            POST: /_search/scroll?pretty=true&error_trace=true
            # Request:
            {
              "scroll": "2m",
              "scroll_id": "scroll_id_value"
            }


            DELETE: /_search/scroll?pretty=true&error_trace=true
            # Request:
            {
              "scroll_id": [
                "scroll_id_value"
              ]
            }
             */
            var field = "FlagCategory";
            object value = "015";

            var queryValue = value.ToString();


            var termQuery = new TermQuery(field) {
                Value = queryValue
            };

            var dateRangeQuery = new DateRangeQuery("IssueDate") {
                Gte = "2010.01.01",
                Lte = "2010.09.01"
            };
            var query = Query.Term(termQuery) & Query.Range(new RangeQuery(dateRangeQuery));

            //创建请求，并返回scrollId
            var searchRequest = new SearchRequest() {
                From = 0,
                Size = 10,
                TrackTotalHits = new TrackHits(true),// Search数量超过1万时显示实际数量
                Query = query,
                Scroll = TimeSpan.FromMinutes(2)
            };

            var searchResponse = await _client.SearchAsync<dynamic>(searchRequest);

            Assert.IsNotNull(searchResponse);
            Assert.IsTrue(searchResponse.IsValidResponse);
            Assert.IsTrue(searchResponse.IsSuccess());

            Assert.IsTrue(searchResponse.Total > 0);

            var scrollId = searchResponse.ScrollId;
            var dataCount = searchResponse.Total - searchResponse.Hits.Count;
            while (dataCount > 0 && dataCount < 20) {

                //2.scroll search
                var scrollRequest = new ScrollRequest() {
                    ScrollId = scrollId,
                    Scroll = TimeSpan.FromMinutes(2)
                };

                var scrollResponse = await _client.ScrollAsync<dynamic>(scrollRequest);
                dataCount = dataCount - scrollResponse.Hits.Count;

                Assert.IsNotNull(scrollResponse);
                Assert.IsTrue(scrollResponse.IsValidResponse);
                Assert.IsTrue(scrollResponse.IsSuccess());
                Assert.IsTrue(scrollResponse.Total > 0);

                Assert.IsTrue(scrollId == scrollResponse.ScrollId);
            }


            //3.清理scrollId
            var clearScrollRequest = new ClearScrollRequest() {
                ScrollId = scrollId
            };

            var clearScrollResponse = await _client.ClearScrollAsync(clearScrollRequest);
            Assert.IsNotNull(clearScrollResponse);
            Assert.IsTrue(clearScrollResponse.IsValidResponse);
            Assert.IsTrue(clearScrollResponse.Succeeded);
            Assert.IsTrue(clearScrollResponse.NumFreed > 0);

        }


        [TestMethod]
        public async Task TestAnalyzeString() {
            /*
             POST: /_analyze?pretty=true&error_trace=true
            # Request:
            {
              "analyzer": "ik_max_word",
              "text": "\u52B3\u52A8\u5408\u540C\u6CD5"
            }
             */
            var analyzeRequest = new AnalyzeIndexRequest() {
                Text = new[] { "劳动合同法" },
                Analyzer = "ik_max_word"
            };
            var response = await _client.Indices.AnalyzeAsync(analyzeRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());
            Assert.IsNotNull(response.Tokens);
            Assert.IsTrue(response.Tokens.Any());
        }

        [TestMethod]
        public async Task TestCountQuery() {
            /*
             POST: /chl/_count?pretty=true&error_trace=true
            # Request:
            {
              "query": {
                "term": {
                  "FlagCategory": {
                    "value": "015"
                  }
                }
              }
            }
             */
            var field = "FlagCategory";
            object value = "015";

            var queryValue = value.ToString();


            var query = new TermQuery(field) {
                Value = queryValue
            };

            var countRequest = new CountRequest(_library) {
                Query = query
            };

            var response = await _client.CountAsync(countRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Count > 0);
        }

        [TestMethod]
        public async Task TestTermsAgg() {
            /*
             POST: /chl/_search?pretty=true&error_trace=true&search_type=query_then_fetch&typed_keys=true
            # Request:
            {
              "aggregations": {
                "CategoryAggs": {
                  "terms": {
                    "field": "Category",
                    "include": [
                      "001",
                      "002",
                      "003",
                      "004"
                    ],
                    "order": {
                      "_count": "asc"
                    }
                  }
                }
              },
              "query": {
                "term": {
                  "FlagCategory": {
                    "value": "015"
                  }
                }
              },
              "size": 0
            }
             */
            var field = "FlagCategory";
            object value = "015";
            var queryValue = value.ToString();

            var query = new TermQuery(field) {
                Value = queryValue
            };
            var aggName = "CategoryAggs";
            var includes = new List<string>() { "001", "002", "003", "004" };
            var termAggs = new TermsAggregation() {
                Field = "Category",
                //MinDocCount = 0,
                Order = new List<KeyValuePair<Field, SortOrder>>() { new KeyValuePair<Field, SortOrder>("_count", SortOrder.Asc) },//order field 如 ：_key 按项的值排序 ;_count 按数量排序； subAggKey 按子聚类项的key值排序
                Include = new TermsInclude(includes)
            };

            var aggs = new Dictionary<string, Aggregation>() {
                {
                    aggName,termAggs
                }
            };

            var aggRequest = new SearchRequest(_library) {
                Size = 0,
                Aggregations = aggs,
                SearchType = SearchType.QueryThenFetch,
                Query = query
            };

            var response = await _client.SearchAsync<dynamic>(aggRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);
            Assert.IsTrue(response.Aggregations.Count > 0);

            var categoryAggs = response.Aggregations[aggName] as StringTermsAggregate;

            if (categoryAggs != null && categoryAggs.Buckets.Any()) {
                Dictionary<string, long> res = categoryAggs.Buckets.Select(t => new KeyValuePair<string, long>(t.Key.ToString(), t.DocCount)).ToDictionary(t => t.Key, v => v.Value);

                Assert.IsTrue(res.Count > 0);
                Assert.IsTrue(res.Count == categoryAggs.Buckets.Count);
            }

        }

        [TestMethod]
        public async Task TestDateHistogramAgg() {

            /*
             POST: /chl/_search?pretty=true&error_trace=true&search_type=query_then_fetch&typed_keys=true
            # Request:
            {
              "aggregations": {
                "IssueDateAggs": {
                  "date_histogram": {
                    "calendar_interval": "year",
                    "field": "IssueDate",
                    "format": "yyyy"
                  }
                }
              },
              "query": {
                "term": {
                  "FlagCategory": {
                    "value": "015"
                  }
                }
              },
              "size": 0
            }
             */

            var field = "FlagCategory";
            object value = "015";
            var queryValue = value.ToString();

            var query = new TermQuery(field) {
                Value = queryValue
            };
            var aggName = "IssueDateAggs";

            var dateHistogramAggs = new DateHistogramAggregation() {
                Field = "IssueDate",
                CalendarInterval = CalendarInterval.Year,
                //FixedInterval =TimeSpan.FromDays(1),
                Format = "yyyy"
            };

            var aggs = new Dictionary<string, Aggregation>() {
                {
                    aggName,dateHistogramAggs
                }
            };

            var aggRequest = new SearchRequest(_library) {
                Size = 0,
                Aggregations = aggs,
                SearchType = SearchType.QueryThenFetch,
                Query = query
            };

            var response = await _client.SearchAsync<dynamic>(aggRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);
            Assert.IsTrue(response.Aggregations.Count > 0);

            var issueDateAggs = response.Aggregations[aggName] as DateHistogramAggregate;

            if (issueDateAggs != null && issueDateAggs.Buckets.Any()) {
                Dictionary<string, long> res = issueDateAggs.Buckets
                                                              .Select(t => new KeyValuePair<string, long>(t.KeyAsString, t.DocCount))
                                                              .Where(t => !string.IsNullOrWhiteSpace(t.Key) && t.Value > 0)
                                                              .OrderByDescending(t => t.Key)
                                                              .ToDictionary(t => t.Key, v => v.Value);

                Assert.IsTrue(res.Count > 0);
                Assert.IsTrue(res.Count <= issueDateAggs.Buckets.Count);
            }
        }


        [TestMethod]
        public async Task TestFilterAgg() {
            /*
             POST: /chl/_search?pretty=true&error_trace=true&search_type=query_then_fetch&typed_keys=true
            # Request:
            {
              "aggregations": {
                "FlagCategoryFilterAggs": {
                  "aggregations": {},
                  "filters": {
                    "filters": [
                      {
                        "term": {
                          "FlagCategory": {
                            "value": "015"
                          }
                        }
                      }
                    ]
                  }
                }
              },
              "size": 0
            }
             */

            var field = "FlagCategory";
            object value = "015";
            var queryValue = value.ToString();

            var query = new TermQuery(field) {
                Value = queryValue
            };

            var aggName = "FlagCategoryFilterAggs";

            var aggs = new Dictionary<string, Aggregation>();
            var filters = Aggregation.Filters(
                new FiltersAggregation {
                    Filters = new Buckets<Query>(new[] { Query.Term(query) })//这里必须用匿名数组类型创建集体，不要使用字典类型创建，client 8.13以上版本修复
                }
            );
            filters.Aggregations = new Dictionary<string, Aggregation>();
            //filters.Aggregations.Add("subagg1", Aggregation.DateHistogram(
            //    new DateHistogramAggregation() {
            //        Field = "IssueDate",
            //        CalendarInterval = CalendarInterval.Year,
            //        Order = new List<KeyValuePair<Field, SortOrder>>() { new KeyValuePair<Field, SortOrder>("_key", SortOrder.Desc) },//order field 如 ：_key 按项的值排序 ;_count 按数量排序； subAggKey 按子聚类项的key值排序
            //        Format = "yyyy"
            //    }));

            aggs.Add(aggName, filters);


            var aggRequest = new SearchRequest(_library) {
                Size = 0,
                Aggregations = aggs,
                SearchType = SearchType.QueryThenFetch,
            };

            var response = await _client.SearchAsync<dynamic>(aggRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);
            Assert.IsTrue(response.Aggregations.Count > 0);

            var filtersAggs = response.Aggregations[aggName] as FiltersAggregate;

            if (filtersAggs != null && filtersAggs.Buckets != null && filtersAggs.Buckets.Any()) {
                var res = filtersAggs.Buckets.FirstOrDefault()?.DocCount ?? default;

                Assert.IsTrue(res > 0);
            }
        }

        [TestMethod]
        public async Task TestMutiAgg() {

            /*
             POST: /chl/_search?pretty=true&error_trace=true&search_type=query_then_fetch&typed_keys=true
            # Request:
            {
              "aggregations": {
                "filterFlagCateroy": {
                  "filters": {
                    "filters": [
                      {
                        "term": {
                          "FlagCategory": {
                            "value": "015"
                          }
                        }
                      }
                    ]
                  }
                },
                "IssueDateAgg": {
                  "date_histogram": {
                    "calendar_interval": "year",
                    "field": "IssueDate",
                    "format": "yyyy",
                    "order": {
                      "_key": "desc"
                    }
                  }
                },
                "CategoryAgg": {
                  "terms": {
                    "field": "Category"
                  }
                }
              },
              "size": 0
            }
             */
            var field = "FlagCategory";
            object value = "015";
            var queryValue = value.ToString();

            var query = new TermQuery(field) {
                Value = queryValue
            };

            var aggs = new Dictionary<string, Aggregation>();
            var filters = Aggregation.Filters(
                new FiltersAggregation {
                    Filters = new Buckets<Query>(new[] { Query.Term(query) })
                }
            );

            aggs.Add("filterFlagCateroy", filters);


            aggs.Add("IssueDateAgg", Aggregation.DateHistogram(
                new DateHistogramAggregation() {
                    Field = "IssueDate",
                    CalendarInterval = CalendarInterval.Year,
                    Order = new List<KeyValuePair<Field, SortOrder>>() { new KeyValuePair<Field, SortOrder>("_key", SortOrder.Desc) },//order field 如 ：_key 按项的值排序 ;_count 按数量排序； subAggKey 按子聚类项的key值排序
                    Format = "yyyy"
                }));

            aggs.Add("CategoryAgg", Aggregation.Terms(
                new TermsAggregation() {
                    Field = "Category"
                }));

            var aggRequest = new SearchRequest(_library) {
                Size = 0,
                Aggregations = aggs,
                SearchType = SearchType.QueryThenFetch,
            };

            var response = await _client.SearchAsync<dynamic>(aggRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);
            Assert.IsTrue(response.Aggregations.Count > 0);
        }


        [TestMethod]
        public async Task TestSubAgg() {

            /*
             POST: /chl/_search?pretty=true&error_trace=true&search_type=query_then_fetch&typed_keys=true
            # Request:
            {
              "aggregations": {
                "filtersAgg": {
                  "aggregations": {
                    "IssueDateAgg": {
                      "date_histogram": {
                        "calendar_interval": "year",
                        "field": "IssueDate",
                        "format": "yyyy",
                        "order": {
                          "_key": "desc"
                        }
                      }
                    },
                    "CategoryAgg": {
                      "terms": {
                        "field": "Category"
                      }
                    }
                  },
                  "filters": {
                    "filters": [
                      {
                        "match_all": {}
                      }
                    ]
                  }
                }
              },
              "size": 0
            }
             */

            var field = "FlagCategory";
            object value = "015";
            var queryValue = value.ToString();

            var query = new TermQuery(field) {
                Value = queryValue
            };

            var aggs = new Dictionary<string, Aggregation>();
            var filters = Aggregation.Filters(
                new FiltersAggregation {
                    Filters = new Buckets<Query>(new[] { Query.MatchAll(new MatchAllQuery()) })
                }
            );

            filters.Aggregations = new Dictionary<string, Aggregation>();


            filters.Aggregations.Add("IssueDateAgg", Aggregation.DateHistogram(
                new DateHistogramAggregation() {
                    Field = "IssueDate",
                    CalendarInterval = CalendarInterval.Year,
                    Order = new List<KeyValuePair<Field, SortOrder>>() { new KeyValuePair<Field, SortOrder>("_key", SortOrder.Desc) },//order field 如 ：_key 按项的值排序 ;_count 按数量排序； subAggKey 按子聚类项的key值排序
                    Format = "yyyy"
                }));

            filters.Aggregations.Add("CategoryAgg", Aggregation.Terms(
                new TermsAggregation() {
                    Field = "Category"
                }));

            aggs.Add("filtersAgg", filters);


            var aggRequest = new SearchRequest(_library) {
                Size = 0,
                Aggregations = aggs,
                SearchType = SearchType.QueryThenFetch,
            };

            var response = await _client.SearchAsync<dynamic>(aggRequest);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsValidResponse);
            Assert.IsTrue(response.IsSuccess());

            Assert.IsTrue(response.Total > 0);
            Assert.IsTrue(response.Aggregations.Count > 0);
        }

        private void GenerateList(SearchResponse<dynamic> response) {
            foreach (var item in response.Documents) {

                var data = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(item.ToString());

                foreach (var itemKey in data.Keys) {

                    var dataValue = data[itemKey];
                    if (dataValue == null || dataValue.ToString() == string.Empty) continue;

                    var dataType = dataValue.GetType().FullName;
                    switch (itemKey) {
                        case "Gid":
                        case "Title":
                        case "UpdateTime":
                        case "IssueDate": {
                                Console.WriteLine($"{itemKey}={dataValue},datatype={dataType}");

                                break;
                            }
                        case "FlagCategory":
                        case "Category": {
                                var list = dataValue as Newtonsoft.Json.Linq.JArray;
                                Console.WriteLine($"{itemKey}={string.Join(";", list.Values<string>())},datatype={dataType}");
                                break;
                            }
                        case "Sort":
                        case "BeReferencedNum": {
                                Console.WriteLine($"{itemKey}={dataValue},datatype={dataType}");
                                break;
                            }
                        case "NavCatalog": {
                                Console.WriteLine($"===== NavCatalog ====");
                                List<NavCatalogItem> navCatalog = JsonConvert.DeserializeObject<List<NavCatalogItem>>(dataValue.ToString());

                                foreach (var navCatglogItem in navCatalog) {
                                    var navValueType = navCatglogItem.GetType().FullName;
                                    Console.WriteLine($"{navCatglogItem}={navCatglogItem.Name},{navCatglogItem.Tier},{navCatglogItem.Title},datatype={navValueType}");
                                }

                                break;
                            }
                        default:
                            break;
                    }
                }
            }
        }

        private string SetHighlightValue(SearchResponse<dynamic> response, string highlightField) {
            if (response == null || !response.IsValidResponse || response.Hits.Count == 0) return null;

            var hightlight = response.Hits.FirstOrDefault()?.Highlight;

            if (hightlight != null && hightlight.ContainsKey(highlightField)) {
                return hightlight[highlightField].FirstOrDefault();//全文检索时，会有多个命中
            }
            return null;
        }
    }
}
