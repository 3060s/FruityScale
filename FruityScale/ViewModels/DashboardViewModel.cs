using System;
using System.Windows;

namespace FruityScale.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private string _statusBarText;
        private Visibility _statusBarVisibility;

        public string StatusBarText
        {
            get => _statusBarText;
            set
            {
                _statusBarText = value;
                OnPropertyChanged();
            }
        }

        public Visibility StatusBarVisibility
        {
            get => _statusBarVisibility;
            set
            {
                _statusBarVisibility = value;
                OnPropertyChanged();
            }
        }

        public void ShowError(string message)
        {
            StatusBarText = message;
            StatusBarVisibility = Visibility.Visible;
        }

        public void HideError()
        {
            StatusBarText = string.Empty;
            StatusBarVisibility = Visibility.Collapsed;
        }
    }
}