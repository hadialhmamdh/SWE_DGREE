using System;
using System.Text.Json;
using IntroSE.Kanban.Backend.ServiceLayer;
using IntroSE.Kanban.Frontend.Model;

namespace IntroSE.Kanban.Frontend.Controllers
{
    class UserController
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {


            _userService = userService;
        }

        public void Register(string email, string password)
        {
            string json = _userService.Register(email, password);
            var response = JsonSerializer.Deserialize<Response>(json);
            if(response?.ErrorOccured == true)
            {
                throw new Exception(response.ErrorMessage);
            }
   
        }


        public UserModel Login(string email, string password)
        {
            string resultJson = _userService.Login(email,password);

            Response response = JsonSerializer.Deserialize<Response>(resultJson);
            if (response?.ErrorOccured == true)
            {
                throw new Exception(response.ErrorMessage);
            }
            return new UserModel(email);

        }
    }
}

