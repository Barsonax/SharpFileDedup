namespace SharpDedup.Test;

public class DeduplicationSerializerTests
{
    [Test]
    public async Task SerializeAndDeserialize_ExpectSameOutput()
    {
        var content = "abcde";
        
        var entries = new[]
        {
            new DedupedEntry("foobar", content.ToStreamFactory())
        };

        var memoryStream = new MemoryStream();
        await DeduplicationSerializer.Serialize(memoryStream, entries);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var deserializedArchive = DeduplicationSerializer.Deserialize(memoryStream);

        Assert.That(deserializedArchive.Entries, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserializedArchive.Entries.Count, Is.EqualTo(1));
            Assert.That(deserializedArchive.Entries.Select(x =>  x.Path), Is.EquivalentTo(entries.Select(x => x.Path)));
            Assert.That(deserializedArchive.Entries.Single().StreamFactory().ReadToEnd().FromBytes(), Is.EqualTo(content));
        });
    }

    [Test]
    public async Task SerializeAndDeserialize_DuplicateFiles_ExpectSameOutput()
    {
        var content = "abcde";

        var entries = new[]
        {
            new DedupedEntry("foobar1", content.ToStreamFactory()),
            new DedupedEntry("foobar2", content.ToStreamFactory())
        };

        var memoryStream = new MemoryStream();
        await DeduplicationSerializer.Serialize(memoryStream, entries);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var deserializedArchive = DeduplicationSerializer.Deserialize(memoryStream);

        Assert.That(deserializedArchive.Entries, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserializedArchive.Entries.Count, Is.EqualTo(2));
            Assert.That(deserializedArchive.Entries.Select(x => x.Path), Is.EquivalentTo(entries.Select(x => x.Path)));
            Assert.That(deserializedArchive.Entries.Select(x => x.StreamFactory().ReadToEnd().FromBytes()), Is.EquivalentTo(entries.Select(x => content)));
        });
    }
}