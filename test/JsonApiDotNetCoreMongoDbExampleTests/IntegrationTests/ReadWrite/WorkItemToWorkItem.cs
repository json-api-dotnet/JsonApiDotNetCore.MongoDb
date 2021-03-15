using JetBrains.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkItemToWorkItem
    {
        public WorkItem FromItem { get; set; }
        public string FromItemId { get; set; }

        public WorkItem ToItem { get; set; }
        public string ToItemId { get; set; }
    }
}
