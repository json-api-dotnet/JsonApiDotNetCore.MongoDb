using FluentAssertions;
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
}
