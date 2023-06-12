namespace DotRecast.Recast.DemoTool.Tools
{
    public class ConvexVolumeToolImpl : ISampleTool
    {
        
        public string GetName()
        {
            return "Create Convex Volumes";
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