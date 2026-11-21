using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using IntroSE.Kanban.Frontend.Controllers;
using IntroSE.Kanban.Frontend.Model;

namespace IntroSE.Kanban.Frontend.ViewModel
{
    class LoginVM : Notifiable
    {
        public LoginVM()
        {
            Message = "";
            Controller = ControllerFactory.Instance.UserController;
        }

        public UserController Controller { get; private set; }

        private string email_;
        public string Email
        {
            get => email_;
            set
            {
                email_ = value;
                RaisePropertyChanged("Email");
            }
        }

        private string password_;
        public string Password
        {
            get => password_;
            set
            {
                password_ = value;
                RaisePropertyChanged("Password");
            }
        }

        private string message_;
        public string Message
        {
            get => message_;
            set
            {
                message_ = value;
                RaisePropertyChanged("Message");
            }
        }

        public UserModel Login()
        {
         
            try
            {
                UserModel user =Controller.Login(Email, Password);
                return user;
            }
            catch (Exception e)
            {
                Message = e.Message;
                return null;
                
            }
        }
        public UserModel Register()
        {
            try
            {
                Controller.Register(Email, Password);
                return new UserModel(Email);
            }
            catch (Exception e)
            { 
                Message = e.Message;
                return null;
            }

        }

        public void ClearData()
        {
            Email = "";
            Message = "";
        }
    }
}
