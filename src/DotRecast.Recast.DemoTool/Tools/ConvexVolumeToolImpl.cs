namespace DotRecast.Recast.DemoTool.Tools
{
    public class ConvexVolumeToolImpl : ISampleTool
    {
        private Sample _sample;
        
        public string GetName()
        {
            return "Create Convex Volumes";
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