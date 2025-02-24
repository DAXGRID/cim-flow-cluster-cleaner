namespace CimFlowClusterCleaner;

internal static class Program
{
    public static void Main()
    {
        Clean.Execute(LoggerFactory.Create(nameof(Clean)));
    }
}
