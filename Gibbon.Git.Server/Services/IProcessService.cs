﻿using System.Diagnostics;
using System.Threading.Tasks;

namespace Gibbon.Git.Server.Services;

public interface IProcessService
{
    Task<ProcessResult> StartProcessWithStreamAsync(ProcessStartInfo startInfo, Stream inStream, Stream outStream, bool endStreamWithClose, int bufferSize = 81920);

    Task<ProcessResult> StartProcessAsync(ProcessStartInfo startInfo);
}
