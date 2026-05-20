using System.Text;

namespace AudioBookStudio.Common.Extensions;
public static class ExceptionExtensions
{
    public static string FullMessage(this Exception exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        builder.AppendLine(exception.Message);

        var innerException = exception.InnerException;
        while (innerException != null)
        {
            builder.AppendLine("Inner Exception:");
            builder.AppendLine(innerException.Message);
            innerException = innerException.InnerException;
        }

        if (!string.IsNullOrEmpty(exception.StackTrace))
        {
            builder.AppendLine("Stack Trace:");
            builder.AppendLine(exception.StackTrace);
        }
        return builder.ToString();
    }
}
