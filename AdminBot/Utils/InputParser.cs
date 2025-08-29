using System.Globalization;
using System.Text.RegularExpressions;

namespace AdminBot.Utils;

public static class InputParser
{

    /// <summary>
    /// Извлекает числовые значения X, YY, ZZ из строки формата "М8О-XYYБ{В}-ZZ".
    /// </summary>
    /// <param name="input">Входная строка, например, "М8О-123Б{В}-21".</param>
    /// <returns>
    /// Кортеж (X, YY, ZZ) с числами в случае успеха.
    /// Null, если строка не соответствует формату.
    /// </returns>
    public static (int X, int YY, int ZZ)? ParseGroupString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }
        
        const string pattern = @"^М8О-(?<x>\d{1})(?<yy>\d{2})(?<uu>Б|БВ)-(?<zz>\d{2})$";
            
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        
        Match match = regex.Match(input);

        if (match.Success)
        {
            try
            {
                int x = int.Parse(match.Groups["x"].Value);
                int yy = int.Parse(match.Groups["yy"].Value);
                int zz = int.Parse(match.Groups["zz"].Value);

                return (x, yy, zz);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        return null;
    }
    

    /// <summary>
    /// Преобразует строку заданного формата "dd.MM.yyyy HH:mm" в объект DateTime.
    /// </summary>
    /// <param name="dateTimeString">Входная строка, например, "18.08.2025 17:00".</param>
    /// <returns>
    /// Объект DateTime в случае успешного преобразования.
    /// Null, если строка имеет неверный формат.
    /// </returns>
    public static DateTime? ParseDateTimeString(string dateTimeString)
    {
        const string format = "dd.MM.yyyy HH:mm";
        
        if (DateTime.TryParseExact(dateTimeString, format, CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal, out DateTime parsedDateTime))
        {
            return parsedDateTime;
        }
        return null;
    }
}