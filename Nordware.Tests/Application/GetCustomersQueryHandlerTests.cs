using FluentAssertions;
using NSubstitute;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Queries;
using Nordware.Domain.Entities;

namespace Nordware.Tests.Application;

public sealed class GetCustomersQueryHandlerTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly GetCustomersQueryHandler _handler;

    public GetCustomersQueryHandlerTests()
    {
        _customerRepository = Substitute.For<ICustomerRepository>();

        _handler = new GetCustomersQueryHandler(
            _customerRepository);
    }

    [Fact]
    public async Task Handle_WhenThereAreNoCustomers_ShouldReturnEmptyCollection()
    {
        // Arrange
        _customerRepository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var result = await _handler.Handle(
            new GetCustomersQuery(),
            CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapCustomersToResponses()
    {
        // Arrange
        var customers = new[]
        {
            new Customer(
                Guid.NewGuid(),
                "Customer 1"),

            new Customer(
                Guid.NewGuid(),
                "Customer 2")
        };

        _customerRepository
            .GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(customers);

        // Act
        var result = await _handler.Handle(
            new GetCustomersQuery(),
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        result[0].Id.Should().Be(customers[0].Id);
        result[0].Name.Should().Be("Customer 1");

        result[1].Id.Should().Be(customers[1].Id);
        result[1].Name.Should().Be("Customer 2");
    }
}
