using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Clients.Elasticsearch.UnitTest.Models {
    public class NavCatalogItem {
        /// <summary>
        /// 
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 目录层级
        /// </summary>
        public int Tier { get; set; }
    }
}
