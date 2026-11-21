using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer.Controllers
{
    internal class BoardController
    {
        private const string TableName = "Boards";
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Insert(BoardDTO board)
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $@"
                        INSERT INTO {TableName} (Id, Name, Owner, BoardCounter)
                        VALUES (@Id, @Name, @Owner, @Counter);";

                    command.Parameters.AddWithValue("@Id", board.Id);
                    command.Parameters.AddWithValue("@Name", board.Name);
                    command.Parameters.AddWithValue("@Owner", board.Owner);
                    command.Parameters.AddWithValue("@Counter", board.TaskCounter);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<BoardDTO> SelectAll()
        {
            List<BoardDTO> boards = new List<BoardDTO>();
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand($"SELECT * FROM {TableName};", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("Id"));
                        string name = reader.GetString(reader.GetOrdinal("Name"));
                        string owner = reader.GetString(reader.GetOrdinal("Owner"));
                        int counter = reader.GetInt32(reader.GetOrdinal("BoardCounter"));
                        BoardDTO boardO = new BoardDTO(id, name, owner, counter);
                        boardO.IsPersisted=true;
                        boards.Add(boardO);
                    }
                }
            }
            return boards;
        }

        public bool Update(int id, string columnName, object newValue)
        {
            int result = -1;

            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"UPDATE {TableName} SET [{columnName}] = @value WHERE Id = @Id;";
                    command.Parameters.AddWithValue("@value", newValue);
                    command.Parameters.AddWithValue("@Id", id);

                    try
                    {
                        connection.Open();
                        result = command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        log.Error($"Failed to update column '{columnName}' for board '{id}': {e.Message}", e);
                        throw new Exception($"Could not update board data for '{id}'.", e);
                    }
                }
            }

            return result > 0;
        }

        public void DeleteBoard(int boardId)
        {
            new BoardMemberController().DeleteAllMembersInBoard(boardId);
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $@"DELETE FROM {TableName} WHERE Id = @Id;";
                    command.Parameters.AddWithValue("@Id", boardId);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Deletes all boards from the Boards table.
        /// </summary>
        public void DeleteAllBoards()
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand("DELETE FROM Boards;", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public int GetMaxBoardId()
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $@"SELECT MAX(Id) FROM Boards;";
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        return -1; // Indicates no boards exist yet
                    }
                }
            }
        }




    }
}