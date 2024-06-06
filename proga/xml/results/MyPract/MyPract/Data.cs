using System.Xml.Linq;

public class Task
{
    public int  Id { get; set; }
    public string  Theme { get; set; }
    public DateTime  Deadline { get; set; } 
}
public class Student
{
    public int  Id { get; set; }
    public string  Surname { get; set; }
    public string  Name { get; set; }
    public string  GroupName { get; set; } 
}

public class Result
{
    public int  TaskId { get; set; }
    public int  StudentId { get; set; }
    public decimal  Score { get; set; }
    public DateTime  SubmitDate { get; set; }
}

public class Data
{
    public List<Task> Tasks { get; set; }
    public List<Student> Students { get; set; }
    public List<Result> Results { get; set; }

    public Data()
    {
        Tasks = new List<Task>();
        Students = new List<Student>();
        Results = new List<Result>();
    }

    public void Load1(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        Tasks = doc.Descendants("Item")
            .Select(x => new Task
            {
                Id = (int)x.Element("Id"),
                Theme = (string)x.Element("Theme"),
                Deadline = (DateTime)x.Element("Deadline")
            })
            .ToList();
    }

    public void Load2(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        Students = doc.Descendants("Item")
            .Select(x => new Student
            {
                Id = (int)x.Element("Id"),
                Surname = (string)x.Element("Surname"),
                Name = (string)x.Element("Name"),
                GroupName = (string)x.Element("GroupName"),
            })
            .ToList();
    }

    public void Load3(string filePath)
    {
        XDocument doc = XDocument.Load(filePath);
        Results = doc.Descendants("Item")
            .Select(x => new Result
            {
                TaskId = (int)x.Element("TaskId"),
                StudentId = (int)x.Element("StudentId"),
                Score = (decimal)x.Element("Score"),
                SubmitDate = (DateTime)x.Element("SubmitDate"),
            })
            .ToList();
    }

    public static decimal CalculateScore(DateTime deadline, DateTime submitted, decimal score)
    {
        if (submitted > deadline)
        {
            return score / 2;
        }

        return score;
    }
    public List<string> TaskA(string output)
    {
        var query = from result in Results
            join task in Tasks on result.TaskId equals task.Id
            join student in Students on result.StudentId equals student.Id
            group new { result, task, student } by student.GroupName
            into groupGroups
            orderby groupGroups.Key
            select new
            {
                GroupName = groupGroups.Key,
                StudentList = from groupG in groupGroups
                    group new {groupG.result,groupG.task,groupG.student}
                        by groupG.student.Name
                        into groupStudents
                        orderby groupStudents.Key
                        select new
                        {
                            StudentName = groupStudents.Key,
                            StudentScore = groupStudents
                                .OrderBy(x => x.task.Id)
                                .Select(x=> $"{x.task.Theme}: {CalculateScore(x.task.Deadline, x.result.SubmitDate,x.result.Score)} ")
                        }
            };
        
        var doc = new XDocument(
            new XElement("Items",
                query.Select(group => new XElement("Group",
                    new XAttribute("GroupName", group.GroupName),
                    group.StudentList.Select(st => new XElement("Student",
                        new XAttribute("StudentName", st.StudentName),
                        st.StudentScore.Select(r => new XElement("Result",
                                new XAttribute("R", r))
                )))))
        ));
        if (output != "")
        {
            doc.Save(output);
        }
        
        var res = new List<string>();
        using (var writer = new StreamWriter("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/MyPract/MyPract/output/output.csv"))
        {
            foreach (var group in query)
            {
                foreach (var student in group.StudentList)
                {
                    foreach (var result in student.StudentScore)
                    {   
                        writer.WriteLine($"{group.GroupName} {student.StudentName} {result}");
                        res.Add($"{group.GroupName} {student.StudentName} {result}");
                    }
                }
            }
        }

        return res;
    }
        public List<string> TaskB(string output)
    {
        var query = from result in Results
            join task in Tasks on result.TaskId equals task.Id
            join student in Students on result.StudentId equals student.Id
            group new { result, task, student } by student.GroupName
            into groupGroups
            orderby groupGroups.Key
            select new
            {
                GroupName = groupGroups.Key,
                SubjectList = from groupG in groupGroups
                    group new {groupG.result,groupG.task,groupG.student}
                        by groupG.task.Theme
                        into groupSubject
                        select new
                        {
                            SubjectName = groupSubject.Key,
                            Student = groupSubject
                                .GroupBy(x => x.student.Surname)
                                .Select(x=> new
                                {
                                    StudentName = x.Key,
                                    TotalScore = x.Sum(y => CalculateScore(y.task.Deadline,y.result.SubmitDate,y.result.Score))
                                })
                        }
            };
        
        var doc = new XDocument(
            new XElement("Items",
                query.Select(group => new XElement("Group",
                    new XAttribute("GroupName", group.GroupName),
                    group.SubjectList.Select(st => new XElement("Subject",
                        new XAttribute("SubjectName", st.SubjectName),
                        st.Student.OrderByDescending(st => st.TotalScore)
                        .Select(r => new XElement("Result",
                                new XAttribute("StudentName", r.StudentName ),
                                new XAttribute("StudentResult", r.TotalScore)
                                )
                        
                )))))
        ));
        if (output != "")
        {
            doc.Save(output);
        }
        
        var res = new List<string>();
        using (var writer = new StreamWriter("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/MyPract/MyPract/output/output.csv"))
        {
            foreach (var group in query)
            {
                foreach (var subj in group.SubjectList)
                {
                    foreach (var result in  subj.Student)
                    {
                        writer.WriteLine($"{group.GroupName} {subj.SubjectName} {result.StudentName} {result.TotalScore}");
                        res.Add($"{group.GroupName} {subj.SubjectName} {result.StudentName} {result.TotalScore}");
                    }
                }
            }
        }
        return res;
    }
    public List<string> TaskC(string output)
    {
        var query = from result in Results
            join task in Tasks on result.TaskId equals task.Id
            join student in Students on result.StudentId equals student.Id
            group new { result, task, student } by student.GroupName
            into groupGroups
            orderby groupGroups.Key
            select new
            {
                GroupName = groupGroups.Key,
                StudentsList = from groupG in groupGroups
                    group new {groupG.result,groupG.task,groupG.student}
                        by $"{groupG.student.Surname} {groupG.student.Name[0]}."
                        into groupStudent
                        orderby groupStudent.Key
                        select new
                        {
                            StudentName = groupStudent.Key,
                            StudentScore = groupStudent
                                .Sum( x=> CalculateScore(x.task.Deadline,x.result.SubmitDate,x.result.Score))
                            }
            };
        
        var doc = new XDocument(
            new XElement("Items",
                query.Select(group => new XElement("Group",
                    new XAttribute("GroupName", group.GroupName),
                    group.StudentsList.Select(st => new XElement("Student",
                        new XAttribute("StudentName", st.StudentName),
                        new XAttribute("StudentScore", st.StudentScore)
                        ))))
            ));
        if (output != "")
        {
            doc.Save(output);
        }
        
        var res = new List<string>();
        using (var writer = new StreamWriter("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/MyPract/MyPract/output/output.csv"))
        {
            foreach (var group in query)
            {
                foreach (var subj in group.StudentsList)
                {
                    {
                        writer.WriteLine($"{group.GroupName} {subj.StudentName} {subj.StudentScore}");
                        res.Add($"{group.GroupName} {subj.StudentName} {subj.StudentScore}");
                    }
                }
            }
        }

        return res;
    }
}
