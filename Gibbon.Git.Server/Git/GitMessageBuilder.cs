namespace Gibbon.Git.Server.Git;

public class GitMessageBuilder
{
    private readonly List<SidebandMessage> _messages = [];

    private GitMessageBuilder()
    {
    }

    public GitMessageBuilder(string refName, string serviceName) : this()
    {
        RefName = refName ?? throw new ArgumentNullException(nameof(refName));
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
    }

    public string RefName { get; }
    public string ServiceName { get; }
    public bool HasSuccess { get; private set; }
    public bool HasError { get; private set; }
    public bool IsDone => HasSuccess || HasError;

    public void AppendInfo(string text)
    {
        Guard();
        AppendInfoLine(text);
    }

    public void AppendOk()
    {
        Guard();
        AppendNestedUnpack($"ok {RefName}");
        HasSuccess = true;
    }

    public void AppendError(string text)
    {
        Guard();
        AppendNestedUnpack($"ng {RefName} {text}");
        HasError = true;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var message in _messages)
        {
            sb.Append(FormatMessage(message));
        }
        sb.Append("0000");
        return sb.ToString();
    }

    private void Guard()
    {
        if (IsDone)
        {
            throw new InvalidOperationException("Cannot append messages after the builder is done.");
        }
    }

    private GitMessageBuilder AppendInfoLine(string text)
    {
        var message = new StringBuilder(text).Append("\n").ToString();
        _messages.Add(new SidebandMessage(message, Sideband.Progress));
        return this;
    }

    private GitMessageBuilder AppendNoneLine(string text)
    {
        var message = new StringBuilder(text).Append("\n").ToString();
        _messages.Add(new SidebandMessage(message));
        return this;
    }

    private void AppendNestedUnpack(string text)
    {
        var nestedMessage = new GitMessageBuilder()
            .AppendNoneLine("unpack ok")
            .AppendNoneLine(text)
            .ToString();

        _messages.Add(new SidebandMessage(nestedMessage, Sideband.Data));
    }

    private static string FormatMessage(SidebandMessage message)
    {
        var length = Encoding.UTF8.GetByteCount(message.Message) + (message.Sideband != Sideband.None ? 1 : 0) + 4;
        var hexLength = length.ToString("X4");

        var result = new StringBuilder(hexLength);
        if (message.Sideband != Sideband.None)
        {
            result.Append((char)message.Sideband);
        }
        result.Append(message.Message);
        return result.ToString();
    }

    private enum Sideband
    {
        None,
        Data = 0x01,
        Progress = 0x02,
        Error = 0x03
    }

    private readonly record struct SidebandMessage(string Message, Sideband Sideband = Sideband.None);
}
