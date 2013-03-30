﻿/// <summary>
/// Serpent, a Python literal expression serializer/deserializer
/// (a.k.a. Python's ast.literal_eval in .NET)
///
/// Copyright 2013, Irmen de Jong (irmen@razorvine.net)
/// Software license: "MIT software license". See http://opensource.org/licenses/MIT
/// </summary>

using System;
using System.Collections.Generic;
using Hashtable = System.Collections.Hashtable;
using IDictionary = System.Collections.IDictionary;
using System.Text;

using NUnit.Framework;
using Razorvine.Serpent.Parsing;
using Razorvine.Serpent.Serializing;

namespace Razorvine.Serpent.Test
{
	[TestFixture]
	public class SerializeTest
	{
		public byte[] strip_header(byte[] data)
		{
			int start=Array.IndexOf(data, (byte)10); // the newline after the header
			if(start<0)
				throw new ArgumentException("need header in string");
			start++;
			byte[] result = new byte[data.Length-start];
			Array.Copy(data, start, result, 0, data.Length-start);
			return result;
		}
		
		public byte[] B(string s)
		{
			return Encoding.UTF8.GetBytes(s);
		}
		
		public string S(byte[] b)
		{
			return Encoding.UTF8.GetString(b);
		}


		[Test]
		public void TestHeader()
		{
			Serializer ser = new Serializer();
			byte[] data = ser.Serialize(null);
			Assert.AreEqual(35, data[0]);
			string strdata = S(data);
			string header = "# serpent utf-8 dotnet-cli"+Environment.Version.ToString(2);
			Assert.AreEqual(header, strdata.Split('\n')[0]);
			
			data = B("# header\nfirst-line");
			data = strip_header(data);
			Assert.AreEqual(B("first-line"), data);
		}
		
		
		[Test]
		public void TestStuff()
		{
			Serializer ser=new Serializer();
			byte[] result = ser.Serialize("blerp");
			result=strip_header(result);
			Assert.AreEqual(B("'blerp'"), result);
			result = ser.Serialize(new Guid("f1f8d00e-49a5-4662-ac1d-d5f0426ed293"));
			result=strip_header(result);
			Assert.AreEqual(B("'f1f8d00e-49a5-4662-ac1d-d5f0426ed293'"), result);
			result = ser.Serialize(123456789.987654321987654321987654321987654321m);
			result=strip_header(result);
			Assert.AreEqual(B("'123456789.98765432198765432199'"), result);
		}

		[Test]
		public void TestNull()
		{
			Serializer ser = new Serializer();
			byte[] data = ser.Serialize(null);
			data=strip_header(data);
			Assert.AreEqual(B("None"),data);
		}
		
		[Test]
		public void TestStrings()
		{
			Serializer serpent = new Serializer();
			byte[] ser = serpent.Serialize("hello");
			byte[] data = strip_header(ser);
			Assert.AreEqual(B("'hello'"), data);
        	ser = serpent.Serialize("quotes'\"");
        	data = strip_header(ser);
        	Assert.AreEqual(B("'quotes\\'\"'"), data);
        	ser = serpent.Serialize("quotes2'");
        	data = strip_header(ser);
        	Assert.AreEqual(B("\"quotes2'\""), data);
		}
		
		[Test]
		public void TestNumbers()
		{
			Serializer serpent = new Serializer();
			byte[] ser = serpent.Serialize((int)12345);
			byte[] data = strip_header(ser);
			Assert.AreEqual(B("12345"), data);
			ser = serpent.Serialize((uint)12345);
			data = strip_header(ser);
			Assert.AreEqual(B("12345"), data);
			ser = serpent.Serialize((long)1234567891234567891L);
	        data = strip_header(ser);
	        Assert.AreEqual(B("1234567891234567891"), data);
			ser = serpent.Serialize((ulong)12345678912345678912L);
	        data = strip_header(ser);
	        Assert.AreEqual(B("12345678912345678912"), data);
	        ser = serpent.Serialize(99.1234);
	        data = strip_header(ser);
	        Assert.AreEqual(B("99.1234"), data);
	        ser = serpent.Serialize(1234.9999999999m);
	        data = strip_header(ser);
	        Assert.AreEqual(B("'1234.9999999999'"), data);
			ser = serpent.Serialize(123456789.987654321987654321987654321987654321m);
			data=strip_header(ser);
			Assert.AreEqual(B("'123456789.98765432198765432199'"), data);
	        ComplexNumber cplx = new ComplexNumber(2.2, 3.3);
	        ser = serpent.Serialize(cplx);
	        data = strip_header(ser);
	        Assert.AreEqual(B("(2.2+3.3j)"), data);
	        cplx = new ComplexNumber(0, 3);
	        ser = serpent.Serialize(cplx);
	        data = strip_header(ser);
	        Assert.AreEqual(B("(0+3j)"), data);
	        cplx = new ComplexNumber(-2, -3);
	        ser = serpent.Serialize(cplx);
	        data = strip_header(ser);
	        Assert.AreEqual(B("(-2-3j)"), data);
		}
		
		[Test]
		public void TestBool()
		{
			Serializer serpent = new Serializer();
			byte[] ser = serpent.Serialize(true);
			byte[] data = strip_header(ser);
			Assert.AreEqual(B("True"),data);
			ser = serpent.Serialize(false);
			data = strip_header(ser);
			Assert.AreEqual(B("False"),data);
		}
		
		[Test]
		public void TestDictionary()
		{
			Serializer serpent = new Serializer();
			Parser p = new Parser();
			
			// test empty dict
			IDictionary ht = new Hashtable();
			byte[] ser = serpent.Serialize(ht);
			Assert.AreEqual(Encoding.UTF8.GetBytes("{}"), strip_header(ser));
			string parsed = p.Parse(ser).Root.ToString();
            Assert.AreEqual("{}", parsed);
			
            // empty dict with indentation
            serpent.Indent=true;
			ser = serpent.Serialize(ht);
			Assert.AreEqual(Encoding.UTF8.GetBytes("{}"), strip_header(ser));
			parsed = p.Parse(ser).Root.ToString();
            Assert.AreEqual("{}", parsed);
			
			// test dict with values
			serpent.Indent=false;
			ht = new Hashtable() {
				{42, "fortytwo"},
				{"sixteen-and-half", 16.5},
				{"name", "Sally"},
				{"status", false}
			};
			
			ser = serpent.Serialize(ht);
			Assert.AreEqual('}', ser[ser.Length-1]);
			Assert.AreNotEqual(',', ser[ser.Length-2]);
			parsed = p.Parse(ser).Root.ToString();
            Assert.AreEqual("{42:'fortytwo','status':False,'name':'Sally','sixteen-and-half':16.5}", parsed);
            
            // test indentation
            serpent.Indent=true;
            ser = serpent.Serialize(ht);
            string indented = Encoding.UTF8.GetString(strip_header(ser));
			Console.WriteLine(indented);
			Assert.AreEqual('}', ser[ser.Length-1]);
			Assert.AreEqual('\n', ser[ser.Length-2]);
			Assert.AreNotEqual(',', ser[ser.Length-3]);
			parsed = p.Parse(ser).Root.ToString();
            Assert.AreEqual("{42:'fortytwo','status':False,'name':'Sally','sixteen-and-half':16.5}", parsed);
		}

		[Test]
		public void TestBytes()
		{
			Serializer serpent = new Serializer();
			Parser p = new Parser();
			byte[] bytes = new byte[] { 97, 98, 99, 100, 101, 102 };	// abcdef
			byte[] ser = serpent.Serialize(bytes);
			string parsed = p.Parse(ser).Root.ToString();
            Assert.AreEqual("{'encoding':'base64','data':'YWJjZGVm'}", parsed);
		}
		
		[Test]
		public void TestIndentation()
		{
			/***
        data = {"first": [1,2, ("a", "b")], "second": {1: False}}
        ser = serpent.serialize(data, indent=True).decode("utf-8")
        _, _, ser = ser.partition("\n")
        self.assertEqual("""{
  'first': [
    1,
    2,
    (
      'a',
      'b'
    )
  ],
  'second': {
    1: False
  }
}""", ser)

***/			 
			Assert.Fail("todo");
		}
		
		[Test]
		public void TestSorting()
		{
/***
    def test_sorting(self):
        obj = [3,2,1]
        ser = serpent.serialize(obj)
        data = strip_header(ser)
        self.assertEqual(b"[3,2,1]", data)
        obj = (3,2,1)
        ser = serpent.serialize(obj)
        data = strip_header(ser)
        self.assertEqual(b"(3,2,1)", data)
        obj = {3: "three", 4: "four", 2:"two", 1:"one"}
        ser = serpent.serialize(obj)
        data = strip_header(ser)
        self.assertEqual(b"{1:'one',2:'two',3:'three',4:'four'}", data)
        obj = {3,4,2,1,6,5}
        ser = serpent.serialize(obj)
        data = strip_header(ser)
        self.assertEqual(b"{1,2,3,4,5,6}", data)

        obj = {3, "something"}
        ser = serpent.serialize(obj, indent=False)
        data = strip_header(ser)
        self.assertTrue(data==b"{3,'something'}" or data==b"{'something',3}")
        ser = serpent.serialize(obj, indent=True)
        data = strip_header(ser)
        self.assertTrue(data==b"{\n  3,\n  'something'\n}" or data==b"{\n  'something',\n  3\n}")

        obj = {3:"three", "something":99}
        ser = serpent.serialize(obj, indent=False)
        data = strip_header(ser)
        self.assertTrue(data==b"{'something':99,3:'three'}" or data==b"{3:'three','something':99}")
        ser = serpent.serialize(obj, indent=True)
        data = strip_header(ser)
        self.assertTrue(data==b"{\n  'something': 99,\n  3: 'three'\n}" or data==b"{\n  3: 'three',\n  'something': 99\n}")
***/			
			Assert.Fail("todo");
		}

/***

    def test_class(self):
        class Class1(object):
            def __init__(self):
                self.attr = 1
        class Class2(object):
            def __getstate__(self):
                return {"attr": 42}
        c = Class1()
        ser = serpent.serialize(c)
        data = serpent.deserialize(ser)
        self.assertEqual({'__class__': 'Class1', 'attr': 1}, data)
        c = Class2()
        ser = serpent.serialize(c)
        data = serpent.deserialize(ser)
        self.assertEqual({'attr': 42}, data)

    def test_time(self):
        ser = serpent.serialize(datetime.datetime(2013, 1, 20, 23, 59, 45, 999888))
        data = strip_header(ser)
        self.assertEqual(b"'2013-01-20T23:59:45.999888'", data)
        ser = serpent.serialize(datetime.time(23, 59, 45, 999888))
        data = strip_header(ser)
        self.assertEqual(b"'23:59:45.999888'", data)
        ser = serpent.serialize(datetime.time(23, 59, 45))
        data = strip_header(ser)
        self.assertEqual(b"'23:59:45'", data)
        ser = serpent.serialize(datetime.timedelta(1, 4000, 999888, minutes=22))
        data = strip_header(ser)
        self.assertEqual(b"91720.999888", data)
        ser = serpent.serialize(datetime.timedelta(seconds=12345))
        data = strip_header(ser)
        self.assertEqual(b"12345.0", data)
***/

//@TODO: dictionary
//@TODO: hashset
//@TODO: collection
//@TODO: datetime, timespan
//@TODO: exception
//@TODO: random class
//@TODO: indentation.
	}
}
