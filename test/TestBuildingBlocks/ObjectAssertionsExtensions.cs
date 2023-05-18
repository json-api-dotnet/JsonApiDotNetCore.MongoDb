using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Numeric;
using JetBrains.Annotations;

namespace TestBuildingBlocks;

[PublicAPI]
public static class ObjectAssertionsExtensions
{
    private const decimal NumericPrecision = 0.00000000001M;

    /// <summary>
    /// Same as <see cref="NumericAssertionsExtensions.BeApproximately(NullableNumericAssertions{decimal}, decimal?, decimal, string, object[])" />, but with
    /// default precision.
    /// </summary>
    [CustomAssertion]
    public static AndConstraint<NullableNumericAssertions<decimal>> BeApproximately(this NullableNumericAssertions<decimal> parent, decimal? expectedValue,
        string because = "", params object[] becauseArgs)
    {
        return parent.BeApproximately(expectedValue, NumericPrecision, because, becauseArgs);
    }

    /// <summary>
    /// Asserts that a "meta" dictionary contains a single element named "total" with the specified value.
    /// </summary>
    [CustomAssertion]
    public static void ContainTotal(this GenericDictionaryAssertions<IDictionary<string, object?>, string, object?> source, int expectedTotal)
    {
        source.ContainKey("total").WhoseValue.Should().BeOfType<JsonElement>().Subject.GetInt32().Should().Be(expectedTotal);
    }
}
