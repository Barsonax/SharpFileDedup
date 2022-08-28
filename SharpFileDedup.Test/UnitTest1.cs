using System.Text;

namespace SharpFileDedup.Test;

public class Tests
{
    [Test]
    public async Task SerializeAndDeserialize_ExpectSameOutput()
    {
        var sourcePath = Path.GetFullPath("foo");
        var content = "abcde";
        await File.WriteAllTextAsync(sourcePath, content);
        
        var entries = new[]
        {
            new DedupedEntry("foobar", sourcePath)
        };

        var memoryStream = new MemoryStream();
        await DedupedFileArchive.Serialize(memoryStream, entries);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var deserializedArchive = DedupedFileArchive.Deserialize(memoryStream);

        Assert.That(deserializedArchive.Groups, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserializedArchive.Groups.Count(), Is.EqualTo(1));
            Assert.That(deserializedArchive.Groups.Single().Files, Is.EquivalentTo(entries.Select(x => x.Key)));
            Assert.That(Encoding.Default.GetString(deserializedArchive.Groups.Single().Data), Is.EqualTo(content));
        });
    }

    [Test]
    public async Task SerializeAndDeserialize_DuplicateFiles_ExpectSameOutput()
    {
        var sourcePath = Path.GetFullPath("foo");
        var content = "abcde";
        await File.WriteAllTextAsync(sourcePath, content);
        
        var entries = new[]
        {
            new DedupedEntry("foobar1", sourcePath),
            new DedupedEntry("foobar2", sourcePath)
        };

        var memoryStream = new MemoryStream();
        await DedupedFileArchive.Serialize(memoryStream, entries);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var deserializedArchive = DedupedFileArchive.Deserialize(memoryStream);

        Assert.That(deserializedArchive.Groups, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserializedArchive.Groups.Count(), Is.EqualTo(1));
            Assert.That(deserializedArchive.Groups.Single().Files, Is.EquivalentTo(entries.Select(x => x.Key)));
            Assert.That(Encoding.Default.GetString(deserializedArchive.Groups.Single().Data), Is.EqualTo(content));
        });
    }
}