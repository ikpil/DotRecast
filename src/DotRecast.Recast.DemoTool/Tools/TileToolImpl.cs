namespace DotRecast.Recast.DemoTool.Tools
{
    public class TileToolImpl : ISampleTool
    {
        private Sample _sample;
        
        public string GetName()
        {
            return "Create Tiles";
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