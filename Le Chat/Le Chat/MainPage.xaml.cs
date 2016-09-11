using System;
using Windows.Networking.Sockets;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Newtonsoft.Json;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Le_Chat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            clientClass.currentFrame = (Frame)Window.Current.Content;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.
            clientClass.connect();
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
        

        private async void done_Click(object sender, RoutedEventArgs e)
        {
            if(userBox.Text!="" || passBox.Text != "")
            {
                Login myLogin = new Login { username = userBox.Text, password = passBox.Text };
                clientClass.send("Login", JsonConvert.SerializeObject(myLogin));
                clientClass.myName = myLogin.username;
            }
            else
            {
                await new MessageDialog("Fill It").ShowAsync();
            }
        }

        private void signup_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(register));
        } 
    }
}
