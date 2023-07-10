using System;
using System.IO;
using System.Linq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using UnityEngine;

namespace PKFramework.Logger.EditorConsoleSink
{
    public class EditorConsoleLogEventSink: ILogEventSink
    {
        private readonly ITextFormatter _formatter;

        public EditorConsoleLogEventSink(ITextFormatter formatter) => _formatter = formatter;

        public void Emit(LogEvent logEvent)
        {
            using var buffer = new StringWriter();
            _formatter.Format(logEvent, buffer);

            var logContent = buffer.ToString().Trim();
            var lines = logContent.Count(c => c == '\n');

            var color = logEvent.Level switch
            {
                LogEventLevel.Verbose => "green",
                LogEventLevel.Debug => "green",
                LogEventLevel.Information => "green",
                LogEventLevel.Warning => "yellow",
                LogEventLevel.Error => "red",
                LogEventLevel.Fatal => "red",
                _ => throw new ArgumentOutOfRangeException(nameof(logEvent.Level), "Unknown log level")
            };

            Action<string> logFunc = logEvent.Level switch
            {
                LogEventLevel.Verbose => Debug.Log,
                LogEventLevel.Debug => Debug.Log,
                LogEventLevel.Information => Debug.Log,
                LogEventLevel.Warning => Debug.LogWarning,
                LogEventLevel.Error => Debug.LogError,
                LogEventLevel.Fatal => Debug.LogError,
                _ => throw new ArgumentOutOfRangeException(nameof(logEvent.Level), "Unknown log level")
            };

            if (lines <= 2)
            {
                logFunc.Invoke($"<color={color}>{logContent}</color>");
            }
            else
            {
                var splitLogContent = logContent.Split("\n");
                var twoFirstLines = string.Join("\n", splitLogContent.Take(2));
                var otherLines = string.Join("\n", splitLogContent.Skip(2));
                logFunc.Invoke($"<color={color}>{twoFirstLines}</color>\n<color={color}>{otherLines}</color>");
            }
        }
    }
}