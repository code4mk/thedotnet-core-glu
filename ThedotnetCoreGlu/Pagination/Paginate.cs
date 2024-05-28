using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using ThedotnetCoreGlu.Request;

namespace ThedotnetCoreGlu.Pagination
{
    public class Paginate<T> where T : class
    {
        private readonly HttpContext _httpContext;

        public Paginate(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public async Task<object> GetPaginate(IQueryable<T> query, string wrap = "data")
        {
            var queryParams = await RequestHelper.the_query(_httpContext.Request);
            var page = queryParams.ContainsKey("page") ? Convert.ToInt32(queryParams["page"]) : 1;
            var perPage = queryParams.ContainsKey("per_page") ? Convert.ToInt32(queryParams["per_page"]) : 10;

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / perPage);

            var pagedQuery = query
                .Skip((page - 1) * perPage)
                .Take(perPage);

            var data = await pagedQuery.ToListAsync();

            var baseUrl = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}{_httpContext.Request.Path}";

            var nextPageUrl = page < totalPages ? $"{baseUrl}?page={page + 1}&per_page={perPage}" : null;
            var previousPageUrl = page > 1 ? $"{baseUrl}?page={page - 1}&per_page={perPage}" : null;

            dynamic response = new ExpandoObject();
            response.current_page = page;
            response.from = (page - 1) * perPage + 1;
            response.to = Math.Min(page * perPage, totalCount);
            response.per_page = perPage;
            response.total = totalCount;
            response.last_page = totalPages;
            response.next_page_url = nextPageUrl;
            response.previous_page_url = previousPageUrl;

            ((IDictionary<string, object>)response).Add(wrap, data);

            return response;
        }
    }
}
