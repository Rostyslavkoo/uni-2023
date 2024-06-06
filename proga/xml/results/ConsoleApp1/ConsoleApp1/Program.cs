class Program()
{
    public static void Main(string[] args)
    {
        Data data = new Data();

        data.Load1("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/ConsoleApp1/ConsoleApp1/input/input1.xml");
        data.Load2("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/ConsoleApp1/ConsoleApp1/input/input2.xml");
        data.Load3("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/ConsoleApp1/ConsoleApp1/input/input3.xml");

        data.TaskA("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/ConsoleApp1/ConsoleApp1/output/output1.xml");
        data.TaskB("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/ConsoleApp1/ConsoleApp1/output/output2.xml");
        data.TaskC("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/ConsoleApp1/ConsoleApp1/output/output3.xml");
    }
}