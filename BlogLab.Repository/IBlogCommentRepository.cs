using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BlogLab.Models.BlogComment;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace BlogLab.Repository
{
    public interface IBlogCommentRepository
    {
        public Task<BlogComment> UpsertAsync(BlogCommentCreate blogCommentCreate, int applicationUserId);
        public Task<List<BlogComment>> GetAllAsync(int blogId);
        public Task<BlogComment> GetAsync(int blogCommentId);
        public Task<int> DeleteAsync(int blogCommentId);
    }

    class BlogCommentRepository : IBlogCommentRepository
    {
        private readonly IConfiguration _configuration;

        public BlogCommentRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<BlogComment> UpsertAsync(BlogCommentCreate blogCommentCreate, int applicationUserId)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("BlogCommentId", typeof(int));
            dataTable.Columns.Add("ParentBlogCommentId", typeof(int));
            dataTable.Columns.Add("BlogId", typeof(int));
            dataTable.Columns.Add("Content", typeof(string));

            dataTable.Rows.Add(
                blogCommentCreate.BlogCommentId, blogCommentCreate.ParentBlogCommentId,
                blogCommentCreate.BlogId, blogCommentCreate.Content);
            int? newBlogCommentId;

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                newBlogCommentId = await connection.ExecuteScalarAsync<int?>(
                    "BlogComment_Upsert",
                    new {BlogComment = dataTable.AsTableValuedParameter("dbo.BlogCommentType")},
                    commandType: CommandType.StoredProcedure
                );
            }
            newBlogCommentId ??= blogCommentCreate.BlogCommentId;
            BlogComment blogComment = await GetAsync(newBlogCommentId.Value);
            return blogComment;
        }

        public async Task<List<BlogComment>> GetAllAsync(int blogId)
        {
            IEnumerable<BlogComment> blogComments;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                blogComments = await connection.QueryAsync<BlogComment>(
                    "BlogComment_GetAll",
                    new {BlogId = blogId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return blogComments.ToList();
        }

        public async Task<BlogComment> GetAsync(int blogCommentId)
        {
            BlogComment blogComment;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                blogComment = await connection.QueryFirstOrDefaultAsync<BlogComment>(
                    "BlogComment_Get",
                    new {BlogCommentId = blogCommentId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return blogComment;
        }

        public async Task<int> DeleteAsync(int blogCommentId)
        {
            int affectedRows = 0;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                affectedRows = await connection.ExecuteAsync(
                    "BlogComment_Delete",
                    new {BlogCommentId = blogCommentId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return affectedRows;
        }
    }
}