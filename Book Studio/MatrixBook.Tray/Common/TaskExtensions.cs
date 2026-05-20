namespace MatrixBook.Tray.Common;
public static class TaskExtensions
{
    public static async Task<T> Retry<T>(this Task<T> task, int maxRetries = 8, int retryIntervalMs = 5000)
    {
        int retries = 0;

        while (retries < maxRetries)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (Exception)
            {
                retries++;

                await Task.Delay(retryIntervalMs).ConfigureAwait(false);
            }
        }

        return await task.ConfigureAwait(false);
    }

    public static async Task Retry(this Func<Task> func, int retryCount = 3, int delayMilliseconds = 1000, Action<Exception>? exceptionHandler = null)
    {
        ArgumentNullException.ThrowIfNull(func, nameof(func));

        for (int attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                await func();
                return; // Exit if the function succeeds
            }
            catch (Exception ex)
            {
                exceptionHandler?.Invoke(ex);
                if (attempt == retryCount - 1)
                    throw; // Rethrow if it's the last attempt
                await Task.Delay(delayMilliseconds); // Wait before retrying
            }
        }
    }

}
