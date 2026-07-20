

using static Mango.Web.SD;

namespace Mango.Web.Models;

public class RequestDto
{
    public ApiType ApiType { get; set; } = SD.ApiType.GET;
    public string Url { get; set; }
    public object? Data { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public ContentType ContentType { get; set; } = ContentType.Json;

}