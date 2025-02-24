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
}
