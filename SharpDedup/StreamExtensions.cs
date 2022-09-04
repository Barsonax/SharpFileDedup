namespace SharpDedup;

public static class StreamExtensions
{
    public static IEnumerable<byte> ReadToEnd(this Stream stream)
    {
        var buffer = new byte[4096];  
        int numBytesRead;
        do
        {
            numBytesRead = stream.Read(buffer);
            for (var index = 0; index < numBytesRead; index++)
            {
                var data = buffer[index];
                yield return data;
            }
        } while (numBytesRead > 0);
    }
    
    public static IEnumerable<byte> ReadToEnd(this Func<Stream> streamFactory)
    {
        using (var stream = streamFactory())
        {
            foreach (var b in stream.ReadToEnd())
            {
                yield return b;
            }
        }
    }
}