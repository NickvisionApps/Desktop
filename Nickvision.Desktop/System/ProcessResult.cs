namespace Nickvision.Desktop.System;

public class ProcessResult
{
    public int ExitCode { get; }
    public string Output { get; }
    public string Error { get; }

    public ProcessResult(int exitCode, string output, string error)
    {
        ExitCode = exitCode;
        Output = output;
        Error = error;
    }
}
