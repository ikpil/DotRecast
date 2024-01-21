using System;

namespace DotRecast.Core
{
    public class RcRand : IRcRand
    {
        private readonly Random _r;

        public RcRand()
        {
            _r = new Random();
        }

        public RcRand(long seed)
        {
            _r = new Random((int)seed); // TODO : 랜덤 시드 확인 필요
        }

        public float Next()
        {
            return (float)_r.NextDouble();
        }

        public int NextInt32()
        {
            return _r.Next();
        }
    }
}