using JetBrains.Annotations;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Filtering;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreMongoDbTests.IntegrationTests.QueryStrings.Filtering")]
public sealed class FilterableResource : HexStringMongoIdentifiable
{
    [Attr]
    public string SomeString { get; set; } = string.Empty;

    [Attr]
    public string? SomeNullableString { get; set; }

    [Attr]
    public bool SomeBoolean { get; set; }

    [Attr]
    public bool? SomeNullableBoolean { get; set; }

    [Attr]
    public int SomeInt32 { get; set; }

    [Attr]
    public int? SomeNullableInt32 { get; set; }

    [Attr]
    public int OtherInt32 { get; set; }

    [Attr]
    public int? OtherNullableInt32 { get; set; }

    [Attr]
    public ulong SomeUnsignedInt64 { get; set; }

    [Attr]
    public ulong? SomeNullableUnsignedInt64 { get; set; }

    [Attr]
    public decimal SomeDecimal { get; set; }

    [Attr]
    public decimal? SomeNullableDecimal { get; set; }

    [Attr]
    public double SomeDouble { get; set; }

    [Attr]
    public double? SomeNullableDouble { get; set; }

    [Attr]
    public Guid SomeGuid { get; set; }

    [Attr]
    public Guid? SomeNullableGuid { get; set; }

    [Attr]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime SomeDateTimeInLocalZone { get; set; }

    [Attr]
    public DateTime SomeDateTimeInUtcZone { get; set; }

    [Attr]
    public DateTime? SomeNullableDateTime { get; set; }

    [Attr]
    public DateTimeOffset SomeDateTimeOffset { get; set; }

    [Attr]
    public DateTimeOffset? SomeNullableDateTimeOffset { get; set; }

    [Attr]
    public TimeSpan SomeTimeSpan { get; set; }

    [Attr]
    public TimeSpan? SomeNullableTimeSpan { get; set; }

    [Attr]
    public DateOnly SomeDateOnly { get; set; }

    [Attr]
    public DateOnly? SomeNullableDateOnly { get; set; }

    [Attr]
    public TimeOnly SomeTimeOnly { get; set; }

    [Attr]
    public TimeOnly? SomeNullableTimeOnly { get; set; }

    [Attr]
    public DayOfWeek SomeEnum { get; set; }

    [Attr]
    public DayOfWeek? SomeNullableEnum { get; set; }

    [HasOne]
    [BsonIgnore]
    public FilterableResource? Parent { get; set; }

    [HasMany]
    [BsonIgnore]
    public ICollection<FilterableResource> Children { get; set; } = new List<FilterableResource>();
}
