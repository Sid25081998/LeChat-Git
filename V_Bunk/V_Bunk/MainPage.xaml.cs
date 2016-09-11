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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Text;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;







// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace V_Bunk
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        public  MainPage()
        {
            
            this.InitializeComponent();
            constructor(); // Check for already data is available
            passBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
            regBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
            InputPane.GetForCurrentView().Showing += MainPage_Showing;
            InputPane.GetForCurrentView().Hiding += MainPage_Hiding;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void MainPage_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            scrlViewer.Height = this.ActualHeight;
        }

        private void MainPage_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            scrlViewer.Height = this.ActualHeight - args.OccludedRect.Height - 50;

        }

        private async void Web_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            string x = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.URL" });
            if(x!= "https://academicscc.vit.ac.in/student/stud_login.asp")
            {
                myClass.web.LoadCompleted -= Web_LoadCompleted;
                myClass.web.LoadCompleted += Web_getSchedule;
                myClass.web.NavigationCompleted-=Web_NavigationCompleted;
                myClass.web.Navigate(new Uri("https://academicscc.vit.ac.in/student/timetable.asp?sem=FS"));
                status.Text = "Fetching Schedule";
            }
        }

        private async void Web_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string x = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.URL" });
            if (x == "https://academicscc.vit.ac.in/student/stud_login.asp")
            {
                cap();// Load the captcha
                load(false);
                string msg = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByName('message')[0].value" });
                if (msg != "")
                {
                    MessageDialog box = new MessageDialog(msg);
                    await box.ShowAsync();
                }
                captchaBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
                captchaBox.Text = "Captcha";
            }
        }
        //get the time table from the page;
        private void Web_getSchedule(object sender, NavigationEventArgs e)
        {
            getFromtable1();  // get code, name, teacher, type, room number.......
        }

        private void Web_getAttendance(object sender, NavigationEventArgs e)
        {
            getAttendance();  // get the attendance and number of classes
        }

        private void Web_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            status.Text = "No Internet Connection!!!";
            loadPic.Visibility = Visibility.Collapsed;
            mainBar.Visibility = Visibility.Collapsed;
        }





        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.
            
            myClass.web.LoadCompleted += Web_LoadCompleted;
            myClass.web.NavigationFailed += Web_NavigationFailed;
            myClass.web.NavigationCompleted += Web_NavigationCompleted;
            load(true);
            myClass.web.Navigate(new Uri("https://academicscc.vit.ac.in/student/stud_login.asp"));
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }
        
        

        private async void signin_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog m;
            if (regBox.Text == "" || passBox.Password == "" || captchaBox.Text == "")
            {
                 m = new MessageDialog("Fill in the credentials");
                await m.ShowAsync();
            }
            else if(captchaBox.Text.Length <6)
            {
                m = new MessageDialog("Check Captcha Again!!");
                await m.ShowAsync();
                
            }
            else
            {
                await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByName('regno')[0].value='" + regBox.Text + "'" });
                await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByName('passwd')[0].value='" + passBox.Password + "'" });
                await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByName('vrfcd')[0].value='" + captchaBox.Text + "'" });
                await myClass.web.InvokeScriptAsync("eval", new string[] { "document.forms[0].submit();" });
                load(true);
            }
        }


        private void load(bool x)
        {

            if (x == false)
            {

                Content.Visibility = Visibility.Visible;
                loading.Visibility = Visibility.Collapsed;
                mainBar.Visibility = Visibility.Visible;
            }
            else
            {
                Content.Visibility = Visibility.Collapsed;
                loading.Visibility = Visibility.Visible;
                mainBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void constructor()
        {
            try
            {
                StorageFolder local = ApplicationData.Current.LocalFolder;
                StorageFile file = await local.GetFileAsync("subjectStorage");
                Stream stream = await file.OpenStreamForReadAsync();
                string text;
                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
                string[] Jsons = text.Split(' ');
                myClass.timeTable = JsonConvert.DeserializeObject<ObservableCollection<ObservableCollection<Subject>>>(Jsons[0]);
                myClass.attCol = JsonConvert.DeserializeObject<ObservableCollection<Subject>>(Jsons[1]);
                stream.Dispose();
                var schedules = myClass.toastNotifier.GetScheduledToastNotifications();
                foreach(var i in schedules)
                {
                    myClass.toastNotifier.RemoveFromSchedule(i);
                }
                myClass.createNotification();
                /*foreach(var i in myClass.attCol)
                {
                    i.myColor = new SolidColorBrush(myClass.avlblColrs[myClass.attCol.IndexOf(i)]);
                }*/
                Frame.Navigate(typeof(Att));
            }
            catch
            {
                //Do Nothing.................
            }
        }
        private async void getFromtable1()
        {
            //delete the first two coloumns as it makes it complicated
            
            await myClass.web.InvokeScriptAsync("eval", new string[] { "var rows=document.getElementsByTagName('table')[1].rows;var c;for(c=0;c<rows.length;c++){if(rows[c].cells.length==15){rows[c].deleteCell(0)}}" });
            await myClass.web.InvokeScriptAsync("eval", new string[] { "var rows=document.getElementsByTagName('table')[1].rows;var c;for(c=0;c<rows.length;c++){if(rows[c].cells.length==14){rows[c].deleteCell(0)}}" });
            
            int rows = Convert.ToInt32(await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[1].rows.length.toString()" }));
            for(int row=1;row< rows-2; row++)
            {
                string text = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[1].rows['" + row + "'].cells[8].innerText.toString()" });
                if(text!= "NIL")
                {
                    Subject sub = new Subject();
                    //code
                    sub.code= await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[1].rows['" + row + "'].cells[1].innerText.toString()" });
                    
                    //title
                    sub.title= await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[1].rows['" + row + "'].cells[2].innerText.toString()" });
                    
                    string t= (await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[1].rows['" + row + "'].cells[9].innerText.toString()" })).Split('-')[0];
                    sub.teacher = t.Substring(0, t.Length - 1);//teacher's name
                    //room No
                    sub.room = text;
                    string type= await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[1].rows['" + row + "'].cells[3].innerText.toString()" });
                    switch (type)
                    {
                        case "Embedded Theory":
                            sub.type = "ETH";
                            break;
                        case "Theory Only":
                            sub.type = "TH";
                            break;
                        case "Lab Only":
                            sub.type = "LO";
                            break;
                        case "Embedded Lab":
                            sub.type = "ELA";
                            break;
                        case "Soft Skill":
                            sub.type = "SS";
                            break;
                    }
                    
                    //sub.myColor = new SolidColorBrush(myClass.avlblColrs[row - 1]);
                    myClass.attCol.Add(sub);
                }
            }
            getFromtable2();
            
        }
        private async void getFromtable2()
        {
            
            await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows[0].deleteCell(7)" });
            for (int row = 2; row <= 6; row++)
            {
                //traverse a new day and get the new day subjects
                
                ObservableCollection<Subject> today = new ObservableCollection<Subject>();
                string colstring = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows['" + row + "'].cells.length.toString()" });
                
                int col = Convert.ToInt32(colstring);
                int extraTime=0;
                for (int cell = 1; cell <= col-1; cell++)
                {
                    string cellColor = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows['" + row + "'].cells['" + cell + "'].bgColor" });
                    int  colspan = Convert.ToInt32(await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows['" + row + "'].cells['" + cell + "'].colSpan.toString()" }));
                    if (colspan > 1)
                    {
                        extraTime = colspan-1;
                    }

                    
                    //check if not a free slot
                    if (cellColor== "#ccff33")
                    {
                        //get the text
                        
                        string text = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows['" + row + "'].cells['" + cell + "'].innerText.toString()" });
                        try
                        {
                            Subject newSubject = new Subject();
                            string rawType = text.Split('-')[1];
                            string type = rawType.Substring(1, rawType.Length - 2);
                            newSubject.code = text.Substring(0, 7);
                            newSubject.type = type;
                            if (type == "ETH" || type == "SS" || type == "TH")
                                {
                                string time = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows[0].cells['" + (cell+extraTime) + "'].innerText.toString()" });
                                newSubject.time = time.Substring(0, 7) + " - " + time.Substring(13, 7);
                                }
                           else
                                {
                                    string sTime = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows[1].cells['" + (cell+extraTime) + "'].innerText.toString()" });
                                    string etime = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows[1].cells['" + (cell + 1+extraTime) + "'].innerText.toString()" });
                                    newSubject.time = sTime.Substring(0, 7) + " - " + etime.Substring(13, 7);
                                    cell++;
                                }
                            
                            today.Add(newSubject);
                            }
                        catch(Exception e)
                        {
                            MessageDialog m = new MessageDialog(e.ToString());
                            await m.ShowAsync();
                        }
                    }
                }

                try
                {
                    myClass.timeTable.Add(today);
                }
                catch(Exception e)
                {
                    await new MessageDialog(e.ToString()).ShowAsync();
                }

            }
            myClass.web.LoadCompleted -= Web_getSchedule;
            myClass.web.LoadCompleted += Web_getAttendance;
            myClass.web.Navigate(new Uri("https://academicscc.vit.ac.in/student/attn_report.asp?sem=FS"));
        }

        private async void getAttendance()
        {
            try
            {
                myClass.semStart = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows[2].cells[2].innerText" });
                myClass.cat1 = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows[2].cells[3].innerText" });
                myClass.cat2 = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows[3].cells[3].innerText" });
                myClass.fat = await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[2].rows[4].cells[3].innerText" });
                myClass.web.LoadCompleted -= Web_getAttendance;
                myClass.web.LoadCompleted += Web_getAttendanceInf;
                myClass.web.Navigate(new Uri("https://academicscc.vit.ac.in/student/attn_report.asp?sem=FS" + "&fmdt=" + myClass.semStart + "&todt=" + myClass.fat));
            }
            catch(Exception e)
            {
                await new MessageDialog(e.ToString()).ShowAsync();
            }
            
        }
        private async void Web_getAttendanceInf(object sender, NavigationEventArgs e)
        {
            try
            {
                for (int i = 1; i <= myClass.attCol.Count; i++)
                {
             
                    myClass.attCol[i-1].att = Convert.ToInt32(await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[4].rows[" + i.ToString() + "].cells[8].innerText" }));
                    myClass.attCol[i-1].ctd = Convert.ToInt32(await myClass.web.InvokeScriptAsync("eval", new string[] { "document.getElementsByTagName('table')[4].rows[" + i.ToString() + "].cells[7].innerText" }));
                }
                foreach (var i in myClass.timeTable)
                {
                    foreach (var j in i)
                    {
                        var sub = myClass.searchByCode(j.code, j.type);
                        try
                        {
                            j.title = sub.title;
                            j.room = sub.room;
                            j.teacher = sub.teacher;
                            j.att = sub.att;
                            j.ctd = sub.ctd;
                            j.attString = j.att.ToString() + "%";
                        }
                        catch
                        {
                            MessageDialog c = new MessageDialog(j.code);
                            await c.ShowAsync();
                        }
                    }
                }
                //await new MessageDialog(JsonConvert.SerializeObject(myClass.timeTable)).ShowAsync();
                myClass.sdtp();
                myClass.createNotification();
                Frame.Navigate(typeof(Att));
            }
            catch(Exception x)
            {
                await new MessageDialog(x.ToString()).ShowAsync();
            }
        }

        private async void cap()
        {
            string y = await myClass.web.InvokeScriptAsync("eval", new string[] { "var img= document.getElementById('imgCaptcha'); var canvas = document.createElement('canvas'); canvas.width = img.naturalWidth; canvas.height = img.naturalHeight; canvas.getContext('2d').drawImage(img, 0, 0); canvas.toDataURL('image/png').replace(/^data:image\\/(png|jpg);base64,/, '');" });
            var ims = new InMemoryRandomAccessStream();
            var bytes = Convert.FromBase64String(y);
            var dataWriter = new DataWriter(ims);
            dataWriter.WriteBytes(bytes);
            await dataWriter.StoreAsync();
            ims.Seek(0);
            var img = new BitmapImage();
            img.SetSource(ims);
            captcha.Source= img;
        }

        private void regBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (regBox.Text == "")
            {
                regBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
                regBox.Text = "Registration No.";
            }
        }

        private void regBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if(regBox.Text=="Registration No.")
            {
                regBox.Text = "";
                regBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            }
            
        }

        private void passBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (passBox.Password == ".......")
            {
                passBox.Password = "";
                passBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            }
        }

        private void passBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (passBox.Password == "")
            {
                passBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
                passBox.Password = ".......";
            }
        }

        private void captchaBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (captchaBox.Text == "Captcha")
            {
                captchaBox.Text = "";
                captchaBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Black);
            }
        }

        private void captchaBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (captchaBox.Text == "")
            {
                captchaBox.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
                captchaBox.Text = "Captcha";
            }
        }
    }
}
