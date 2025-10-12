namespace AdminUI.Services;

using Microsoft.JSInterop;


public class FileDownloader
{
    private readonly IJSRuntime _jsRuntime;

    public FileDownloader(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task DownloadFileFromStreamAsync(string fileName, Stream stream)
    {
        // Используем встроенный в .NET механизм для работы с потоками в Blazor
        using var streamRef = new DotNetStreamReference(stream);
        
        // Вызываем JavaScript-функцию, которая "скачает" поток данных как файл
        await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}