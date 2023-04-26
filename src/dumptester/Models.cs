using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DumpTester
{
    class TestClass_A
    {
        private string privatetext;
        public int aid { get; set; }
        public string aname { get; set; }

        public TestClass_B ab { get; set; }
        public TestClass_C ac { get; set; }
        public List<int> bids { get; set; }
        public List<int> bids2 { get; set; }
        public List<int> bids3 { get; set; }
        public List<TestClass_B> bs { get; set; }
        public DataSet adataset { get; set; }
        public byte abtye { get; set; } = 89;
        public byte[] abytearray { get; set; } = new byte[] { 66, 117, 32, 100, 97, 32, 98, 105, 114, 32, 109, 101, 116, 105, 110, 46 };

        public TestClass_A()
        {
            privatetext = "abc";
        }
    }

    class TestClass_B
    {
        private Guid privateguid;
        public int bid { get; set; }
        public string bname { get; set; }
        public TestClass_A parent { get; set; }
        public TestClass_C sibling { get; set; }
        public DataTable btable { get; set; }
        public Dictionary<string, TestClass_C> complexDict { get; set; }

        public TestClass_B()
        {
            privateguid = Guid.NewGuid();
        }
    }

    class TestClass_C
    {
        public int cid { get; set; }
        public string cname { get; set; }
        public DateTime cdate { get; set; }
        public TestClass_A parent { get; set; }
        public TestClass_B sibling { get; set; }
        public Dictionary<string, DateTime> cdict { get; set; }

        public TestClass_C()
        {
            cdate = DateTime.Now;
        }
    }
}
