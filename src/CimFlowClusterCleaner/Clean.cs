using Microsoft.Extensions.Logging;

namespace CimFlowClusterCleaner;

internal static class Clean
{
    public static void Execute(ILogger logger, Setting setting)
    {
        logger.LogInformation(
            "Running in cluster config {InClusterConfig}.",
            setting.InCluster);

        logger.LogInformation("Starting cleaning process.");
    }
}
