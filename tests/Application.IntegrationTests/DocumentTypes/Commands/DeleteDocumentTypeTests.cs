
using FluentAssertions;
using System.Threading.Tasks;
using NUnit.Framework;
using Application.Documents.Commands.Delete;
using Application.Common.Exceptions;
using Application.Documents.Commands.AddEdit;
using Domain.Entities;
using Application.DocumentTypes.Commands.AddEdit;
using Application.DocumentTypes.Commands.Delete;

namespace Imonas.Application.IntegrationTests.Documents.Commands
{
    using static Testing;

    public class DeleteDocumentTypeTests : TestBase
    {
        [Test]
        public void ShouldRequireValidDocumentTypeId()
        {
            var command = new DeleteDocumentTypeCommand { Id = 99 };

            FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task ShouldDeleteDocumentType()
        {
            var addcommand = new AddEditDocumentTypeCommand()
            {
                Name = "Word",
                Description = "For Test"
            };
           var result= await SendAsync(addcommand);
             
            await SendAsync(new DeleteDocumentTypeCommand
            {
                Id = result.Value
            });

            var item = await FindAsync<Document>(result.Value);

            item.Should().BeNull();
        }
         
    }
}
