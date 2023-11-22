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
    public void TestAssemblyAttributes()
    {
        var message = this.CreateSubjectFor<AssemblyAttributesData>();
        
        PublisherAttributes.Apply(message);
        
        message.Received().SetApplicationId("unitTest");
    }

    [Fact]
    public void TestRoutingKeyAttribute()
    {
        var message = this.CreateSubjectFor<RoutingKeyAttributeData>();
        
        PublisherAttributes.Apply(message);

        message.Received().SetApplicationId("unitTest");
        message.Received().SetRoutingKey("route");
    }

    [Fact]
    public void TestHeaderAttribute()
    {
        var message = this.CreateSubjectFor<HeaderAttributeData>();
        
        PublisherAttributes.Apply(message);

        message.Received().SetApplicationId("unitTest");
        message.Received().SetHeader("headerA", "value1");
        message.Received().SetHeader("headerB", "value2");
    }

    [Fact]
    public void TestMultipleAttributes()
    {
        var message = this.CreateSubjectFor<MultipleAttributesData>();
        
        PublisherAttributes.Apply(message);

        message.Received().SetApplicationId("unitTest");
        message.Received().SetRoutingKey("route");
        message.Received().SetPriority(7);
    }

    private IMessageBuilder CreateSubjectFor<TContentType>()
    {
        var message = Substitute.For<IMessageBuilder>();
        message.ClrType.Returns(typeof(TContentType));
        return message;
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