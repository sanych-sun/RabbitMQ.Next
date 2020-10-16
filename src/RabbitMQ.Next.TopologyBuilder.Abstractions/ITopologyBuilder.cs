namespace RabbitMQ.Next.TopologyBuilder.Abstractions
{
    public interface ITopologyBuilder
    {
        IExchangeBuilder DeclareExchange(string name, string type);

        IQueueBuilder DeclareQueue(string name);

        IBindingBuilder Bind(string source, BindingTarget type, string destination);
    }
}