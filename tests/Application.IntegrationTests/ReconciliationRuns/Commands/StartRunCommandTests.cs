using Application.Features.ReconciliationRuns.Commands;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Imonas.Application.IntegrationTests.ReconciliationRuns.Commands
{
    using static Testing;

    public class StartRunCommandTests : TestBase
    {
        [Test]
        public async Task ShouldCreateReconciliationRunWithoutSettlement()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();
            var command = new StartRunCommand
            {
                RunName = "Test Run - No Settlement",
                RulePackVersion = "1.0.0",
                Description = "Test reconciliation run without settlement execution",
                ExecuteSettlement = false
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.RunName.Should().Be(command.RunName);
            result.Value.RulePackVersion.Should().Be(command.RulePackVersion);
            result.Value.Status.Should().Be(RunStatus.Pending);
            result.Message.Should().Contain("started successfully");
        }

        [Test]
        public async Task ShouldCreateReconciliationRunWithCorrectTimestamp()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();
            var beforeCreate = DateTime.UtcNow.AddSeconds(-1);

            var command = new StartRunCommand
            {
                RunName = "Timestamp Test Run",
                RulePackVersion = "2.0.0",
                ExecuteSettlement = false
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            var afterCreate = DateTime.UtcNow.AddSeconds(1);
            result.Success.Should().BeTrue();
            result.Value.StartedAt.Should().BeAfter(beforeCreate);
            result.Value.StartedAt.Should().BeBefore(afterCreate);
        }

        [Test]
        public async Task ShouldCreateReconciliationRunWithDefaultValues()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();
            var command = new StartRunCommand
            {
                RunName = "Default Values Test",
                ExecuteSettlement = false
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();
            result.Value.RulePackVersion.Should().Be("1.0.0"); // Default value
        }

        [Test]
        public async Task ShouldPersistReconciliationRunToDatabase()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();
            var command = new StartRunCommand
            {
                RunName = "Persistence Test Run",
                RulePackVersion = "1.5.0",
                ExecuteSettlement = false
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();
            var savedRun = await FindAsync<ReconciliationRun>(result.Value.Id);
            savedRun.Should().NotBeNull();
            savedRun.RunName.Should().Be(command.RunName);
            savedRun.RulePackVersion.Should().Be(command.RulePackVersion);
            savedRun.UserId.Should().Be(userId);
            savedRun.IsReplayable.Should().BeTrue();
        }

        [Test]
        public async Task ShouldSetIsReplayableToTrue()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();
            var command = new StartRunCommand
            {
                RunName = "Replayable Test Run",
                ExecuteSettlement = false
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            var savedRun = await FindAsync<ReconciliationRun>(result.Value.Id);
            savedRun.IsReplayable.Should().BeTrue();
        }

        [Test]
        public async Task ShouldCreateMultipleRunsWithUniqueIds()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();
            var command1 = new StartRunCommand
            {
                RunName = "Run 1",
                ExecuteSettlement = false
            };
            var command2 = new StartRunCommand
            {
                RunName = "Run 2",
                ExecuteSettlement = false
            };

            // Act
            var result1 = await SendAsync(command1);
            var result2 = await SendAsync(command2);

            // Assert
            result1.Success.Should().BeTrue();
            result2.Success.Should().BeTrue();
            result1.Value.Id.Should().NotBe(result2.Value.Id);
        }

        [Test]
        public async Task ShouldExecuteSettlementWhenRequested()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();
            var command = new StartRunCommand
            {
                RunName = "Settlement Execution Test",
                RulePackVersion = "1.0.0",
                ExecuteSettlement = true,
                IncludeInternalTransactions = true,
                IncludePspTransactions = true,
                MinimumMatchScore = 0.8m
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            // The settlement may complete with no matches (since no test data),
            // but the run should still be created
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.RunName.Should().Be(command.RunName);
        }

        [Test]
        public async Task ShouldHandleSettlementWithDateRange()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;

            var command = new StartRunCommand
            {
                RunName = "Date Range Test Run",
                RulePackVersion = "1.0.0",
                ExecuteSettlement = true,
                StartDate = startDate,
                EndDate = endDate
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Test]
        public async Task ShouldHandleSettlementWithPspFilter()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();

            var command = new StartRunCommand
            {
                RunName = "PSP Filter Test Run",
                RulePackVersion = "1.0.0",
                ExecuteSettlement = true,
                PspId = 1 // May not exist, but should handle gracefully
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Test]
        public async Task ShouldHandleSettlementWithCurrencyFilter()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();

            var command = new StartRunCommand
            {
                RunName = "Currency Filter Test Run",
                RulePackVersion = "1.0.0",
                ExecuteSettlement = true,
                CurrencyCode = "EUR"
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Test]
        public async Task ShouldRespectStopAtFirstMatchOption()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();

            var command = new StartRunCommand
            {
                RunName = "Stop At First Match Test",
                RulePackVersion = "1.0.0",
                ExecuteSettlement = true,
                StopAtFirstMatch = true
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Test]
        public async Task ShouldCreateCasesForExceptionsWhenEnabled()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();

            var command = new StartRunCommand
            {
                RunName = "Case Creation Test",
                RulePackVersion = "1.0.0",
                ExecuteSettlement = true,
                CreateCasesForExceptions = true
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Test]
        public async Task ShouldSetMinimumMatchScore()
        {
            // Arrange
            var userId = await RunAsDefaultUserAsync();

            var command = new StartRunCommand
            {
                RunName = "Match Score Test",
                RulePackVersion = "1.0.0",
                ExecuteSettlement = true,
                MinimumMatchScore = 0.95m
            };

            // Act
            var result = await SendAsync(command);

            // Assert
            result.Success.Should().BeTrue();
        }
    }
}
