namespace PspConnectors.Configs
{
    public class RunConfigs
    {
        private PairingsConfig[] pairings;

        public class PairingsConfig
        {
            public string Source { get; set; }
            public string[] SourceMethodName { get; set; }
            public string Target { get; set; }
        }

        public required string SummaryPath {  get; set; }

        public string Source { get; set; }
        public string Target { get; set; }

        public required PairingsConfig[] Pairings 
        { 
            get => pairings; 
            set
            {
                var distinctSources = value.DistinctBy(x => x.SourceMethodName).Count();
                var distinctTargets = value.DistinctBy(x => x.Target).Count();

                if (distinctSources > value.Length)
                {
                    throw new Exception("The PairingsConfig section contains duplicate Source Pairings.");
                }

                if (distinctTargets > value.Length)
                {
                    throw new Exception("The PairingsConfig section contains duplicate Target Pairings.");
                }

                pairings = value;
            }
        }
    }
}
