using Microsoft.Extensions.Logging;

namespace CimFlowClusterCleaner;

internal static class Program
{
    public static async Task Main()
    {
        var setting = AppSetting.Load<Setting>();
        var logger = LoggerFactory.Create(nameof(Program));

        try
        {
            await Clean
                .ExecuteAsync(LoggerFactory.Create(nameof(Clean)), setting)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogCritical("{Exception}", ex);
            throw;
        }
    }
}
