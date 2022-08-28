using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using ProtoBuf;

namespace SharpFileDedup;

[ProtoContract(SkipConstructor = true)]
public record DedupedFileArchive([property: ProtoMember(1)] IEnumerable<HashedFileGroupConstract> Groups)
{
    private static async Task<string> HashFile(string path, CancellationToken token)
    {
        await using var fileStream = File.OpenRead(path);
        var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(fileStream, token);

        return Convert.ToBase64String(hash);
    }
    
    public static async Task Serialize(Stream destination, params string[] paths)
    {
        var dictionary = new Dictionary<string, HashedFileGroup>();
        
        var locker = new object();
        await Parallel.ForEachAsync(paths, async (path, token) =>
        {
            var hashString = await HashFile(path, token);

            lock (locker)
            {
                if (dictionary.TryGetValue(hashString, out var list))
                {
                    list.Files.Add(path);
                }
                else
                {
                    var group = new HashedFileGroup(hashString, new List<string> { path });
                    dictionary.Add(hashString, group);
                }
            }
        });

        var groups = dictionary.Values.ToArray();
        
        var groupsWithData = groups.Select(x => new HashedFileGroupConstract(x.Hash, File.ReadAllBytes(x.Files.First()), x.Files));
        
        var archive = new DedupedFileArchive(groupsWithData);
        
        Serializer.Serialize(destination, archive);
    }

    public static DedupedFileArchive Deserialize(Stream source)
    {
        return Serializer.Deserialize<DedupedFileArchive>(source);
    }
}

public record HashedFileGroup(string Hash, List<string> Files);

[ProtoContract(SkipConstructor = true)]
public record HashedFileGroupConstract([property: ProtoMember(1)] string Hash, [property: ProtoMember(2)] byte[] Data, [property: ProtoMember(3)] List<string> Files)
{
    
}