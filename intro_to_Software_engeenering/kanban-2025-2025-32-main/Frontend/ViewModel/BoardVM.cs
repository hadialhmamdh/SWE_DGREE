using System;
using System.Collections.ObjectModel;
using IntroSE.Kanban.Frontend.Controllers;
using IntroSE.Kanban.Frontend.Model;

namespace IntroSE.Kanban.Frontend.ViewModel
{
    internal class BoardVM : Notifiable
    {
        private readonly BoardController _controller;

        public string BoardName { get; }
        public string OwnerEmail { get; }

        public ColumnModel Backlog { get; }
        public ColumnModel InProgress{ get; }
        public ColumnModel Done { get; }

        private ObservableCollection<string> _members ;
        public ObservableCollection<string> Members
        {
            get => _members;
            set { _members = value; RaisePropertyChanged(nameof(Members)); }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; RaisePropertyChanged(nameof(ErrorMessage)); }
        }

        public BoardVM(BoardModel board)
        {
            OwnerEmail = board.Owner;
            BoardName = board.Name;
            this.Backlog = board.Columns[0];
            this.InProgress = board.Columns[1];
            this.Done = board.Columns[2];
            this.Members=new ObservableCollection<string>(board.Members);
        }
      

        public void ClearError() => ErrorMessage = "";
    }
}
