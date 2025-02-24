using k8s;
using Microsoft.Extensions.Logging;

namespace CimFlowClusterCleaner;

internal static class Clean
{
    public static async Task ExecuteAsync(ILogger logger, Setting setting)
    {
        logger.LogInformation(
            "Running in cluster config {InClusterConfig}.",
            setting.InCluster);

        logger.LogInformation("Starting cleaning process.");

        await DeleteOldCimJobsAsync(logger, setting).ConfigureAwait(false);
    }

    private static async Task DeleteOldCimJobsAsync(ILogger logger, Setting setting)
    {
        var config = setting.InCluster
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildConfigFromConfigFile();

        using var kubernetesClient = new Kubernetes(config);

        var labelSelector = $"pod-name-prefix={setting.PodNamePrefix}";
        var pods = await kubernetesClient.CoreV1
            .ListNamespacedPodAsync(setting.PodNamespace, labelSelector: labelSelector)
            .ConfigureAwait(false);

        var completedJobs = pods.Items.OrderBy(x => x.Metadata.CreationTimestamp).ToArray();
        if (completedJobs.Length > setting.MaxJobCount)
        {
            foreach (var completedJob in completedJobs.Take(completedJobs.Length - setting.MaxJobCount))
            {
                var deletedPod = await kubernetesClient.CoreV1
                    .DeleteNamespacedPodAsync(completedJob.Metadata.Name, setting.PodNamespace)
                    .ConfigureAwait(false);

                logger.LogInformation(
                    "Deleted {PodName} in {PodNamespace}",
                    deletedPod.Metadata.Name,
                    deletedPod.Metadata.NamespaceProperty);
            }
        }
        else
        {
            logger.LogInformation(
                "No CIM jobs deleted. Current amount of jobs {CompletedJobs} and max count is {MaxCount}",
                completedJobs.Length,
                setting.MaxJobCount);
        }
    }
}
