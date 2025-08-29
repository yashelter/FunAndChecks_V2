using System.Globalization;

namespace TelegramBot.Utils;

public static class StringUtil
{
    public static string ToProperNameCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        
        var textInfo = CultureInfo.CurrentCulture.TextInfo;

        var words = input.Split(' ');

        var processedWords = words.Select(word => 
        {
            var subWords = word.Split('-');
            var processedSubWords = subWords.Select(subWord => textInfo.ToTitleCase(subWord));
            return string.Join("-", processedSubWords);
        });

        return string.Join(" ", processedWords);
    }
}