using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Labo09_ASPNETMVC.Data
{
    public class TheaterContext : DbContext
    {
        public TheaterContext (DbContextOptions<TheaterContext> options)
            : base(options)
        {
        }

        public DbSet<Labo09_ASPNETMVC.Models.NewsMessage> NewsMessage { get; set; } = default!;
    }
}
