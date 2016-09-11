using System;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Le_Chat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class register : Page
    {
        public register()
        {
            this.InitializeComponent();
            clientClass.currentFrame = (Frame)Window.Current.Content;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void done_Click(object sender, RoutedEventArgs e)
        {
            if (userBox.Text != "" || passBox.Text != "" || repassBox.Text != "")
            {
                if (passBox.Text == repassBox.Text)
                {
                    newsignup sendDetails = new newsignup { username = userBox.Text, password = passBox.Text };
                    clientClass.send("Register", JsonConvert.SerializeObject(sendDetails));
                }
                else
                {
                    await new MessageDialog("Password Doesn't match!").ShowAsync();
                }
            }
            else
            {
                await new MessageDialog("Fill it!").ShowAsync();
            }
            
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
        
    }
}
