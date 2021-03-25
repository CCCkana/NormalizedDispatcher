using System;
using System.Collections.Generic;
using System.Linq;

namespace NormalizedDispatcher.Core
{
    public class Calculator
    {
        private readonly Constraint _constraint = new();
        private readonly StaticParameters _sp = new();

        private double ZLimit(DateTime date)
        {
            return date.Month > 9 ? _sp.ZCommonMax : _sp.ZFloodMax;
        }

        public List<AppResult> Process(List<List<Loader.FlowData>> flows, double output)
        {
            List<AppResult> result = new();
            List<List<AppResult>> results = new();

            foreach (var year in flows) results.Add(ProcessOneYear(year, output));

            for (var i = 0; i < results[0].Count; i++)
                result.Add(new AppResult(results[0][i].Tag, results.Max(x => x[i].Value)));

            return result;
        }

        public List<AppResult> ProcessDownLine(List<List<Loader.FlowData>> flows, double output)
        {
            List<AppResult> result = new();
            List<List<AppResult>> results = new();

            foreach (var year in flows) results.Add(ProcessOneYear(year, output));

            for (var i = 0; i < results[0].Count; i++)
                result.Add(new AppResult(results[0][i].Tag, results.Min(x => x[i].Value)));
            return result;
        }

        private List<AppResult> ProcessOneYear(List<Loader.FlowData> flows, double n)
        {
            var data = new AppResult[flows.Count];
            var waterLevel = _sp.ZMin;
            for (var i = 0; i < flows.Count; i++)
            {
                var date = flows[i].Date;
                if (waterLevel > ZLimit(date))
                {
                    data[i] = new AppResult(date, ZLimit(date));
                    break;
                }
                data[i] = new AppResult(date, waterLevel);
                var days = i == flows.Count - 1 ? 11 : (flows[i + 1].Date - date).Days + 1;
                waterLevel = Run(waterLevel, n, flows[i].Value, days);
            }

            waterLevel = _sp.ZMin;
            for (var i = flows.Count - 1; i >= 0; i--)
            {
                var date = flows[i].Date;
                data[i] = new AppResult(date, waterLevel);
                var days = i == flows.Count - 1 ? 11 : (flows[i + 1].Date - date).Days + 1;
                waterLevel = ReversedRun(waterLevel, n, flows[i].Value, days);
            }

            var result = data.ToList();
            InflateNullValues(result, flows);
            return result;
        }

        private double Run(double start, double n, double q, int days)
        {
            var volume = _constraint.Level2Volume(start);
            var flow = GuessFlow(start, n, q,days);
            return _constraint.Volume2Level((volume*1e8 + (q-flow) *24* 3600 * days)/1e8);
        }

        private double ReversedRun(double end, double n, double q, int days)
        {
            var volume = _constraint.Level2Volume(end);
            var flow = ReversedGuessFlow(end, n, q,days);
            return _constraint.Volume2Level((volume*1e8 + (flow - q) *24* 3600 * days)/1e8);
        }

        private void InflateNullValues(in List<AppResult> results, in List<Loader.FlowData> flows)
        {
            for (var i = 0; i < results.Count; i++)
                if (results[i].Tag == null)
                {
                    var date = flows[i].Date;
                    results[i] = new AppResult(date, ZLimit(date));
                }
        }

        private double GuessFlow(double level, double n, double q,int days)
        {
            var flow = _sp.NormalFlow * 0.5;
            double n2=0;
            while (true)
            {
                var v2 = _constraint.Level2Volume(level) * 1e8 + (q - flow) * 24 * 3600 * days;
                var z1 = _constraint.Volume2Level((_constraint.Level2Volume(level) + v2) / 2 / 1e8);
                var z2 = _constraint.GetDownstreamLevel(flow);
                var deltaH = _constraint.DeltaH(flow);
                n2 = _sp.K * flow * (z1 - z2 - deltaH);
                if (Math.Abs(n2 - n) < Double.Epsilon)
                {
                    break;
                }
                else
                {
                    flow = flow - (n-n2) / _sp.K / (z1 - z2 - deltaH);
                }
            }
            return flow;
        }
        private double ReversedGuessFlow(double level, double n, double q,int days)
        {
            var flow = _sp.NormalFlow * 2;
            double n2=0;
            while (true)
            {
                var v2 = _constraint.Level2Volume(level) * 1e8 + (flow-q) * 24 * 3600 * days;
                var z1 = _constraint.Volume2Level((_constraint.Level2Volume(level) + v2) / 2 / 1e8);
                var z2 = _constraint.GetDownstreamLevel(flow);
                var deltaH = _constraint.DeltaH(flow);
                n2 = _sp.K * flow * (z1 - z2 - deltaH);
                if (Math.Abs(n2 - n) < Double.Epsilon)
                {
                    break;
                }
                else
                {
                    flow = flow - (n2 - n) / _sp.K / (z1 - z2 - deltaH);
                }
            }
            return flow;
        }

        public record AppResult
        {
            public string Tag;
            public double Value;

            public AppResult(DateTime date, double value)
            {
                Value = value;
                var status = date.Day < 10 ? "上旬" : date.Day < 20 ? "中旬" : "下旬";
                Tag = $"{date.Month}-{status}";
            }

            public AppResult(string tag, double value)
            {
                Value = value;
                Tag = tag;
            }
        }
    }
}