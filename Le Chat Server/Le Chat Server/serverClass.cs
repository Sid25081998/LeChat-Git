using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Data;
using MySql.Data.MySqlClient;


namespace Le_Chat_Server
{
    
    class serverClass
    {
        public static Dictionary<Socket,user> userList= new Dictionary<Socket, user>(); 
        private static Socket socket= new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);        //Socket Connection                                
        private static MySqlCommand cmd;
        public static ListBox logList;
        private static MySqlConnection con = new MySqlConnection("datasource=localhost;port=3306;database=chatdb;username=root;password=Sid25081998!;"); //Database Connection


        public static async void setup() //SERVER SETUP ->CONNNETING TO DATABASE ANS START LISTENING TO CLIENT SOCKETS...
        {
            addLog("Initiating Server...");
            addLog("Connecting To Database...");
            try
            {
                await con.OpenAsync();
                while (con.State == ConnectionState.Closed)
                {
                    //Wait for Connect to database......
                }
                addLog("Connected to Database");
            }
            catch(Exception e)
            {
                addLog(e.ToString());
                return;
            }
            addLog("Server Initiation Complete");
            socket.Bind(new IPEndPoint(IPAddress.Any, 1000));
            socket.Listen(5);
            socket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            addLog("Listening For Clients");
        }
        
        private static void AcceptCallBack(IAsyncResult result)  //CALLBACAK FUNCTION FOR ACCEPTING THE CONNECTION FROM THE CLIENT...
        {
            Socket newSocket = socket.EndAccept(result);
            addLog("Socket Connected. Listening Again....");
            user newuser = new user();
            userList.Add(newSocket, newuser);
            beginReceive(newSocket);
            socket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }

        private static void beginReceive(Socket mySocket) //START RECEIVING FROM THE CLIENT BY GIVEN SOCKET.....
        {
            byte[] byteArray = new byte[1023];
            mySocket.BeginReceive(byteArray, 0, byteArray.Length, SocketFlags.None, new AsyncCallback(onReceive),Tuple.Create(mySocket,byteArray));
        }
        
        private static async void onReceive(IAsyncResult result)  //CALLBACK FOR RECEIVING -> Handles the information after receiving the from client
        {
            Tuple<Socket, byte[]> state = (Tuple<Socket, byte[]>)result.AsyncState;
            Socket mySocket = state.Item1;
            byte[] readByte = state.Item2;
            
            try
            {
                mySocket.EndReceive(result);
                //Data received and ready to execute server process//
                Data received = getData(readByte);
                switch (received.cmd)
                {
                    case "Login":
                        Login checkUser = JsonConvert.DeserializeObject<Login>(received.message);
                        string r = await getUserInformationAsync(Tuple.Create(checkUser.username, checkUser.password), mySocket);
                        send(received.cmd, r, mySocket);
                        break;
                    case "Register":
                        newsignup newUser = JsonConvert.DeserializeObject<newsignup>(received.message);
                        user newUsr = new user();
                        register(newUser, newUsr);
                        if (newUsr.name != null)
                        {
                            send(received.cmd, "Success", mySocket);
                            addLog(newUsr.name + " Successfully Registered");
                        }
                        else
                        {
                            send(received.cmd, "Failed", mySocket);
                        }
                        break;
                    case "allFriends":
                            send(received.cmd, getFriends(mySocket), mySocket);
                        break;
                    case "findFriend":
                        string friendname = received.message;
                        List<string> fList = new List<string>();
                        findFriend(friendname, fList,mySocket);
                        send(received.cmd, JsonConvert.SerializeObject(fList), mySocket);
                        break;
                    case "addFriend":
                        addFriend(received.message, mySocket);
                        send("addFriend", received.message, mySocket);
                        Socket myNewfriend = getSocketByName(received.message);
                        if (myNewfriend != null)
                        {
                            userList[myNewfriend].requests.Add(userList[mySocket].name);
                            send("getRequests", JsonConvert.SerializeObject(userList[myNewfriend].requests), myNewfriend);
                        }
                        break;
                    case "getRequests":
                        send(received.cmd, JsonConvert.SerializeObject(userList[mySocket].requests),mySocket);
                        break;
                    case "txtMsg":
                        textMessage text = JsonConvert.DeserializeObject<textMessage>(received.message);
                        try
                        {
                            Socket toSocket = getSocketByName(text.path);
                            textMessage msg = new textMessage { msg = text.msg, path = userList[mySocket].name };
                            send("txtMsg", JsonConvert.SerializeObject(msg), toSocket);
                        }
                        catch (Exception e)
                        {
                            //MessageBox.Show(e.ToString());
                        }
                        user me = userList[mySocket];
                        saveMyText(text.path,text.msg, me);
                        break;
                    case "getMessage":
                        List <textMessage> msgs = new List<textMessage>();
                        /*msgs.Add(new textMessage { path = "pipun", msg = "Hello" });
                        msgs.Add(new textMessage { path = "sparsha", msg = "Hey Dude" });
                        msgs.Add(new textMessage { path = "pipun", msg = "How are you" });
                        msgs.Add(new textMessage { path = "sparsha", msg = "fine Bro hgcdjydvjydcrdrsrjjdcrddvtvdjytdcyrsxrectyeytjvtrcytexjyreec6vr6rvk6rcecjcyeyrjrcdn" });*/
                        getMyConversations(received.message, userList[mySocket], msgs);
                        send(received.cmd, JsonConvert.SerializeObject(msgs), mySocket);
                        break;
                    case "terminate":
                        send("terminate", "Wish You get it soon", mySocket);
                        break;
                    case "requestapproval":
                        Friendrequest response = JsonConvert.DeserializeObject<Friendrequest>(received.message);
                        if (response.status == "accept")
                        {
                            userList[mySocket].requests.Remove(response.name);
                            userList[mySocket].friends.Add(response.name);
                            updateMyRelation(response.name, 0,mySocket);
                        }
                        else
                        {
                            userList[mySocket].requests.Remove(response.name);
                            updateMyRelation(response.name, 1,mySocket);
                        }
                        break;
                }
                addLog(received.cmd + " from " + userList[mySocket].name);
                beginReceive(mySocket);
            }
            catch(Exception e)
            {
                user me = userList[mySocket];
                userList.Remove(mySocket);
                if (me.name != null)
                {
                    addLog(me.name + " Disconnected");
                    notifyMyStatus(me);
                }
                else addLog("Unknown Socket Disconnected");
            }
        }
        
        public static Data getData(byte[] DataByte) // CONVERT BYTES RECEIVED TO STRING AND RETURN DATA OBJECT.......
        {
            string DataString = Encoding.UTF8.GetString(DataByte, 0, DataByte.Length);
            return JsonConvert.DeserializeObject<Data>(DataString);
        }

        public static void addLog(string msg)  //Add logs to the Server logs
        {
            if (logList.InvokeRequired)
            {
                logList.Invoke(new Action(() => { logList.Items.Add(msg+"     "+DateTime.Now.ToString());logList.SelectedIndex = logList.Items.Count - 1; }));
            }

            else
            {
                logList.Items.Add(msg);
                logList.SelectedIndex = logList.Items.Count - 1;
            }
            
        }

        private static void send(string command,string msg, Socket mySocket)  // SEND DATA TO THE GIVEN SOCKET............. 
        {
            Data response = new Data { cmd = command, message = msg };
            byte[] myBytes = response.getBytes();
            mySocket.BeginSend(myBytes, 0, myBytes.Length, SocketFlags.None, new AsyncCallback(onSend), mySocket);
        }

        private static void onSend(IAsyncResult result) //SEND CALL BACK............
        {
            Socket mySocket = (Socket)result.AsyncState;
            mySocket.EndSend(result);
        }

        private async static void register(newsignup newUser, user usr) //CHECK FOR NEW USER AND ALLOW..........
        {
            MySqlDataReader reader;
            int count = 0;
            cmd = new MySqlCommand("select * from users where name = '" + newUser.username + "'", con);
            IAsyncResult result;
            result = cmd.BeginExecuteReader();
            reader = cmd.EndExecuteReader(result);
            while (await reader.ReadAsync())
            {
                count++;
            }
            reader.Close();
            if (count > 0)
            {
                usr = null;
            }
            else
            {
                cmd = new MySqlCommand("INSERT INTO users(name,password) VALUES(@user,@password)", con);
                cmd.Parameters.AddWithValue("@user", newUser.username);
                cmd.Parameters.AddWithValue("@password", newUser.password);
                result = cmd.BeginExecuteNonQuery();
                cmd.EndExecuteNonQuery(result);
                usr.name = newUser.username;
            }
        }

        private static async void getMyRelations(string id, List<string> friendList,List<string> requests, MySqlConnection con)
        {
            MySqlDataReader reader;
            friendList.Clear();
            requests.Clear();
            cmd = new MySqlCommand("select name from (select userid s,status from friends where friendid="+id+" union select friendid s,status from friends where userid="+id+ ") A inner join users on A.s=users.id where A.status=1;select name from users where id in (select userid from friends where friendid=" + id + " and status=0);", con);
            IAsyncResult result = cmd.BeginExecuteReader();
            reader = cmd.EndExecuteReader(result);
            while (await reader.ReadAsync())
            {
                string name = reader.GetString(0);
                friendList.Add(name);
            }
            reader.NextResult();
            
            while(await reader.ReadAsync())
            {
                requests.Add(reader.GetString(0));
            }
            reader.Close();
        } //GET THE FRIENDS LIST AND REQUESTS OF THE CLIENT

        private static async void findFriend(string name, List<string> list, Socket mySocket)
        {
            MySqlDataReader reader;
            cmd = new MySqlCommand("select name from users where name like '%"+name+ "%'  and name not in (select name from (select userid s from friends where friendid=" + userList[mySocket].id + " union select friendid s from friends where userid=" + userList[mySocket].id + ") A inner join users on A.s=users.id);", con);
            IAsyncResult result = cmd.BeginExecuteReader();
            reader = cmd.EndExecuteReader(result);
            while (await reader.ReadAsync())
            {
                string res = reader.GetString(0);
                list.Add(reader.GetString(0));
            }
            list.Remove(userList[mySocket].name);
            reader.Close();
        } //SEARCH THE LIST AS GIVEN

        private static string getFriends(Socket mySocket) //GET THE FRIENDS OF THE GIVEN USER SOCKET............
        {
            List<friend> allFriends = new List<friend>();
            int f = 0;
            foreach (var i in userList[mySocket].friends)
            {
                friend myFriend = new friend();
                myFriend.name = i;
                f = 0;
                foreach (var j in userList.Values)
                {
                    
                    if (j.name == i)
                    {
                        myFriend.IsOnline = true;
                        f = 1;
                        break;
                    }
                    
                }
                if (f == 0)
                {
                    myFriend.IsOnline = false;
                }
                allFriends.Add(myFriend);
            }
            return JsonConvert.SerializeObject(allFriends);
        }

        private static void addFriend(string name,Socket mySocket)
        {
            cmd = new MySqlCommand("insert into friends (userid,friendid,status) values("+userList[mySocket].id+",(select id from users where name ='"+name+"'),0);", con);
            IAsyncResult result = cmd.BeginExecuteNonQuery();
            cmd.EndExecuteNonQuery(result);
        }

        private static Socket getSocketByName(string name)
        {
            return userList.FirstOrDefault(x => x.Value.name == name).Key;
        }  //GET THE SOCKET OF THE GIVEN NAME

        private static void notifyMyStatus(user me)
        {
            foreach (var i in me.friends)
            {
                Socket fSocket = getSocketByName(i);
                if (fSocket != null)
                {
                    send("allFriends", getFriends(fSocket), fSocket);
                }
            }
        } //NOTFY MY ONLINE STATUS ALL MY FRIEND

        public static Task<string> getUserInformationAsync(Tuple<string,string> creds,Socket mySocket)
        {
            MySqlConnection con = new MySqlConnection("datasource=localhost;port=3306;database=chatdb;username=root;password=Sid25081998!;");
            MySqlDataReader reader;
            return Task.Run(() =>
            {

                con.Open();
                cmd = new MySqlCommand("select * from users where name = '" + creds.Item1 + "'", con);
                reader = cmd.ExecuteReader();
                string resultId = null;
                string resultUser = null;
                string resultPass = null;
                int count = 0;
                while (reader.Read())
                {
                    count++;
                    resultId = reader.GetString(0);
                    resultUser = reader.GetString(1);
                    resultPass = reader.GetString(2);
                }
                reader.Close();
                if (count>0 && creds.Item1 == resultUser && creds.Item2 == resultPass)
                {
                    user addUser = new user();
                    addUser.id = Convert.ToInt32(resultId);
                    addUser.name = resultUser;
                    addUser.friends = new List<string>();
                    addUser.requests = new List<string>();
                    addUser.con = con;
                    getMyRelations(resultId, addUser.friends, addUser.requests,con);
                    userList[mySocket] = addUser;
                    notifyMyStatus(addUser);
                    addLog(resultUser);
                return "Success";
                }
                else
                {
                return "Failed";
                }
            });
        }  //CHECK CREDENTIALS AND GET INFORMATION

        public static void updateMyRelation(string name,int response,Socket mySocket)
        {
            MySqlCommand cmd;
            IAsyncResult result;
            MySqlConnection con= userList[mySocket].con;
            int id = userList[mySocket].id;
            if (response == 0)
            {
                cmd = new MySqlCommand("update friends set status=1 where userid=(select id from users where name='" + name + "') and friendid="+id+";", con);
                result = cmd.BeginExecuteNonQuery();
                cmd.EndExecuteNonQuery(result);
                Socket s = getSocketByName(name);
                if (s != null) {
                    userList[s].friends.Add(userList[mySocket].name);
                    send("allFriends", getFriends(s), s);
                }
            }
            else
            {
                cmd = new MySqlCommand("delete from friends where userid=(select id from users where name='" + name + "') and friendid="+id+"", con);
                result = cmd.BeginExecuteNonQuery();
                cmd.EndExecuteNonQuery(result);
            }
            getMyRelations(userList[mySocket].id.ToString(), userList[mySocket].friends, userList[mySocket].requests, userList[mySocket].con);
        }  //SET MY FRIEND OR DELETE MY RELATIONSHIP

        private static void saveMyText(string to,string msg,user me)
        {
            MySqlCommand cmd = new MySqlCommand("insert into messages (UCI,msgFrom,msg) values((select UCI from friends where userid=" + me.id + " and friendid=(select id from users where name='" + to + "') or userid=(select id from users where name='" + to + "') and friendid=" + me.id + "),'" + me.name + "','" + msg + "');", me.con);
            Task.Run(() =>
            {
                IAsyncResult result = cmd.BeginExecuteNonQuery();
                cmd.EndExecuteNonQuery(result);
            });
        }  //ADD MY TEXT TO THE TABLE

        private async static void getMyConversations(string from,user me, List<textMessage> mList)
        {
            MySqlCommand cmd = new MySqlCommand("select msgFrom,msg from messages where UCI=(select UCI from friends where userid=" + me.id + " and friendid=(select id from users where name='" + from + "') or userid=(select id from users where name='" + from + "') and friendid=" + me.id + ");", me.con);
            IAsyncResult result = cmd.BeginExecuteReader();
            MySqlDataReader reader = cmd.EndExecuteReader(result);
            while (await reader.ReadAsync())
            {
                mList.Add(new textMessage { path = reader.GetString(0), msg = reader.GetString(1) });
            }
            reader.Close();
        } //GET MY CONVERSATION FROM THE DATABASE

    }
    class Login
    {
        public string username { get; set; }
        public string password { get; set; }
 
    }
    class newsignup
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    class newMsg
    {
        public string to { get; set; }
        public string msg { get; set; }
        public string from { get; set; }
    }
    class friend
    {
       public string name;
       public bool IsOnline;
    }
    class user
    {
        public int id;
        public string name { get; set; }
        public Socket socket;
        public List<string> friends;
        public List<string> requests;
        public MySqlConnection con;
        public user()
        {
            name = null;
        }
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
            List<byte> myData = new List<byte>();
            byte[] dataBytes=Encoding.UTF8.GetBytes(DataString);
            int dataLength = dataBytes.Length;
            string dataLengthString = dataLength.ToString();
            if (dataLength < 100)
            {
                dataLengthString = "0" + dataLengthString;
            }
            myData.AddRange(Encoding.UTF8.GetBytes(dataLengthString));
            myData.AddRange(dataBytes);
            byte[] myBytes = myData.ToArray();
            return myBytes;
        }
    }
    class textMessage
    {
        public string msg { get; set; }
        public string path { get; set; }
    }
    class Friendrequest
    {
        public string name;
        public string status;
    }
}
