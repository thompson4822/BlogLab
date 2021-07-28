using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BlogLab.Models.Account;
using BlogLab.Models.Photo;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace BlogLab.Repository
{
    public interface IPhotoRepository
    {
        public Task<Photo> InsertAsync(PhotoCreate photoCreate, int applicationUserId);
        public Task<Photo> GetAsync(int photoId);
        public Task<List<Photo>> GetAllByUserIdAsync(int applicationUserId);
        public Task<int> DeleteAsync(int photoId);
    }

    class PhotoRepository : IPhotoRepository
    {
        private readonly IConfiguration _configuration;

        public PhotoRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Photo> InsertAsync(PhotoCreate photoCreate, int applicationUserId)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("PublicId", typeof(string));
            dataTable.Columns.Add("ImageUrl", typeof(string));
            dataTable.Columns.Add("Description", typeof(string));

            dataTable.Rows.Add(photoCreate.PublicId, photoCreate.ImageUrl, photoCreate.Description);
            int newPhotoId;
            
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                newPhotoId = await connection.ExecuteScalarAsync<int>(
                    "Photo_Insert",
                    new {Photo = dataTable.AsTableValuedParameter("dbo.PhotoType")},
                    commandType: CommandType.StoredProcedure
                );
            }
            Photo photo = await GetAsync(newPhotoId);
            return photo;
        }

        public async Task<Photo> GetAsync(int photoId)
        {
            Photo photo;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                photo = await connection.QueryFirstOrDefaultAsync<Photo>(
                    "Photo_Get",
                    new {PhotoId = photoId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return photo;
        }

        public async Task<List<Photo>> GetAllByUserIdAsync(int applicationUserId)
        {
            IEnumerable<Photo> photos;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                photos = await connection.QueryAsync<Photo>(
                    "Photo_GetByUserId",
                    new {ApplicationUserId = applicationUserId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return photos.ToList();
        }

        public async Task<int> DeleteAsync(int photoId)
        {
            int affectedRows = 0;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                affectedRows = await connection.ExecuteAsync(
                    "Photo_Delete",
                    new {PhotoId = photoId},
                    commandType: CommandType.StoredProcedure
                );
            }
            return affectedRows;
        }
    }
}