using Application.Common.Exceptions;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using System.Threading.Tasks;
using Application.KeyValues.Commands.SaveChanged;
using Application.KeyValues.DTOs;

namespace Imonas.Application.IntegrationTests.KeyValues.Commands
{
    using static Testing;

    public class SaveChangedKeyValuesTests : TestBase
    {
        [Test]
        public void ShouldRequireMinimumFields()
        {
            var command = new SaveChangedKeyValuesCommand();

            FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task ShouldCreateDocumentType()
        {
            var userId = await RunAsDefaultUserAsync();
            var command = new SaveChangedKeyValuesCommand()
            {
                Items = new KeyValueDto[] {
                    new KeyValueDto(){ Name="Name",Value="value1",Text="text1", TrackingState= Domain.Enums.TrackingState.Added },
                    new KeyValueDto(){ Name="Name",Value="value2",Text="text2", TrackingState= Domain.Enums.TrackingState.Added },
                    new KeyValueDto(){ Name="Name",Value="value3",Text="text3", TrackingState= Domain.Enums.TrackingState.Added }
                    }
            };
            var result = await SendAsync(command);
            
            var count = await CountAsync<KeyValue>();

            result.Success.Should().BeTrue();
            count.Should().Be(3);
        }
    }
}
