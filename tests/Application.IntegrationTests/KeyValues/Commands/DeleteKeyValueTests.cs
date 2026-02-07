
using FluentAssertions;
using System.Threading.Tasks;
using NUnit.Framework;
using Application.KeyValues.Commands.Delete;
using Application.Common.Exceptions;
using Application.KeyValues.Commands.AddEdit;
using Domain.Entities;

namespace Imonas.Application.IntegrationTests.KeyValues.Commands
{
    using static Testing;

    public class DeleteKeyValueTests : TestBase
    {
        [Test]
        public void ShouldRequireValidKeyValueId()
        {
            var command = new DeleteKeyValueCommand { Id = 99 };

            FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task ShouldDeleteKeyValue()
        {
            var addcommand = new AddEditKeyValueCommand()
            {
                Name = "Word",
                Text= "Word",
                Value = "Word",
                Description = "For Test"
            };
           var result= await SendAsync(addcommand);
             
            await SendAsync(new DeleteKeyValueCommand
            {
                Id = result.Value
            });

       

       
        }
         
    }
}
