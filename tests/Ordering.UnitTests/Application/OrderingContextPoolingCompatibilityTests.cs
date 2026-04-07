using System.Reflection;
using Microsoft.EntityFrameworkCore;
using eShop.Ordering.Infrastructure;

namespace eShop.Ordering.UnitTests.Application;

[TestClass]
public class OrderingContextPoolingCompatibilityTests
{
    [TestMethod]
    public void OrderingContext_Should_HaveSinglePublicOptionsConstructor_ForDbContextPooling()
    {
        ConstructorInfo[] constructors = typeof(OrderingContext).GetConstructors(BindingFlags.Instance | BindingFlags.Public);

        Assert.HasCount(1, constructors, "DbContext pooling requires a single public constructor.");

        var parameters = constructors[0].GetParameters();
        Assert.HasCount(1, parameters, "DbContext pooling requires a single DbContextOptions parameter.");
        Assert.AreEqual(typeof(DbContextOptions<OrderingContext>), parameters[0].ParameterType);
    }
}
