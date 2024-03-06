using System;
namespace Task
{
    class Program
    {
        class Computer
        {
            public string Mark { get;}
            public double ProcessorSpeed { get; set; }
            public int RAM { get; set; }

            public int Drive { get; set; }
            public double Price { get; set; }

            public Computer(string m, double proc, int ram, int drive, double p)
            {
                Mark = m;
                ProcessorSpeed = proc;
                RAM = ram;
                Drive = drive;
                Price = p;
            }

            public Computer()
            {
                Mark = "None";
                ProcessorSpeed = 0;
                RAM = 0;
                Price = 0;
            }


            public override string ToString()
            {
                return "Mark: " + Mark + "; Speed: " + ProcessorSpeed + "; RAM: " + RAM + "; Drive:" + Drive + "; Price: " + Price;
            }
        }

        class Server: Computer
        {
            public int Disk { get; set; }
            public Server(string m, double proc, int ram, int drive, double p, int d) : base(m, proc, ram, drive, p)
            {
                Disk = d;
            }


            public override string ToString()
            {
                return base.ToString() + "; Disk: " + Disk;
            }
        }

        class WorkStation : Computer
        {
            public string MonitorMark { get;}
            public double Diagonal { get;}

            public WorkStation(string m, double proc, int ram, int drive, double p, string monitor, double diag): base(m, proc, ram, drive, p)
            {
                MonitorMark = monitor;
                Diagonal = diag;
            }

            public override string ToString()
            {
                return base.ToString() + "; Monitor: " + MonitorMark + "; Diagonal: " + Diagonal;
            }

        }

        private static int FindInArray(string[] arr, string f)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] == f)
                {
                    return i;
                }
                else if (arr[i] == null)
                {
                    arr[i] = f;
                    return i;
                }
            }
            return 0;
        }

        static void Main(string[] args)
        {
            Computer c1 = new Computer("Lenovo", 5.1, 16, 500, 1500);
            Computer c2 = new Computer("Samsung", 5.2, 16, 300, 2500);
            Computer c3 = new Computer("Lenovo", 2, 8, 400, 600);

            Server s1 = new Server("Samsung", 5.2, 16, 500, 2500, 2000);
            Server s2 = new Server("Samsung", 5.2, 8, 350, 3500, 2500);
            Server s3 = new Server("Dell", 5.2, 16, 2000, 1500, 1000);

            WorkStation w1 = new WorkStation("Lenovo", 5.3, 16, 2500, 3000, "LG", 16);
            WorkStation w2 = new WorkStation("Dell", 2.3, 8, 300, 1400, "LG", 16);
            WorkStation w3 = new WorkStation("Lenovo", 6, 16, 3000, 5600, "LG", 24);

            Computer[] computers = [ c1, c2, c3, s1, s2, s3, w1, w2, w3 ];

            //a
            for (int i = 0; i < computers.Length; i++)
            {
                if (computers[i].GetType() == typeof(Server))
                {
                    Console.WriteLine("Server:");
                }
                else if (computers[i].GetType() == typeof(WorkStation))
                {
                    Console.WriteLine("Workstation:");
                }
                else
                {
                    Console.WriteLine("Computer:");
                }
                Console.WriteLine(computers[i]);
            }

            Console.WriteLine("-------------------------------------------------------------------------");

            //b
            string[] computerKeys = new string[computers.Length];
            string[] serverKeys = new string[computers.Length];
            string[] workStationKeys = new string[computers.Length];

            double[] computerPrice = new double[computers.Length];
            double[] serverPrice = new double[computers.Length];
            double[] workStationPrice = new double[computers.Length];

            for (int i = 0; i < computers.Length; i++)
            {
                string mark = computers[i].Mark;
                double price = computers[i].Price;
                if (computers[i].GetType() == typeof(Server))
                {
                    int ind = FindInArray(serverKeys, mark);
                    serverPrice[ind] += price;
                }
                else if (computers[i].GetType() == typeof(WorkStation))
                {
                    int ind = FindInArray(workStationKeys, mark);
                    workStationPrice[ind] += price;
                }
                else
                {
                    int ind = FindInArray(computerKeys, mark);
                    computerPrice[ind] += price;
                }
            }

            Console.WriteLine("Computers:");
            for (int i = 0; i < computerKeys.Length; i++)
            {
                if (computerKeys[i] == null)
                {
                    break;
                }
                Console.WriteLine(computerKeys[i] + " - " + computerPrice[i]);
            }

            Console.WriteLine("Servers:");
            for (int i = 0; i < serverKeys.Length; i++)
            {
                if (serverKeys[i] == null)
                {
                    break;
                }
                Console.WriteLine(serverKeys[i] + " - " + serverPrice[i]);
            }

            Console.WriteLine("WorkStations:");
            for (int i = 0; i < workStationKeys.Length; i++)
            {
                if (workStationKeys[i] == null)
                {
                    break;
                }
                Console.WriteLine(workStationKeys[i] + " - " + workStationPrice[i]);
            }

            Console.WriteLine("-------------------------------------------------------------------------");
            //c
            for (int i = 0; i < computers.Length; i++)
            {
                if (computers[i].GetType() == typeof(Server))
                {
                    Server ser = (Server)computers[i];
                    Console.WriteLine(computers[i]);
                    Console.WriteLine("Total space: " + (ser.Drive + ser.Disk));
                }
            }

            Console.WriteLine("-------------------------------------------------------------------------");
            //d
            double max_value = 0;
            for (int i = 0; i < computers.Length; i++)
            {
                
                if (computers[i].GetType() == typeof(WorkStation))
                {
                    WorkStation ws = (WorkStation)computers[i];
                    if (ws.Diagonal > max_value)
                    {
                        max_value = ws.Diagonal;

                    }
                }
            }

            for (int i = 0; i < computers.Length; i++)
            {

                if (computers[i].GetType() == typeof(WorkStation))
                {
                    WorkStation ws = (WorkStation)computers[i];
                    if (ws.Diagonal == max_value)
                    {
                        Console.WriteLine(ws);

                    }
                }
            }

        }
    }
}