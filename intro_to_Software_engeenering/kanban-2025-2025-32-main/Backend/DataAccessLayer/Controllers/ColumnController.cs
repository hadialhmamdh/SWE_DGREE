using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer.Controllers
{
    internal class ColumnController
    {
        private const string ColumnTableName = "Columns";
        private readonly string _connectionString;
        private readonly string _tableName;
        private static readonly ILog log = LogManager.GetLogger(typeof(ColumnController));

        public ColumnController()
        {
            string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Kanban.db"));
            this._connectionString = $"Data Source={path}; Version=3;";
            this._tableName = ColumnTableName;
            log.Info("ColumnController initialized with DB path: " + path);
        }
        public bool UpdateLimit(string attributeName, string columnName, int boardId, int attributeValue)
        {
            log.Info($"[UpdateLimit] Updating '{attributeName}' for Column: '{columnName}' and BoardID: {boardId}");
            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $"UPDATE {_tableName} SET [{attributeName}] = @value " +
                      $"WHERE [{ColumnDTO.BoardIdColumnName}] = @boardId AND [{ColumnDTO.ColumnColumnName}] = @columnName;";
                command.Parameters.AddWithValue("@value", attributeValue);
                command.Parameters.AddWithValue("@boardId", boardId);
                command.Parameters.AddWithValue("@columnName", columnName);
                res = command.ExecuteNonQuery();
                log.Info($"[UpdateLimit] Updated {res} row(s) successfully for Column: '{columnName}', BoardID: {boardId}.");
            }
            catch (Exception ex)
            {
                log.Error($"[UpdateLimit] Failed to update '{attributeName}' for Column: '{columnName}', BoardID: {boardId}.", ex);
            }

            return res > 0;
        }


        public List<ColumnDTO> GetAllColumns()
        {
            log.Info("[GetAllColumns] Retrieving all columns.");
            var result = SelectAll();
            log.Info($"[GetAllColumns] Retrieved {result.Count} column(s).");
            return result;
        }

        public List<ColumnDTO> SelectAll()
        {
            log.Info("[SelectAll] Selecting all columns from the database.");
            List<ColumnDTO> results = new List<ColumnDTO>();

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand($"SELECT * FROM {_tableName};", connection);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(ConvertReaderToColumn(reader));
                }

                log.Info($"[SelectAll] Retrieved {results.Count} column(s) from the database.");
            }
            catch (Exception ex)
            {
                log.Error("[SelectAll] Error occurred while selecting all columns.", ex);
            }

            return results;
        }

        private ColumnDTO ConvertReaderToColumn(SQLiteDataReader reader)
        {
            ColumnDTO columndto = new ColumnDTO(reader["Column"].ToString());

            columndto.Limit = Convert.ToInt32(reader["Limit"]);
            columndto.BoardId = Convert.ToInt32(reader["BoardID"]);
            columndto.IsPersisted=true;
            return columndto;
        }

        public bool DeleteAllColumns()
        {
            log.Info("[DeleteAllColumns] Attempting to delete all tasks from the table.");

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $"DELETE FROM {_tableName};";
                res = command.ExecuteNonQuery();
                log.Info($"[DeleteAllColumns] Successfully deleted {res} column(s).");
            }
            catch (Exception ex)
            {
                log.Error("[DeleteAllColumns] Failed to delete all column.", ex);
            }

            return res >= 0;
        }

        public bool DeleteColumnsByBoard(int boardId)
        {
            log.Info($"[DeleteColumnsByBoard] Attempting to delete all columns for BoardID={boardId}");

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $"DELETE FROM {_tableName} WHERE BoardID = @boardId;";
                command.Parameters.AddWithValue("@boardId", boardId);

                res = command.ExecuteNonQuery();
                log.Info($"[DeleteColumnsByBoard] Successfully deleted {res} column(s) for BoardID={boardId}");
            }
            catch (Exception ex)
            {
                log.Error($"[DeleteColumnsByBoard] Failed to delete columns for BoardID={boardId}", ex);
            }

            return res >= 0;
        }

        public List<ColumnDTO> GetColumnById(int id)
        {
            log.Info($"Retrieving Columns with BoardID {id}");
            var result = SelectById(id);
            log.Info($"Retrieved {result.Count} columns for BoardID {id}");
            return result;
        }

        public List<ColumnDTO> SelectById(int id)
        {
            log.Info($"Selecting columns from DB with BoardID = {id}");
            List<ColumnDTO> results = new List<ColumnDTO>();
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"SELECT * FROM {_tableName} WHERE BoardID=@boardId;";
                command.Parameters.AddWithValue("@boardId", id);
                SQLiteDataReader dataReader = null;

                try
                {
                    connection.Open();
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        results.Add(ConvertReaderToColumn(dataReader));
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error selecting columns for BoardID {id}", ex);
                }
                finally
                {
                    dataReader?.Close();
                    command.Dispose();
                    connection.Close();
                }
            }
            return results;
        }

        public bool Insert(ColumnDTO columnDTO)
        {
            log.Info($"[Insert] Inserting Column with Name = {columnDTO.ColumnName}");
            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $@"
                INSERT INTO {ColumnTableName} 
                ([{ColumnDTO.ColumnColumnName}], [{ColumnDTO.LimitColumnName}], [{ColumnDTO.BoardIdColumnName}]) 
                VALUES (@columnVal, @limitVal, @boardIdVal);";
                command.Parameters.AddWithValue("@columnVal", columnDTO.ColumnName);
                command.Parameters.AddWithValue("@limitVal", columnDTO.Limit);
                command.Parameters.AddWithValue("@boardIdVal", columnDTO.BoardId);

                command.Prepare();
                res = command.ExecuteNonQuery();

                log.Info($"[Insert] Column inserted successfully: Name = {columnDTO.ColumnName}, BoardID = {columnDTO.BoardId}");
            }
            catch (Exception ex)
            {
                log.Error($"[Insert] Failed to insert column: Name = {columnDTO.ColumnName}, BoardID = {columnDTO.BoardId}", ex);
            }

            return res > 0;
        }

    }
}
