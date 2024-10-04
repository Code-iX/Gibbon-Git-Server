using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Gibbon.Git.Server.Helpers;

public class FileResult(string data, string name) : IActionResult
{
    private readonly string _data = data;
    private readonly string _name = name;

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;

        if (!string.IsNullOrEmpty(_name))
        {
            // Füge den Content-Disposition Header hinzu, um den Download zu initiieren
            response.Headers["Content-Disposition"] = $"attachment; filename={_name}";
        }

        // Setze den richtigen Content-Type, z. B. als Text-Datei
        response.ContentType = "application/octet-stream"; // Oder ein anderer passender ContentType

        // Schreibe die Daten in die Antwort
        if (!string.IsNullOrEmpty(_data))
        {
            var buffer = Encoding.UTF8.GetBytes(_data);
            await response.Body.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}