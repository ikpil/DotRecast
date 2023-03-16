using System.Threading;

namespace DotRecast.Core
{


public class AtomicBoolean
{
    private volatile int _location;

    public bool set(bool v)
    {
        return 0 != Interlocked.Exchange(ref _location, v ? 1 : 0);
    }

    public bool get()
    {
        return 0 != _location;
    }
}
}