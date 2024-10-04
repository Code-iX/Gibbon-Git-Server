using System.Diagnostics;
using System.Threading.Tasks;

namespace Gibbon.Git.Server.Git;

public class ProcessService : IProcessService
{
    public async Task<ProcessResult> StartProcessWithStreamAsync(ProcessStartInfo startInfo, Stream inStream, Stream outStream, bool endStreamWithClose, int bufferSize = 81920)
    {
        var result = new ProcessResult();

        try
        {
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start process.");

            // Schreibe Eingabestream zum Prozess, aber mit einem Buffer
            if (inStream != null)
            {
                await inStream.CopyToAsync(process.StandardInput.BaseStream, bufferSize);
            }

            // Schließe den Eingabestream, wenn gewünscht
            if (endStreamWithClose)
            {
                process.StandardInput.Close();
            }
            else
            {
                await process.StandardInput.WriteAsync('\0');
            }

            // Kopiere den Ausgabestream des Prozesses, ebenfalls mit einem Buffer
            await process.StandardOutput.BaseStream.CopyToAsync(outStream, bufferSize);

            // Warte auf das Ende des Prozesses
            await process.WaitForExitAsync();

            result.IsSuccess = process.ExitCode == 0;
            result.Output = "Process completed successfully.";
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Error = $"An error occurred: {ex.Message}";
        }

        return result;
    }

    public async Task<bool> RunProcessAsync(ProcessStartInfo startInfo)
    {
        try
        {
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return false;
            }

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ProcessResult> StartProcessAsync(ProcessStartInfo startInfo)
    {
        var result = new ProcessResult();
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                result.IsSuccess = false;
                result.Error = "Failed to start process.";
                return result;
            }

            outputBuilder.Append(await process.StandardOutput.ReadToEndAsync());
            errorBuilder.Append(await process.StandardError.ReadToEndAsync());

            await process.WaitForExitAsync();

            result.Output = outputBuilder.ToString();
            result.Error = errorBuilder.ToString();
            result.IsSuccess = process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Error = $"An error occurred: {ex.Message}";
        }

        return result;
    }
}
