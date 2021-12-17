using JsonApiDotNetCore.Resources;

#pragma warning disable AV1008 // Class should not be static

namespace TestBuildingBlocks;

public static class Unknown
{
    public static class StringId
    {
        private const string StringValue = "ffffffffffffffffffffffff";

        public static string For<TResource, TId>()
            where TResource : IIdentifiable<TId>
        {
            Type type = typeof(TId);

            if (type == typeof(string))
            {
                return StringValue;
            }

            throw new NotSupportedException(
                $"Unsupported '{nameof(Identifiable<object>.Id)}' property of type '{type}' on resource type '{typeof(TResource).Name}'.");
        }
    }
}
