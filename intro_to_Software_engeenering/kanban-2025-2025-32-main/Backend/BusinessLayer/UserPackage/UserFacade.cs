using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;
using IntroSE.Kanban.Backend.DataAccessLayer.DTOs;
using IntroSE.Kanban.Backend.ServiceLayer;
using log4net;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    /// <summary>
    /// The UserFacade class provides a simplified interface for managing user registration, login, and logout.
    /// Maintains a collection of users and handles authentication-related functionality.
    /// </summary>
    public class UserFacade
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // email -> user 
        private readonly Dictionary<string, User> users = new Dictionary<string, User>();
        internal IReadOnlyDictionary<string, User> Users => users;

        /// <summary>
        /// Registers a new user with the provided email and password.
        /// </summary>
        /// <param name="email">The email address of the user to register.</param>
        /// <param name="password">The password of the user to register.</param>
        /// <returns>A Response indicating success or failure with an error message if applicable.</returns>
        public Response Register(string email, string password)
        {
            try
            {
                Email mail = new Email(email);

                if (users.ContainsKey(mail.Value))
                {
                    log.Warn($"Attempt to register an already registered email: {email}");
                    return new Response("Email is already registered.");
                }

                User newUser = new User(email, password);
                users.Add(mail.Value, newUser);
                log.Info($"User registered successfully: {email}");
                return new Response();
            }
            catch (Exception e)
            {
                log.Error($"Registration failed for email: {email}. Error: {e.Message}");
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// Logs in an existing user with the provided email and password.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>A Response indicating success or failure, including the email if successful.</returns>
        public Response Login(string email, string password)
        {
            try
            {
                if (!users.ContainsKey(email))
                {
                    log.Warn($"Login failed. Email not found: {email}");
                    return new Response("User is not registered.");
                }

                User user = users[email];

                if (user.Pass.Value != password)
                {
                    log.Warn($"Login failed. Incorrect password for email: {email}");
                    return new Response("Incorrect password.");
                }

                user.Login();
                log.Info($"User logged in successfully: {email}");
                return new Response(null,email);
            }
            catch (Exception e)
            {
                log.Error($"Login failed for email: {email}. Error: {e.Message}");
                return new Response(e.Message);
            }
        }

        /// <summary>
        /// Logs out a logged-in user by their email.
        /// </summary>
        /// <param name="email">The email address of the user to log out.</param>
        /// <returns>A Response indicating success or failure.</returns>
        public Response Logout(string email)
        {
            if (!users.ContainsKey(email))
            {
                log.Warn($"Logout failed. User not found: {email}");
                return new Response("User not found.");
            }
            if (!((users[email].IsLoggedIn)))
            {
                log.Error($"Logout failed. User is not loged in: {email}");
                return new Response($"User {email} is not loggedIn");
            }
            users[email].Logout();
            log.Info($"User logged out successfully: {email}");
            return new Response();
        }

        /// <summary>
        /// Checks if a user with the given email is registered.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if the user is registered; otherwise, false.</returns>
        public bool IsRegistered(string email)
        {
            return users.ContainsKey(email);
        }

        /// <summary>
        /// Checks if a user with the given email is currently logged in.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <returns>True if the user is logged in; otherwise, false.</returns>
        public bool IsLoggedIn(string email)
        {
            if (!users.ContainsKey(email))
                return false;

            return users[email].IsLoggedIn;
        }

        /// <summary>
        /// Retrieves the User object for the given email.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The User object corresponding to the email.</returns>
        internal User GetUser(string email)
        {
            return users[email];
        }


        //------------------------------------------------------------------ don't need this function ----------------------------------------------------------
        /// <summary>
        /// Checks whether a user exists with the given email.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <returns>True if a user exists; otherwise, false.</returns>
        internal bool UserExists(string email)
        {
            return users.ContainsKey(email);
        }
        //------------------------------------------------------------------ don't need this function ----------------------------------------------------------

        public void LoadAllUsers()
        {
            List<UserDTO> allusers = new UserController().SelectAll();
            foreach (UserDTO user in allusers)
            {
                User loaded = new User(user);
                users.Add(user.Email, loaded);
                
            }
            
        }

        public void SaveAllUsers()
        {
            foreach (User user in users.Values)
            {
                user.UserDTO.Save();
            }
            log.Info("All users saved successfully.");
        }

        public void ResetUserData()
        {
            new UserController().DeleteAllUsers();
        }
        public void print()
        {
            foreach (User user in Users.Values)
            {
                Console.WriteLine(user.Email.Value);
            }
        }
       
    }
}
