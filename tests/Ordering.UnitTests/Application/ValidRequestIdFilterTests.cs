namespace eShop.Ordering.UnitTests.Application;

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

[TestClass]
public class ValidRequestIdFilterTests
{
    [TestMethod]
    public async Task Returns_bad_request_for_empty_guid()
    {
        // Arrange
        var sut = new ValidRequestIdFilter();
        var context = Substitute.For<EndpointFilterInvocationContext>();
        context.Arguments.Returns(new List<object> { Guid.Empty });
        EndpointFilterDelegate next = _ => new ValueTask<object>(TypedResults.Ok());

        // Act
        var result = await sut.InvokeAsync(context, next);

        // Assert
        Assert.IsInstanceOfType<BadRequest<string>>(result);
    }

    [TestMethod]
    public async Task Calls_next_for_valid_guid()
    {
        // Arrange
        var sut = new ValidRequestIdFilter();
        var context = Substitute.For<EndpointFilterInvocationContext>();
        context.Arguments.Returns(new List<object> { Guid.NewGuid() });
        var nextCalled = false;

        EndpointFilterDelegate next = _ =>
        {
            nextCalled = true;
            return new ValueTask<object>(TypedResults.Ok());
        };

        // Act
        var result = await sut.InvokeAsync(context, next);

        // Assert
        Assert.IsTrue(nextCalled);
        Assert.IsInstanceOfType<Ok>(result);
    }
}
