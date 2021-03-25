using NormalizedDispatcher.Core;

namespace NormalizedDispatcher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var data = new Loader().LoadHistoryFlowData();
            var calculator = new Calculator();
            const double n = 40.52*10000000;
            var normal = calculator.Process(data, n);

        }
    }
}