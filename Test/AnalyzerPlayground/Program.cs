namespace AnalyzerPlayground;

internal static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            _ = Task.WhenAll();
        }
        catch (OperationCanceledException)
        {
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch
        {
            throw;
        }
        finally
        {
        }
    }
}
