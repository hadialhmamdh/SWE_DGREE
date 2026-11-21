using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.ServiceLayer;
using Newtonsoft.Json.Linq;

namespace BackendTests.ServiceLayerTests
{
    internal class UserServiceTest
    {
        private readonly UserService userService;

        public UserServiceTest()
        {
            userService = new UserService(new IntroSE.Kanban.Backend.BusinessLayer.UserFacade());
        }

        /// <summary>
        /// This function tests Requirement 6: successful user registration and auto-login
        /// </summary>
        public void TestRegisterUserSuccess()
        {
            Console.WriteLine("Running TestRegisterUserSuccess...");
            string email = "sucessfulTest@gmail.com";
            string password = "Spassword24";
            string result = userService.Register(email, password);
            Response response = JsonSerializer.Deserialize<Response>(result);
            bool success = response.ErrorMessage == null;
            if (success)
            {
                Console.WriteLine("Passed TestRegisterUserSuccess" + "\n");
            }
            else
            {
                Console.WriteLine("Expected registration to succeed but it failed" + "\n");
                Console.WriteLine(response.ErrorMessage);
            }
        }

        /// <summary>
        /// This function tests Requirement 6: invalid password format should be rejected
        /// </summary>
        public void TestRegisterUserInvalidPassword()
        {
            Console.WriteLine("Running TestRegisterUserInvalidPassword...");
            string email = "FaliedTest@gmail.com";
            string password = "password";
            string result = userService.Register(email, password);
            Response response = JsonSerializer.Deserialize<Response>(result);
            bool success = response.ErrorMessage == null;
            if (success)
            {
                Console.WriteLine("Expected registration to failed but it succeeded" + "\n");
            }
            else
            {
                Console.WriteLine("Passed TestRegisterUserSuccess" + "\n");
            }
        }

        /// <summary>
        /// This function tests Requirement 6: duplicate user registration should fail
        /// </summary>
        public void TestRegisterUserDuplicate()
        {
            Console.WriteLine("Running TestRegisterUserDuplicate...");
            string email = "sucessfulTest@gmail.com";
            string password = "Failedpassword24";
            string result = userService.Register(email, password);
            Response response = JsonSerializer.Deserialize<Response>(result);
            bool success = response.ErrorMessage == null;
            if (success)
            {
                Console.WriteLine("Duplicate registration should have failed but succeeded" + "\n");
            }
            else
            {
                Console.WriteLine("Passed TestRegisterUserDuplicate" + "\n");
            }
        }

        /// <summary>
        /// This function tests Requirement 7: successful login and logout
        /// </summary>
        public void LogoutSuccess()
        {
            Console.WriteLine("Running TestLogoutSuccess ...");
            string email = "logouttest@example.com";
            string password = "Password1";
            userService.Register(email, password);
            string logoutResponse = userService.Logout(email);
            Response logoutJson = JsonSerializer.Deserialize<Response>(logoutResponse);
            bool logoutSuccess = logoutJson.ErrorMessage == null;
            if (!logoutSuccess)
                Console.WriteLine("Logout failed when it should have succeeded" + "\n");
            else
                Console.WriteLine("Passed TestLogoutSuccess" + "\n");
        }

        /// <summary>
        /// This function tests Requirement 7: login failure with incorrect credentials
        /// </summary>
        public void TestLoginFailure()
        {
            Console.WriteLine("Running TestLoginFailure ...");
            string email = "invaliduser@example.com";
            string correctPassword = "Password1";
            string loginResponse = userService.Login(email, correctPassword);
            Response json = JsonSerializer.Deserialize<Response>(loginResponse);
            bool loginFailed = json.ErrorMessage != null;
            if (!loginFailed)
                Console.WriteLine("Login with non-existent user should have failed" + "\n");
            else
                Console.WriteLine("Passed TestLoginFailure" + "\n");
        }

        /// <summary>
        /// This function tests invalid email format during registration (Requirement 6).
        /// </summary>
        public void TestRegisterUserInvalidEmail()
        {
            Console.WriteLine("Running TestRegisterUserInvalidEmail...");
            string email = "invalidEmail.com"; // Missing '@'
            string password = "ValidPass1";

            string result = userService.Register(email, password);
            Response response = JsonSerializer.Deserialize<Response>(result);
            bool failed = response.ErrorMessage != null;

            if (failed)
            {
                Console.WriteLine("Passed TestRegisterUserInvalidEmail: " + response.ErrorMessage + "\n");
            }
            else
            {
                Console.WriteLine("Expected registration to fail due to invalid email, but it succeeded.\n");
            }
        }

    }
}
