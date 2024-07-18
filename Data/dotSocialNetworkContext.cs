using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using dotSocialNetwork.Models;

namespace dotSocialNetwork.Data
{
    public class dotSocialNetworkContext : DbContext
    {
        public dotSocialNetworkContext (DbContextOptions<dotSocialNetworkContext> options)
            : base(options)
        {
        }
        
        public DbSet<dotSocialNetwork.Models.User> User { get; set; } = default!;
        public DbSet<dotSocialNetwork.Models.Message> Message { get; set; } = default!;
        public DbSet<dotSocialNetwork.Models.Friend> Friend { get; set; } = default!;
    }
}
