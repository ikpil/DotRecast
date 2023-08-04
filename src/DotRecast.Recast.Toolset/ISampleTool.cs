namespace DotRecast.Recast.Toolset
{
    public interface ISampleTool
    {
        string GetName();
        void SetSample(Sample sample);
        Sample GetSample();
    }
}