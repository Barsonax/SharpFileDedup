namespace SharpDedup;

public class DedupedArchive
{
    public List<DedupedEntry> Entries { get; }
    
    public DedupedArchive(IEnumerable<HashedFileGroupContract> groups, IFileSystem? fileSystem = null)
    {
        FileSystem = fileSystem ?? new RealFileSystem();
        Entries = groups.SelectMany(x => x.Files.Select(y => new DedupedEntry(y, x.Data))).ToList();
    }

    private IFileSystem FileSystem { get; }

    public async Task Save(string destinationPath)
    {
        await using var stream = FileSystem.OpenWrite(destinationPath);
        await DeduplicationSerializer.Serialize(stream, Entries);
    }

    public async Task Extract(string destinationPath)
    {
        var directory = FileSystem.CreateDirectory(destinationPath);

        await Parallel.ForEachAsync(Entries,  async(entry, token) =>
        {
            var file = FileSystem.GetFileInfo($"{directory.FullName}\\{entry.Path}");
            file.Directory?.Create();
            await using var stream = entry.StreamFactory();

            var fileStream = FileSystem.OpenWrite(file.FullName);
            var data = stream.ReadToEnd().ToArray();
            await fileStream.WriteAsync(data, 0, data.Length, token).ConfigureAwait(false);
        });
    }
}