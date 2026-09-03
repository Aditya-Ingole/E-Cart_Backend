namespace ECart.Application.DTOs.Product
{
    public class ProductFilterDto
    {
        // Search
        public string? Search { get; set; }

        // Filter
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public bool? InStock { get; set; }

        // Sort
        public string SortBy { get; set; } = "newest";
        // Options: "newest", "price_asc", "price_desc", "rating", "name_asc", "name_desc"

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}