using Serilog.Core;
using Serilog.Events;

namespace Matrix.Audio.Server.Common;

public class ClassNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            var fullName = sourceContext.ToString().Trim('"');
            var className = fullName.Split('.').Last(); // Extract the class name
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("SourceContext", className));
        }
    }
}
