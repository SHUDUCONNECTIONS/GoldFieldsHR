namespace GoldFieldsHR.Application.Common;

/// <summary>
/// Character-class patterns for request fields that should be restricted to "words" or
/// "numbers" from a user's perspective, shared across validators so the rule stays
/// consistent everywhere the same kind of field appears.
/// </summary>
public static class ValidationPatterns
{
    /// <summary>Letters, spaces, hyphens, and apostrophes — for person names (e.g. "Mary-Anne O'Neil").</summary>
    public const string PersonName = @"^[A-Za-z\s'-]+$";

    /// <summary>Letters, digits, and hyphens — employee numbers are alphanumeric IDs (e.g. "LM-1024"), not pure numbers.</summary>
    public const string EmployeeNumber = @"^[A-Za-z0-9-]+$";

    /// <summary>Digits plus common phone formatting characters (spaces, +, -, parentheses).</summary>
    public const string PhoneNumber = @"^[0-9+()\s-]+$";
}
