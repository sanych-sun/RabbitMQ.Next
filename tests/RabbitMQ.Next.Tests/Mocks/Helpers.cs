using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RabbitMQ.Next.Buffers;

namespace RabbitMQ.Next.Tests.Mocks;

internal static class Helpers
{
    private static IReadOnlyList<(string Charset, string Text, ReadOnlyMemory<byte> Bytes)> Texts = new []
    {
        MakeText("Latin", "Lorem ipsu"),
        MakeText("Latin", "Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci "),
        MakeText("Latin", "Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber ca"),
        MakeText("Latin", "Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber cau"),
        MakeText("Latin", "Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber causae saperet et. Ne ipsum congue graecis sed,"),

        MakeText("Cyrillic", "Лорем"),
        MakeText("Cyrillic", "Лорем ипсум долор сит амет санцтус фабеллас ан яуи, хас дицит инвенире"),
        MakeText("Cyrillic", "Лорем ипсум долор сит амет санцтус фабеллас ан яуи, хас дицит инвенире сусципиантур еу аугуе проприае те нам. Сит модус еррем персиус ид. Ри!"),
        MakeText("Cyrillic", "Лорем ипсум долор сит амет санцтус фабеллас ан яуи, хас дицит инвенире сусципиантур еу аугуе проприае те нам. Сит модус еррем персиус ид. Рид"),
        MakeText("Cyrillic", "Лорем ипсум долор сит амет санцтус фабеллас ан яуи, хас дицит инвенире сусципиантур еу аугуе проприае те нам. Сит модус еррем персиус ид. Риденс вертерем инструцтио"),

        MakeText("Georgian", "ლოaე"),
        MakeText("Georgian", "ლოaემ იფსუმ დოლორ სით ამეთ ორათიო ფუისსეთ რეცუსა"),
        MakeText("Georgian", "ლოaემ იფსუმ დოლორ სით ამეთ ორა. თიო ფუისსეთ რეცუსაბო ნეც ად. მოლლის ფრაესენთ ცუმ იდ, უსუ ასსუმ რეცთ"),
        MakeText("Georgian", "ლოaემ იფსუმ დოლორ სით ამეთ ორათიო ფუისსეთ რეცუსაბო ნეც ად. მოლლის ფრაესენთ ცუმ იდ, უსუ ასსუმ რეცთე"),
        MakeText("Georgian", "ლოaემ იფსუმ დოლორ სით ამეთ ორათიო ფუისსეთ რეცუსაბო ნეც ად. მოლლის ფრაესენთ ცუმ იდ, უსუ ასსუმ რეცთეყუე ათ. იდ სედ გრა"),

        MakeText("Chinese", "片+目表"),
        MakeText("Chinese", "片+目表合専逮放実郎提望月作。応索奈意給率-置億場活者調載撲記歳事動。言民敏演選無山対婚"),
        MakeText("Chinese", "片+目表合専逮放実郎提望月作。応索奈意給率-置億場活者調載撲記歳事動。言民敏演選無山対婚認/芸会室太工負未可綺。争強格告集周条催中保度初質界。分窓禁却佐市蘇向周像車弁呼優質背"),
        MakeText("Chinese", "片+目表合専逮放実郎提望月作。応索奈意給率-置億場活者調載撲記歳事動。言民敏演選-無山対婚認/芸会室太工負未可綺。争強格告集周条催中保度初質界。分窓禁却佐市蘇向周像車弁呼優質背"),
        MakeText("Chinese", "片+目表合専逮放実郎提望月作。応索奈意給率-置億場活者調載撲記歳事動。言民敏演選無山対婚認/芸会室太工負未可綺。争強格告集周条催中保度初質界。分窓禁却佐市蘇向周像車弁呼優質背味区年式。米危歳夏画阪掲者番通"),
    };

    private static (string Charset, string Text, ReadOnlyMemory<byte> Bytes) MakeText(string charset, string text)
        => (charset, text, Encoding.UTF8.GetBytes(text));

    public static IEnumerable<(string Charset, string Text, ReadOnlyMemory<byte> Bytes)> GetDummyTexts(int minBytes, int maxBytes = 0)
        => Texts.Where(i => i.Bytes.Length >= minBytes).Where(i => maxBytes == 0 || i.Bytes.Length <= maxBytes);

    public static ReadOnlySequence<byte> MakeSequence(params byte[][] parts)
    {
        if (parts.Length == 0)
        {
            return ReadOnlySequence<byte>.Empty;
        }

        if (parts.Length == 1)
        {
            return new ReadOnlySequence<byte>(parts[0]);
        }

        MemorySegment<byte> first = null;
        MemorySegment<byte> last = null;
        foreach (var part in parts)
        {
            if (first == null)
            {
                first = new MemorySegment<byte>(part);
                last = first;
            }
            else
            {
                last = last.Append(part);
            }
        }

        return new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length);

    }

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