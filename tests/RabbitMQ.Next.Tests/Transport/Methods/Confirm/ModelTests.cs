using RabbitMQ.Next.Transport.Methods.Confirm;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Confirm
{
    public class ModelTests
    {
        [Fact]
        public void SelectMethod()
        {
            var method = new SelectMethod();

            Assert.Equal(MethodId.ConfirmSelect, method.MethodId);
        }

        [Fact]
        public void SelectOkMethod()
        {
            var method = new SelectOkMethod();

            Assert.Equal(MethodId.ConfirmSelectOk, method.MethodId);
        }
    }
}