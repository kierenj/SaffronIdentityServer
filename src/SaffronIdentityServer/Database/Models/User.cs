using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedRiver.Saffron.EntityFramework;

namespace SaffronIdentityServer.Database.Models
{
    public class User : IdentityUser<string>
    {
        public string AdditionalDataJson { get; set; }
        public DateTime? DeletedUtc { get; set; }
    }

    public class UserConfiguration : EntityMappingConfiguration<User>
    {
        public override void Map(EntityTypeBuilder<User> b)
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.DeletedUtc);
        }
    }
}
