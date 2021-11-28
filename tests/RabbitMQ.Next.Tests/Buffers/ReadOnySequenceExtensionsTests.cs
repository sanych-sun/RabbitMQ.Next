// using System;
// using System.Buffers;
// using System.Collections.Generic;
// using RabbitMQ.Next.Buffers;
// using Xunit;
//
// namespace RabbitMQ.Next.Tests.Buffers
// {
//     public class ReadOnySequenceExtensionsTests
//     {
//         [Theory]
//         [MemberData(nameof(ToSequenceTestCases))]
//         internal void ToSequenceTests(byte[] expected, MemoryBlock[] chunks)
//         {
//             var result = chunks.ToSequence();
//
//             Assert.Equal(expected, result.ToArray());
//         }
//
//         public static IEnumerable<object[]> ToSequenceTestCases()
//         {
//             yield return new object[]
//             {
//                 Array.Empty<byte>(),
//                 Array.Empty<MemoryBlock>()
//             };
//
//             yield return new object[]
//             {
//                 Array.Empty<byte>(),
//                 new StaticMemoryBlock[] { Array.Empty<byte>() }
//             };
//
//             yield return new object[]
//             {
//                 new byte[] { 0x01 },
//                 new StaticMemoryBlock[] { new byte[] { 0x01 } }
//             };
//
//             yield return new object[]
//             {
//                 new byte[] { 0x01, 0x02, 0x03 },
//                 new StaticMemoryBlock[] { new byte[] { 0x01 }, new byte[] { 0x02, 0x03 } }
//             };
//
//             yield return new object[]
//             {
//                 new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 },
//                 new StaticMemoryBlock[] { new byte[] { 0x01 }, new byte[] { 0x02, 0x03 }, new byte[] { 0x04, 0x05 } }
//             };
//         }
//     }
// }