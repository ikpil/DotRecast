namespace DotRecast.Recast.DemoTool.Tools
{
    public class JumpLinkBuilderToolImpl : ISampleTool
    {
        public string GetName()
        {
            return "Annotation Builder";
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