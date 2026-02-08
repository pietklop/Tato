namespace Core.Infra;

public static class StringHelper
{
    public static bool HasContent(this string txt)
    {
        return !string.IsNullOrWhiteSpace(txt);
    }
}