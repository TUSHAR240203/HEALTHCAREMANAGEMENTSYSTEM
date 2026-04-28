namespace Hms.DoctorsApi.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? TraceId { get; set; }

    public static ApiResponse<T> Ok(T? data, string message = "Success", string? traceId = null) => new()
    {
        Success = true,
        Message = message,
        Data = data,
        TraceId = traceId
    };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null, string? traceId = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors?.ToList() ?? new List<string>(),
        TraceId = traceId
    };
}
