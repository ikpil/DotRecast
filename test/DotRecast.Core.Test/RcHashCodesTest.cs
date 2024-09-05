using NUnit.Framework;

namespace DotRecast.Core.Test;

public class RcHashCodesTest
{
    [Test]
    public void TestCombineHashCodes()
    {
        Assert.That(RcHashCodes.CombineHashCodes(0, 0), Is.EqualTo(0));
        Assert.That(RcHashCodes.CombineHashCodes(int.MaxValue, int.MaxValue), Is.EqualTo(32));
        Assert.That(RcHashCodes.CombineHashCodes(int.MaxValue, int.MinValue), Is.EqualTo(-33));
        Assert.That(RcHashCodes.CombineHashCodes(int.MinValue, int.MinValue), Is.EqualTo(0));
        Assert.That(RcHashCodes.CombineHashCodes(int.MinValue, int.MaxValue), Is.EqualTo(-1));
        Assert.That(RcHashCodes.CombineHashCodes(int.MaxValue / 2, int.MaxValue / 2), Is.EqualTo(32));
    }

    [Test]
    public void TestIntHash()
    {
        Assert.That(RcHashCodes.WangHash(0), Is.EqualTo(4158654902));
        Assert.That(RcHashCodes.WangHash(1), Is.EqualTo(357654460));
        Assert.That(RcHashCodes.WangHash(2), Is.EqualTo(715307540));
        Assert.That(RcHashCodes.WangHash(3), Is.EqualTo(1072960876));
        
        Assert.That(RcHashCodes.WangHash(4), Is.EqualTo(1430614333));
        Assert.That(RcHashCodes.WangHash(5), Is.EqualTo(1788267159));
        Assert.That(RcHashCodes.WangHash(6), Is.EqualTo(2145921005));
        Assert.That(RcHashCodes.WangHash(7), Is.EqualTo(2503556531));
        
        Assert.That(RcHashCodes.WangHash(8), Is.EqualTo(2861226262));
        Assert.That(RcHashCodes.WangHash(9), Is.EqualTo(3218863982));
        Assert.That(RcHashCodes.WangHash(10), Is.EqualTo(3576533554));
        Assert.That(RcHashCodes.WangHash(11), Is.EqualTo(3934169234));
        
        //
        Assert.That(RcHashCodes.WangHash(int.MaxValue), Is.EqualTo(1755403298));
        Assert.That(RcHashCodes.WangHash(uint.MaxValue), Is.EqualTo(3971045735));
    }
}