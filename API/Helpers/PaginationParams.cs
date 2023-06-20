namespace API.Helpers
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _PageSize = 10;
        public int PageNumber { get; set; }

        public int PageSize
        {
            get => _PageSize;
            set => _PageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

    }
}