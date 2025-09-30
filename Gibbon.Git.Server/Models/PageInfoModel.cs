namespace Gibbon.Git.Server.Models;

public class PageInfoModel
{
    public int TotalItems { get; set; }
    public int ItemsPerPage { get; set; }
    public int CurrentPage { get; set; }

    public PageInfoModel()
    {
        CurrentPage = 1;
    }

    // starting item number in the page
    public int PageStart
    {
        get { return (CurrentPage - 1) * ItemsPerPage + 1; }
    }

    // last item number in the page
    public int PageEnd
    {
        get
        {
            int currentTotal = (CurrentPage - 1) * ItemsPerPage + ItemsPerPage;
            return currentTotal < TotalItems ? currentTotal : TotalItems;
        }
    }

    public int LastPage
    {
        get { return (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage); }
    }
}
