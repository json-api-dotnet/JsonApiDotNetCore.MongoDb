using FluentAssertions;
using FluentAssertions.Numeric;
using FluentAssertions.Primitives;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

// ReSharper disable UnusedMethodReturnValue.Global

namespace TestBuildingBlocks;

public static class FluentExtensions
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

    // Workaround for source.Should().NotBeNull().And.Subject having declared type 'object'.
    [System.Diagnostics.Contracts.Pure]
    public static StrongReferenceTypeAssertions<T> RefShould<T>([SysNotNull] this T? actualValue)
        where T : class
    {
        actualValue.Should().NotBeNull();
        return new StrongReferenceTypeAssertions<T>(actualValue);
    }

    public static void With<T>(this T subject, [InstantHandle] Action<T> continuation)
    {
        continuation(subject);
    }

    public sealed class StrongReferenceTypeAssertions<TReference>(TReference subject)
        : ReferenceTypeAssertions<TReference, StrongReferenceTypeAssertions<TReference>>(subject)
    {
        protected override string Identifier => "subject";
    }
}
