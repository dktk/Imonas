
using FluentAssertions;
using System.Threading.Tasks;
using NUnit.Framework;
using Application.Customers.Commands.Delete;
using Application.Common.Exceptions;
using Application.Customers.Commands.AddEdit;
using Domain.Entities;

namespace Imonas.Application.IntegrationTests.Customers.Commands
{
    using static Testing;

    public class DeleteCustomerTests : TestBase
    {
        [Test]
        public void ShouldRequireValidCustomerId()
        {
            var command = new DeleteCustomerCommand { Id = 99 };

            FluentActions.Invoking(() =>
                SendAsync(command)).Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task ShouldDeleteCustomer()
        {
            var result = await SendAsync(new AddEditCustomerCommand
            {
                Name = "Name",
                NameOfEnglish = "NameOfEnglish",
                GroupName = "GroupName",
                Region = "Region",
                Sales = "Sales",
                RegionSalesDirector = "RegionSalesDirector",
                PartnerType = "IC"
            });
            await SendAsync(new DeleteCustomerCommand
            {
                Id = result.Value
            });

            var item = await FindAsync<Customer>(result.Value);

            item.Should().BeNull();
        }
        [Test]
        public async Task ShouldDeleteCheckedCustomers()
        {
            var result1 = await SendAsync(new AddEditCustomerCommand
            {
                Name = "Name",
                NameOfEnglish = "NameOfEnglish",
                GroupName = "GroupName",
                Region = "Region",
                Sales = "Sales",
                RegionSalesDirector = "RegionSalesDirector",
                PartnerType = "IC"
            });
            var result2 = await SendAsync(new AddEditCustomerCommand
            {
                Name = "Name",
                NameOfEnglish = "NameOfEnglish",
                GroupName = "GroupName",
                Region = "Region",
                Sales = "Sales",
                RegionSalesDirector = "RegionSalesDirector",
                PartnerType = "IC"
            });
            await SendAsync(new DeleteCheckedCustomersCommand
            {
                Id = new int[] {result1.Value,result2.Value}
            });

            var item = await FindAsync<Customer>(result1.Value);
            item.Should().BeNull();
        }
    }
}
