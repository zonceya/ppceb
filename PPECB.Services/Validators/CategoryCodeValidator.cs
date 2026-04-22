using System.Text.RegularExpressions;

namespace PPECB.Services.Validators;

public interface ICategoryCodeValidator
{
    bool IsValidFormat(string code);
    string GetFormatDescription();
}

public class CategoryCodeValidator : ICategoryCodeValidator
{
    private static readonly Regex CodeRegex = new Regex(@"^[A-Z]{3}[0-9]{3}$", RegexOptions.Compiled);

    public bool IsValidFormat(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        return CodeRegex.IsMatch(code.ToUpper());
    }

    public string GetFormatDescription()
    {
        return "3 letters followed by 3 numbers (e.g., ABC123)";
    }
}