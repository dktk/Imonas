namespace PspConnectors.Methods.Paysage
{
    public class PaysageConfig
    {
        private PaySageConfigSecret[] secrets;

        public class PaySageConfigSecret
        {
            public string Secret { get; set; }  
            public string ShopId { get; set; }
        }
        
        public string ReportUrl { get; set; }
        public string ApiVersion { get; set; }
        public PaySageConfigSecret[] Secrets 
        { 
            get => secrets; 
            set
            {
                var distinctSecrets = value.DistinctBy(x => x.Secret).Count();
                var distinctShops = value.DistinctBy(x => x.ShopId).Count();

                if (distinctSecrets > value.Length)
                {
                    throw new Exception("The PairingsConfig section contains duplicate Secret Pairings.");
                }

                if (distinctShops > value.Length)
                {
                    throw new Exception("The PairingsConfig section contains duplicate ShopId Pairings.");
                }

                secrets = value;
            }
        }
    }
}