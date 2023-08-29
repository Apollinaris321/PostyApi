namespace LearnApi.Services;

public class ServiceResponse<T> : IServiceResponse<T>
{
    public T Data { get; set; }
    public string Message { get; set; } = "";
    public IList<string> Errors { get; set; } = new List<string>();
    public bool Success { get; set; } = false;
    public int CurrentPage { get; set; } = 0;
    public int LastPage { get; set; } = 0;
}