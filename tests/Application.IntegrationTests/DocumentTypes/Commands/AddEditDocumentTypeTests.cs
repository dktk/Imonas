using Application.Common.Exceptions;
using Application.Documents.Commands.AddEdit;
using Application.DocumentTypes.Commands.AddEdit;
using Domain.Entities;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Imonas.Application.IntegrationTests.Documents.Commands
{
    using static Testing;

    public class AddEditDocumentTypeTests : TestBase
    {
        [Test]
        public void ShouldRequireMinimumFields()
        {
            var command = new AddEditDocumentTypeCommand();

            FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Test]
        public async Task ShouldCreateDocumentType()
        {
            var userId = await RunAsDefaultUserAsync();
            var command = new AddEditDocumentTypeCommand()
            {
                Name = "Word",
                Description = "For Test"
            };
            var result = await SendAsync(command);
            
            var item = await FindAsync<DocumentType>(result.Value);

            item.Should().NotBeNull();
            item.Id.Should().Be(result.Value);
            item.Name.Should().Be(command.Name);
            item.UserId.Should().Be(userId);
            item.Created.Should().BeCloseTo(DateTime.Now,new TimeSpan(0,0,10));
            item.LastModifiedBy.Should().BeNull();
            item.LastModified.Should().BeNull();
        }
    }
}
