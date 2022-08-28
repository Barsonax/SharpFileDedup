using System.Security.Cryptography;
using ProtoBuf;

namespace SharpFileDedup;

public record HashedFileGroup(string Hash, List<string> Files, string SourceFile);
public record DedupedEntry(string Key, string SourcePath);

[ProtoContract(SkipConstructor = true)]
public record HashedFileGroupContract([property: ProtoMember(1)] string Hash, [property: ProtoMember(2)] byte[] Data, [property: ProtoMember(3)] string[] Files);

[ProtoContract(SkipConstructor = true)]
public record DedupedFileArchive([property: ProtoMember(1)] IEnumerable<HashedFileGroupContract> Groups)
{
    private static async Task<string> HashFile(string path, CancellationToken token)
    {
        await using var fileStream = File.OpenRead(path);
        var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(fileStream, token);

        return Convert.ToBase64String(hash);
    }

    public static async Task Serialize(Stream destination, IEnumerable<DedupedEntry> paths)
    {
        var dictionary = new Dictionary<string, HashedFileGroup>();

        var locker = new object();
        await Parallel.ForEachAsync(paths, async (entry, token) =>
        {
            var hashString = await HashFile(entry.SourcePath, token);

            lock (locker)
            {
                if (dictionary.TryGetValue(hashString, out var list))
                {
                    list.Files.Add(entry.Key);
                }
                else
                {
                    var group = new HashedFileGroup(hashString, new List<string> { entry.Key }, entry.SourcePath);
                    dictionary.Add(hashString, group);
                }
            }
        });

        var groups = dictionary.Values.ToArray();

        var groupsWithData = groups.Select(x => new HashedFileGroupContract(x.Hash, File.ReadAllBytes(x.SourceFile), x.Files.ToArray()));

        var archive = new DedupedFileArchive(groupsWithData);

        archive.Serialize(destination);
    }

    public static DedupedFileArchive Deserialize(Stream source)
    {
        return Serializer.Deserialize<DedupedFileArchive>(source);
    }

    public void Serialize(Stream destination)
    {
        Serializer.Serialize(destination, this);
    }

    public async Task Extract(string destinationPath)
    {
        var directory = Directory.CreateDirectory(destinationPath);
        foreach (var hashedFileGroup in Groups)
        {
            await Parallel.ForEachAsync(hashedFileGroup.Files, async (path, token) =>
            {
                var file = new FileInfo($"{directory.FullName}\\{path}");
                file.Directory?.Create();
                await File.WriteAllBytesAsync(file.FullName, hashedFileGroup.Data, token);
            });
        }
    }
}