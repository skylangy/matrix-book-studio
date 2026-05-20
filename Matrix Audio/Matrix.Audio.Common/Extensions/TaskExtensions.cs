namespace Matrix.Audio.Common.Extensions;
public static class TaskExtensions
{
    public static TResult RunWithRetry<T, TResult>(
        this Func<T, TResult> func,
        T input,
        int maxAttempts = 3,
        int delayMilliseconds = 1000)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));
        if (maxAttempts <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), "Max attempts must be greater than zero.");

        int attempt = 0;
        Exception lastException;

        while (attempt < maxAttempts)
        {
            try
            {
                return func(input);
            }
            catch (Exception ex)
            {
                lastException = ex;
                attempt++;

                if (attempt >= maxAttempts)
                {
                    throw new Exception($"Operation failed after {maxAttempts} attempts.", lastException);
                }

                Thread.Sleep(delayMilliseconds);
            }
        }

        throw new InvalidOperationException("Unexpected state in Retry method.");
    }
}
