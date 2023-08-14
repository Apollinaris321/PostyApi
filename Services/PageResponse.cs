namespace LearnApi.Services;

public class PageResponse<T>
{
     public T Data { get; set; }
     public int currentPage { get; set; } = 1;
     public int lastPage { get; set; } = 1;
}