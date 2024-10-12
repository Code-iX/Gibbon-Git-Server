namespace Gibbon.Git.Server.Git;

public class ReplicatingStream(Stream source, Stream target) : Stream
{
    private readonly Stream _source = source;
    private readonly Stream _target = target;

    public override bool CanRead => _source.CanRead;

    public override bool CanSeek => _source.CanSeek;

    public override bool CanWrite => _source.CanWrite;

    public override void Flush()
    {
        _source.Flush();
        _target.Flush();
    }

    public override long Length => _source.Length;

    public override long Position
    {
        get => _source.Position;
        set
        {
            _source.Position = value;
            _target.Position = value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        _target.Read(buffer, offset, count);
        return _source.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        _target.Seek(offset, origin);
        return _source.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _target.SetLength(value);
        _source.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _target.Write(buffer, offset, count);
        _target.Flush();
        _source.Write(buffer, offset, count);
    }
}
