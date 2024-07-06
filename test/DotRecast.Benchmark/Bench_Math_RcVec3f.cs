using System.Numerics;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using DotRecast.Core.Numerics;

namespace CSharpBencchmark
{
    /*
    */
    public class Bench_Math_RcVec3f
    {
        Consumer _consumer = new();

        [Benchmark]
        public void Dot_Vector3()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(1, 2, 3);
            var v = Vector3.Dot(v1, v2);
            _consumer.Consume(v);
        }

        [Benchmark]
        public void Dot_RcVec3f()
        {
            var v1 = new RcVec3f(1, 2, 3);
            var v2 = new RcVec3f(1, 2, 3);
            var v = RcVec3f.Dot(v1, v2);
            _consumer.Consume(v);
        }

        [Benchmark]
        public void Cross_Vector3()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(1, 2, 3);
            var v = Vector3.Cross(v1, v2);
            _consumer.Consume(v);
        }

        [Benchmark]
        public void Cross_RcVec3f()
        {
            var v1 = new RcVec3f(1, 2, 3);
            var v2 = new RcVec3f(1, 2, 3);
            var v = RcVec3f.Cross(v1, v2);
            _consumer.Consume(v);
        }

        [Benchmark]
        public void Normalize_Vector3()
        {
            var v1 = new Vector3(1, 2, 3);
            var v = Vector3.Normalize(v1);
            _consumer.Consume(v);
        }

        [Benchmark]
        public void Normalize_RcVec3f()
        {
            var v1 = new RcVec3f(1, 2, 3);
            var v = RcVec3f.Normalize(v1);
            _consumer.Consume(v);
        }
    }
}
