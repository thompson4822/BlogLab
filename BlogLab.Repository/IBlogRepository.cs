using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BlogLab.Models.Blog;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace BlogLab.Repository
{
    public interface IBlogRepository
    {
        public Task<Blog> UpsertAsync(BlogCreate blogCreate, int applicationUserId);
        public Task<PagedResults<Blog>> GetAllAsync(BlogPaging blogPaging);
        public Task<Blog> GetAsync(int blogId);
        public Task<List<Blog>> GetAllByUserIdAsync(int applicationUserId);
        public Task<List<Blog>> GetAllFamousAsync(int applicationUserId);
        public Task<int> DeleteAsync(int blogId);
    }

    class BlogRepository : IBlogRepository
    {
        private readonly IConfiguration _configuration;

        public BlogRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Blog> UpsertAsync(BlogCreate blogCreate, int applicationUserId)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("BlogId", typeof(int));
            dataTable.Columns.Add("Title", typeof(string));
            dataTable.Columns.Add("Content", typeof(string));
            dataTable.Columns.Add("PhotoId", typeof(int));
            dataTable.Rows.Add(blogCreate.BlogId, blogCreate.Title, blogCreate.Content, blogCreate.PhotoId);
            int? newBlogId;

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                newBlogId = await connection.ExecuteScalarAsync<int?>(
                    "Blog_Upsert",
                    new {Blog = dataTable.AsTableValuedParameter("dbo.BlogType"), ApplicationUserId = applicationUserId},
                    commandType: CommandType.StoredProcedure
                );
            }

            newBlogId ??= blogCreate.BlogId;
            Blog blog = await GetAsync(newBlogId.Value);
            return blog;
        }

        public async Task<PagedResults<Blog>> GetAllAsync(BlogPaging blogPaging)
        {
            var results = new PagedResults<Blog>();
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var multi = await connection.QueryMultipleAsync(
                    "Blog_All",
                    new
                    {
                        Offset = (blogPaging.Page - 1) * (blogPaging.PageSize),
                        PageSize = blogPaging.PageSize
                    },
                    commandType: CommandType.StoredProcedure))
                {
                    results.Items = multi.Read<Blog>();
                    results.TotalCount = multi.ReadFirst<int>();
                }
            }
            return results;
        }

        public async Task<Blog> GetAsync(int blogId)
        {
            Blog blog;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                blog = await connection.QueryFirstOrDefaultAsync<Blog>(
                    "Blog_Get",
                    new {BlogId = blogId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return blog;
        }

        public async Task<List<Blog>> GetAllByUserIdAsync(int applicationUserId)
        {
            IEnumerable<Blog> blogs;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                blogs = await connection.QueryAsync<Blog>(
                    "Blog_GetByUserId",
                    new {ApplicationUserId = applicationUserId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return blogs.ToList();
        }

        public async Task<List<Blog>> GetAllFamousAsync(int applicationUserId)
        {
            IEnumerable<Blog> blogs;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                blogs = await connection.QueryAsync<Blog>(
                    "Blog_GetAllFamous",
                    new { },
                    commandType: CommandType.StoredProcedure
                );
            }
            return blogs.ToList();
        }

        public async Task<int> DeleteAsync(int blogId)
        {
            int affectedRows = 0;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                affectedRows = await connection.ExecuteAsync(
                    "Blog_Delete",
                    new {BlogId = blogId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return affectedRows;
        }
    }
}