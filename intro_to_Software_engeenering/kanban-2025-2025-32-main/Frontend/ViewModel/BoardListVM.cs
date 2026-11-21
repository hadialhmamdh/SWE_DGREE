using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using IntroSE.Kanban.Frontend.Controllers;
using IntroSE.Kanban.Frontend.Model;

namespace IntroSE.Kanban.Frontend.ViewModel
{
    internal class BoardListVM : Notifiable
    {
        private BoardController controller;
        public string Email { get; }
        public string WelcomeMessage { get { return $"Welcome, {Email}!"; } }
        private ObservableCollection<BoardModel> _boards;
        public ObservableCollection<BoardModel> Boards
        {
            get => _boards;
            set
            {
                _boards = value;
                RaisePropertyChanged(nameof(Boards)); 
            }
        }
        private string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }
        private string _createBoardError;

        public string CreateBoardError
        {
            get => _createBoardError;
            set
            {
                _createBoardError = value;
                RaisePropertyChanged(nameof(CreateBoardError));
            }
        }

        public BoardListVM(string email)
        {
            Email = email;
           controller = ControllerFactory.Instance.BoardController;
           Boards = controller.GetALLBoards(email);
           ErrorMessage = "";
           CreateBoardError = "";
        }

        public void DeleteBoard(BoardModel board)
        {
            try
            {
                controller.DeleteBoard(board.Owner, board.Name);
                restartANDLoad();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
        public bool CreateBoard(string email, string name)
        {
            try
            {
                controller.CreateBoard(email, name);
                restartANDLoad();
                return true;
            }
            catch (Exception ex)
            {
                CreateBoardError = ex.Message;
                return false;   
            }
        }

        
        public void restartANDLoad()
        {
            try
            {
                Boards = controller.GetALLBoards(Email);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }
   

        public void ClearCreateBoardError()
        {
            CreateBoardError = "";
        }
    }
}
