namespace DotRecast.Recast.Toolset.Tools
{
    public class CrowdToolImpl : ISampleTool
    {
        private Sample _sample;
        
        public string GetName()
        {
            return "Create Crowd";
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