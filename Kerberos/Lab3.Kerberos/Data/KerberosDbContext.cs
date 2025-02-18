using Microsoft.EntityFrameworkCore;
using Lab3.Kerberos.Models;

namespace Lab3.Kerberos.Data
{
    public class KerberosDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<KerberosTicket> Tickets { get; set; }
        public KerberosDbContext(DbContextOptions<KerberosDbContext> options) : base(options) { }
    }
}
