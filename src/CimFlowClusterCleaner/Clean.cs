using System.Net.Http.Headers;
using System.Text;
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

        logger.LogInformation("Starting cleaning old CIM jobs.");
        await DeleteOldCimJobsAsync(logger, setting).ConfigureAwait(false);

        logger.LogInformation("Deleting files in the archive folder on the file server.");
        await DeleteOldFilesAsync(
            logger,
            new Uri($"{setting.FileServerUrl}/{setting.ArchivePath}"),
            setting.MaxFilesCount,
            setting.FileServerUsername,
            setting.FileServerPassword
        ).ConfigureAwait(false);

        logger.LogInformation("Deleting files in the output folder on the file server.");
        await DeleteOldFilesAsync(
            logger,
            new Uri($"{setting.FileServerUrl}/{setting.OutputPath}"),
            setting.MaxFilesCount,
            setting.FileServerUsername,
            setting.FileServerPassword
        ).ConfigureAwait(false);
    }

    private static async Task DeleteOldFilesAsync(
        ILogger logger,
        Uri uri,
        int maxFilesCount,
        string username,
        string password)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{username}:{password}")));

        var files = (await FileServer.GetFilesAsync(httpClient, uri).ConfigureAwait(false))
            .ToArray()
            .AsReadOnly();

        foreach (var file in files.OrderBy(x => x.LastWriteTimeUtc).Take(maxFilesCount - files.Count))
        {
            logger.LogInformation("Deleting old file {FileName} from {Path}.", file.Name, uri);
            await FileServer.DeleteFileAsync(httpClient, uri).ConfigureAwait(false);
        }
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
                    "Deleted {PodName} in {PodNamespace} that was {CreatedDate}.",
                    deletedPod.Metadata.Name,
                    deletedPod.Metadata.NamespaceProperty,
                    deletedPod.Metadata.CreationTimestamp);
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
