using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Transport.Methods.Confirm;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Confirm
{
    public class ModelTests
    {
        [Fact]
        public void SelectMethod()
        {
            var noWait = true;

            var method = new SelectMethod(noWait);

            Assert.Equal(MethodId.ConfirmSelect, method.MethodId);
            Assert.Equal(noWait, method.NoWait);
        }

        [Fact]
        public void SelectOKMethod()
        {
            var method = new SelectOkMethod();

            Assert.Equal(MethodId.ConfirmSelectOk, method.MethodId);
        }
    }
}