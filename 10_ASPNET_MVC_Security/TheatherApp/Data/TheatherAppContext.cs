using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheatherApp.Models;

namespace TheatherApp.Data
{
    public class TheatherAppContext : DbContext
    {
        public TheatherAppContext (DbContextOptions<TheatherAppContext> options)
            : base(options)
        {
        }

        public DbSet<TheatherApp.Models.NewsMessage> NewsMessage { get; set; } = default!;
    }
}
