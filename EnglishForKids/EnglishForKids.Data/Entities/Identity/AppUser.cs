using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace EnglishForKids.Data.Entities.Identity  
{
    public class AppUser : IdentityUser  
    {
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
        public ICollection<ChildProfile> Children { get; set; }
    }  
}  