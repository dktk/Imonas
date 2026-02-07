// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using PspConnectors.Configs;

using Microsoft.Extensions.Options;

using static PspConnectors.Configs.RunConfigs;

namespace Imonas.Exchange.Consumers.Methods
{
    public class MethodSettingsService(IOptions<RunConfigs> configs)
    {
        public PairingsConfig[] Get(string name) => configs.Value.Pairings;
    }
}
