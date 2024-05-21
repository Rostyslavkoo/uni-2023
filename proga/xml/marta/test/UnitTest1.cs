using ServiceCenter;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using System.Linq;


namespace TestServiceCenter
{
    public class TestData : IDisposable
    {
        public List<Category> Categories { get; private set; } = new List<Category>();
        public List<Operation> Operations { get; private set; } = new List<Operation>();
        public List<Bill> Bills { get; private set; } = new List<Bill>();

        public TestData()
        {
            LoadCategories("C:\\C# practise\\ServiceCenter\\ServiceCenter\\category.xml");
            LoadOperations("C:\\C# practise\\ServiceCenter\\ServiceCenter\\operation.xml");
            LoadBills("C:\\C# practise\\ServiceCenter\\ServiceCenter\\bill.xml");
        }

        private void LoadCategories(string path)
        {
            var categoriesXml = XElement.Load(path);
            Categories = categoriesXml.Elements("Category").Select(c => new Category(
                (int)c.Element("CategoryId"),
                (string)c.Element("Name"),
                (int)c.Element("GarantyYear")
            )).ToList();
        }

        private void LoadBills(string path)
        {
            var billsXml = XElement.Load(path);
            Bills = billsXml.Elements("Bill").Select(b => new Bill(
                (int)b.Element("CategoryId"),
                (int)b.Element("YearOfRelease"),
                (int)b.Element("OperationNumber")
            )).ToList();
        }

        private void LoadOperations(string path)
        {
            var operationsXml = XElement.Load(path);
            Operations = operationsXml.Elements("Operation").Select(o => new Operation(
                (int)o.Element("OperationId"),
                (string)o.Element("OperationName"),
                (int)o.Element("Price")
            )).ToList();
        }


        public void Dispose()
        {
            Categories.Clear();
            Bills.Clear();
            Operations.Clear();
        }
    }


    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
        }
    }
}