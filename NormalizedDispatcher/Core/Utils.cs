using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NormalizedDispatcher.Core
{
    public record StaticParameters
    {
        public double K = 8.6;
        public double ZCommonMax = 780.0;
        public double ZFloodMax = 773.1;
        public double ZMin = 731.0;
        public double HMin = 83.0;
        public double HMax = 143;
        public double NormalFlow = 301.2;
    }

    public class Loader
    {
        private readonly string _data;
        private readonly string _root = Directory.GetCurrentDirectory();

        public Loader()
        {
            _data = Path.Combine(_root, "Data");
        }

        public List<UpstreamWaterLevel> LoadUpstreamInfo()
        {
            using var reader = new StreamReader(Path.Combine(_data, "Z_V.txt"));
            string line;
            var data = new List<UpstreamWaterLevel>();
            while ((line = reader.ReadLine()) != null) data.Add(new UpstreamWaterLevel(line));

            return data;
        }

        public List<DownstreamWaterLevel> LoadDownstreamInfo()
        {
            using var reader = new StreamReader(Path.Combine(_data, "Q_Z.txt"));
            string line;
            var data = new List<DownstreamWaterLevel>();
            while ((line = reader.ReadLine()) != null) data.Add(new DownstreamWaterLevel(line));

            return data;
        }


        public List<NMaxLimit> LoadNLimit()
        {
            using var reader = new StreamReader(Path.Combine(_data, "N_MAX_LIMITS.txt"));
            string line;
            List<NMaxLimit> data = new();
            while ((line = reader.ReadLine()) != null) data.Add(new NMaxLimit(line));

            return data;
        }

        private List<FlowData> ReBaseFlowData(List<FlowData> flows)
        {
            List<FlowData> result = new();
            foreach (var f in flows)
            {
                const int delta = 10;
                double value = f.Value; 
                var now = f.Date;
                var next = now.AddDays(delta);
                var nextNext = next.AddDays(delta);
                result.Add(f);
                result.Add(new FlowData(next,value));
                result.Add(new FlowData(nextNext,value));
            }

            return result;
        }

        private List<List<FlowData>> SplitIntoYears(List<FlowData> data)
        {
            List<List<FlowData>> result = new();
            var upLimit = new DateTime(data[0].Date.Year+1,5,day:1);
            var downLimit = upLimit.AddYears(1);
            while (upLimit > data.Max(x => x.Date))
            {
                var items = data
                    .Where(x => x.Date > downLimit && x.Date < upLimit)
                    .Select(x => x);
                result.Add(items.ToList());
            }
            return result;
        }
        public List<FlowData> LoadDesignedFlowData()
        {
            using var reader = new StreamReader(Path.Combine(_data, "DESIGNED_FLOW.txt"));
            string line;
            var data = new List<FlowData>();
            while ((line = reader.ReadLine()) != null)
                if (!Regex.IsMatch(line, @"\w*"))
                {
                    var a = line.Split("\t")[0];
                    var b = line.Split("\t")[1];
                    data.Add(new FlowData($"2020-{a}-01", b));
                }
            return ReBaseFlowData(data);
        }

        public List<List<FlowData>> LoadHistoryFlowData()
        {
            using var reader = new StreamReader(Path.Combine(_data, "HIST_FLOW.txt"));
            string line;
            var data = new List<FlowData>();
            while ((line = reader.ReadLine()) != null)
            {
                var items = line.Split("\t");
                var year = items[0];
                for (var i = 1; i <= 12; i++)
                    if (!Regex.IsMatch(line, @"\w*"))
                        data.Add(new FlowData($"{year}-{i}-01", items[i]));
            }

            return SplitIntoYears(ReBaseFlowData(data));
        }

        public record FlowData
        {
            public DateTime Date;
            public double Value;

            public FlowData(string date, string value)
            {
                Date = DateTime.Parse(date);
                Value = double.Parse(value);
            }

            public FlowData(DateTime date, double value)
            {
                Date = date;
                Value = value;
            }
        }

        public class UpstreamWaterLevel
        {
            public double V;
            public double Z;

            public UpstreamWaterLevel(string input)
            {
                var items = input.Split("\t");
                Z = double.Parse(items[1]);
                V = double.Parse(items[2]);
            }
        }

        public class DownstreamWaterLevel
        {
            public double Q;
            public double Z;

            public DownstreamWaterLevel(string input)
            {
                var items = input.Split("\t");
                Z = double.Parse(items[1]);
                Q = double.Parse(items[2]);
            }
        }

        public class NMaxLimit
        { 
            public double H;
            public double N;

            public NMaxLimit(string input)
            {
                var items = input.Split("\t");
                H = double.Parse(items[1]);
                N = double.Parse(items[2]);
            }
        }
    }
}