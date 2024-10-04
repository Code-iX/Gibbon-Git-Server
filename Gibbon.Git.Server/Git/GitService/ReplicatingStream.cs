namespace Gibbon.Git.Server.Git.GitService;

public class ReplicatingStream : Stream
{
    private readonly Stream source;
    private readonly Stream target;

    public ReplicatingStream(Stream source, Stream target)
    {
        this.source = source;
        this.target = target;
    }

    public override bool CanRead => source.CanRead;

    public override bool CanSeek => source.CanSeek;

    public override bool CanWrite => source.CanWrite;

    public override void Flush()
    {
        source.Flush();
        target.Flush();
    }

    public override long Length => source.Length;

    public override long Position
    {
        get => source.Position;
        set
        {
            source.Position = value;
            target.Position = value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        target.Read(buffer, offset, count);
        return source.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        target.Seek(offset, origin);
        return source.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        target.SetLength(value);
        source.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        target.Write(buffer, offset, count);
        target.Flush();
        source.Write(buffer, offset, count);
    }
}