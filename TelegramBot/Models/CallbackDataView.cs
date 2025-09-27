namespace TelegramBot.Models;

public class CallbackDataView
{
    public required string CallbackName { get; set; } // name or "page"
    public required string CallbackParam { get; set; } // returning value 
    public string? ExtraParam { get; set; } = null; // page number or null
    

    public override string ToString()
    {
        if (ExtraParam is null)
        {
            return $"{CallbackName}:{CallbackParam}";
        }
        return $"{CallbackName}:{CallbackParam}:{ExtraParam}";
    }

    public static CallbackDataView GenerateCallback(string a, string b, string? c = null)
    {
        CallbackDataView callbackData = new CallbackDataView()
        {
            CallbackName = a,
            CallbackParam = b,
            ExtraParam = c,
        };
        
        return callbackData;
    }

    public static CallbackDataView LoadFromCallback(string callback)
    {
        string[] split = callback.Split(':');
        
        if (split.Length < 2)
        {
            throw new ArgumentException("Invalid callback format");
        }
        
        CallbackDataView callbackData = new CallbackDataView()
        {
            CallbackName = split[0],
            CallbackParam = split[1],
            ExtraParam = split.Length == 3 ? split[2] : null,
        };
        
        return callbackData;
    }
}