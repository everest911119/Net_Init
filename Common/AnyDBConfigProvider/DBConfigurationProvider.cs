using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AnyDBConfigProvider
{

    public class DBConfigurationProvider : ConfigurationProvider, IDisposable
    {
        private DBConfigOptions _options;
        private bool isDisposed = false;
        public DBConfigurationProvider(DBConfigOptions options)
        {
            _options = options;
            TimeSpan interval = TimeSpan.FromSeconds(3);
            if (options.ReloadInterval != null)
            {
                interval = options.ReloadInterval.Value;
            }
            if (options.ReloadOnChange)
            {
                ThreadPool.QueueUserWorkItem(obj =>
                {
                    while(!isDisposed)
                    {
                        Load();
                        Thread.Sleep(interval);
                    }
                });
            }
        }

        // allow mutli reading and single write
        private ReaderWriterLockSlim lockObj = new ReaderWriterLockSlim();
        public override void Load()
        {
            base.Load();
            IDictionary<string, string> cloneData = null;
            try
            {
                lockObj.EnterWriteLock();
                cloneData = this.Data.Clone();
                string tableName = _options.TableName;
                Data.Clear();
                using (var conn = _options.CreateDBConnection())
                {
                    conn.Open();
                    DoLoad(tableName, conn);
                }
            }
            catch (DbException)
            {
                this.Data = cloneData;
                throw;
            }
            finally
            {
                lockObj.ExitWriteLock();
            }
            if (Helper.IsChanged(cloneData,Data))
            {
                OnReload();
            }

        }
        public override bool TryGet(string key, out string? value)
        {
            lockObj.EnterReadLock();
            try
            {
                return base.TryGet(key, out value);
            }finally { lockObj.ExitReadLock(); }
            
        }


        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
        {
            lockObj.EnterReadLock();
            try
            {
                return base.GetChildKeys(earlierKeys, parentPath);
            }finally { lockObj.ExitReadLock(); }
            
        }



        private void DoLoad(string tableName, IDbConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"select Name,Value from {tableName} where Id in(select Max(Id) from {tableName} group by Name)";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string value = reader.GetString(1);
                        if (value == null)
                        {
                            this.Data[name] = value;
                            continue;
                        }
                        value = value.Trim();
                        if (value.StartsWith("[") && value.EndsWith("]")
                            || value.StartsWith("{") && value.EndsWith("}")
                            )
                        {

                            TryLoadAsJson(name, value);
                        }
                        else
                        {
                            this.Data[name] = value;
                        }
                    }
                }
            }
        }

        private void LoadJsonElement(string name, JsonElement jsonRoot)
        {
            if (jsonRoot.ValueKind == JsonValueKind.Array)
            {
                int index = 0;
                foreach (var item in jsonRoot.EnumerateArray())
                {
                    string path = name + ConfigurationPath.KeyDelimiter + index;
                    LoadJsonElement(path, item);
                    index++;
                }
            }
            else if (jsonRoot.ValueKind == JsonValueKind.Object)
            {
                foreach (var jsonObj in jsonRoot.EnumerateObject())
                {
                    string pathObj = name + ConfigurationPath.KeyDelimiter + jsonObj.Name;
                    var value = jsonObj.Value;
                    LoadJsonElement(pathObj, value);
                }
            }
            else
            {
                this.Data[name] = jsonRoot.GetValueForConfig();
            }
        }


        private void TryLoadAsJson(string name, string value)
        {
            var jsonOptions = new JsonDocumentOptions { AllowTrailingCommas = true, 
                CommentHandling = JsonCommentHandling.Skip };
            try
            {
                var jsonRoot = JsonDocument.Parse(value, jsonOptions).RootElement;
                LoadJsonElement(name, jsonRoot);
            }
            catch (JsonException ex)
            {
                // not JsonFormat
                this.Data[name] = value;
                Debug.WriteLine($"When trying to parse {value} as json object, exception was thrown. {ex}");
            }
        }
        public void Dispose()
        {
           this.isDisposed = true;
        }
    }
}
