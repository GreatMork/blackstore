using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteDB;

namespace LiteDBSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Test1();
            Console.WriteLine("任务完成");
            Console.ReadLine();
        }
        static void Test1()
        {
            //打开或者创建新的数据库
            using (var db = new LiteDatabase("sample.db"))
            {
                //获取 customers 集合，如果没有会创建，相当于表
                var col = db.GetCollection<Customer>("customers");

                //创建 customers 实例
                var customer = new Customer
                {
                    Name = "John Doe",
                    Phones = new string[] { "8000-0000", "9000-0000" },
                    IsActive = true
                };
                // 将新的对象插入到数据表中，Id是自增，自动生成的
                col.Insert(customer);

                // 更新实例
                customer.Name = "Joana Doe";
                //保存到数据库
                col.Update(customer);

                // 使用文档的属性来进行检索
                col.EnsureIndex(x => x.Name);

                //使用LINQ语法来检索
                var results = col.Find(x => x.Name.StartsWith("Jo"));
            }
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string[] Phones { get; set; }
        public bool IsActive { get; set; }
    }
}
