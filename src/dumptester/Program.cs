using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DumpTester
{
    class Program
    {
        static void Main(string[] args)
        {
            TestConcurrency();
            TestPerformance();
        }

        private static void TestConcurrency()
        {
            TestClass_A a = GenerateTestObject();
            int runCount = 100000;
            var w = Stopwatch.StartNew();
            Parallel.For(0, runCount, i =>
            {
                ObjectDumper.Dumper.Dump(a);
            });
            w.Stop();
            Console.WriteLine($"{runCount} runs: {((double)w.ElapsedMilliseconds) / runCount} ms per dump");
        }

        private static void TestPerformance()
        {
            Stopwatch w;
            TestClass_A a = GenerateTestObject();

            w = Stopwatch.StartNew();
            var s = ObjectDumper.Dumper.Dump(a);
            w.Stop();
            Console.WriteLine(s);
            Console.WriteLine($"First run: {w.ElapsedMilliseconds} ms");

            w = Stopwatch.StartNew();
            int runCount = 100000;
            for (int i = 0; i < runCount; i++)
            {
                ObjectDumper.Dumper.Dump(a);
            }
            w.Stop();
            Console.WriteLine($"{runCount} runs: {((double)w.ElapsedMilliseconds) / runCount} ms");
        }

        private static TestClass_A GenerateTestObject()
        {
            TestClass_A a = new TestClass_A()
            {
                aid = 2,
                aname = "name of a",
                ab = new TestClass_B()
                {
                    bid = 4,
                    bname = "name of b"
                },
                ac = new TestClass_C()
                {
                    cid = 12,
                    cname = "name of c"
                },
                bids = new List<int>() { 3, 5, 7, 9, 11, 21, 33 }
            };
            a.ab.sibling = a.ac;
            a.ac.sibling = a.ab;
            a.ab.parent = a;
            a.ac.parent = a;
            a.bs = new List<TestClass_B>()
            {
                new TestClass_B(){ bid=145, bname="child145", parent=a },
                new TestClass_B(){ bid=178, bname="child178", parent=a },
                a.ab
            };
            a.ab.btable = GenerateTable("btable of b of a");
            a.ab.complexDict = new();
            a.ab.complexDict["first"] = new TestClass_C() { cid = 123, cname = "first dict c", parent = a };
            a.ab.complexDict["second"] = new TestClass_C() { cid = 124, cname = "second dict c", parent = a };
            a.ac.cdict = new();
            a.ac.cdict["today"] = DateTime.Now;
            a.ac.cdict["tomorrow"] = DateTime.Now.AddDays(1);
            a.adataset = new DataSet("dataset of a");
            a.adataset.Tables.Add(GenerateTable("1st table of adataset of a"));
            a.adataset.Tables.Add(GenerateTable("2nd table of adataset of a"));
            return a;
        }

        private static DataTable GenerateTable(string tableName)
        {
            var table = new DataTable(tableName);
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("name", typeof(string));
            table.Rows.Add(1, "1name");
            table.Rows.Add(2, "2name");
            table.Rows.Add(3, null);
            table.Rows.Add(4, DBNull.Value);
            return table;
        }
    }
}
