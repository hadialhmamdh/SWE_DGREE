using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using log4net.Repository.Hierarchy;


namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// Represents a user in the system.
    /// Each user has an email "unique", password, login status, and a list of boards.
    /// </summary>
    internal class User
    {

        public Email Email { get; private set; }

        public Password Pass { get; private set; }

        public bool IsLoggedIn { get; private set; }

        public UserDTO UserDTO { get; private set; }

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="password">The user's password.</param>
        public User(string email, string password)
        {
            UserDTO = new UserDTO(email, password);
            Email = new Email(email);
            Pass = new Password(password);
            IsLoggedIn = true;
            UserDTO.Save();
            log.Info($"User created and logged in: {email}");
        }


        public User(UserDTO user) {
            Email = new Email(user.Email);
            Pass = new Password(user.Password);
            IsLoggedIn = false;
            UserDTO = user;
        }

        /// <summary>
        /// Logs in the user.
        /// </summary>
        public void Login()
        {
            IsLoggedIn = true;
            log.Info($"User {Email} logged in.");
        }

        /// <summary>
        /// Logs out the user.
        /// </summary>
        public void Logout()
        {
            IsLoggedIn = false;
            log.Info($"User {Email} logged out.");
        }

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="newPass">The new password to set.</param>
        public void ChangePassword(string newPass)
        {
            Pass = new Password(newPass);
            log.Info($"User {Email} changed password.");
        }


        public override string ToString()
        {
            return $"User [Email={Email}, Name={Pass}]";
        }
    }
}  
