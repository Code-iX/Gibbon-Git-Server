﻿using System.Runtime.InteropServices;
using System.Text.Json;

using Gibbon.Git.Server.Configuration;
using Gibbon.Git.Server.Security;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Gibbon.Git.Server.Services;

public class DiagnosticReporter(IPathResolver pathResolver, IConfiguration configuration, IMembershipService membershipService, IOptions<ApplicationSettings> applicationOptions, IOptions<GitSettings> gitOptions, IServerSettingsService settingsService)
    : IDiagnosticReporter
{
    private readonly IPathResolver _pathResolver = pathResolver;
    private readonly IConfiguration _configuration = configuration;
    private readonly IMembershipService _membershipService = membershipService;
    private readonly IServerSettingsService _settingsService = settingsService;
    private readonly GitSettings _gitSettings = gitOptions.Value;
    private readonly StringBuilder _report = new();
    private readonly ApplicationSettings _applicationSettings = applicationOptions.Value;

    public string GetVerificationReport()
    {
        RunReport();
        return _report.ToString();
    }

    private void RunReport()
    {
        CheckServerConfigurationFile();
        CheckRepositoryDirectory();
        CheckGitSettings();
        CheckInternalMembership();
        ExceptionLog();
        DumpMachineVariables();
        DumpAppSettings();
    }
    private void DumpMachineVariables()
    {
        var driveInfo = new DriveInfo(Environment.GetEnvironmentVariable("SystemDrive"));
        var totalMemoryInBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        QuotedReport(Environment.MachineName, "Machine Name");
        QuotedReport(Environment.OSVersion.ToString(), "OS Version");
        QuotedReport(Environment.ProcessorCount, "Processor Count");
        QuotedReport(TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(@"dd\.hh\:mm\:ss"), "System Uptime");
        QuotedReport(Environment.UserInteractive, "User Interactive");
        QuotedReport(Environment.CurrentDirectory, "Current Directory");
        QuotedReport(Environment.SystemPageSize, "System Page Size");
        QuotedReport(Environment.Is64BitOperatingSystem, "Is 64-bit Operating System");
        QuotedReport(Environment.Is64BitProcess, "Is 64-bit Process");
        QuotedReport(Environment.WorkingSet / (1024 * 1024), "Working Set (MB)");
        QuotedReport(Environment.GetEnvironmentVariable("SystemDrive"), "System Drive");
        QuotedReport(Environment.UserDomainName, "User Domain Name");
        QuotedReport(Environment.UserName, "User Name");
        QuotedReport(RuntimeInformation.FrameworkDescription, "Framework Description");
        QuotedReport(RuntimeInformation.OSArchitecture.ToString(), "OS Architecture");
        QuotedReport(RuntimeInformation.ProcessArchitecture.ToString(), "Process Architecture");
        QuotedReport(driveInfo.TotalSize / (1024 * 1024 * 1024), "Total Disk Space (GB)");
        QuotedReport(driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024), "Free Disk Space (GB)");
        QuotedReport(totalMemoryInBytes / (1024 * 1024 * 1024), "Total Physical Memory (GB)");
    }

    private void DumpAppSettings()
    {
        _report.AppendLine("AppSettings");
        foreach (KeyValuePair<string, string> kvp in _configuration.AsEnumerable().OrderBy(x => x.Key))
        {
            QuotedReport(kvp.Value, kvp.Key);
        }
    }

    private void CheckServerConfigurationFile()
    {
        var serverSettings = _settingsService.GetSettings();
        _report.AppendLine("Server Configuration");
        _report.AppendLine(JsonSerializer.Serialize(serverSettings, new JsonSerializerOptions { WriteIndented = true }));

    }

    private void CheckRepositoryDirectory()
    {
        _report.AppendLine("Repo Directory");
        QuotedReport(_applicationSettings.RepositoryPath, "Configured repo path");
        QuotedReport(_pathResolver.GetRepositories(), "Effective repo path");
        ReportDirectoryStatus("Repo dir", _pathResolver.GetRepositories());
    }

    private void CheckGitSettings()
    {
        _report.AppendLine("Git Exe");
        var gitPath = _pathResolver.Resolve(_gitSettings.BinaryPath);
        QuotedReport(gitPath, "Git path");
        SafelyReport("Git.exe exists", () => File.Exists(gitPath));
    }

    private void ReportDirectoryStatus(string text, string directory)
    {
        var sb = new StringBuilder();
        if (Directory.Exists(directory))
        {
            sb.Append($"Exists, {Directory.GetFiles(directory).Length} files, {Directory.GetFileSystemEntries(directory).Length} entries, ");
            sb.Append(DirectoryIsWritable(directory) ? "writeable" : "NOT WRITEABLE");
        }
        else
        {
            sb.Append("Doesn't exist");
        }
        Report(text, sb.ToString());
    }

    private bool DirectoryIsWritable(string directory)
    {
        var probeFile = Path.Combine(directory, "Probe.txt");
        try
        {
            File.WriteAllBytes(probeFile, new byte[16]);
            return true;
        }
        catch (Exception ex)
        {
            Report("Exception probing dir " + directory, ex.Message);
            return false;
        }
        finally
        {
            try
            {
                File.Delete(probeFile);
            }
            catch
            {
                // We deliberately ignore these exceptions, we don't care
            }
        }
    }

    private void CheckInternalMembership()
    {
        _report.AppendLine("Internal Membership");

        if (AppSetting("MembershipService") == "Internal")
        {
            SafelyReport("User count", () => _membershipService.GetAllUsers().Count);
        }
        else
        {
            Report("Not Enabled");
        }
    }

    /// <summary>
    /// Append the last 10K of the exception log to the report
    /// </summary>
    private void ExceptionLog()
    {
        _report.AppendLine("**********************************************************************************");
        _report.AppendLine("Exception Log");
        SafelyRun(() =>
        {
            //var nameFormat = MvcApplication.GetLogFileNameFormat();
            //var todayLogFileName = nameFormat.Replace("{Date}", DateTime.Now.ToString("yyyyMMdd"));
            //SafelyReport("LogFileName: ", () => todayLogFileName);
            //var chunkSize = 10000;
            //var length = new FileInfo(todayLogFileName).Length;
            //Report("Log File total length", length);

            //var startingPoint = Math.Max(0, length - chunkSize);
            //Report("Starting log dump from ", startingPoint);

            //using (var logText = File.Open(todayLogFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //{
            //    logText.Seek(startingPoint, SeekOrigin.Begin);
            //    var reader = new StreamReader(logText);
            //    _report.AppendLine(reader.ReadToEnd());
            //}
        });
    }

    private void SafelyRun(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Report("Diag error", FormatException(ex));
        }
    }

    private void SafelyReport(string tag, Func<object> func)
    {
        try
        {
            var result = func();
            if (result is bool b)
            {
                Report(tag, b ? "OK" : "FAIL");
            }
            else
            {
                Report(tag, result.ToString());
            }
        }
        catch (Exception ex)
        {
            Report(tag, FormatException(ex));
        }
    }

    private string AppSetting(string name)
    {
        return _configuration[name];
    }

    private static string FormatException(Exception ex)
    {
        return "EXCEPTION: " + ex.ToString().Replace("\r\n", "***");
    }

    private void Report(string tag, object value = null)
    {
        if (value != null)
        {
            _report.AppendFormat("--{0}: {1}" + Environment.NewLine, tag, value);
        }
        else
        {
            _report.AppendLine("--" + tag);
        }
    }

    private void QuotedReport(object value, string tag)
    {
        Report(tag, $"'{value}'");
    }
}
