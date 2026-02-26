using System.Data;

namespace AnyDBConfigProvider
{
    public class DBConfigOptions
    {
        public Func<IDbConnection> CreateDBConnection { get; set; }
        public string TableName { get; set; } = "T_Configs";
        public bool ReloadOnChange { get; set; }= false;
        public TimeSpan? ReloadInterval {  get; set; }
    }
}