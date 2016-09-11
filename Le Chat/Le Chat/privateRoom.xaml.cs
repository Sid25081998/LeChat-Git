using System;
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
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.Phone.UI.Input;
using Windows.UI.ViewManagement;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Le_Chat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class privateRoom : Page
    {
        public privateRoom()
        {
            this.InitializeComponent();
            msgBox.KeyDown += MsgBox_KeyDown;
            
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

       
        private void MsgBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (msgBox.Text != "" & msgBox.Text != "Type Your Text Here")
                {
                    textMessage text = new textMessage { msg = msgBox.Text, path = toName.Text };
                    clientClass.send("txtMsg", JsonConvert.SerializeObject(text));
                    clientClass.myConversations[toName.Text].Add(new conv { path = clientClass.myName, msg = msgBox.Text, align = HorizontalAlignment.Right });
                    msgBox.Text = "";
                }
            }
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            Frame.Navigate(clientClass.currentFrame.BackStack.Last().SourcePageType);
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Tuple<string, ObservableCollection<conv>> param = (Tuple<string, ObservableCollection<conv>>)e.Parameter;
            toName.Text = (string)e.Parameter;
            chatView.ItemsSource = clientClass.myConversations[toName.Text];
            ScrollToBottom();
        }

        private void textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (msgBox.Text == "Type Your Text Here")
            {
                msgBox.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                msgBox.Text = "";
            }
            Header.Margin = new Thickness(0, 340, 0, 0);
            chatView.Margin = new Thickness(0, 380, 0, 47);
            ScrollToBottom();
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            if (msgBox.Text != "" & msgBox.Text != "Type Your Text Here")
            {
                textMessage text = new textMessage { msg = msgBox.Text, path = toName.Text };
                clientClass.send("txtMsg", JsonConvert.SerializeObject(text));
                clientClass.myConversations[toName.Text].Add(new conv { path = clientClass.myName, msg = msgBox.Text, align = HorizontalAlignment.Right });
                clientClass.recentmsgs.Add(text);
                msgBox.Text = "";
            }
        }

        private void msgBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (msgBox.Text == "")
            {
                msgBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
                msgBox.Text = "Type Your Text Here";
            }
            Header.Margin = new Thickness(0, 0, 0, 0);
            chatView.Margin = new Thickness(0, 42, 0, 47);
            chatView.Height = 550;
        }

        private void ScrollToBottom()
        {
            var selectedIndex =chatView.Items.Count - 1;
            if (selectedIndex < 0)
                return;

            chatView.SelectedIndex = selectedIndex;
            chatView.UpdateLayout();

            chatView.ScrollIntoView(chatView.SelectedItem);
        }

        private void chatView_Loaded(object sender, RoutedEventArgs e)
        {
            clientClass.myConversations[toName.Text].CollectionChanged += (s, args) => ScrollToBottom();
        }
    }
}
