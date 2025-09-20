namespace Gastos.Shared.Entities.Request;

public class PaginationParameters
{
    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        set => _page = value <= 0 ? 1 : value;
    }
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 1 : value;
    }

    public void UpdatePageInfo(int page, int pageSize)
    {
        Page = page + 1;
        PageSize = pageSize;
    }
}
