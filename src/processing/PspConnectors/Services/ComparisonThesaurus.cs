using Domain;

using PspConnectors.Methods;
using PspConnectors.Methods.Btcbit;
using PspConnectors.Methods.CubixPay;
using PspConnectors.Methods.Dns;
using PspConnectors.Methods.GumBallPay;
using PspConnectors.Methods.Noda.NodaCsv;
using PspConnectors.Methods.Noda.NodaPay;
using PspConnectors.Methods.Nummuspay;
using PspConnectors.Methods.Paysage;
using PspConnectors.Methods.Rastpay;
using PspConnectors.Methods.Skrill;
using PspConnectors.Sources.Omega;
using PspConnectors.Sources.SquarePay;

namespace PspConnectors.Services
{
    public class ComparisonThesaurus : IComparisonThesaurus
    {
        private readonly Dictionary<(string, SystemType), BaseMethodService> _externalSystems = new Dictionary<(string, SystemType), BaseMethodService>();
        private readonly Dictionary<(string, SystemType), ISourceService> _internalSystems = new Dictionary<(string, SystemType), ISourceService>();

        // This is an awesome name for a class
        public ComparisonThesaurus(

            RastPayService rastPayService,
            GumballPayService gumballPayService,
            CubixPayService cubixPayService,
            NodaPayCsvService nodapayService,
            NodaPayApiService nodaService,
            PaysageService paysageService,
            DnsService dnsService,
            BtcbitService btcbitService,
            SkrillService skrillService,

            OmegaSourceService omegaSourceService,
            SquarePaySourceService squarePaySourceService)
        {
            _internalSystems.Add((OmegaSourceService.Name, SystemType.API), omegaSourceService);
            _internalSystems.Add((SquarePaySourceService.Name, SystemType.API), squarePaySourceService);

            _externalSystems.Add((PaysageService.PspName, SystemType.API), paysageService);
            _externalSystems.Add((DnsService.PspName, SystemType.CSV), dnsService);
            _externalSystems.Add((NodaPayCsvService.PspName, SystemType.CSV), nodapayService);
            _externalSystems.Add((CubixPayService.PspName, SystemType.CSV), cubixPayService);
            _externalSystems.Add((GumballPayService.PspName, SystemType.CSV), gumballPayService);
            _externalSystems.Add((RastPayService.PspName, SystemType.API), rastPayService);
            _externalSystems.Add((BtcbitService.PspName, SystemType.CSV), btcbitService);
            _externalSystems.Add((SkrillService.PspName, SystemType.CSV), skrillService);
            _externalSystems.Add((NummusPayService.PspName, SystemType.API), skrillService);
            _externalSystems.Add((NodaPayApiService.PspName, SystemType.API), nodaService);
        }

        public IMethodService GetExternalService(string targetName, SystemType systemType) => _externalSystems.TryGetValue((targetName, systemType), out var value) ? value : throw new Exception($"No {targetName} service mapped");
        public ISourceService GetInternalService(string sourceName, SystemType systemType) => _internalSystems.TryGetValue((sourceName, systemType), out var value) ? value : throw new Exception($"No {sourceName} service mapped");
    }
}
