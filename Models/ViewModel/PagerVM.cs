using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace Web_PC_Builder.Models.ViewModel
{
    /// <summary>Dữ liệu cho partial view _Pager (thanh phân trang dùng chung cho các trang quản trị).</summary>
    public class PagerVM
    {
        public PagerVM(IPagedList list, Func<int, string> urlFor)
        {
            List = list;
            UrlFor = urlFor;
        }

        public IPagedList List { get; private set; }
        public Func<int, string> UrlFor { get; private set; }
    }
}
