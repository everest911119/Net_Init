using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyDBConfigProvider
{
    public static class DBConfigurationProviderExtensions
    {
        public static IConfigurationBuilder AddDBConfiguration(this IConfigurationBuilder builder,
            DBConfigOptions setup)
        {
            return builder.Add(new DBConfigurationSource(setup));
        }

        public static IConfigurationBuilder AddDBConfiguration(this IConfigurationBuilder builder,
             Func<IDbConnection> createDbConnection, 
             string tableName = "T_Configs", 
             bool reloadOnChange = false,
             TimeSpan? reloadInterval = null
            )
        {
            return AddDBConfiguration(builder, new DBConfigOptions
            {
                CreateDBConnection = createDbConnection,
                TableName = tableName,
                ReloadOnChange = reloadOnChange,
                ReloadInterval = reloadInterval
            });
        }
    }
}
