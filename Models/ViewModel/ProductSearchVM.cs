using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace Web_PC_Builder.Models.ViewModel
{
    /// <summary>Tiêu chí tìm kiếm + kết quả (đã phân trang) của trang Quản lý sản phẩm.</summary>
    public class ProductSearchVM
    {
        public string SearchTerm { get; set; }          // từ khóa trong tên / mô tả / danh mục / thương hiệu
        public decimal? MinPrice { get; set; }          // giá từ
        public decimal? MaxPrice { get; set; }          // giá đến
        public string SortOrder { get; set; }           // name_asc | name_desc | price_asc | price_desc
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IPagedList<Product> Products { get; set; }
    }
}
