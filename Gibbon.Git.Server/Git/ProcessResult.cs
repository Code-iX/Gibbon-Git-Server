﻿namespace Gibbon.Git.Server.Git;

public class ProcessResult
{
    public bool IsSuccess { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
}