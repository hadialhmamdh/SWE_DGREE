using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Frontend.Model
{
   public class UserModel
    {
        public UserModel(string email)
        {
            Email = email;
        }

        public string Email { get; set; }
        public string Password { get; set; }
    }
}
