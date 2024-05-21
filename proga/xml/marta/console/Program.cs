//Розробити типи для iнформацiйної системи сервiсного центру.
//Вироби вiдносяться до однiєї з категорiй.


//Категорiя виробу характеризується iдентифiкацiйним номером, назвою i кiлькiстю рокiв гарантiйного
//обслуговування, яка вiдраховується вiд року випуску виробу.



//Операцiя з виробом характеризуються iдентифiкацiйним номером, назвою i вартiстю.

//Обслуговування в перiод гарантiї безкоштовне.



//Квитанцiя про обслуговування мiстить данi у форматi < iдентифiкацiйний номер категорiї виробу, рiк випуску виробу, номер операцiї>
//Усi данi задано окремими xml-файлами.
using System.Collections.Generic;
using System.Xml.Linq;

namespace ServiceCenter
{
    //public class Item
    //{
    //    public int ItemId { get;  }
    //    public int CategoryId { get;  }
    //    public int YearOfRelease { get; }
    //    public Item(int itemId, int categoryId, int yearOfRelease)
    //    {
    //        ItemId = itemId;
    //        CategoryId = categoryId;
    //        YearOfRelease = yearOfRelease;
    //    }
    //}

    public class Category
    {
        public int CategoryId { get; }
        public string Name { get; set; }
        public int GarantyYear { get; set; }
        public Category(int categoryId, string name, int guarantyyear)
        {
            CategoryId = categoryId;
            Name = name;
            GarantyYear = guarantyyear;
        }
    }

    public class Operation
    {
        public int OperationId { get; }
        public string OperationName { get; set; }
        public int Price { get; set; }
        public Operation(int operationId, string operationName, int price)
        {
            OperationId = operationId;
            OperationName = operationName;
            Price = price;
        }
    }
    //Квитанцiя про обслуговування мiстить данi у форматi < iдентифiкацiйний номер категорiї виробу, рiк випуску виробу, номер операцiї>
    //Усi данi задано окремими xml-файлами.
    public class Bill
    {
        public int CategoryId { get; }
        public int YearOfRelease { get; }
        public int OperationNumber { get; }
        public Bill(int categoryId, int yearOfRelease, int operationNumber)
        {
            CategoryId = categoryId;
            YearOfRelease = yearOfRelease;
            OperationNumber = operationNumber;
        }
    }

    public static class Program
    {
        public static void GenerateCsvReport(string outputPath, List<Category> categories, List<Operation> operations,
            List<Bill> bills)
        {

            //            Отримати csv-файл, де для кожної категорiї виробiв(впорядкування
            //у лексико - графiчному порядку) вказати кiлькiсть кожної з виконаних операцiй
            //у форматi<назва операцiї: кiлькiсть >, цей перелiк впорядкувати у
            //спадному порядку для кожної категорiї.

            var task_a = (from bill in bills
                          join category in categories on bill.CategoryId equals category.CategoryId
                          join operation in operations on bill.OperationNumber equals operation.OperationId
                          orderby category.Name 
                          group new { bill, category } by new { category.Name, operation.OperationName } into g
                          select new
                          {
                              CategoryName = g.Key.Name,
                              OperationName = g.Key.OperationName,
                              Quantity = g.Count()
                          });

            using (var writer = new StreamWriter(outputPath))
            {
                
                writer.WriteLine("CategoryName,OperationName,Quantity");

               
                foreach (var item in task_a)
                {
                    writer.WriteLine($"{item.CategoryName},{item.OperationName},{item.Quantity}");
                }
            }
           
        }




        public static void Main(string[] args)
        {
            List<Category> Categories = new List<Category>();
            List<Operation> Operations = new List<Operation>();
            List<Bill> Bills = new List<Bill>();


            void LoadCategories(string path)
            {
                var categoriesXml = XElement.Load(path);
                Categories = categoriesXml.Elements("Category").Select(c => new Category(
                    (int)c.Element("CategoryId"),
                    (string)c.Element("Name"),
                    (int)c.Element("GarantyYear")
                )).ToList();
            }

            void LoadBills(string path)
            {
                var billsXml = XElement.Load(path);
                Bills = billsXml.Elements("Bill").Select(b => new Bill(
                    (int)b.Element("CategoryId"),
                    (int)b.Element("YearOfRelease"),
                    (int)b.Element("OperationNumber")
                )).ToList();
            }
            void LoadOperations(string path)
            {
                var operationsXml = XElement.Load(path);
                Operations = operationsXml.Elements("Operation").Select(o => new Operation(
                    (int)o.Element("OperationId"),
                    (string)o.Element("OperationName"),
                    (int)o.Element("Price")
                )).ToList();
            }
            LoadCategories("/Users/rostislavurdejcuk/Downloads/ServiceCenter/category.xml");
            LoadOperations("/Users/rostislavurdejcuk/Downloads/ServiceCenter/operation.xml");
            LoadBills("/Users/rostislavurdejcuk/Downloads/ServiceCenter/bill.xml");
            Program.GenerateCsvReport("/Users/rostislavurdejcuk/Downloads/ServiceCenter/output.csv",
                Categories, Operations, Bills);

        }

    }
}