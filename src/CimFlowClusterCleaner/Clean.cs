using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using k8s;
using System.Text;

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

        logger.LogInformation("Found {FileCount}, deleting files if exceding {MaxFilesCount} for {Uri}.", files.Count, maxFilesCount, uri);

        foreach (var file in files.OrderBy(x => x.LastWriteTimeUtc).Take(files.Count - maxFilesCount))
        {
            var deletePath = new Uri($"{uri}/{file.Name}");
            logger.LogInformation("Deleting old file {FileName} from {Path}.", file.Name, deletePath);
            await FileServer.DeleteFileAsync(httpClient, deletePath).ConfigureAwait(false);
        }
    }

    private static async Task DeleteOldCimJobsAsync(ILogger logger, Setting setting)
    {
        var config = setting.InCluster
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildConfigFromConfigFile();

        using var kubernetesClient = new Kubernetes(config);

        var labelSelector = $"pod-name-prefix={setting.PodNamePrefix}";
        var pods = (
            await kubernetesClient.CoreV1
            .ListNamespacedPodAsync(setting.PodNamespace, labelSelector: labelSelector)
            .ConfigureAwait(false))
            .Items
            .ToArray();

        logger.LogInformation(
            "Found {PodsCount}, deleting pods if exceding {MaxJobCount} in {PodNamespace} with {Label}.",
            pods.Length,
            setting.MaxJobCount,
            setting.PodNamespace,
            labelSelector
        );

        foreach (var completedJob in pods.OrderBy(x => x.Metadata.CreationTimestamp).Take(pods.Length - setting.MaxJobCount))
        {
            var deletedPod = await kubernetesClient.CoreV1
                .DeleteNamespacedPodAsync(completedJob.Metadata.Name, setting.PodNamespace)
                .ConfigureAwait(false);

            logger.LogInformation(
                "Deleted {PodName} in {PodNamespace} using {LabelSelector}, the pod was created on {CreatedDate}.",
                deletedPod.Metadata.Name,
                deletedPod.Metadata.NamespaceProperty,
                labelSelector,
                deletedPod.Metadata.CreationTimestamp);
        }
    }
}
