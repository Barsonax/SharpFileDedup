namespace SharpDedup.Test;

public class StreamExtensionsTest
{
    [Test]
    public void ReadToEnd_ReturnsCorrectData()
    {
        var data = new byte[]{ 1,2,3,4,5, 80, 40, 53, 21};
        var stream = new MemoryStream(data);
        var result = stream.ReadToEnd();

        CollectionAssert.AreEqual(data, result);
    }
    
    [Test]
    public void ReadToEnd_Factory_ReturnsCorrectData()
    {
        var data = new byte[]{ 1,2,3,4,5, 80, 40, 53, 21};
        var streamFactory = () => new MemoryStream(data);
        var result = streamFactory.ReadToEnd();

        CollectionAssert.AreEqual(data, result);
    }
}