using CloudAppServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudAppServer
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) 
        {
            
        }

        public DbSet<UploadedFile> Files { get; set; }
    }
}
