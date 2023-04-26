Dumps .Net objects to string form for debugging purposes.

Sample dump code:

    TestClass_A a = GenerateTestObject();
    ObjectDumper.Dumper.MaxNestLevel = 20; // default is 10
    string s = ObjectDumper.Dumper.Dump(a);

Sample class:

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
  
  
Sample output:

	[Dumping: DumpTester.TestClass_A, hashcode: 48132822]
	privatetext = "abc"
	<aid> = "2"
	<aname> = "name of a"
	<ab> = [Dumping: DumpTester.TestClass_B, hashcode: 30542218]
		privateguid = "f87283ac-e6aa-467b-bb2c-f7becebd4457"
		<bid> = "4"
		<bname> = "name of b"
		<parent> = [Dumped before: 48132822]
		<sibling> = [Dumping: DumpTester.TestClass_C, hashcode: 6444509]
			<cid> = "12"
			<cname> = "name of c"
			<cdate> = "26.04.2023 14:52:05"
			<parent> = [Dumped before: 48132822]
			<sibling> = [Dumped before: 30542218]
			<cdict> = [Dumping: System.Collections.Generic.Dictionary`2<System.String, System.DateTime>, hashcode: 58000584]
				"today" = "26.04.2023 14:52:05"
				"tomorrow" = "27.04.2023 14:52:05"
		<btable> = [Dumping: System.Data.DataTable, hashcode: 52243212]
			[TableName: btable of b of a]
			id;name
			1;1name
			2;2name
			3;<null>
			4;<null>		
		<complexDict> = [Dumping: System.Collections.Generic.Dictionary`2<System.String, DumpTester.TestClass_C>, hashcode: 426867]
			"first" = [Dumping: DumpTester.TestClass_C, hashcode: 3841804]
				<cid> = "123"
				<cname> = "first dict c"
				<cdate> = "26.04.2023 14:52:05"
				<parent> = [Dumped before: 48132822]
				<sibling> = <null>
				<cdict> = <null>
			"second" = [Dumping: DumpTester.TestClass_C, hashcode: 34576242]
				<cid> = "124"
				<cname> = "second dict c"
				<cdate> = "26.04.2023 14:52:05"
				<parent> = [Dumped before: 48132822]
				<sibling> = <null>
				<cdict> = <null>
	<ac> = [Dumped before: 6444509]
	<bids> = [Dumping: System.Collections.Generic.List`1<System.Int32>, hashcode: 42750725]
		{
		"3",
		"5",
		"7",
		"9",
		"11",
		"21",
		"33",
		}
	<bids2> = <null>
	<bids3> = <null>
	<bs> = [Dumping: System.Collections.Generic.List`1<DumpTester.TestClass_B>, hashcode: 49212206]
		{
		[Dumping: DumpTester.TestClass_B, hashcode: 40256670]
			privateguid = "74f63c1a-c070-497b-90f2-485a6a9571f8"
			<bid> = "145"
			<bname> = "child145"
			<parent> = [Dumped before: 48132822]
			<sibling> = <null>
			<btable> = <null>
			<complexDict> = <null>,
		[Dumping: DumpTester.TestClass_B, hashcode: 26765710]
			privateguid = "f98c8685-401c-4fa0-9d2a-e8f4ac88abd5"
			<bid> = "178"
			<bname> = "child178"
			<parent> = [Dumped before: 48132822]
			<sibling> = <null>
			<btable> = <null>
			<complexDict> = <null>,
		[Dumped before: 30542218],
		}
	<adataset> = [Dumping: System.Data.DataSet, hashcode: 39564799]
		{
		[DataSetName: dataset of a]
		[TableName: 1st table of adataset of a]
		id;name
		1;1name
		2;2name
		3;<null>
		4;<null>	
		[TableName: 2nd table of adataset of a]
		id;name
		1;1name
		2;2name
		3;<null>
		4;<null>		}
	<abtye> = "89"
	<abytearray> = "QnUgZGEgYmlyIG1ldGluLg=="

