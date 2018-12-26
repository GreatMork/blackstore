﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LiteDB;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.Specialized;

namespace UnitTest
{
    public enum MyEnum { First, Second }

    public class MyClass
    {
        [BsonId(false)]
        public int MyId { get; set; }
        [BsonField("MY-STRING")]
        public string MyString { get; set; }
        public Guid MyGuid { get; set; }
        public DateTime MyDateTime { get; set; }
        public DateTime? MyDateTimeNullable { get; set; }
        public int? MyIntNullable { get; set; }
        public MyEnum MyEnumProp { get; set; }
        public char MyChar { get; set; }
        public byte MyByte { get; set; }
        public decimal MyDecimal { get; set; }

        [BsonIndex(true)]
        public Uri MyUri { get; set; }

        // do not serialize this properties
        [BsonIgnore]
        public string MyIgnore { get; set; }
        public string MyReadOnly { get; private set; }
        public string MyWriteOnly { set; private get; }
        public string MyField = "DoNotSerializeThis";
        internal string MyInternalProperty { get; set; }

        // special types
        public NameValueCollection MyNameValueCollection { get; set; }

        // lists
        public string[] MyStringArray { get; set; }
        public List<string> MyStringList { get; set; }
        public Dictionary<int, string> MyDict { get; set; }

        // interfaces
        public IMyInterface MyInterface { get; set; }
        public List<IMyInterface> MyListInterface { get; set; }
        public IList<IMyInterface> MyIListInterface { get; set; }

        // objects
        public object MyObjectString { get; set; }
        public object MyObjectInt { get; set; }
        public object MyObjectImpl { get; set; }
        public List<object> MyObjectList { get; set; }


    }

    public interface IMyInterface
    {
        string Name { get; set; }
    }

    public class MyImpl : IMyInterface
    {
        public string Name { get; set; }
    }

    [TestClass]
    public class MapperTest
    {
        private MyClass CreateModel()
        {
            var c = new MyClass
            {
                MyId = 123,
                MyString = "John",
                MyGuid = Guid.NewGuid(),
                MyDateTime = DateTime.Now,
                MyIgnore = "IgnoreTHIS",
                MyIntNullable = 999,
                MyStringList = new List<string>() { "String-1", "String-2" },
                MyWriteOnly = "write-only",
                MyInternalProperty = "internal-field",
                MyNameValueCollection = new NameValueCollection(),
                MyDict = new Dictionary<int, string>() { { 1, "Row1" }, { 2, "Row2" } },
                MyStringArray = new string[] { "One", "Two" },
                MyEnumProp = MyEnum.Second,
                MyChar = 'Y',
                MyUri = new Uri("http://www.numeria.com.br"),
                MyByte = 255,
                MyDecimal = 19.9m,

                MyInterface = new MyImpl { Name = "John" },
                MyListInterface = new List<IMyInterface>() { new MyImpl { Name = "John" } },
                MyIListInterface = new List<IMyInterface>() { new MyImpl { Name = "John" } },

                MyObjectString = "MyString",
                MyObjectInt = 123,
                MyObjectImpl = new MyImpl { Name = "John" },
                MyObjectList = new List<object>() { 1, "ola", new MyImpl { Name = "John" }, new Uri("http://www.cnn.com") }
            };

            c.MyNameValueCollection["key-1"] = "value-1";
            c.MyNameValueCollection["KeyNumber2"] = "value-2";

            return c;
        }

        [TestMethod]
        public void Mapper_Test()
        {
            var mapper = new BsonMapper();

            mapper.UseLowerCaseDelimiter('_');

            var obj = CreateModel();
            var doc = mapper.ToDocument(obj);

            var json = JsonSerializer.Serialize(doc, true);

            var nobj = mapper.ToObject<MyClass>(doc);

            // compare object to document
            Assert.AreEqual(doc["_id"].AsInt32, obj.MyId);
            Assert.AreEqual(doc["MY-STRING"].AsString, obj.MyString);
            Assert.AreEqual(doc["my_guid"].AsGuid, obj.MyGuid);

            // compare 2 objects
            Assert.AreEqual(obj.MyId, nobj.MyId);
            Assert.AreEqual(obj.MyString, nobj.MyString);
            Assert.AreEqual(obj.MyGuid, nobj.MyGuid);
            Assert.AreEqual(obj.MyDateTime, nobj.MyDateTime);
            Assert.AreEqual(obj.MyDateTimeNullable, nobj.MyDateTimeNullable);
            Assert.AreEqual(obj.MyIntNullable, nobj.MyIntNullable);
            Assert.AreEqual(obj.MyEnumProp, nobj.MyEnumProp);
            Assert.AreEqual(obj.MyChar, nobj.MyChar);
            Assert.AreEqual(obj.MyByte, nobj.MyByte);
            Assert.AreEqual(obj.MyDecimal, nobj.MyDecimal);
            Assert.AreEqual(obj.MyUri, nobj.MyUri);
            Assert.AreEqual(obj.MyNameValueCollection["key-1"], nobj.MyNameValueCollection["key-1"]);
            Assert.AreEqual(obj.MyNameValueCollection["KeyNumber2"], nobj.MyNameValueCollection["KeyNumber2"]);


            // list
            Assert.AreEqual(obj.MyStringArray[0], nobj.MyStringArray[0]);
            Assert.AreEqual(obj.MyStringArray[1], nobj.MyStringArray[1]);
            Assert.AreEqual(obj.MyDict[2], nobj.MyDict[2]);

            // interfaces
            Assert.AreEqual(obj.MyInterface.Name, nobj.MyInterface.Name);
            Assert.AreEqual(obj.MyListInterface[0].Name, nobj.MyListInterface[0].Name);
            Assert.AreEqual(obj.MyIListInterface[0].Name, nobj.MyIListInterface[0].Name);

            // objects
            Assert.AreEqual(obj.MyObjectString, nobj.MyObjectString);
            Assert.AreEqual(obj.MyObjectInt, nobj.MyObjectInt);
            Assert.AreEqual((obj.MyObjectImpl as MyImpl).Name, (nobj.MyObjectImpl as MyImpl).Name);
            Assert.AreEqual(obj.MyObjectList[0], obj.MyObjectList[0]);
            Assert.AreEqual(obj.MyObjectList[1], obj.MyObjectList[1]);
            Assert.AreEqual(obj.MyObjectList[3], obj.MyObjectList[3]);

        }
    }
}
