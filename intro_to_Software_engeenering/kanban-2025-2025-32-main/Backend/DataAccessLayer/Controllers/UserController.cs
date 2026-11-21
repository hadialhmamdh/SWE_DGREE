using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net;

namespace IntroSE.Kanban.Backend.DataAccessLayer.Controllers
{
    /// <summary>
    /// this class responsilbe for handling user related database operations.
    /// </summary>
    internal class UserController
    {
        private const string TableName = "Users";
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// this method insterts a new user to the users table.
        /// </summary>
        /// <param name="user"> the UserDTO to insert. </param>
        public void Insert(UserDTO user)
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"INSERT INTO {TableName} (Email, Password) " + "VALUES (@Email, @Password);";
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// this method selects all the users from the users table.
        /// </summary>
        /// <returns> a list contraning UserDTO object representing all users. </returns>
        public List<UserDTO> SelectAll()
        {
            List<UserDTO> users = new List<UserDTO>();
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand($"SELECT * FROM {TableName};", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string email = reader.GetString(reader.GetOrdinal("Email"));
                        string password = reader.GetString(reader.GetOrdinal("Password"));
                        UserDTO userO = new UserDTO(email, password);
                        userO.IsPesisted = true;
                        users.Add(userO);
                    }
                }
            }
            return users;
        }

        public bool Update(string email, string columnName, string newValue)
        {
            int result = -1;

            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = $"UPDATE {TableName} SET [{columnName}] = @value WHERE Email = @Email;";
                    command.Parameters.AddWithValue("@value", newValue);
                    command.Parameters.AddWithValue("@Email", email);

                    try
                    {
                        connection.Open();
                        result = command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        log.Error($"Failed to update column '{columnName}' for user '{email}': {e.Message}", e);
                        throw new Exception($"Could not update user data for '{email}'.", e);
                    }
                }
            }

            return result > 0;
        }

        /// <summary>
        /// Deletes all users from the Users table.
        /// </summary>
        public void DeleteAllUsers()
        {
            using (var connection = new SQLiteConnection("Data Source=kanban.db; Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand($"DELETE FROM Users;", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }



    }
}
