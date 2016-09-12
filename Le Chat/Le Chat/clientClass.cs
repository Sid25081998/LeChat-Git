using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
using Newtonsoft.Json;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;


namespace Le_Chat
{
    public enum Command
    {
        Login,
        Null,
    }

    class clientClass
    {

        public static StreamSocket socket = new StreamSocket();
        public static DataWriter writer;
        public static DataReader reader = new DataReader(socket.InputStream);
        public static byte[] readByte;
        public static Frame currentFrame=null;
        public static bool IsConnected=false;
        public static string myName;
        public static ObservableCollection<friend> friends;
        public static ObservableCollection<addFriend> requests;
        public static ListView resultList;
        public static ListView fList;
        public static ListView requestList;
        public static int chatIFIndex=0;
        public static TextBlock requestCount;
        public static Button friendRequestsButton;
        public static string to;
        public static Dictionary<string,ObservableCollection<conv>> myConversations= new Dictionary<string, ObservableCollection<conv>>();
        public static ObservableCollection<textMessage> recentmsgs = new ObservableCollection<textMessage>();


        public static async void connect()
        {
            
            try
            {
                
                if(IsConnected==false)
                {
                    await socket.ConnectAsync(new HostName("192.168.137.1"), 1000.ToString());
                    recieve();
                    IsConnected = true;
                }
            }
            catch(Exception e)
            {
                await new MessageDialog(e.ToString()).ShowAsync();
            }
        } //CONNECTS TO THE SERVER

        public static async void send(string command, string Message)
        {
            try
            {
                Data sendinf = new Data { cmd = command, message = Message };
                byte[] sendByte = sendinf.getBytes();
                writer= new DataWriter(socket.OutputStream);
                writer.WriteBytes(sendByte);
                await writer.StoreAsync();
                writer.DetachStream();
                writer.Dispose();
            }
            catch(Exception e)
            {
                await new MessageDialog(e.ToString()).ShowAsync();
            }
        }   //SENDS THE GIVEN BYTE TO THE SERVER

        public static Data getData(byte[] DataByte)
        {
            string DataString = Encoding.UTF8.GetString(DataByte,0,DataByte.Length);
            return JsonConvert.DeserializeObject<Data>(DataString);
        }  //CONVERTS BYTES TO DATA

        public async static void recieve()
        {
            uint numFileBytes=1;
            try
            {
                reader.InputStreamOptions = InputStreamOptions.Partial;
                numFileBytes = await reader.LoadAsync(3);
                readByte = new byte[numFileBytes];
                reader.ReadBytes(readByte);
                uint stringLength = Convert.ToUInt32(Encoding.UTF8.GetString(readByte, 0, readByte.Length));
                numFileBytes = await reader.LoadAsync(stringLength);
                readByte = new byte[stringLength];
                reader.ReadBytes(readByte);
                Data received = getData(readByte);
                switch (received.cmd)
                {
                    case "Login":
                        if (received.message == "Success")
                        {
                            //Succesful Login................
                            send("allFriends","");
                        }
                        else
                        {
                            await new MessageDialog("Invalid Credentials").ShowAsync();
                        }
                        break;
                    case "Register":
                        if (received.message == "Success")
                        {
                            currentFrame.Navigate(typeof(MainPage));
                        }
                        else
                        {
                            await new MessageDialog("User Name Already Exist!").ShowAsync();
                        }
                        break;
                    case "allFriends":
                        friends = new ObservableCollection<friend>();
                        if (received.message != "")
                        {
                            List<friend> rList = JsonConvert.DeserializeObject<List<friend>>(received.message);
                            foreach(var x in rList)
                            {
                                friend myFriend = new friend();
                                myFriend.name = x.name;
                                myFriend.IsOnline = x.IsOnline;
                                if (x.IsOnline == true)
                                {
                                    myFriend.onImage = Visibility.Visible;
                                    myFriend.offImage = Visibility.Collapsed;
                                    friends.Insert(0, myFriend);
                                }
                                else
                                {
                                    myFriend.onImage = Visibility.Collapsed;
                                    myFriend.offImage = Visibility.Visible;
                                    friends.Add(myFriend);
                                }
                            }
                            if (currentFrame.SourcePageType == typeof(MainPage))
                            {
                                send("getRequests", "");
                            }
                            else if (currentFrame.SourcePageType == typeof(chatIF))
                            {
                                fList.ItemsSource = friends;
                            }
                        }
                        break;
                    case "findFriend":
                        List<string> foundList = JsonConvert.DeserializeObject<List<string>>(received.message);
                        ObservableCollection<addFriend> searchList = new ObservableCollection<addFriend>();
                        foreach(var i in foundList)
                        {
                            searchList.Add(new addFriend(i));
                        }
                        resultList.ItemsSource = searchList;
                        break;
                    case "getRequests":
                        List<string> list = JsonConvert.DeserializeObject<List<string>>(received.message);
                        requests = new ObservableCollection<addFriend>();
                        foreach(var name in list)
                        {
                            requests.Add(new addFriend(name));
                        }
                        if (currentFrame.SourcePageType == typeof(MainPage))
                        {
                            currentFrame.Navigate(typeof(chatIF));
                        }
                        else if (currentFrame.SourcePageType == typeof(chatIF))
                        {
                            int count = requests.Count;
                            requestList.ItemsSource = requests;
                            if (count > 0)
                            {
                                requestCount.Text = count.ToString();
                                friendRequestsButton.Visibility=Visibility.Visible;
                            }
                            else
                            {
                                requestCount.Text = "";
                                friendRequestsButton.Visibility=Visibility.Collapsed;
                            }
                        }
                        break;
                    case "txtMsg":
                        textMessage gotText = JsonConvert.DeserializeObject<textMessage>(received.message);
                        if (!myConversations.ContainsKey(gotText.path))
                        {
                            myConversations[gotText.path]= new ObservableCollection<conv>();
                        }
                        myConversations[gotText.path].Add(new conv { path = gotText.path, msg = gotText.msg, align = HorizontalAlignment.Left });
                        searchRecentsAndRemove(gotText.path);
                        recentmsgs.Insert(0,new textMessage { path = gotText.path, msg = gotText.msg });
                        break;
                    case "getMessage":
                        ObservableCollection<conv> msgs = new ObservableCollection<conv>(JsonConvert.DeserializeObject<List<conv>>(received.message));
                        foreach(var i in msgs)
                        {
                            if (i.path == myName)
                            {
                                i.align = HorizontalAlignment.Right;
                            }
                            else
                            {
                                i.align = HorizontalAlignment.Left;
                            }
                        }
                        myConversations[to] = msgs;
                        currentFrame.Navigate(typeof(privateRoom),to);
                        break;
                }
                
                recieve();
            }
            catch(Exception e)
            {
                await new MessageDialog(e.ToString()).ShowAsync();
            }
        }   //ACTIONS AFTER RECEIVING FROM SERVER
        private static void searchRecentsAndRemove(string name)
        {
            for(int i=0;i<recentmsgs.Count;i++)
            {
                if (recentmsgs[i].path == name)
                {
                    recentmsgs.RemoveAt(i);
                    return;
                }
            }
        }
    }
    class textMessage
    {
        public string msg { get; set; }
        public string path { get; set; }
    }
    class Login
    {
        public string username { get; set; }
        public string password { get; set; }
        public Login()
        {
            this.username = null;
            this.password = null;
        }
    }
    class newsignup
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    class Data
    {
        public string cmd { get; set; }
        public string message { get; set; }
        public Data()
        {
            this.cmd = null;
            this.message = null;
        }
        public byte[] getBytes()
        {
            string DataString = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(DataString);
        }
    }
    class friend
    {
        public string name { get; set; }
        public bool IsOnline {get; set; }
        public Visibility onImage { get; set; }
        public Visibility offImage { get; set; }
    }
    class addFriend
    {
        public string name { get; set; }
        public addFriend(string foundName)
        {
            name = foundName;
        }
    }
    class conv
    {
        public string msg { get; set; }
        public string path { get; set; }
        public HorizontalAlignment align{get;set;}
    }
    class Friendrequest
    {
        public string name;
        public string status;
    }
}
