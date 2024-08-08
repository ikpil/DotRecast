﻿using System;

namespace DotRecast.Core
{
    public class RcRand : IRcRand
    {
        private readonly Random _r;

        public RcRand() : this(new Random())
        {
        }

        public RcRand(Random r)
        {
            _r = r;
        }

        public RcRand(long seed)
        {
            _r = new Random((int)seed); // TODO : 랜덤 시드 확인 필요
        }

        public float Next()
        {
            return (float)_r.NextDouble();
        }

        public double NextDouble()
        {
            return _r.NextDouble();
        }

        public int NextInt32()
        {
            return _r.Next();
        }
    }
}