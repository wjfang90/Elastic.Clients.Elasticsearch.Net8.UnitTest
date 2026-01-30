using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elastic.Clients.Elasticsearch.UnitTest.Models {  

    public class Chl {
        public string Gid { get; set; }
        public string Title { get; set; }

        public string Library { get; set; }
        public List<string> EffectivenessDic { get; set; }
        public List<string> Category { get; set; }
        public string UpdateTime { get; set; }
        public string IssueDate { get; set; }
        public int BeReferencedNum { get; set; }

        public string FullText { get; set; }
    }
}
