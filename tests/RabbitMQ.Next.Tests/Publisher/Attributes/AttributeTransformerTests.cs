using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RabbitMQ.Next.Publisher;
using RabbitMQ.Next.Publisher.Attributes;
using Xunit;

[assembly:ApplicationId("unitTest")]

namespace RabbitMQ.Next.Tests.Publisher.Attributes;

public class AttributeTransformerTests
{
    [Fact]
    public async Task TestAssemblyAttributes()
    {
        var next = Substitute.For<IPublishMiddleware>();
        var message = Substitute.For<IMessageBuilder>();
        var content = new AssemblyAttributesData();
        
        var attributeTransformer = new AttributePublishMiddleware(next);

        await attributeTransformer.InvokeAsync(content, message, default);

        message.Received().SetApplicationId("unitTest");
        await next.Received().InvokeAsync(content, message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestRoutingKeyAttribute()
    {
        var next = Substitute.For<IPublishMiddleware>();
        var message = Substitute.For<IMessageBuilder>();
        var content = new RoutingKeyAttributeData();
        
        var attributeTransformer = new AttributePublishMiddleware(next);

        await attributeTransformer.InvokeAsync(content, message, default);

        message.Received().SetApplicationId("unitTest");
        message.Received().SetRoutingKey("route");
        await next.Received().InvokeAsync(content, message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestHeaderAttribute()
    {
        var next = Substitute.For<IPublishMiddleware>();
        var message = Substitute.For<IMessageBuilder>();
        var content = new HeaderAttributeData();
        
        var attributeTransformer = new AttributePublishMiddleware(next);

        await attributeTransformer.InvokeAsync(content, message, default);

        message.Received().SetApplicationId("unitTest");
        message.Received().SetHeader("headerA", "value1");
        message.Received().SetHeader("headerB", "value2");
        await next.Received().InvokeAsync(content, message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TestMultipleAttributes()
    {
        var next = Substitute.For<IPublishMiddleware>();
        var message = Substitute.For<IMessageBuilder>();
        var content = new MultipleAttributesData();
        
        var attributeTransformer = new AttributePublishMiddleware(next);

        await attributeTransformer.InvokeAsync(content, message, default);

        message.Received().SetApplicationId("unitTest");
        message.Received().SetRoutingKey("route");
        message.Received().SetPriority(7);
        await next.Received().InvokeAsync(content, message, Arg.Any<CancellationToken>());
    }

    private class AssemblyAttributesData
    {

    }

    [RoutingKey("route")]
    private class RoutingKeyAttributeData
    {
    }

    [Header("headerA", "value1")]
    [Header("headerB", "value2")]
    private class HeaderAttributeData
    {
    }

    [RoutingKey("route")]
    [Priority(7)]
    private class MultipleAttributesData
    {
    }
}