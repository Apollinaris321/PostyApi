namespace LearnApi.Services;

public interface IServiceResponse<T>
{
    public T Data { get; set; } 
    public string Message { get; set; }
    public IList<string> Errors { get; set; }
    public bool Success { get; set; }
    public int CurrentPage { get; set; }
    public int LastPage { get; set; }
}