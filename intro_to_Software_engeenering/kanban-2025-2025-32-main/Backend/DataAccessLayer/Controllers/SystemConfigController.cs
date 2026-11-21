using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer.Controllers
{
    internal class SystemConfigController
    {
        private const string TableName = "SystemConfig";
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Insert(SystemConfigDTO config)
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $@"
                        INSERT INTO {TableName} (Key, Value)
                        VALUES (@Key, @Value);";
                    command.Parameters.AddWithValue("@Key", config.Key);
                    command.Parameters.AddWithValue("@Value", config.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        public bool Update(string key, string columnName, string newValue)
        {
            int result;

            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"UPDATE {TableName} SET [{columnName}] = @Value WHERE [Key] = @Key;";
                    command.Parameters.AddWithValue("@Value", newValue);
                    command.Parameters.AddWithValue("@Key", key);
                    result = command.ExecuteNonQuery();
                }
            }

            return result > 0;
        }


        public List<SystemConfigDTO> SelectAll()
        {
            List<SystemConfigDTO> configs = new List<SystemConfigDTO>();

            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand($"SELECT * FROM {TableName};", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string key = reader.GetString(reader.GetOrdinal("Key"));
                        string value = reader.GetString(reader.GetOrdinal("Value"));
                        configs.Add(new SystemConfigDTO(key, value));
                    }
                }
            }

            return configs;
        }
        public void Delete(string key)
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"DELETE FROM {TableName} WHERE Key = @Key;";
                    command.Parameters.AddWithValue("@Key", key);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteAll()
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand($"DELETE FROM {TableName};", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// Gets the value associated with the given key from the SystemConfig table.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The value as string, or throws if not found.</returns>
        public string GetValue(string key)
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"SELECT Value FROM {TableName} WHERE Key = @Key;";
                    command.Parameters.AddWithValue("@Key", key);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(reader.GetOrdinal("Value"));
                        }
                        else
                        {
                            throw new KeyNotFoundException($"SystemConfig key '{key}' not found.");
                        }
                    }
                }
            }
        }

    }
}
