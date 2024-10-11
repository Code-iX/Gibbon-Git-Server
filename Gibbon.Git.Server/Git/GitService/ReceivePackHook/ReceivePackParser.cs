using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Gibbon.Git.Server.Git.GitService.ReceivePackHook;

public class ReceivePackParser(IGitService gitService, IHookReceivePack receivePackHandler)
    : IGitService
{
    private readonly IGitService _gitService = gitService;
    private readonly IHookReceivePack _receivePackHandler = receivePackHandler;

    public async Task ExecuteServiceByName(string correlationId, string repositoryName, string serviceName, ExecutionOptions options, Stream inStream, Stream outStream, string userName, int userId)
    {
        ParsedReceivePack receivedPack = null;

        if (serviceName == "receive-pack" && inStream.Length > 0)
        {
            // PARSING RECEIVE-PACK THAT IS OF THE FOLLOWING FORMAT: 
            // (NEW LINES added for ease of reading)
            // (LLLL is length of the line (expressed in HEX) until next LLLL value)
            //
            // LLLL------ REF LINE -----------\0------- OHTER DATA -----------
            // LLLL------ REF LINE ----------------
            // ...
            // ...
            // 0000PACK------- REST OF PACKAGE --------
            //

            var pktLines = new List<ReceivePackPktLine>();

            var buff1 = new byte[1];
            var buff4 = new byte[4];
            var buff20 = new byte[20];
            var buff16K = new byte[1024 * 16];

            while (true)
            {
                ReadStream(inStream, buff4);
                var len = Convert.ToInt32(Encoding.UTF8.GetString(buff4), 16);
                if (len == 0)
                {
                    break;
                }
                len -= buff4.Length;

                var accum = new LinkedList<byte>();

                while (len > 0)
                {
                    len -= 1;
                    ReadStream(inStream, buff1);
                    if (buff1[0] == 0)
                    {
                        break;
                    }
                    accum.AddLast(buff1[0]);
                }
                if (len > 0)
                {
                    inStream.Seek(len, SeekOrigin.Current);
                }
                var pktLine = Encoding.UTF8.GetString(accum.ToArray());
                var pktLineItems = pktLine.Split(' ');

                var fromCommit = pktLineItems[0];
                var toCommit = pktLineItems[1];
                var refName = pktLineItems[2];

                pktLines.Add(new ReceivePackPktLine(fromCommit, toCommit, refName));
            }

            // parse PACK contents
            var packCommits = new List<ReceivePackCommit>();

            // PACK format
            // https://www.kernel.org/pub/software/scm/git/docs/technical/pack-format.html
            // http://schacon.github.io/gitbook/7_the_packfile.html

            if (inStream.Position < inStream.Length)
            {
                ReadStream(inStream, buff4);
                if (Encoding.UTF8.GetString(buff4) != "PACK")
                {
                    throw new Exception("Unexpected receive-pack 'PACK' content.");
                }
                ReadStream(inStream, buff4);
                Array.Reverse(buff4);
                BitConverter.ToInt32(buff4, 0);

                ReadStream(inStream, buff4);
                Array.Reverse(buff4);
                var numObjects = BitConverter.ToInt32(buff4, 0);

                while (numObjects > 0)
                {
                    numObjects -= 1;

                    ReadStream(inStream, buff1);
                    var type = (GitObjectType)((buff1[0] >> 4) & 7);
                    long len = buff1[0] & 15;

                    var shiftAmount = 4;
                    while ((buff1[0] >> 7) == 1)
                    {
                        ReadStream(inStream, buff1);
                        len |= ((long)(buff1[0] & 127) << shiftAmount);

                        shiftAmount += 7;
                    }

                    if (type == GitObjectType.RefDelta)
                    {
                        // read ref name
                        ReadStream(inStream, buff20);
                    }
                    if (type == GitObjectType.OfsDelta)
                    {
                        // read negative offset
                        ReadStream(inStream, buff1);
                        while ((buff1[0] >> 7) == 1)
                        {
                            ReadStream(inStream, buff1);
                        }
                    }

                    var origPosition = inStream.Position;
                    long offsetVal = 0;

                    await using (var zlibStream = new InflaterInputStream(inStream))
                    {
                        // read compressed data max 16KB at a time
                        var readRemaining = len;
                        var startPosition = inStream.Position; // Speichere die aktuelle Position des inStreams

                        do
                        {
                            var bytesUncompressed = zlibStream.Read(buff16K, 0, buff16K.Length);
                            readRemaining -= bytesUncompressed;
                        } while (readRemaining > 0);

                        if (type == GitObjectType.Commit)
                        {
                            var parsedCommit = ParseCommitDetails(buff16K, len);
                            packCommits.Add(parsedCommit);
                        }

                        // Berechne, wie viel der InflaterInputStream gelesen hat
                        var bytesReadByInflater = inStream.Position - startPosition;

                        // move back position a bit if needed
                        inStream.Seek(startPosition + bytesReadByInflater, SeekOrigin.Begin);
                    }

                    // move back position a bit because ZLibStream reads more than needed for inflating
                    inStream.Seek(origPosition + offsetVal, SeekOrigin.Begin);
                }
            }
            // -------------------

            receivedPack = new ParsedReceivePack(correlationId, repositoryName, pktLines, userName, DateTime.Now, packCommits);

            inStream.Seek(0, SeekOrigin.Begin);

            _receivePackHandler.PrePackReceive(receivedPack);
        }

        GitExecutionResult execResult;
        using (var capturedOutputStream = new MemoryStream())
        {
            await _gitService.ExecuteServiceByName(correlationId, repositoryName, serviceName, options, inStream, new ReplicatingStream(outStream, capturedOutputStream), userName, userId);

            // parse captured output
            capturedOutputStream.Seek(0, SeekOrigin.Begin);
            execResult = GitServiceResultParser.ParseResult(capturedOutputStream);
        }

        if (receivedPack != null)
        {
            _receivePackHandler.PostPackReceive(receivedPack, execResult);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReceivePackCommit ParseCommitDetails(byte[] buff, long commitMsgLengthLong)
    {
        if (commitMsgLengthLong > buff.Length)
        {
            // buff at the moment is 16KB, should be enough for commit messages
            // but break just in case this ever does happen so it could be addressed then
            throw new Exception("Encountered unexpectedly large commit message");
        }
        var commitMsgLength = (int)commitMsgLengthLong; // guaranteed no truncation because of above guard clause

        var commitMsg = Encoding.UTF8.GetString(buff, 0, commitMsgLength);
        string treeHash = null;
        var parentHashes = new List<string>();
        ReceivePackCommitSignature author = null;
        ReceivePackCommitSignature committer = null;

        var commitLines = commitMsg.Split('\n');

        var commitHeadersEndIndex = 0;
        foreach (var commitLine in commitLines)
        {
            commitHeadersEndIndex += 1;

            // Make sure we have safe default values in case the string is empty.
            var commitHeaderData = "";

            // Find the index of the first space.
            var firstSpace = commitLine.IndexOf(' ');
            if (firstSpace < 0)
            {
                // Ensure that we always have a valid length for the type.
                firstSpace = commitLine.Length;
            }

            // Take everything up to the first space as the type.
            string commitHeaderType = commitLine[..firstSpace];

            // Data starts immediately following the space (if there is any).
            var dataStart = firstSpace + 1;
            if (dataStart < commitLine.Length)
            {
                commitHeaderData = commitLine[dataStart..];
            }

            switch (commitHeaderType)
            {
                case "tree":
                    treeHash = commitHeaderData;
                    break;

                case "parent":
                    parentHashes.Add(commitHeaderData);
                    break;

                case "author":
                    author = ParseSignature(commitHeaderData);
                    break;

                case "committer":
                    committer = ParseSignature(commitHeaderData);
                    break;

                case "":
                    // The first empty type indicates the end of the headers.
                    break;
            }
        }

        var commitComment = string.Join("\n", commitLines.Skip(commitHeadersEndIndex).ToArray()).TrimEnd('\n');

        // Compute commit hash
        using var sha1 = SHA1.Create();
        var commitHashHeader = Encoding.UTF8.GetBytes($"commit {commitMsgLength}\0");

        sha1.TransformBlock(commitHashHeader, 0, commitHashHeader.Length, commitHashHeader, 0);
        sha1.TransformFinalBlock(buff, 0, commitMsgLength);

        var commitHashBytes = sha1.Hash;

        var sb = new StringBuilder();
        foreach (var b in commitHashBytes)
        {
            var hex = b.ToString("x2");
            sb.Append(hex);
        }
        var commitHash = sb.ToString();

        return new ReceivePackCommit(commitHash, treeHash, parentHashes,
            author, committer, commitComment);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReceivePackCommitSignature ParseSignature(string commitHeaderData)
    {
        // Find the start and end markers of the email address.
        var emailStart = commitHeaderData.IndexOf('<');
        var emailEnd = commitHeaderData.IndexOf('>');

        // Leave out the trailing space.
        var nameLength = emailStart - 1;

        // Leave out the starting bracket.
        var emailLength = emailEnd - emailStart - 1;

        // Parse the name and email values.
        var name = commitHeaderData[..nameLength];
        var email = commitHeaderData.Substring(emailStart + 1, emailLength);

        // The rest of the string is the timestamp, it may include a timezone.
        var timestampString = commitHeaderData[(emailEnd + 2)..];
        var timestampComponents = timestampString.Split(' ');

        // Start with epoch in UTC, add the timestamp seconds.
        var timestamp = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        timestamp = timestamp.AddSeconds(long.Parse(timestampComponents[0]));

        return new ReceivePackCommitSignature(name, email, timestamp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReadStream(Stream s, byte[] buff)
    {
        var readBytes = s.Read(buff, 0, buff.Length);
        if (readBytes != buff.Length)
        {
            throw new Exception($"Expected to read {buff.Length} bytes, got {readBytes}");
        }
    }
}
