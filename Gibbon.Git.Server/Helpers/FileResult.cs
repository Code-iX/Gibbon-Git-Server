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
            response.Headers["Content-Disposition"] = $"attachment; filename={_name}";
        }

        response.ContentType = "application/octet-stream";

        if (!string.IsNullOrEmpty(_data))
        {
            var buffer = Encoding.UTF8.GetBytes(_data);
            await response.Body.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
