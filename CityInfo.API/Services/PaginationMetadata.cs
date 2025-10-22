using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{
    public class PaginationMetadata
    {

        public int TotalItemsCount { get; set; }
        public int TotalPagesCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        public PaginationMetadata(int totalItemsCount, int currentPage, int pageSize)
        {
            TotalItemsCount = totalItemsCount;
            TotalPagesCount = (int)Math.Ceiling((double)totalItemsCount / pageSize);
            CurrentPage = currentPage;
            PageSize = pageSize;
        }
        
    }
}