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

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var message = this.CreateSubjectFor<AssemblyAttributesData>();
After:
        var message = AttributeTransformerTests.CreateSubjectFor<AssemblyAttributesData>();
*/
        var message = CreateSubjectFor<AssemblyAttributesData>();
        
        PublisherAttributes.Apply(message);
        
        message.Received().SetApplicationId("unitTest");
    }

    [Fact]
    public void TestRoutingKeyAttribute()
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var message = this.CreateSubjectFor<RoutingKeyAttributeData>();
After:
        var message = AttributeTransformerTests.CreateSubjectFor<RoutingKeyAttributeData>();
*/
        var message = CreateSubjectFor<RoutingKeyAttributeData>();
        
        PublisherAttributes.Apply(message);

        message.Received().SetApplicationId("unitTest");
        message.Received().SetRoutingKey("route");
    }

    [Fact]
    public void TestHeaderAttribute()
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var message = this.CreateSubjectFor<HeaderAttributeData>();
After:
        var message = AttributeTransformerTests.CreateSubjectFor<HeaderAttributeData>();
*/
        var message = CreateSubjectFor<HeaderAttributeData>();
        
        PublisherAttributes.Apply(message);

        message.Received().SetApplicationId("unitTest");
        message.Received().SetHeader("headerA", "value1");
        message.Received().SetHeader("headerB", "value2");
    }

    [Fact]
    public void TestMultipleAttributes()
    {

/* Unmerged change from project 'RabbitMQ.Next.Tests'
Before:
        var message = this.CreateSubjectFor<MultipleAttributesData>();
After:
        var message = AttributeTransformerTests.CreateSubjectFor<MultipleAttributesData>();
*/
        var message = CreateSubjectFor<MultipleAttributesData>();
        
        PublisherAttributes.Apply(message);

        message.Received().SetApplicationId("unitTest");
        message.Received().SetRoutingKey("route");
        message.Received().SetPriority(7);
    }

    private static IMessageBuilder CreateSubjectFor<TContentType>()
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
