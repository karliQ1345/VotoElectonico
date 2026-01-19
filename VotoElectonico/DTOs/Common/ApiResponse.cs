namespace TuProyecto.DTOs.Common;

public class ApiResponse<T>
{
    public bool Ok { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }

    public static ApiResponse<T> Success(T data, string message = "")
        => new() { Ok = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message)
        => new() { Ok = false, Message = message, Data = default };
}

