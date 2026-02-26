using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using AnyDBConfigProvider;
using Microsoft.Extensions.DependencyInjection;
using Commons;
using Common.Infrastructure;
using Microsoft.EntityFrameworkCore;
using JWT;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using FluentValidation.AspNetCore;
using EventBus;
using StackExchange.Redis;
using Microsoft.AspNetCore.HttpOverrides;
using FluentValidation;
namespace CommonInitializer
{
    public static class WebApplicationBuildExtensions
    {
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        public static void ConfigureDbConfiguration(this WebApplicationBuilder builder)
        {
            builder.Host.ConfigureAppConfiguration((hostCtx, configBuilder) =>
            {
                //不能使用ConfigureAppConfiguration中的configBuilder去读取配置，否则就循环调用了，因此这里直接自己去读取配置文件

                string connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr")!;
                configBuilder.AddDBConfiguration(() => new SqlConnection(connStr), reloadOnChange: false, reloadInterval: TimeSpan.FromSeconds(5));
            });
        }

        public static void ConfigureExtraService(this WebApplicationBuilder builder,
          InitializerOptions options)
        {
            IServiceCollection service = builder.Services;
            IConfiguration configuration = builder.Configuration;
            var assemblies = ReflectionHelper.GetAllReferencedAssemblies();

            var validortype = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(t => !t.IsAbstract && IsAssignableToGenericType(t, typeof(AbstractValidator<>)));
                var num = types.Count();
                validortype.AddRange(types);
            }
            foreach (var type in validortype)
            {

            }

            string connStr = configuration.GetValue<string>("DefaultDB:ConnStr");
            service.RunModuleInitializers(assemblies);
            service.AddAllDbContext(ctx =>
            {
                string connStr = configuration.GetValue<string>("DefaultDB:ConnStr")!;
                ctx.UseSqlServer(connStr);
            }, assemblies);
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication();
            JWTOptions jwtOpt = configuration.GetSection("JWT").Get<JWTOptions>()!;
            //启用Swagger中的【Authorize】按钮。这样就不用每个项目的AddSwaggerGen中单独配置了
            builder.Services.AddJWTAuthetication(jwtOpt);
            builder.Services.Configure<SwaggerGenOptions>(c =>
            {
                c.AddAuthenticationHeader();
            });
            // meidaR
            builder.Services.AddMyMediatR(assemblies);
            builder.Services.Configure<JsonOptions>(opt =>
            {
                opt.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
            });
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(c =>
            {
                var corsOpt = configuration.GetSection("Cors").Get<CorsSetting>()!;
                string[] urls = corsOpt.Origins;
             //   Console.WriteLine(urls[0]);
                //c.AddPolicy(name: MyAllowSpecificOrigins, p =>
                //{
                //    p.WithOrigins(urls).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                //});
                c.AddDefaultPolicy(builder => builder.WithOrigins(urls)
                            .AllowAnyMethod().AllowAnyHeader().AllowCredentials());
            });

            builder.Services.Configure<MvcOptions>(opt =>
            {
                opt.Filters.Add<UunitOfWorkFilter>();
            });
            builder.Services.AddLogging(builder =>
            {
                Log.Logger = new LoggerConfiguration().WriteTo.Console().
                WriteTo.File(options.LogFilePath).CreateLogger();
                builder.AddSerilog();
            });
            builder.Services.AddFluentValidation(fv =>
            {
                fv.RegisterValidatorsFromAssemblies(assemblies);
            });
            builder.Services.Configure<JWTOptions>(configuration.GetSection("JWT"));
            builder.Services.Configure<IntegrationEventRabbitMQOptions>(configuration.GetSection("RabbitMQ"));
            builder.Services.AddEventBus(options.EventBusQueueName, assemblies);

            // redis
            string redisConn = configuration.GetValue<string>("Redis:ConnStr");
            IConnectionMultiplexer redisConnMultiper = ConnectionMultiplexer.Connect(redisConn);
            builder.Services.AddSingleton(typeof(IConnectionMultiplexer), redisConnMultiper);
            builder.Services.Configure<ForwardedHeadersOptions>(opt =>
            {
                opt.ForwardedHeaders = ForwardedHeaders.All;
            });
        }

    }
}
