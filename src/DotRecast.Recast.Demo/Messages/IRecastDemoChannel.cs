namespace DotRecast.Recast.Demo.Messages;

public interface IRecastDemoChannel
{
    void SendMessage(IRecastDemoMessage message);
}