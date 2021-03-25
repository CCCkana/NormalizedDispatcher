using System;
using System.Collections.Generic;

namespace NormalizedDispatcher.Core
{
    public class Calculator
    {
        private readonly Constraint _constraint = new();
        private readonly StaticParameters _sp = new();

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
        }

        private double WaterLevelLimit(DateTime date)
        {
            return date.Month > 9 ? _sp.ZCommonMax : _sp.ZFloodMax;
        }

        public List<AppResult> Process(List<List<Loader.FlowData>> flows, double output)
        {
            List<AppResult> result = new();
            List<List<AppResult>> results = new();
            
            foreach (var year in flows)
            {
                results.Add(ProcessOneYear(year,output));
            }

            return result;
        }

        private List<AppResult> ProcessOneYear(List<Loader.FlowData> flows,double n)
        {
            List<AppResult> result = new();
            var waterLevel = _sp.ZMin;
            for (int i = 0; i < flows.Count; i++)
            {
                var date = flows[i].Date;
                result.Add(new AppResult(date,waterLevel));
                int days = i == flows.Count - 1 ? 11 : (flows[i + 1].Date - date).Days+1;
                waterLevel = Run(waterLevel, n, flows[i].Value, days);
            }
            return result;
        }

        private double Run(double start, double n, double q,int days)
        {
            var volume = _constraint.Level2Volume(start);
            double flow = GuessFlow(start,n,q);
            return _constraint.Volume2Level(volume + (q - flow) * 3600 * days);
        }

        private double GuessFlow(double level, double n,double q)
        {
            var flow = _sp.NormalFlow*10;
            var h = CalcH(level,flow);
            while (Math.Abs(_sp.K * flow * h - n) > double.Epsilon&&(h>_sp.HMax||h<_sp.HMin))
            {
                flow -=0.1;
                h = CalcH(level,flow);
            }

            return flow;
        }

        private double CalcH(double level, double q)
        {
            return level-_constraint.GetDownstreamLevel(q) - _constraint.DeltaH(q);
        }
    }
}