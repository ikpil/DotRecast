namespace DotRecast.Recast.DemoTool.Tools
{
    public class OffMeshConnectionToolImpl : ISampleTool
    {
        private Sample _sample;
        
        public string GetName()
        {
            return "Create Off-Mesh Links";
        }
        
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