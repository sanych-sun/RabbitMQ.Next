using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RabbitMQ.Next.Tests
{
    internal static class Helpers
    {
        private const string DummyText = "What is Lorem Ipsum? Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";

        public static string GetDummyText(int lenght) => DummyText.Substring(0, lenght);

        public static byte[] GetFileContent(string resourcePath)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            using var memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public static bool DictionaryEquals(IReadOnlyDictionary<string, object> left, IReadOnlyDictionary<string, object> right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            var result =  left.All(i =>
                right.ContainsKey(i.Key)
                &&
                (
                    i.Value.Equals(right[i.Key])
                    ||
                    (i.Value is IReadOnlyDictionary<string, object> dict && DictionaryEquals(dict, (IReadOnlyDictionary<string, object>) right[i.Key]))
                ));

            return result;
        }
    }
}