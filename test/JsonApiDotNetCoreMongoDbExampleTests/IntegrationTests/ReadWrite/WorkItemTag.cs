using JetBrains.Annotations;

namespace JsonApiDotNetCoreMongoDbExampleTests.IntegrationTests.ReadWrite
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class WorkItemTag
    {
        public WorkItem Item { get; set; }
        public string ItemId { get; set; }

        public WorkTag Tag { get; set; }
        public string TagId { get; set; }
    }
}
