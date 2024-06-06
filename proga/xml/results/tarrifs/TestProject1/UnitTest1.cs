using System.Text.RegularExpressions;

namespace TestProject1
{
    public class TaskResultsProjectTests : IClassFixture<Data>
    {
        Data fixture;

        public TaskResultsProjectTests(Data fixture)
        {
            this.fixture = fixture;
            fixture.Load1("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputBrigade.xml");
            fixture.Load2("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputWorkerxml.xml");
            fixture.Load3("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputTarrif.xml");
            fixture.Load4("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputItem.xml");
            var report1 = fixture.Load5("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputReport_1.xml");
            var report2 = fixture.Load5("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/tarrifs/tarrifs/input/inputReport_2.xml");
            fixture.Reports = report1.Concat(report2).ToList();

        }
        [Fact]
        public void TestA()
        {
            var expected = new List<string>
            {
                "Brigade 1 Ivanov 1400",
                "Brigade 1 Urdeichuk 600",
                "Brigade 2 Furman 400"

            };

            var result = fixture.TaskA("");

            Assert.Equal(expected.Count, result.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], result[i]);
            }
        }
        [Fact]
        public void TestB()
        {
            var expected = new List<string>
            {
                "Item_1 45",
                "Item_2 20",
            };

            var result = fixture.TaskB("");

            Assert.Equal(expected.Count, result.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], result[i]);
            }
        }
    }
}