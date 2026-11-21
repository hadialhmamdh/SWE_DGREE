using System;
using IntroSE.Kanban.Backend.DataAccessLayer.Controllers;

namespace IntroSE.Kanban.Backend.DataAccessLayer.DTOs
{
    internal class UserDTO
    {
        public const string EmailColumnName = "Email";
        public const string PasswordColumnName = "Password";

        public string Email { get; private set; }

        private string _password;
        private UserController _controller;
        public bool IsPesisted { get;  set; } = false;
        public string Password
        {
            get => _password;
            set
            {
                if (_password == value)
                    return;

                _controller.Update(Email, PasswordColumnName, value);
                _password = value;
            }
        }


        public UserDTO(string email, string password)
        {
            Email = email;
            _password = password;
            _controller = new UserController();
        }

        public void Save()
        {
            if (IsPesisted)
            {
                throw new InvalidOperationException("User already exists in the database.");
            }

            _controller.Insert(this);
            IsPesisted = true;
        }

    }
}
