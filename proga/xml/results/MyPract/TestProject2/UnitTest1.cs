using System.Text.RegularExpressions;

namespace TestProject1
{
    public class TaskResultsProjectTests : IClassFixture<Data>
    {
        Data fixture;

        public TaskResultsProjectTests(Data fixture)
        {
            this.fixture = fixture;
            fixture.Load1("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/MyPract/MyPract/input/input1.xml");
            fixture.Load2("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/MyPract/MyPract/input/input2.xml");
            fixture.Load3("/Users/rostislavurdejcuk/My Drive/lnu/uni-2023/proga/xml/results/MyPract/MyPract/input/input3.xml");
        }

        [Fact]
        public void TestA()
        {
            var expected = new List<string>
            {
                "PMI-21 Egor Theme Name 1: 35 ",
                "PMI-21 Egor Theme Name 1: 35 ",
                "PMI-21 Ivan Theme Name 1: 50 ",
                "PMI-21 Oleksandr Theme Name 2: 10 "
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
                "PMI-21 Theme Name 1 Ivankiv 50",
                "PMI-21 Theme Name 1 Furman 70"   
            };
        
            var result = fixture.TaskB("");
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], result[i]);
            }
        }

        [Fact]
        public void TestC()
        {
            var expected = new List<string>
            {
                "PMI-21 Dudynets O. 10",
                "PMI-21 Furman E. 70",
                "PMI-21 Ivankiv I. 50",
            };
        
            var result = fixture.TaskC("");
        
            Assert.Equal(expected.Count, result.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], result[i]);
            }
        }

    }
}