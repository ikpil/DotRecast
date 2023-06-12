namespace DotRecast.Recast.DemoTool.Tools
{
    public class TestNavmeshToolImpl : ISampleTool
    {
        public string GetName()
        {
            return "Test Navmesh";
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