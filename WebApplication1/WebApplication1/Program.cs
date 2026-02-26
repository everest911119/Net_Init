using CommonInitializer;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.ConfigureDbConfiguration();
            builder.ConfigureExtraService(new InitializerOptions
            {
                EventBusQueueName = "abc.WebApi",
                LogFilePath = "c:/temp/abc.log"
            });
            builder.Services.AddControllers();


            builder.Services.AddIdentityCore<User>(opt =>
            {
                opt.Password.RequireDigit = false;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequiredLength = 6;
                opt.Lockout.MaxFailedAccessAttempts = 10;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMilliseconds(5);
                opt.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                opt.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;

            });


            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");
            app.UseMyDefault();
            app.Run();
        }
    }
}
