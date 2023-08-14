using System.Configuration;
using Humanizer.DateTimeHumanizeStrategy;

namespace LearnApi.Services;

public class PaginationFilter
{
    public int CurrentPage { get; set; }
    public int LastPage { get; set; }
    public int PageSize { get; set; }
    public int Offset { get; set; }

    public PaginationFilter()
    {
        this.LastPage = 1;
        this.CurrentPage = 1;
        this.PageSize = 10;
        this.Offset = 1;
    }

    public PaginationFilter(int pageSize, int length)
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
    }

    public void SetCurrentPage(int number)
    {
        if (number > LastPage)
        {
            this.CurrentPage = this.LastPage;
            this.Offset = (this.CurrentPage - 1) * this.PageSize;
        }
        else if (number < 1)
        {
            this.CurrentPage = 1;
            this.Offset = 0;
        }
        else
        {
            this.CurrentPage = number;
            this.Offset = (this.CurrentPage - 1) * this.PageSize;
        }
    }
}