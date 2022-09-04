namespace SharpDedup;

public sealed class EnumerableStream : Stream, IDisposable
{
    private readonly IEnumerator<byte> _bytes;
    private bool _disposed;
    
    public EnumerableStream(IEnumerable<byte> bytes)
    {
        _bytes = bytes.GetEnumerator();
    }
    
    public override int Read(byte[] buffer, int offset, int count)
    {
        var i = 0;
        for (; i < count && _bytes.MoveNext(); i++)
            buffer[i + offset] = _bytes.Current;
        return i;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new InvalidOperationException();
    public override void SetLength(long value) => throw new InvalidOperationException();
    public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();
    public override void Flush() => throw new InvalidOperationException();

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => 0;
    public override long Position { get; set; } = 0;
    
    void IDisposable.Dispose()
    {
        if (_disposed)
            return;
        _bytes.Dispose();
        _disposed=  true;
    }
}