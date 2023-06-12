namespace DotRecast.Recast.DemoTool.Tools
{
    public class OffMeshConnectionToolImpl : ISampleTool
    {
        public string GetName()
        {
            return "Create Off-Mesh Links";
        }
        
        private Sample _sample;
        public void SetSample(Sample sample)
        {
            _sample = sample;
        }

        public Sample GetSample()
        {
            return _sample;
        }

    }
}