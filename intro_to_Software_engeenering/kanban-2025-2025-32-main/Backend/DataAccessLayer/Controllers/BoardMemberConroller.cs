using System;
using System.Collections.Generic;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer.Controllers
{
    internal class BoardMemberController
    {
        private const string TableName = "BoardMembers";
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Insert(BoardMemberDTO member)
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $@"
                        INSERT INTO {TableName} (Board_id, User)
                        VALUES (@Board_id, @User);";

                    command.Parameters.AddWithValue("@Board_id", member.BoardId);
                    command.Parameters.AddWithValue("@User", member.UserEmail);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<BoardMemberDTO> SelectAll()
        {
            List<BoardMemberDTO> members = new List<BoardMemberDTO>();
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand($"SELECT * FROM {TableName};", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int boardId = reader.GetInt32(reader.GetOrdinal("Board_id"));
                        string userEmail = reader.GetString(reader.GetOrdinal("User"));
                        BoardMemberDTO memberO = new BoardMemberDTO(boardId, userEmail);
                        memberO.IsPersisted = true;
                        members.Add(memberO);
                    }
                }
            }
            return members;
        }

        public void DeleteMember(int boardId, string userEmail)
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $@"DELETE FROM {TableName} WHERE Board_id = @BoardId AND User = @User;";
                    command.Parameters.AddWithValue("@BoardId", boardId);
                    command.Parameters.AddWithValue("@User", userEmail);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteAllMembersInBoard(int boardId)
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $@"DELETE FROM {TableName} WHERE Board_id = @BoardId;";
                    command.Parameters.AddWithValue("@BoardId", boardId);
                    command.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// Retrieves the list of user emails that are members of a specific board.
        /// </summary>
        /// <param name="boardId">The ID of the board.</param>
        /// <returns>List of user emails.</returns>
        public List<string> GetMembersOfBoard(int boardId)
        {
            List<string> members = new List<string>();

            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $@"
                SELECT User FROM {TableName}
                WHERE Board_id = @BoardId;";
                    command.Parameters.AddWithValue("@BoardId", boardId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string userEmail = reader.GetString(reader.GetOrdinal("User"));
                            members.Add(userEmail);
                        }
                    }
                }
            }

            return members;
        }

        /// <summary>
        /// Deletes all board members from the BoardMembers table.
        /// </summary>
        public void DeleteAllBoardMembers()
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand("DELETE FROM BoardMembers;", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Retrieves a specific BoardMemberDTO by board ID and user email.
        /// </summary>
        /// <param name="boardId">The ID of the board.</param>
        /// <param name="userEmail">The user's email.</param>
        /// <returns>A BoardMemberDTO if found; otherwise, null.</returns>
        public BoardMemberDTO Select(int boardId, string userEmail)
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $@"
                SELECT * FROM {TableName}
                WHERE Board_id = @BoardId AND User = @User;";
                    command.Parameters.AddWithValue("@BoardId", boardId);
                    command.Parameters.AddWithValue("@User", userEmail);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("Board_id"));
                            string email = reader.GetString(reader.GetOrdinal("User"));
                            return new BoardMemberDTO(id, email);
                        }
                    }
                }
            }

            return null;
        }
    }
}
