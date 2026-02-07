// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using SG.Common;

namespace Application.Common.Interfaces;

public interface IExcelService
{
    Task<byte[]> CreateTemplateAsync(IEnumerable<string> fields, string sheetName = "Sheet1");
    Task<byte[]> ExportAsync<TData>(IEnumerable<TData> data
        , Dictionary<string, Func<TData, object>> mappers
, string sheetName = "Sheet1");

    Task<Result<IEnumerable<TEntity>>> ImportAsync<TEntity>(byte[] data
        , Dictionary<string, Func<DataRow, TEntity, object>> mappers
        , string sheetName = "Sheet1");
}
