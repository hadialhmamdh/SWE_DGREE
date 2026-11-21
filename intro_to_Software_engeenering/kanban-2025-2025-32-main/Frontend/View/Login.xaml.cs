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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Login : Window
    {
        private LoginVM loginVM;
        public Login()
        {
            InitializeComponent();
            loginVM = new LoginVM();
            this.DataContext = loginVM;
        }
        private void ShowLoginDialog_Click(object sender, RoutedEventArgs e)
        {
            
            loginVM.ClearData();
            LoginPasswordBox.Password = "";
            LoginDialogOverlay.Visibility = Visibility.Visible;
            LoginEmailBox.Focus();
        }
        private void ShowRegisterDialog_Click(object sender, RoutedEventArgs e)
        {
            loginVM.ClearData();
            RegisterPasswordBox.Password = "";
            RegisterDialogOverlay.Visibility = Visibility.Visible;
            RegisterEmailBox.Focus();
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            UserModel user = loginVM.Login();
            if (user != null)
            {
                BoardList boardListWindow = new BoardList(user.Email);
                boardListWindow.Show();
                //
                loginVM.ClearData();
                this.Close();
            }
            
        }
        private void CancelLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginDialogOverlay.Visibility = Visibility.Collapsed;
            loginVM.ClearData();
            LoginPasswordBox.Password = "";
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            UserModel user = loginVM.Register();
            if (user != null)
            {
                BoardList boardListWindow = new BoardList(user.Email);
                boardListWindow.Show();
                //
                loginVM.ClearData();
                this.Close();
            }
              
        }
        private void CancelRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterDialogOverlay.Visibility = Visibility.Collapsed;
            loginVM.ClearData();
            RegisterPasswordBox.Password = "";
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginVM vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }


    }
}
