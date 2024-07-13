namespace DotRecast.Recast
{
    public struct RcCompactSpanBuilder
    {
        public int y;
        public int reg;
        public int con;
        public int h;

        public static RcCompactSpanBuilder NewBuilder(ref RcCompactSpan span)
        {
            var builder = new RcCompactSpanBuilder
            {
                y = span.y,
                reg = span.reg,
                con = span.con,
                h = span.h
            };
            return builder;
        }

        public RcCompactSpanBuilder WithReg(int reg)
        {
            this.reg = reg;
            return this;
        }

        public RcCompactSpan Build()
        {
            return new RcCompactSpan(this);
        }
    }
}