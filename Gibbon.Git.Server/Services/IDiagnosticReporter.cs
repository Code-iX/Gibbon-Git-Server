namespace Gibbon.Git.Server.Services;

/// <summary>
/// This class can produce a textual diagnostic report of Bonobo's configuration
/// The idea is to give a one-shot collection of everything which might be needed to help diagnose problems people are having
/// It's written to be incredible defensive about all the checks it does, so that if things are misconfigured
/// we can still get a complete report
/// </summary>
public interface IDiagnosticReporter
{
    string GetVerificationReport();
}