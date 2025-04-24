using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Collections;

namespace TestBuildingBlocks;

public static class FluentMetaExtensions
{
    /// <summary>
    /// Asserts that a "meta" dictionary contains a single element named "total" with the specified value.
    /// </summary>
    [CustomAssertion]
#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    public static void ContainTotal(this GenericDictionaryAssertions<IDictionary<string, object?>, string, object?> source, int expected,
        string? keyName = null)
#pragma warning restore AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    {
        JsonElement element = GetMetaJsonElement(source, keyName ?? "total");
        element.GetInt32().Should().Be(expected);
    }

    /// <summary>
    /// Asserts that a "meta" dictionary contains a single element named "requestBody" that isn't empty.
    /// </summary>
    [CustomAssertion]
    public static void HaveRequestBody(this GenericDictionaryAssertions<IDictionary<string, object?>, string, object?> source)
    {
        JsonElement element = GetMetaJsonElement(source, "requestBody");
        element.ToString().Should().NotBeEmpty();
    }

    private static JsonElement GetMetaJsonElement(GenericDictionaryAssertions<IDictionary<string, object?>, string, object?> source, string metaKey)
    {
        object? value = source.ContainKey(metaKey).WhoseValue;
        return value.Should().BeOfType<JsonElement>().Subject;
    }
}
