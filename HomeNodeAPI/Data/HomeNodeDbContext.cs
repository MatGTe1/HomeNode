using HomeNodeAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeNodeAPI.Data
{
    public class HomeNodeDbContext : IdentityDbContext<ApplicationUser>
    {
        public HomeNodeDbContext(DbContextOptions<HomeNodeDbContext> options) : base(options) 
        {
        }
    }
}
