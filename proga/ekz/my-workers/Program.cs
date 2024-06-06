class Program()
{
    public static void Main(string[] args)
    {
        Data data = new Data();

        data.Load1("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputBrigade.xml");
        data.Load2("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputWorkerxml.xml");
        data.Load3("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputTarrif.xml");
        data.Load4("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputItem.xml");
        var report1 = data.Load5("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputReport_1.xml");
        var report2 = data.Load5("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputReport_2.xml");
        data.Reports = report1.Concat(report2).ToList();
        

        data.TaskA("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/output/output1.xml");
        data.TaskB("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/output/output2.xml");
        //data.TaskC("output/output3.xml");
    }
}