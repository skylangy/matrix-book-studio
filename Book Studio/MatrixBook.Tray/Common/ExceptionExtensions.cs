using System.Text;

namespace MatrixBook.Tray.Common;
public static class ExceptionExtensions
{
    public static string GetFullMessage(this Exception exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }
        var message = new StringBuilder();
        var currentException = exception;
        while (currentException != null)
        {
            message.AppendLine(currentException.Message);
            currentException = currentException.InnerException;
        }
        return message.ToString().TrimEnd();
    }
}
