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
}
