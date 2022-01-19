using NuGet.Common;
using System;
using System.Threading.Tasks;

namespace PackageSourceMapper.Logging
{
    internal class Logger : ILogger
    {
        public LogLevel VerbosityLevel { get; set; } = LogLevel.Debug;

        public void Log(LogLevel level, string data)
        {
            if (DisplayMessage(level))
            {
                Console.WriteLine(data);
            }
        }

        /// <summary>
        /// True if the message meets the verbosity level.
        /// </summary>
        public virtual bool DisplayMessage(LogLevel messageLevel)
        {
            return (messageLevel >= VerbosityLevel);
        }

        public virtual void LogDebug(string data)
        {
            Log(LogLevel.Debug, data);
        }

        public virtual void LogError(string data)
        {
            Log(LogLevel.Error, data);
        }

        public virtual void LogInformation(string data)
        {
            Log(LogLevel.Information, data);
        }

        public virtual void LogInformationSummary(string data)
        {
            Log(LogLevel.Information, data);
        }

        public virtual void LogMinimal(string data)
        {
            Log(LogLevel.Minimal, data);
        }

        public virtual void LogVerbose(string data)
        {
            Log(LogLevel.Verbose, data);
        }

        public virtual void LogWarning(string data)
        {
            Log(LogLevel.Warning, data);
        }

        public Task LogAsync(LogLevel level, string data)
        {
            throw new NotImplementedException();
        }

        public void Log(ILogMessage message)
        {
            throw new NotImplementedException();
        }

        public Task LogAsync(ILogMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
