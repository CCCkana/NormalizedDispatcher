using System;
using System.Linq;

namespace NormalizedDispatcher.Core
{
    public class Constraint
    {
        private readonly Loader _loader = new();

        public double DeltaH(double q)
        {
            return 2.08 * q * q / 100000;
        }

        public double Volume2Level(double v)
        {
            var data = _loader.LoadUpstreamInfo();
            var deltas = data.OrderBy(d => Math.Pow(d.V - v, 2)).Take(2).ToList();
            var a = deltas[0];
            var b = deltas[1];
            return (a.Z - b.Z) / (a.V - b.V) * (v - a.V) + a.Z;
        }

        public double Level2Volume(double z)
        {
            var data = _loader.LoadUpstreamInfo();
            var deltas = data.OrderBy(d => Math.Pow(d.Z - z, 2)).Take(2).ToList();
            var a = deltas[0];
            var b = deltas[1];
            return (a.V - b.V) / (a.Z - b.Z) * (z - a.Z) + a.V;
        }

        public double GetDownstreamLevel(double q)
        {
            var data = _loader.LoadDownstreamInfo();
            var deltas = data.OrderBy(d => Math.Pow(d.Q - q, 2)).Take(2).ToList();
            var a = deltas[0];
            var b = deltas[1];
            return (a.Z - b.Z) / (a.Q - b.Q) * (q - a.Q) + a.Z;
        }

        public double GetMaxN(double h)
        {
            var data = _loader.LoadNLimit();
            var deltas = data.OrderBy(d => Math.Pow(d.H - h, 2)).Take(2).ToList();
            var a = deltas[0];
            var b = deltas[1];
            return (a.N - b.N) / (a.H - b.H) * (h - a.H) + a.N;
        }
    }
}