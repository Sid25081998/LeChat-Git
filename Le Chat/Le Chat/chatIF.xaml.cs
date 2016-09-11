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
using Windows.Phone.UI.Input;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Le_Chat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class chatIF : Page
    {
        public chatIF()
        {
            this.InitializeComponent();
            requestFlyout.Closed += RequestFlyout_Closed;
            clientClass.currentFrame = (Frame)Window.Current.Content;
            clientClass.fList = friends;
            friends.ItemsSource = clientClass.friends;
            clientClass.resultList = fList;
            clientClass.requestList = requestListView;
            clientClass.requestCount = nrequest;
            clientClass.friendRequestsButton = rbutton;
            requestListView.ItemsSource = clientClass.requests;
            int count = clientClass.requests.Count;
            
            if (count > 0)
            {
                nrequest.Text = count.ToString();
                rbutton.Visibility=Visibility.Visible;
            }
            else
            {
                nrequest.Text = "";
                rbutton.Visibility=Visibility.Collapsed;
            }
            recents.ItemsSource = clientClass.recentmsgs;
        }

        private void RequestFlyout_Closed(object sender, object e)
        {
            clientClass.send("allFriends", "");
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (searchFriend.Visibility == Visibility.Visible)
            {
                e.Handled = true;
                searchFriend.Visibility = Visibility.Collapsed;
                pivotView.Visibility = Visibility.Visible;
                dashBar.Visibility = Visibility.Visible;
                fList.ItemsSource = null;
                HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            chatDash.SelectedIndex = clientClass.chatIFIndex;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            clientClass.chatIFIndex = chatDash.SelectedIndex;
            clientClass.currentFrame = (Frame)Window.Current.Content;
        }

        private void friends_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as friend;
            clientClass.to = item.name;
            clientClass.send("getMessage", item.name);
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (chatDash.SelectedIndex)
            {
                case 0:
                    NewConv.Visibility = Visibility.Visible;
                    NewFriend.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    NewConv.Visibility = Visibility.Collapsed;
                    NewFriend.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    NewConv.Visibility = Visibility.Collapsed;
                    NewFriend.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void NewFriend_Click(object sender, RoutedEventArgs e)
        {
            pivotView.Visibility = Visibility.Collapsed;
            searchFriend.Visibility = Visibility.Visible;
            dashBar.Visibility = Visibility.Collapsed;
            friendname.Text = "";
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (friendname.Text != "")
            {
                clientClass.send("findFriend", friendname.Text);
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton x = sender as AppBarButton;
            addFriend friend = x.DataContext as addFriend;
            clientClass.send("addFriend", friend.name);
            x.Label = "Sent";
            x.Icon = new SymbolIcon(Symbol.Accept);
            x.Click -= AppBarButton_Click;
        }


        private void accept_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton accept = sender as AppBarButton;
            addFriend user = accept.DataContext as addFriend;
            clientClass.send("requestapproval", Newtonsoft.Json.JsonConvert.SerializeObject(new Friendrequest { name = user.name, status = "accept" }));
            clientClass.requests.Remove(user);
            if (clientClass.requests.Count == 0)
            {
                requestFlyout.Hide();
                rbutton.Visibility = Visibility.Collapsed;
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton cancel = sender as AppBarButton;
            addFriend user = cancel.DataContext as addFriend;
            clientClass.send("requestapproval", Newtonsoft.Json.JsonConvert.SerializeObject(new Friendrequest { name=user.name,status="reject"}));
            clientClass.requests.Remove(user);
            if (clientClass.requests.Count == 0)
            {
                requestFlyout.Hide();
                rbutton.Visibility = Visibility.Collapsed;
            }
        }

        private void recents_ItemClick(object sender, ItemClickEventArgs e)
        {
            textMessage clickedMessage = e.ClickedItem as textMessage;
            Frame.Navigate(typeof(privateRoom), clickedMessage.path);
        }
    }
}
