namespace SharpDedup;

public interface IFileSystem
{
    IDirectoryInfo? CreateDirectory(string path);
    IFileInfo GetFileInfo(string path);
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);
    Stream OpenRead(string path);
    Stream OpenWrite(string path);
}

public interface IDirectoryInfo
{
    void Create();
    string FullName { get; }
}

public interface IFileInfo
{
    IDirectoryInfo Directory { get; }
    string FullName { get; }
}

public class RealFileSystem : IFileSystem
{
    public IDirectoryInfo? CreateDirectory(string path)
    {
        return new RealDirectoryInfo(Directory.CreateDirectory(path));
    }

    public IFileInfo GetFileInfo(string path)
    {
        return new RealFileInfo(new FileInfo(path));
    }

    public Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        return File.WriteAllBytesAsync(path, bytes, cancellationToken);
    }

    public Stream OpenRead(string path)
    {
        return File.OpenRead(path);
    }

    public Stream OpenWrite(string path)
    {
        return File.OpenWrite(path);
    }
}

public record RealDirectoryInfo(DirectoryInfo DirectoryInfo) : IDirectoryInfo
{
    public void Create()
    {
        DirectoryInfo.Create();
    }

    public string FullName => DirectoryInfo.FullName;
}

public record RealFileInfo(FileInfo FileInfo) : IFileInfo
{
    public IDirectoryInfo Directory => new RealDirectoryInfo(FileInfo.Directory);
    public string FullName => FileInfo.FullName;
}