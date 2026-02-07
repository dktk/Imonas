// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Domain
{
    public enum SystemType { CSV, API };

    public static class SystemTypeMapper
    {
        public static SystemType Map(bool isCsvBased) => isCsvBased ? SystemType.CSV : SystemType.API;
    }

    public interface ITargetThesaurus
    {
        IMethodService GetExternalService(string targetName, SystemType systemType);
    }

    public interface IComparisonThesaurus : ITargetThesaurus
    {
        ISourceService GetInternalService(string sourceName, SystemType systemType);
    }
}
