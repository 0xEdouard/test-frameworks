using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NieuwsAPI.Model;

namespace NieuwsAPI.Data
{
    public class NieuwsAPIContext : DbContext
    {
        public NieuwsAPIContext (DbContextOptions<NieuwsAPIContext> options)
            : base(options)
        {
        }

        public DbSet<NieuwsAPI.Model.Nieuwsbericht> Nieuwsbericht { get; set; } = default!;
    }
}
