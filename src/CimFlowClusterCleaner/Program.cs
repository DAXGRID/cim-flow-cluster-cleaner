using Microsoft.Extensions.Logging;

namespace CimFlowClusterCleaner;

internal static class Program
{
    public static void Main()
    {
        var logger = LoggerFactory.Create(nameof(Program));

        try
        {
            Clean.Execute(LoggerFactory.Create(nameof(Clean)));
        }
        catch (Exception ex)
        {
            logger.LogCritical("{Exception}", ex);
            throw;
        }
    }
}
