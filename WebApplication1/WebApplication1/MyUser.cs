using Microsoft.AspNetCore.Identity;

namespace WebApplication1
{
    public class MyUser : IdentityUser<int>
    {
        public long JWTVersion { get; set; }
    }
}
