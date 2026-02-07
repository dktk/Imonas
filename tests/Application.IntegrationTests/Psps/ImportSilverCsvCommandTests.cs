using System;
using System.IO;
using System.Threading.Tasks;

using Application.Features.Psps.Commands;

using Domain.Entities;
using Domain.Entities.MedalionData.Bronze;
using Domain.Entities.MedalionData.Silver;

using FluentAssertions;

using FluentValidation;

using Imonas.Application.IntegrationTests;
using NUnit.Framework;

using PspConnectors.Methods.Btcbit;

namespace Application.IntegrationTests.Psps
{
    using static Testing;

    public class ImportSilverCsvCommandTests() : TestBase
    {
        private byte[] _testCsvContent;

        [SetUp]
        public async Task Setup()
        {
            await TestSetUp();

            // Try to load the sample CSV file from the test project
            var csvPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Uploads", "Btcbit", "orders.csv");

            if (File.Exists(csvPath))
            {
                _testCsvContent = File.ReadAllBytes(csvPath);
            }
            else
            {
                // Fallback: Create minimal CSV content for testing
                _testCsvContent = GetDummyCsvContent();
            }
        }

        public static byte[] GetDummyCsvContent()
        {
            // todo: the dummy.csv needs to be trimmed down ... it is too big 
            var csvContent = File.ReadAllText("./psps/btcbit.csv");

            return System.Text.Encoding.UTF8.GetBytes(csvContent);
        }

        [Test]
        public async Task ShouldRequireFileName()
        {
            var command = new ImportSilverCsvCommand
            {
                FileName = "",
                Data = _testCsvContent,
                PspId = 1,
                RawPaymentId = 1
            };

            await FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task ShouldRequireFileData()
        {
            var command = new ImportSilverCsvCommand
            {
                FileName = "test.csv",
                Data = null,
                PspId = 1,
                RawPaymentId = 1
            };

            await FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task ShouldRequireNonEmptyFile()
        {
            var command = new ImportSilverCsvCommand
            {
                FileName = "test.csv",
                Data = Array.Empty<byte>(),
                PspId = 1,
                RawPaymentId = 1
            };

            await FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task ShouldRejectFileLargerThan5MB()
        {
            var largeData = new byte[6 * 1024 * 1024]; // 6MB

            var command = new ImportSilverCsvCommand
            {
                FileName = "large.csv",
                Data = largeData,
                PspId = 1,
                RawPaymentId = 1
            };

            await FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task ShouldRequireValidPspId()
        {
            var command = new ImportSilverCsvCommand
            {
                FileName = "test.csv",
                Data = _testCsvContent,
                PspId = 0,
                RawPaymentId = 1
            };

            await FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task ShouldRequireValidRawPaymentId()
        {
            var command = new ImportSilverCsvCommand
            {
                FileName = "test.csv",
                Data = _testCsvContent,
                PspId = 1,
                RawPaymentId = 0
            };

            await FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task ShouldFailWhenPspNotFound()
        {
            var command = new ImportSilverCsvCommand
            {
                FileName = "test.csv",
                Data = _testCsvContent,
                PspId = 99999,
                RawPaymentId = 1
            };

            var result = await SendAsync(command);

            result.Success.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Contains("No PSP exists with ID"));
        }

        [Test]
        public async Task ShouldImportCsvForCsvBasedPsp()
        {
            var internalSystem = new InternalSystem
            {
                Name = "Omega"
            };

            // Arrange: Create a CSV-based PSP
            var pspId = await AddAsync(new Psp
            {
                Name = BtcbitService.PspName,
                IsCsvBased = true,
                IsActive = true,
                InternalSystem = internalSystem
            });

            // Create a RawPayment record
            var rawPaymentId = await AddAsync(new RawPayment
            {
                RawContent = _testCsvContent,
                FileName = "./uploads/btcbit/orders.csv",
                FileHash = "h1",
                PspId = 1,
                FileSizeBytes = 10
            });

            var command = new ImportSilverCsvCommand
            {
                FileName = "orders.csv",
                Data = _testCsvContent,
                PspId = pspId,
                RawPaymentId = rawPaymentId,
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();
            result.Value.Should().BeTrue();

            var paymentsCount = await CountAsync<ExternalPayment>();
            paymentsCount.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task ShouldHandleEmptyCSV()
        {
            var internalSystem = new InternalSystem
            {
                Name = "Omega"
            };

            // Arrange
            var pspId = await AddAsync(new Psp
            {
                Name = BtcbitService.PspName,
                IsCsvBased = true,
                IsActive = true,
                InternalSystem = internalSystem
            });

            var rawPaymentId = await AddAsync(new RawPayment
            {
                RawContent = _testCsvContent,
                FileName = "empty.csv",
                FileHash = "h1",
                PspId = 1,
                FileSizeBytes = 10
            });

            var emptyCsvContent = System.Text.Encoding.UTF8.GetBytes("Header1,Header2\n");

            var command = new ImportSilverCsvCommand
            {
                FileName = "empty.csv",
                Data = emptyCsvContent,
                PspId = pspId,
                RawPaymentId = rawPaymentId
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Failed.Should().BeTrue();
            //result.Errors.Should().Contain(e => e == importSilverCsvCommandHandlerLocalizer.GetString("Starting CSV processing flow."));
        }

        [Test]
        public async Task ShouldLinkPaymentsToRawPayment()
        {
            var internalSystem = new InternalSystem
            {
                Name = "Omega"
            };

            // Arrange
            var pspId = await AddAsync(new Psp
            {
                Name = BtcbitService.PspName,
                IsCsvBased = true,
                IsActive = true,
                InternalSystem = internalSystem
            });

            var rawPaymentId = await AddAsync(new RawPayment
            {
                RawContent = _testCsvContent,
                FileName = "test.csv",
                FileHash = "h1",
                PspId = 1,
                FileSizeBytes = 10
            });

            var command = new ImportSilverCsvCommand
            {
                FileName = "test.csv",
                Data = _testCsvContent,
                PspId = pspId,
                RawPaymentId = rawPaymentId
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();

            var payments = await FindAsync<ExternalPayment>(rawPaymentId);
            payments.Should().NotBeNull();
            payments.RawPaymentId.Should().Be(rawPaymentId);
            payments.PspId.Should().Be(pspId);
        }

        [Test]
        public async Task ShouldMapCsvFieldsToPaymentProperties()
        {
            // Arrange
            var internalSystem = new InternalSystem
            {
                Name = "Omega"
            };

            var pspId = await AddAsync(new Psp
            {
                Name = BtcbitService.PspName,
                IsCsvBased = true,
                IsActive = true,
                InternalSystem = internalSystem
            });

            var rawPaymentId = await AddAsync(new RawPayment
            {
                RawContent = _testCsvContent,
                FileName = "test.csv",
                FileHash = "h1",
                PspId = 1,
                FileSizeBytes = 10
            });

            var command = new ImportSilverCsvCommand
            {
                FileName = "test.csv",
                Data = _testCsvContent,
                PspId = pspId,
                RawPaymentId = rawPaymentId
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();

            var payment = await FindAsync<ExternalPayment>(rawPaymentId);
            payment.Should().NotBeNull();
            payment.Amount.Should().NotBe(0);
            payment.CurrencyCode.Should().NotBeNullOrEmpty();
            payment.PspId.Should().Be(pspId);
            payment.RawPaymentId.Should().Be(rawPaymentId);
        }
    }
}
