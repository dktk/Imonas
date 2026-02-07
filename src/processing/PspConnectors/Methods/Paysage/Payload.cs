namespace PspConnectors.Methods.Paysage
{
    /*
     "report_params": {
          "date_type":"created_at",
          "from":"2025-02-09 00:00:00",
          "to":"2025-02-09 23:59:59",
          "status":"successful",  //  "must be one of the following: all, successful, failed, pending"
          "payment_method_type":"credit_card",
          "time_zone":"Etc/UTC"
        }
     */
    internal class Payload
    {
        // "must be one of the following: created_at, paid_at, settled_at"
        public string date_type { get; set; }
        public string from { get; set; }
        public string to { get; set; }

        // "must be one of the following: all, successful, failed, pending"
        public string status { get; set; }

        // "must be one of the following: credit_card, erip, alternative"
        public string payment_method_type { get; set; }

        public string time_zone { get; set; }
    }

    internal class PaysagePayload
    {
        public Payload report_params { get; set; }

        public static PaysagePayload Create(DateTime from, DateTime to)
        {
            return new PaysagePayload
            {
                report_params = new Payload
                {
                    date_type = "created_at",
                    from = from.PrettyDateTime(),
                    to = to.PrettyDateTime(),
                    status = "all",
                    payment_method_type = "credit_card",
                    time_zone = "Etc/UTC"
                }
            };
        }
    }
}