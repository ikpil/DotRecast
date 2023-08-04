namespace DotRecast.Recast.Toolset.Tools
{
    public class DynamicUpdateToolImpl : ISampleTool
    {
        public string GetName()
        {
            return "Dynamic Updates";
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