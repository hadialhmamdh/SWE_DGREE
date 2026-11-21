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

    internal class TaskController
    {
        private const string TaskTableName = "Tasks";
        private readonly string _connectionString;
        private readonly string _tableName;
        private static readonly ILog log = LogManager.GetLogger(typeof(TaskController));

        public TaskController()
        {
            string path = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Kanban.db"));
            this._connectionString = $"Data Source={path}; Version=3;";
            this._tableName = TaskTableName;
            log.Info("TaskController initialized with DB path: " + path);
        }

        public bool Insert(TaskDTO taskDTO)
        {
            log.Info($"Inserting task with ID {taskDTO.Id}");
            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                int res = -1;
                try
                {
                    connection.Open();
                    command.CommandText = $"INSERT INTO {TaskTableName} ({TaskDTO.IDColumnName},{TaskDTO.TitleColumnName},{TaskDTO.DescriptionColumnName},{TaskDTO.CreationTimeColumnName},{TaskDTO.DueDateColumnName},{TaskDTO.BoardIdColumnName},{TaskDTO.ColumnColumnName},{TaskDTO.AssignColumnName}) " +
                       $"VALUES (@idVal,@titleVal,@desVal,@creationVal,@dueDateVal,@boardIdVal,@columnVal,@assignVal);";

                    command.Parameters.AddWithValue("@idVal", taskDTO.Id);
                    command.Parameters.AddWithValue("@titleVal", taskDTO.Title);
                    command.Parameters.AddWithValue("@desVal", taskDTO.Description);
                    command.Parameters.AddWithValue("@creationVal", taskDTO.CreationDate.ToString("o"));
                    command.Parameters.AddWithValue("@dueDateVal", taskDTO.DueDate.ToString("o"));
                    command.Parameters.AddWithValue("@boardIdVal", taskDTO.BoardId);
                    command.Parameters.AddWithValue("@columnVal", taskDTO.ColumnName);
                    command.Parameters.AddWithValue("@assignVal", taskDTO.Assign);
                    command.Prepare();
                    res = command.ExecuteNonQuery();
                    log.Info($"Task inserted successfully: ID {taskDTO.Id}");
                }
                catch (Exception ex)
                {
                    log.Error($"Failed to insert task {taskDTO.Id}", ex);
                }
                finally
                {
                    command.Dispose();
                    connection.Close();
                }
                return res > 0;
            }
        }


        public bool UpdateColumnName(string attributeName, int boardId, int id, string attributeValue)
        {
            log.Info($"[UpdateColumnName] Updating '{attributeName}' for Task ID={id}, BoardID={boardId}");
            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;
            try
            {
                connection.Open();
                command.CommandText = $"UPDATE {_tableName} SET [{attributeName}] = @{attributeName} " +
                                      "WHERE Id = @id AND BoardID = @boardId;";
                command.Parameters.AddWithValue($"@{attributeName}", attributeValue);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@boardId", boardId);

                res = command.ExecuteNonQuery();
                log.Info($"[UpdateColumnName] Updated {res} row(s) successfully.");
            }
            catch (Exception ex)
            {
                log.Error($"[UpdateColumnName] Failed to update task {id}.", ex);
            }
            return res > 0;
        }

        public bool UpdateTaskAssign(string attributeName, int boardId, int id, string columnName, string attributeValue)
        {
            log.Info($"[UpdateTaskAssign] Updating '{attributeName}' for Task ID={id}, Column='{columnName}', BoardID={boardId}");

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $"UPDATE {_tableName} SET [{attributeName}] = @{attributeName} " +
                                      "WHERE Id = @id AND [Column] = @column AND BoardID = @boardId;";
                command.Parameters.AddWithValue($"@{attributeName}", attributeValue);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@column", columnName);
                command.Parameters.AddWithValue("@boardId", boardId);

                res = command.ExecuteNonQuery();
                log.Info($"[UpdateTaskAssign] Successfully updated {res} row(s).");
            }
            catch (Exception ex)
            {
                log.Error($"[UpdateTaskAssign] Failed to update Task ID={id}.", ex);
            }

            return res > 0;
        }

        public bool UpdateTaskDescription(string attributeName, int boardId, int id, string columnName, string attributeValue)
        {
            log.Info($"[UpdateTaskDescription] Updating '{attributeName}' for Task ID={id}, Column='{columnName}', BoardID={boardId}");

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $"UPDATE {_tableName} SET [{attributeName}] = @{attributeName} " +
                                      "WHERE Id = @id AND [Column] = @column AND BoardID = @boardId;";
                command.Parameters.AddWithValue($"@{attributeName}", attributeValue);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@column", columnName);
                command.Parameters.AddWithValue("@boardId", boardId);

                res = command.ExecuteNonQuery();
                log.Info($"[UpdateTaskDescription] Successfully updated {res} row(s).");
            }
            catch (Exception ex)
            {
                log.Error($"[UpdateTaskDescription] Failed to update Task ID={id}.", ex);
            }

            return res > 0;
        }

        public bool UpdateTaskDueDate(string attributeName, int boardId, int id, string columnName, string attributeValue)
        {
            log.Info($"[UpdateTaskDueDate] Updating '{attributeName}' for Task ID={id}, Column='{columnName}', BoardID={boardId}");

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $"UPDATE {_tableName} SET [{attributeName}] = @{attributeName} " +
                                      "WHERE Id = @id AND [Column] = @column AND BoardID = @boardId;";
                command.Parameters.AddWithValue($"@{attributeName}", attributeValue);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@column", columnName);
                command.Parameters.AddWithValue("@boardId", boardId);

                res = command.ExecuteNonQuery();
                log.Info($"[UpdateTaskDueDate] Successfully updated {res} row(s).");
            }
            catch (Exception ex)
            {
                log.Error($"[UpdateTaskDueDate] Failed to update Task ID={id}.", ex);
            }

            return res > 0;
        }

        public bool UpdateTaskTitle(string attributeName, int boardId, int id, string columnName, string attributeValue)
        {
            log.Info($"[UpdateTaskTitle] Updating '{attributeName}' for Task ID={id}, Column='{columnName}', BoardID={boardId}");

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $"UPDATE {_tableName} SET [{attributeName}] = @{attributeName} " +
                                      "WHERE Id = @id AND [Column] = @column AND BoardID = @boardId;";
                command.Parameters.AddWithValue($"@{attributeName}", attributeValue);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@column", columnName);
                command.Parameters.AddWithValue("@boardId", boardId);

                res = command.ExecuteNonQuery();
                log.Info($"[UpdateTaskTitle] Successfully updated {res} row(s).");
            }
            catch (Exception ex)
            {
                log.Error($"[UpdateTaskTitle] Failed to update Task ID={id}.", ex);
            }

            return res > 0;
        }


        public List<TaskDTO> GetTaskById(int id)
        {
            log.Info($"Retrieving tasks with BoardID {id}");
            var result = SelectById(id).Cast<TaskDTO>().ToList();
            log.Info($"Retrieved {result.Count} tasks for BoardID {id}");
            return result;
        }

        public List<TaskDTO> SelectById(int id)
        {
            log.Info($"Selecting tasks from DB with BoardID = {id}");
            List<TaskDTO> results = new List<TaskDTO>();
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
                        results.Add(ConvertReaderToMessage(dataReader));
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error selecting tasks for BoardID {id}", ex);
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

        public List<TaskDTO> GetAllTasks()
        {
            log.Info("[GetAllTasks] Retrieving all tasks.");
            var result = SelectAll();
            log.Info($"[GetAllTasks] Retrieved {result.Count} task(s).");
            return result;
        }

        public List<TaskDTO> SelectAll()
        {
            log.Info("[SelectAll] Selecting all tasks from the database.");
            List<TaskDTO> results = new List<TaskDTO>();

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand($"SELECT * FROM {_tableName};", connection);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(ConvertReaderToMessage(reader));
                }

                log.Info($"[SelectAll] Retrieved {results.Count} task(s) from the database.");
            }
            catch (Exception ex)
            {
                log.Error("[SelectAll] Error occurred while selecting all tasks.", ex);
            }

            return results;
        }

        private TaskDTO ConvertReaderToMessage(SQLiteDataReader reader)
        {
            int id = Convert.ToInt32(reader["Id"]);
            string title = reader["Title"].ToString();
            string description = reader["Description"].ToString();

            DateTime creationTime = DateTime.Parse(reader["CreationTime"].ToString(), null, DateTimeStyles.RoundtripKind);
            DateTime dueDate = DateTime.Parse(reader["DueDate"].ToString(), null, DateTimeStyles.RoundtripKind);

            int boardId = Convert.ToInt32(reader["BoardID"]);
            string columnName = reader["Column"].ToString();
            string assignee = reader["Assignee"] != DBNull.Value ? reader["Assignee"].ToString() : null;

            TaskDTO taskdto = new TaskDTO(id, title, description, creationTime, dueDate, assignee);
            taskdto.BoardId = boardId;
            taskdto.ColumnName = columnName;
            taskdto.IsPersisted = true;
            return taskdto;
        }


        public bool DeleteTasksByBoard(int boardId)
        {
            log.Info($"[DeleteTasksByBoard] Attempting to delete all tasks for BoardID={boardId}");

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $"DELETE FROM {_tableName} WHERE BoardID = @boardId;";
                command.Parameters.AddWithValue("@boardId", boardId);

                res = command.ExecuteNonQuery();
                log.Info($"[DeleteTasksByBoard] Successfully deleted {res} task(s) for BoardID={boardId}");
            }
            catch (Exception ex)
            {
                log.Error($"[DeleteTasksByBoard] Failed to delete tasks for BoardID={boardId}", ex);
            }

            return res >= 0;
        }
        public bool DeleteAllTasks()
        {
            log.Info("[DeleteAllTasks] Attempting to delete all tasks from the table.");

            using var connection = new SQLiteConnection(_connectionString);
            using var command = new SQLiteCommand(null, connection);
            int res = -1;

            try
            {
                connection.Open();
                command.CommandText = $"DELETE FROM {_tableName};";
                res = command.ExecuteNonQuery();
                log.Info($"[DeleteAllTasks] Successfully deleted {res} task(s).");
            }
            catch (Exception ex)
            {
                log.Error("[DeleteAllTasks] Failed to delete all tasks.", ex);
            }

            return res >= 0;
        }
        public List<TaskDTO> GetTaskByIdColumn(int boardId, string column)
        {
            log.Info($"Retrieving tasks with BoardID {boardId} and Column '{column}'");
            var result = SelectByIdColumn(boardId, column);
            log.Info($"Retrieved {result.Count} tasks for BoardID {boardId} and Column '{column}'");
            return result;
        }
        public List<TaskDTO> SelectByIdColumn(int boardId, string column)
        {
            log.Info($"Selecting tasks from DB with BoardID = {boardId} and Column = '{column}'");
            List<TaskDTO> results = new List<TaskDTO>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(null, connection);
                command.CommandText = $"SELECT * FROM {_tableName} WHERE BoardID = @boardId AND [Column] = @column;";
                command.Parameters.AddWithValue("@boardId", boardId);
                command.Parameters.AddWithValue("@column", column);

                SQLiteDataReader dataReader = null;

                try
                {
                    connection.Open();
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        results.Add(ConvertReaderToMessage(dataReader));
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error selecting tasks for BoardID = {boardId} and Column = '{column}'", ex);
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



    }
}

