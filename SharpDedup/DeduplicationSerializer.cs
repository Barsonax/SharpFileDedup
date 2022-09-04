using System.Security.Cryptography;
using ProtoBuf;

namespace SharpDedup;

public static class DeduplicationSerializer
{
    private static async Task<string> HashFile(Func<Stream> streamFactory, CancellationToken token)
    {
        var sha = SHA256.Create();
        byte[] hash;
        using (var stream = streamFactory())
        {
            hash = await sha.ComputeHashAsync(stream, token);
        }

        return Convert.ToBase64String(hash);
    }

    public static Task Serialize(Stream destination, IEnumerable<string> paths, IFileSystem? fileSystem = null)
    {
        fileSystem ??= new RealFileSystem();
        return Serialize(destination, paths.Select(path => new DedupedEntry(path, fileSystem)));
    }

    public static async Task Serialize(Stream destination, IEnumerable<DedupedEntry> entries)
    {
        var dictionary = new Dictionary<string, HashedFileGroup>(StringComparer.Ordinal);

        var locker = new object();
        await Parallel.ForEachAsync(entries, async (entry, token) =>
        {
            var hashString = await HashFile(entry.StreamFactory, token);

            lock (locker)
            {
                if (dictionary.TryGetValue(hashString, out var list))
                {
                    list.Files.Add(entry.Path);
                }
                else
                {
                    var group = new HashedFileGroup(new List<string> { entry.Path }, entry.StreamFactory);
                    dictionary.Add(hashString, group);
                }
            }
        });
        
        var groupsWithData = dictionary.Values.Select(x => new HashedFileGroupContract(x.StreamFactory.ReadToEnd(), x.Files));

        Serialize(groupsWithData, destination);
    }

    public static DedupedArchive Deserialize(Stream source)
    {
        var groups = Serializer.Deserialize<IEnumerable<HashedFileGroupContract>>(source);
        return new DedupedArchive(groups);
    }

    private static void Serialize(IEnumerable<HashedFileGroupContract> archive, Stream destination)
    {
        Serializer.Serialize(destination, archive);
    }
}