using System;

namespace DotRecast.Core
{
    public class FRand
    {
        private readonly Random r;

        public FRand()
        {
            r = new Random();
        }

        public FRand(long seed)
        {
            r = new Random((int)seed); // TODO : 랜덤 시드 확인 필요
        }

        public float Frand()
        {
            return (float)r.NextDouble();
        }
    }
}