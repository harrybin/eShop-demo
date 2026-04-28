namespace eShop.Ordering.UnitTests.Application;

using Microsoft.AspNetCore.Http.HttpResults;
using eShop.Ordering.API.Application.Queries;
using Order = eShop.Ordering.API.Application.Queries.Order;
using NSubstitute.ExceptionExtensions;

[TestClass]
public class OrdersWebApiTest
{
    private readonly IMediator _mediatorMock;
    private readonly IOrderQueries _orderQueriesMock;
    private readonly IIdentityService _identityServiceMock;
    private readonly ILogger<OrderServices> _loggerMock;

    public OrdersWebApiTest()
    {
        _mediatorMock = Substitute.For<IMediator>();
        _orderQueriesMock = Substitute.For<IOrderQueries>();
        _identityServiceMock = Substitute.For<IIdentityService>();
        _loggerMock = Substitute.For<ILogger<OrderServices>>();
    }

    [TestMethod]
    public async Task Cancel_order_with_requestId_success()
    {
        // Arrange
        _mediatorMock.Send(Arg.Any<IdentifiedCommand<CancelOrderCommand, bool>>(), default)
            .Returns(Task.FromResult(true));

        // Act
        var orderServices = new OrderServices(_mediatorMock, _orderQueriesMock, _identityServiceMock, _loggerMock);
        var result = await OrdersApi.CancelOrderAsync(Guid.NewGuid(), new CancelOrderCommand(1), CancellationToken.None, orderServices);

        // Assert
        Assert.IsInstanceOfType<Ok>(result.Result);
    }

    [TestMethod]
    public async Task Ship_order_with_requestId_success()
    {
        // Arrange
        _mediatorMock.Send(Arg.Any<IdentifiedCommand<ShipOrderCommand, bool>>(), default)
            .Returns(Task.FromResult(true));

        // Act
        var orderServices = new OrderServices(_mediatorMock, _orderQueriesMock, _identityServiceMock, _loggerMock);
        var result = await OrdersApi.ShipOrderAsync(Guid.NewGuid(), new ShipOrderCommand(1), CancellationToken.None, orderServices);

        // Assert
        Assert.IsInstanceOfType<Ok>(result.Result);

    }

    [TestMethod]
    public async Task Get_orders_success_with_default_query_values()
    {
        // Arrange
        var fakeDynamicResult = Enumerable.Empty<OrderSummary>();
        var userId = Guid.NewGuid().ToString();

        _identityServiceMock.GetUserIdentity()
            .Returns(userId);

        _orderQueriesMock.GetOrdersFromUserAsync(
                userId,
                Arg.Is<OrderHistoryQuery>(q =>
                    q.Status == null &&
                    q.FromDateUtc == null &&
                    q.ToDateUtc == null &&
                    q.PageNumber == 1 &&
                q.PageSize == 20),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(fakeDynamicResult));

        // Act
        var orderServices = new OrderServices(_mediatorMock, _orderQueriesMock, _identityServiceMock, _loggerMock);
        var result = await OrdersApi.GetOrdersByUserAsync(CancellationToken.None, new OrderHistoryQuery(), orderServices);

        // Assert
        Assert.IsInstanceOfType<Ok<IEnumerable<OrderSummary>>>(result);
        Assert.AreSame(fakeDynamicResult, result.Value);
    }

    [TestMethod]
    public async Task Get_orders_success_with_explicit_query_filters()
    {
        // Arrange
        var fakeDynamicResult = Enumerable.Empty<OrderSummary>();
        var userId = Guid.NewGuid().ToString();
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;
        var query = new OrderHistoryQuery
        {
            Status = eShop.Ordering.Domain.AggregatesModel.OrderAggregate.OrderStatus.Paid,
            FromDateUtc = fromDate,
            ToDateUtc = toDate,
            PageNumber = 2,
            PageSize = 10
        };

        _identityServiceMock.GetUserIdentity().Returns(userId);
        _orderQueriesMock.GetOrdersFromUserAsync(
                userId,
                Arg.Is<OrderHistoryQuery>(q =>
                    q.Status == query.Status &&
                    q.FromDateUtc == query.FromDateUtc &&
                    q.ToDateUtc == query.ToDateUtc &&
                    q.PageNumber == query.PageNumber &&
                q.PageSize == query.PageSize),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(fakeDynamicResult));

        // Act
        var orderServices = new OrderServices(_mediatorMock, _orderQueriesMock, _identityServiceMock, _loggerMock);
        var result = await OrdersApi.GetOrdersByUserAsync(CancellationToken.None, query, orderServices);

        // Assert
        Assert.IsInstanceOfType<Ok<IEnumerable<OrderSummary>>>(result);
        Assert.AreSame(fakeDynamicResult, result.Value);
    }

    [TestMethod]
    public async Task Get_order_success()
    {
        // Arrange
        var fakeOrderId = 123;
        var fakeDynamicResult = new Order { Timeline = [] };
        _orderQueriesMock.GetOrderAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(fakeDynamicResult));

        // Act
        var orderServices = new OrderServices(_mediatorMock, _orderQueriesMock, _identityServiceMock, _loggerMock);
        var result = await OrdersApi.GetOrderAsync(fakeOrderId, CancellationToken.None, orderServices);

        // Assert
        Assert.IsInstanceOfType<Ok<Order>>(result.Result);
        Assert.AreSame(fakeDynamicResult, ((Ok<Order>)result.Result).Value);
    }

    [TestMethod]
    public async Task Get_order_fails()
    {
        // Arrange
        var fakeOrderId = 123;
#pragma warning disable NS5003
        _orderQueriesMock.GetOrderAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Throws(new KeyNotFoundException());
#pragma warning restore NS5003

        // Act
        var orderServices = new OrderServices(_mediatorMock, _orderQueriesMock, _identityServiceMock, _loggerMock);
        var result = await OrdersApi.GetOrderAsync(fakeOrderId, CancellationToken.None, orderServices);

        // Assert
        Assert.IsInstanceOfType<NotFound>(result.Result);
    }

    [TestMethod]
    public async Task Get_cardTypes_success()
    {
        // Arrange
        var fakeDynamicResult = Enumerable.Empty<CardType>();
        _orderQueriesMock.GetCardTypesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(fakeDynamicResult));

        // Act
        var result = await OrdersApi.GetCardTypesAsync(CancellationToken.None, _orderQueriesMock);

        // Assert
        Assert.IsInstanceOfType<Ok<IEnumerable<CardType>>>(result);
        Assert.AreSame(fakeDynamicResult, result.Value);
    }
}
