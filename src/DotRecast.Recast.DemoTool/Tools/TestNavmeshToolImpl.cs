namespace DotRecast.Recast.DemoTool.Tools
{
    public class TestNavmeshToolImpl : ISampleTool
    {
        private Sample _sample;
        private readonly TestNavmeshToolOption _option;

        public TestNavmeshToolImpl()
        {
            _option = new TestNavmeshToolOption();
        }

        public string GetName()
        {
            return "Test Navmesh";
        }


        public void SetSample(Sample sample)
        {
            _sample = sample;
        }

        public Sample GetSample()
        {
            return _sample;
        }

        public TestNavmeshToolOption GetOption()
        {
            return _option;
        }
    }
}