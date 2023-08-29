using Humanizer.DateTimeHumanizeStrategy;

namespace LearnApi.Services;

public class PaginationFilter
{
    public int CurrentPage { get; set; }
    public int LastPage { get; set; }
    public int PageSize { get; set; }

    public PaginationFilter()
    {
        this.LastPage = 1;
        this.CurrentPage = 1;
        this.PageSize = 10;
    }

    public PaginationFilter(int currentPage, int pageSize, int length)
    {
        if (pageSize > 10)
        {
            this.PageSize = 10;
        }else if (pageSize < 1)
        {
            this.PageSize = 1;
        }
        else
        {
            this.PageSize = pageSize;
        }
        this.LastPage = (int)Math.Ceiling((float)length / (float)this.PageSize);
        if (this.LastPage == 0)
        {
            this.LastPage = 1;
        }
        
        this.SetCurrentPage(currentPage);
    }

    public int Skip()
    {
        return (this.CurrentPage - 1) * this.PageSize;
    }

    public void SetCurrentPage(int number)
    {
        if (number > LastPage)
        {
            this.CurrentPage = this.LastPage;
        }
        else if (number < 1)
        {
            this.CurrentPage = 1;
        }
        else
        {
            this.CurrentPage = number;
        }
    }
}