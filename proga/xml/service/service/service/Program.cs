using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

public class ProductCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int WarrantyYears { get; set; }
}

public class Operation
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Cost { get; set; }
}

public class ServiceReceipt
{
    public int CategoryId { get; set; }
    public int Year { get; set; }
    public int OperationId { get; set; }
}

public class ServiceCenter
{
    public List<ProductCategory> LoadCategories(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        return doc.Descendants("Category")
            .Select(x => new ProductCategory
            {
                Id = (int)x.Element("Id"),
                Name = (string)x.Element("Name"),
                WarrantyYears = (int)x.Element("WarrantyYears")
            })
            .ToList();
    }

    public List<Operation> LoadOperations(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        return doc.Descendants("Operation")
            .Select(x => new Operation
            {
                Id = (int)x.Element("Id"),
                Name = (string)x.Element("Name"),
                Cost = (decimal)x.Element("Cost")
            })
            .ToList();
    }

    public List<ServiceReceipt> LoadReceipts(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        return doc.Descendants("Receipt")
            .Select(x => new ServiceReceipt
            {
                CategoryId = (int)x.Element("CategoryId"),
                Year = (int)x.Element("Year"),
                OperationId = (int)x.Element("OperationId")
            })
            .ToList();
    }

    public void GenerateCsvReport(string categoriesFilePath, string operationsFilePath, string receiptsFilePath, string outputFilePath)
    {
        var categories = LoadCategories(categoriesFilePath);
        var operations = LoadOperations(operationsFilePath);
        var receipts = LoadReceipts(receiptsFilePath);

        var query = from category in categories
            join receipt in receipts on category.Id equals receipt.CategoryId
            join operation in operations on receipt.OperationId equals operation.Id
            group new { operation, receipt } by category.Name
            into groupC
            orderby groupC.Key
            select new
            {
                CategoryName = groupC.Key,
                OperationList = groupC
                    .GroupBy(x => x.operation.Name)
                    .Select(x => new { OperationName = x.Key, OpertionCount = x.Count()})
                    .OrderByDescending(o => o.OpertionCount)
                    .ToList()
                
            };

        using (var writer = new StreamWriter(outputFilePath))
        {
            writer.WriteLine("Category,Operations");
            foreach (var category in query)
            {
                var operationsList = string.Join(", ", category.OperationList.Select(op => $"{op.OperationName}:{op.OpertionCount} "));
                writer.WriteLine($"{category.CategoryName},{operationsList}");
            }
        }
    }

    public void GenerateEarningsReport(string categoriesFilePath, string operationsFilePath, string receiptsFilePath, string outputFilePath)
    {
        var categories = LoadCategories(categoriesFilePath);
        var operations = LoadOperations(operationsFilePath);
        var receipts = LoadReceipts(receiptsFilePath);

        var query = from receipt in receipts
                    join operation in operations on receipt.OperationId equals operation.Id
                    join category in categories on receipt.CategoryId equals category.Id
                    group new { receipt, operation } by category into categoryGroup
                    orderby categoryGroup.Key.Name
                    select new
                    {
                        CategoryName = categoryGroup.Key.Name,
                        Operations = categoryGroup
                            .GroupBy(x => x.operation.Name)
                            .Select(g => new { OperationName = g.Key, TotalEarnings = g.Sum(x => x.operation.Cost) })
                            .OrderByDescending(op => op.TotalEarnings)
                            .ToList()
                    };

        var doc = new XDocument(
            new XElement("Categories",
                query.Select(category => new XElement("Category",
                    new XAttribute("Name", category.CategoryName),
                    category.Operations.Select(op => new XElement("Operation",
                        new XAttribute("Name", op.OperationName),
                        new XAttribute("TotalEarnings", op.TotalEarnings)
                    ))
                ))
            )
        );

        doc.Save(outputFilePath);
    }

    public void GenerateWarrantyReport(string categoriesFilePath, string operationsFilePath, string receiptsFilePath, string outputFilePath, int categoryId, int currentYear)
    {
        var categories = LoadCategories(categoriesFilePath);
        var operations = LoadOperations(operationsFilePath);
        var receipts = LoadReceipts(receiptsFilePath);

        var selectedCategory = categories.FirstOrDefault(c => c.Id == categoryId);
        if (selectedCategory == null)
        {
            throw new ArgumentException("Invalid category ID");
        }

        var warrantyReceipts = receipts
            .Where(r => r.CategoryId == categoryId && (currentYear - r.Year) <= selectedCategory.WarrantyYears)
            .ToList();

        var query = from receipt in warrantyReceipts
                    join operation in operations on receipt.OperationId equals operation.Id
                    group operation by operation.Name into operationGroup
                    select new
                    {
                        OperationName = operationGroup.Key,
                        Count = operationGroup.Count()
                    };

        var doc = new XDocument(
            new XElement("Category",
                new XAttribute("Name", selectedCategory.Name),
                query.OrderByDescending(op => op.Count)
                    .Select(op => new XElement("Operation",
                        new XAttribute("Name", op.OperationName),
                        new XAttribute("Count", op.Count)
                    ))
            )
        );

        doc.Save(outputFilePath);
    }
}

class Program
{
    static void Main(string[] args)
    {
        string categoriesFilePath = "/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/service/service/service/categories.xml";
        string operationsFilePath = "/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/service/service/service/operations.xml";
        string receiptsFilePath = "/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/service/service/service/receipts.xml";
        string csvReportFilePath = "/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/service/service/service/report.csv";
        string earningsReportFilePath = "/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/service/service/service/earnings_report.xml";
        string warrantyReportFilePath = "/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/service/service/service/warranty_report.xml";
        int categoryId = 1; // ID категорії для отримання звіту по гарантійним операціям
        int currentYear = 2024; // Поточний рік для розрахунку гарантії

        ServiceCenter serviceCenter = new ServiceCenter();
        serviceCenter.GenerateCsvReport(categoriesFilePath, operationsFilePath, receiptsFilePath, csvReportFilePath);
        serviceCenter.GenerateEarningsReport(categoriesFilePath, operationsFilePath, receiptsFilePath, earningsReportFilePath);
        serviceCenter.GenerateWarrantyReport(categoriesFilePath, operationsFilePath, receiptsFilePath, warrantyReportFilePath, categoryId, currentYear);

        Console.WriteLine("Reports generated successfully.");
    }
}
