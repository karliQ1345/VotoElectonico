namespace VotoElect.MVC.ApiContracts;

public class ApiResponse<T>
{
    public bool Ok { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }
}

