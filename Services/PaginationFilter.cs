using Humanizer.DateTimeHumanizeStrategy;

namespace LearnApi.Services;

public class PaginationFilter
{
    public int CurrentPage { get; set; }
    public int NextPage { get; set; }
    public int PrevPage { get; set; }
    public int FirstPage { get; set; }
    public int LastPage { get; set; }
    public int PageSize { get; set; }
    public int Length { get; set; }

    public PaginationFilter()
    {
        this.FirstPage = 1;
        this.LastPage = 1;
        this.PageSize = 10;
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
        this.FirstPage = 1;
        this.CurrentPage = 1;
        this.PrevPage = 1;
        this.NextPage = this.LastPage > 1 ? 2 : 1;
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
        this.SetNextPage();
        this.SetPrevPage();
    }
 
    public void SetPrevPage()
    {
        if (this.CurrentPage - 1 < 1)
        {
            this.PrevPage = this.FirstPage;
        }
        else
        {
            this.PrevPage = this.CurrentPage - 1;
        }
    }

    public void SetNextPage()
    {
        if (this.CurrentPage + 1 > this.LastPage)
        {
            this.NextPage = this.LastPage;
        }
        else
        {
            this.NextPage = this.CurrentPage + 1;
        }
    }
}