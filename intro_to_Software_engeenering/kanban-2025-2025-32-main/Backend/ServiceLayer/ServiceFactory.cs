using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;

namespace IntroSE.Kanban.Backend.ServiceLayer
{
    public class ServiceFactory
    {
        public TaskService TaskService { get;  }
        public BoardService BoardService { get; }   
        public UserService UserService { get; }
        private readonly UserFacade userFacade;
        private readonly BoardFacade boardFacade;
        
        public ServiceFactory() { 
            userFacade = new UserFacade();
            boardFacade  = new BoardFacade(userFacade);
            UserService = new UserService(userFacade);
            BoardService = new BoardService(boardFacade);
            TaskService = new TaskService(boardFacade);
            DBInitializer.Initialize();
            LoadData();
        }
        public string LoadData()
        {
            try
            {
                userFacade.LoadAllUsers();
                boardFacade.LoadAllBoards();
            }
            catch (Exception e)
            {
                return JsonSerializer.Serialize(new Response(e.Message));
            }
            return JsonSerializer.Serialize(new Response());
        }

        public string DeleteData()
        {
            try
            {
                userFacade.ResetUserData();
                boardFacade.ResetBoardData();
            }
            catch (Exception e)
            {
                return JsonSerializer.Serialize(new Response(e.Message));
            }
            return JsonSerializer.Serialize(new Response());
        }

        public void printData()
        {
            boardFacade.prints();
        }

    }
}
