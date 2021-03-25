using System.IO;
using System.Linq;
using NormalizedDispatcher.Core;

namespace NormalizedDispatcher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var data = new Loader().LoadHistoryFlowData();
            var calculator = new Calculator();
            const double n = 40.52 * 10000000;
            var normal = calculator.Process(data, n);
            var d8 = calculator.Process(data, n * 0.8);
            var e2 = calculator.Process(data, n * 1.2);
            var e4 = calculator.Process(data, n * 1.4);
            var e6 = calculator.Process(data, n * 1.6);
            var normalDown = calculator.ProcessDownLine(data, n);
            var path = new Loader().GetContentPath();
            using StreamWriter writer = new(Path.Combine(path, "result.csv"));
            var sep = ",";
            writer.WriteLine(string.Join(sep, "NormalUp", "NormalDown", "Down0.8", "Up1.2", "Up1.4", "Up1.6"));
            for (var i = 0; i < normal.Count; i++)
            {
                Calculator.AppResult[] values = {normal[i], normalDown[i], d8[i], e2[i], e4[i], e6[i]};
                writer.Write(values[0].Tag);
                writer.Write(sep);
                writer.Write(string.Join(sep, values.Select(x => x.Value).ToList()));
                writer.Write("\n");
            }

            writer.Close();
        }
    }
}