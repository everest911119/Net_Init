namespace WebApplication1
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NoJWTVersionCheckAttribute : Attribute
    {
    }
}