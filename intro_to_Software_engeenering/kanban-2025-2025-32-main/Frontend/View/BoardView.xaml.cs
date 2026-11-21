using System.Windows;
using IntroSE.Kanban.Frontend.Model;
using IntroSE.Kanban.Frontend.ViewModel;

namespace IntroSE.Kanban.Frontend.View
{
    public partial class BoardView : Window
    {
        internal BoardView(BoardModel boardModel)
        {
            InitializeComponent();
            BoardVM boardView = new BoardVM(boardModel);
            this.DataContext = boardView;
        }
    }
}
