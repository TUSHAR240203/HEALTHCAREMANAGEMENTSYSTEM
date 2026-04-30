namespace Hms.DoctorsApi.Common;

public sealed class PaginationParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 5;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : Math.Max(1, value);
    }

    public int Skip => (Math.Max(1, PageNumber) - 1) * PageSize;
}
