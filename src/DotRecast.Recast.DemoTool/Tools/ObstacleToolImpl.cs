namespace DotRecast.Recast.DemoTool.Tools
{
    public class ObstacleToolImpl : ISampleTool
    {
        private Sample _sample;
        
        public string GetName()
        {
            return "Create Temp Obstacles";
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