using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyDBConfigProvider
{
    public class DBConfigurationSource : IConfigurationSource
    {
        private DBConfigOptions _options;

        public DBConfigurationSource(DBConfigOptions options)
        {
            _options = options;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DBConfigurationProvider(_options);
        }
    }
}
