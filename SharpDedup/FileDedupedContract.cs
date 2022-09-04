using ProtoBuf;

namespace SharpDedup;

public record HashedFileGroup(List<string> Files, Func<Stream> StreamFactory);

public record DedupedEntry(string Path, Func<Stream> StreamFactory)
{
    public DedupedEntry(string path, IFileSystem fileSystem) : this(path, () => fileSystem.OpenRead(path)) {}
    
    public DedupedEntry(string path, IEnumerable<byte> bytes) : this(path, () => new EnumerableStream(bytes)) {}
}

[ProtoContract(SkipConstructor = true)]
public record HashedFileGroupContract([property: ProtoMember(2)] IEnumerable<byte> Data, [property: ProtoMember(3)] List<string> Files);