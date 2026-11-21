using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IntroSE.Kanban.Frontend.Model;
using IntroSE.Kanban.Frontend.ViewModel;

namespace IntroSE.Kanban.Frontend.View
{
    /// <summary>
    /// Interaction logic for BoardList.xaml
    /// </summary>
    public partial class BoardList : Window
    {
        private string email;
        private BoardListVM boardListVM;
        public BoardList(string email)
        {
            InitializeComponent();
            this.email = email;
            boardListVM = new BoardListVM(email);
            DataContext = boardListVM;

        }
        private void CreateBoard_Click(object sender, RoutedEventArgs e)
        {
            boardListVM.ClearCreateBoardError();
            CreateDialogOverlay.Visibility = Visibility.Visible;
            BoardNameBox.Focus();
        }

        private void ConfirmCreateBoard_Click(object sender, RoutedEventArgs e)
        {
            string boardName = BoardNameBox.Text.Trim();

            bool success = boardListVM.CreateBoard(email, boardName);

            if (success)
            {
                
                BoardNameBox.Text = "";
                CreateDialogOverlay.Visibility = Visibility.Collapsed;
            }
            
        }

        private void CancelCreateBoard_Click(object sender, RoutedEventArgs e)
        {
            BoardNameBox.Text = "";
            boardListVM.ClearCreateBoardError(); 
            CreateDialogOverlay.Visibility = Visibility.Collapsed;
        }
        private void DeleteBoard_Click(object sender, RoutedEventArgs e)
        {
            BoardModel selectedBoard = (BoardModel)((Button)sender).DataContext;
            boardListVM.DeleteBoard(selectedBoard);
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BoardModel board = (BoardModel)((ListViewItem)sender).DataContext;
            BoardView boardView = new BoardView(board);
            boardView.Show();
           // this.Close();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                BoardListView.SelectedItem = null;
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
