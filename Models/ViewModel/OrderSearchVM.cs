using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace Web_PC_Builder.Models.ViewModel
{
    public class OrderSearchVM
    {
        public string SearchTerm { get; set; }          // mã đơn / tên KH / SĐT
        public string Status { get; set; }              // lọc theo OrderStatus
        public int? CustomerID { get; set; }            // xem đơn của 1 khách hàng cụ thể
        public Customer Customer { get; set; }          // thông tin khách hàng đang lọc (nếu có)
        public string SortOrder { get; set; }           // date_asc | amount_desc | amount_asc
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IPagedList<Order> Orders { get; set; }
    }
}
