using Microsoft.Extensions.Logging;

namespace CimFlowClusterCleaner;

internal static class Clean
{
    public static void Execute(ILogger logger)
    {
        logger.LogInformation("Starting cleaning process.");
    }
}
