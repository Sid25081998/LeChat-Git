using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace V_Bunk
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Att : Page
    {
        public Att()
        {
            this.InitializeComponent();
            
            //this.DataContext = attCol;
            

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Attendance.ItemsSource = myClass.attCol;
            taskList.ItemsSource = myClass.myTasks;
        }

        private void tTable_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(tTable));
        }
        private async void refresh_Click(object sender, RoutedEventArgs e)
        {
            if (tapAgain.Visibility==Visibility.Visible)
            {
                var schedules = myClass.toastNotifier.GetScheduledToastNotifications();
                foreach (var i in schedules)
                {
                    myClass.toastNotifier.RemoveFromSchedule(i);
                }
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("subjectStorage");
                await file.DeleteAsync();
                MessageDialog refresh = new MessageDialog("App is about to close.\nRestart the App to Refresh");
                await refresh.ShowAsync();
                Application.Current.Exit();
            }
            tapAgain.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            tapAgain.Visibility = Visibility.Collapsed;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (vDash.SelectedIndex == 1)
            {
                Pin.Visibility = Visibility.Visible;
                refresh.Visibility = Visibility.Collapsed;
                tTable.Visibility = Visibility.Collapsed;
            }
            else
            {
                refresh.Visibility = Visibility.Visible;
                tTable.Visibility = Visibility.Visible;
                Pin.Visibility = Visibility.Collapsed;
            }
        }

        private void Pin_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddTask));
        }
    }
}
