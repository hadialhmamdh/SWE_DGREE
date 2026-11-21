using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    /// <summary>
    /// This class provides all user services (registration, login and logout).
    /// </summary>
    public class UserService
    {

        public UserFacade userFacade { get; internal set; }

        public UserService(UserFacade userFacade)
        {
            this.userFacade = userFacade;
        }

        /// <summary>
        /// Registers a new user using email and password as separate arguments.
        /// </summary>
        /// <param name="email">The email used for registration.</param>
        /// <param name="pass">The password used for registration.</param>
        /// <returns>JSON string representing a Response object with success or error.</returns>
        public string Register(string email, string pass)
        {
            try
            {
                Response res = userFacade.Register(email, pass);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception e)
            {
                return JsonSerializer.Serialize(new Response($"Service error: {e.Message}"));
            }

        }

        /// <summary>
        /// Registers a new user using a JSON string containing email and password.
        /// </summary>
        /// <param name="json">JSON with "email" and "pass" fields.</param>
        /// <returns>JSON string representing a Response object with success or error.</returns>
        public string Register(string json)
        {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return Register(data["email"], data["pass"]);
        }

        /// <summary>
        /// Logs in a user using email and password as separate arguments.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <param name="pass">The user's password.</param>
        /// <returns>JSON string representing a Response object with success or error.</returns>
        public string Login(string email, string pass)
        {
            try
            {
                Response res = userFacade.Login(email, pass);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception e)
            {
                return JsonSerializer.Serialize(new Response($"Service error: {e.Message}"));
            }
        }

        /// <summary>
        /// Logs in a user using a JSON string containing email and password.
        /// </summary>
        /// <param name="json">JSON with "email" and "pass" fields.</param>
        /// <returns>JSON string representing a Response object with success or error.</returns>
        public string Login(string json)
        {
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return Login(data["email"], data["pass"]);
          
        }

        /// <summary>
        /// Logs out a user using their email - the input can be string or json.
        /// </summary>
        /// <param name="input">The user's email address.</param>
        /// <returns>JSON string representing a Response object with success or error.</returns>
        public string Logout(string input)
        {
            try
            {
                string email;
                // to know whether the input is in a json or primitive format
                if (input.Trim().StartsWith("{"))
                {
                    var data = JsonSerializer.Deserialize<Dictionary<string, string>>(input);
                    email = data["email"];
                }
                else
                {
                    email = input;
                }

                Response res = userFacade.Logout(email);
                return JsonSerializer.Serialize(res);
            }
            catch (Exception e)
            {
                return JsonSerializer.Serialize(new Response($"Service error: {e.Message}"));
            }
        }


    }
}

