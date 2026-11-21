using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    public class AuthenticationFacade
    {
        private Dictionary<string, bool> authentication;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationFacade"/> class.
        /// </summary>
        public AuthenticationFacade()
        {
            authentication = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Logs in a user by setting their login status to true.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        public void Login(string email)
        {
            authentication[email]=true;
        }

        /// <summary>
        /// Logs out a user by setting their login status to false.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        public void Logout(string email)
        {
            authentication[email] = false;
        }

        /// <summary>
        /// Checks if the user is currently logged in.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns><c>true</c> if the user is logged in; otherwise, <c>false</c>.</returns>
        public bool IsLoggedIn(string email)
        {
            if (authentication.ContainsKey(email) && (authentication[email]))
            {
                return true;
            }
            return false;
        }

    }
}
