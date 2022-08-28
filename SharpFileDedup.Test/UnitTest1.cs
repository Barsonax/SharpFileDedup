using System.Diagnostics;
using System.IO.Compression;
using ProtoBuf;

namespace SharpFileDedup.Test;

public class Tests
{
    [Test]
    public async Task SerializeDuplicateFiles()
    {
        var stringSize = 5000000;
        var largeString = new string('*', stringSize);
        var files = new Dictionary<string, string>();

        for (int i = 0; i < 1000; i++)
        {
              files.Add($"foo{i}", largeString);  
        }

        //await Parallel.ForEachAsync(files,async (keyValuePair, token) =>
        //{
        //    await File.WriteAllTextAsync(keyValuePair.Key, keyValuePair.Value, token);    
        //});


        var watch = Stopwatch.StartNew();
        using (var file = File.Create("deduped"))
        {
            await DedupedFileArchive.Serialize(file,files.Select(x => x.Key).ToArray());
        }

        Console.WriteLine(watch.Elapsed);
        
        using (var file = File.Create("deduped.gz"))
        {
            using (var gzipStream = new GZipStream(file, CompressionMode.Compress))
            {
                await DedupedFileArchive.Serialize(gzipStream,files.Select(x => x.Key).ToArray());
            }
        }

        var deserializedFileDeduped =  DedupedFileArchive.Deserialize(File.OpenRead("deduped"));

    }
}