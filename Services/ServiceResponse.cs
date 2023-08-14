namespace LearnApi.Services;

public class ServiceResponse<T>
{
    public T Data { get; set; }
    public int LastPage { get; set; } = 1;
    public int CurrentPage { get; set; } = 1;
    public bool Success { get; set; } = true;
    public string Message { get; set; } = null;
    public string Error { get; set; } = null;
    public List<string> ErrorMessages { get; set; } = null;
}