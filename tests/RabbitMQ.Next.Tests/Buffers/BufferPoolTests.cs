// using NSubstitute;
// using RabbitMQ.Next.Abstractions.Buffers;
// using RabbitMQ.Next.Buffers;
// using Xunit;
//
// namespace RabbitMQ.Next.Tests.Buffers
// {
//     public class BufferPoolTests
//     {
//         [Theory]
//         [InlineData(100, 0, 100)]
//         [InlineData(100, 50, 50)]
//         [InlineData(100, 200, 200)]
//         public void CreateMemoryTests(int managerSize, int size, int expectedSize)
//         {
//             var pool = new BufferPool(MockBufferManager(managerSize));
//
//             var memory = pool.CreateMemory(size);
//
//             Assert.Equal(expectedSize, memory.Memory.Length);
//         }
//
//         private static IBufferManager MockBufferManager(int bufferSize)
//         {
//             var bufferManager = Substitute.For<IBufferManager>();
//             bufferManager.Rent(Arg.Any<int>()).Returns(x =>
//             {
//                 var requestedSize = x.Arg<int>();
//                 return (requestedSize < bufferSize) ? new byte[bufferSize] : new byte[requestedSize];
//             });
//
//             return bufferManager;
//         }
//     }
// }