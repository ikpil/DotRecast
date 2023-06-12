namespace DotRecast.Recast.DemoTool.Tools
{
    public class CrowdToolImpl : ISampleTool
    {
        public string GetName()
        {
            return "Crowd";
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