// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Features.Psps.Commands;

using SG.Common;

namespace SmartAdmin.WebUI.HttpHandlers
{
    public class FileUploadHandlers
    {
        public async Task<Result<bool>> Upload(ISender mediator, MemoryStream stream, string fileName, int pspId)
        {
            try
            {
                var data = stream.ToArray();

                var bronzeCommand = new ImportBronzeCsvCommand
                {
                    FileName = fileName,
                    Data = data,
                    PspId = pspId
                };
                var bronzeResult = await mediator.Send(bronzeCommand);

                if (bronzeResult.Failed)
                {
                    return Result<bool>.CreateFailure(bronzeResult.Message);
                }

                // todo: a message should be pushed to a queue that asynchronously takes this file, processes it and notifies the UI back when done
                // 
                var silverCommand = new ImportSilverCsvCommand
                {
                    FileName = fileName,
                    Data = data,
                    PspId = pspId,
                    RawPaymentId = bronzeResult.Value
                };
                var silverResult = await mediator.Send(silverCommand);

                if (silverResult.Failed)
                {
                    return Result<bool>.CreateFailure(silverResult.Message);
                }

                return Result<bool>.CreateSuccess(true, $"File '{fileName}' uploaded successfully.");
            }
            catch (Exception ex)
            {
                // todo: come up with a proper way of doing this
                return Result<bool>.CreateFailure(ex.Message);
            }
        }
    }
}
