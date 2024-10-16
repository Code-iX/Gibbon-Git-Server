namespace Gibbon.Git.Server.Git;

public class GitCapabilities
{
    public bool SideBand64K { get; set; }
    public bool ReportStatus { get; set; }
    public bool ReportStatusV2 { get; set; }
    public string ObjectFormat { get; set; }
    public string Agent { get; set; }
    public List<string> OtherCapabilities { get; } = [];
}
