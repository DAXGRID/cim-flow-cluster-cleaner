using System.Text.Json.Serialization;

internal sealed record Setting
{
    /// <summary>
    /// Set this flag to false if you wanna run it using local Kubernetes config.
    /// Otherwise it will use an in cluster configuration.
    /// Default is: True
    /// </summary>
    [JsonPropertyName("inCluster")]
    public required bool InCluster { get; init; } = true;

    /// <summary>
    /// The namespace of the pods.
    /// </summary>
    [JsonPropertyName("podNamespace")]
    public required string PodNamespace { get; init; }

    /// <summary>
    /// The prefix that is used to find jobs that are to be deleted.
    /// </summary>
    [JsonPropertyName("podNamePrefix")]
    public required string PodNamePrefix { get; init; }

    /// <summary>
    /// Max job counts before we start deleting the oldest ones.
    /// </summary>
    [JsonPropertyName("maxJobCount")]
    public int MaxJobCount { get; init; } = 5;

    /// <summary>
    /// The URL to the file server.
    /// </summary>
    [JsonPropertyName("fileServerUrl")]
    public required string FileServerUrl { get; init; }

    /// <summary>
    /// The username to the file server.
    /// </summary>
    [JsonPropertyName("fileServerUsername")]
    public required string FileServerUsername { get; init; }

    /// <summary>
    /// The password to the file server.
    /// </summary>
    [JsonPropertyName("fileServerPassword")]
    public required string FileServerPassword { get; init; }

    /// <summary>
    /// The output folder path.
    /// </summary>
    [JsonPropertyName("outputPath")]
    public string OutputPath { get; init; } = "output";

    /// <summary>
    /// The archive folder path.
    /// </summary>
    [JsonPropertyName("archivePath")]
    public string ArchivePath { get; init; } = "archive";

    /// <summary>
    /// Max files count before the oldest files are deleted.
    /// </summary>
    [JsonPropertyName("maxFilesCount")]
    public int MaxFilesCount { get; init; } = 7;
}
