using Abp.Authorization;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.Roles.Dto
{

    [AutoMapFrom(typeof(Permission))]
    [AutoMapTo(typeof(Permission))]
    public class FlatPermissionWithLevelDto
    {
        public string ParentName { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public bool IsGrantedByDefault { get; set; }

        public int Level { get; set; }
    }
}
