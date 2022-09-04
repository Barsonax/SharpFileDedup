using System.Text;

namespace SharpDedup.Test;

public static class StringExtensions
{
    public static Stream ToStream(this string input) => new MemoryStream(Encoding.Default.GetBytes(input));
    public static Func<Stream> ToStreamFactory(this string input) => input.ToStream;

    public static string FromBytes(this IEnumerable<byte> bytes) => Encoding.Default.GetString(bytes.ToArray());
}