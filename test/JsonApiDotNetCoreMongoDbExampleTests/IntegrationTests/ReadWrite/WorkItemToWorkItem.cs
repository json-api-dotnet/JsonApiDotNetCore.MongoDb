namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    public sealed class WorkItemToWorkItem
    {
        public WorkItem FromItem { get; set; }
        public string FromItemId { get; set; }

        public WorkItem ToItem { get; set; }
        public string ToItemId { get; set; }
    }
}