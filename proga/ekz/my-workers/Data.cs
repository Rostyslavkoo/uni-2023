using System.Xml.Linq;
using System.Xml.Schema;

public class Brigade
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Worker
{
    public int Id { get; set; }
    public string Surname { get; set; }
    public int TarrifId { get; set; }
    public int BrigadeId { get; set; }
}

public class Tarrif
{
    public int Id { get; set; }
    public int Price { get; set; }
}
public class Report
{
    public int WorkerId { get; set; }
    public int ReducedTime { get; set; }
    public int ItemId { get; set; }
    public int Amount { get; set; }
}
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Data
{
    public List<Brigade> Brigades { get; set; }
    public List<Worker> Workers { get; set; }
    public List<Tarrif> Tarrifs { get; set; }
    public List<Item> Items { get; set; }
    public List<Report> Reports { get; set; }



    public Data()
    {
        Brigades = new List<Brigade>();
        Workers = new List<Worker>();
        Tarrifs = new List<Tarrif>();
        Items = new List<Item>();
        Reports = new List<Report>();
    }
    
    public void Load1(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        Brigades = doc.Descendants("Item")
            .Select(x => new Brigade
            {
                Id = (int)x.Element("Id"),
                Name = (string)x.Element("Name"),
            })
            .ToList();
    }
    public void Load2(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        Workers = doc.Descendants("Item")
            .Select(x => new Worker
            {
                Id = (int)x.Element("Id"),
                Surname = (string)x.Element("Surname"),
                TarrifId = (int)x.Element("TarrifId"),
                BrigadeId = (int)x.Element("BrigadeId")
            })
            .ToList();
    }
    public void Load3(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        Tarrifs = doc.Descendants("Item")
            .Select(x => new Tarrif
            {
                Id = (int)x.Element("Id"),
                Price = (int)x.Element("Price"),
            })
            .ToList();
    }
    public void Load4(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        Items = doc.Descendants("Item")
            .Select(x => new Item
            {
                Id = (int)x.Element("Id"),
                Name = (string)x.Element("Name"),
            })
            .ToList();
    }
    public List<Report> Load5(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        return doc.Descendants("Item")
            .Select(x => new Report
            {
                WorkerId = (int)x.Element("WorkerId"),
                ReducedTime = (int)x.Element("ReducedTime"),
                ItemId = (int)x.Element("ItemId"),
                Amount = (int)x.Element("Amount"),
            })
            .ToList();
    }
    
    public List<string> TaskA(string output)
    {
        var query = from report in Reports
                    join worker in Workers on report.WorkerId equals worker.Id
                    join item in Items on report.ItemId equals item.Id
                    join brigade in Brigades on worker.BrigadeId equals brigade.Id
                    join tarrif in Tarrifs on worker.TarrifId equals tarrif.Id
                    group new { report, worker, item, brigade, tarrif} by brigade.Name into groupGroups
                    orderby groupGroups.Key
                    select new
                    {
                        BrigadeName = groupGroups.Key,
                        WorkersList = from groupG in groupGroups
                                      group new { groupG.worker, groupG.brigade, groupG.tarrif,groupG.item ,groupG.report}
                                      by  groupG.worker.Surname
                                      into groupWorkers
                                      orderby groupWorkers.Key
                                      select new
                                        {
                                            WorkerName = groupWorkers.Key,
                                            WorkerTotal = groupWorkers.Sum(x => x.tarrif.Price * x.report.ReducedTime)
                                        }
                        
                    };
    
        var doc = new XDocument(
            new XElement("Items",
                query.Select(group => new XElement("Group",
                    new XAttribute("GroupName", group.BrigadeName),
                    group.WorkersList.Select(worker => new XElement("Worker",
                        new XAttribute("WorkerName", worker.WorkerName),
                        new XAttribute("WorkerTotal", worker.WorkerTotal)
                    )
                )
            )))
        );
    
        if (output != "")
        {
            doc.Save(output);
        }
    
        var res = new List<string>();
        using (var writer = new StreamWriter("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/output/output.csv"))
        {
            foreach (var brigade in query)
            {
                foreach (var workers in brigade.WorkersList)
                {
                    
                        writer.WriteLine($"{brigade.BrigadeName} {workers.WorkerName} {workers.WorkerTotal}");
                        res.Add($"{brigade.BrigadeName} {workers.WorkerName} {workers.WorkerTotal}");
                    
                }
            }
        }
        return res;
    }
        public List<string> TaskB(string output)
    {
        var query = from report in Reports
                    join worker in Workers on report.WorkerId equals worker.Id
                    join item in Items on report.ItemId equals item.Id
                    join brigade in Brigades on worker.BrigadeId equals brigade.Id
                    join tarrif in Tarrifs on worker.TarrifId equals tarrif.Id
                    group new { report, worker, item, brigade, tarrif } by item.Name into groupGroups
                    orderby groupGroups.Key
                    select new
                    {
                        Name = groupGroups.Key,
                        Count = groupGroups.Sum(x => x.report.Amount)
                    };
        
                var doc = new XDocument(new XElement("Prystryis", 
                    query.OrderByDescending(g => g.Count).
                Select(g => new XElement("Prystryi",
                            new XAttribute("Name", g.Name),
                            new XAttribute("Count", g.Count)
                        ))
                ));
    
        if (output != "")
        {
            doc.Save(output);
        }
    
        var res = new List<string>();
        using (var writer = new StreamWriter("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/output/output.csv"))
        {
            foreach (var item in query)
            {
                        writer.WriteLine($"{item.Name} {item.Count}");
                        res.Add($"{item.Name} {item.Count}");
            }
        }
        return res;
    }

}