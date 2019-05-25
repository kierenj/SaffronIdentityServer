using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedRiver.Saffron.EntityFramework;
using RedRiver.Saffron.EntityFramework.AuthStore.Entities;

namespace SaffronIdentityServer.Database.Models
{
    public class Role : IdentityRole<string>
    {
        public string Description { get; set; }
        public string Type { get; set; }
        public string AdditionalDataJson { get; set; }
        public DateTime? DeletedUtc { get; set; }

        public List<RolePermission> Permissions { get; set; }
    }

    public class RoleConfiguration : EntityMappingConfiguration<Role>
    {
        public override void Map(EntityTypeBuilder<Role> b)
        {
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.DeletedUtc);
        }
    }
}
