using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.ServiceLayer;

namespace IntroSE.Kanban.Frontend.Controllers
{
    internal class ControllerFactory
    {
        public static ControllerFactory Instance { get; } = new ControllerFactory();
        public BoardController BoardController { get;  }
        public UserController UserController { get; }
        private ControllerFactory() {
            ServiceFactory serviceFactory = new ServiceFactory();
            UserController= new UserController(serviceFactory.UserService);
            BoardController = new BoardController(serviceFactory.BoardService);
        }
    }
}
