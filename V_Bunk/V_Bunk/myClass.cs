using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;


namespace V_Bunk
{
    public class myClass
    {
        public static WebView web = new WebView();
        public static ObservableCollection<Subject> attCol = new ObservableCollection<Subject>();
        public static ObservableCollection<ObservableCollection<Subject>> timeTable = new ObservableCollection<ObservableCollection<Subject>>();
        public static ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier();
        public static string semStart;
        public static string cat1;
        public static string cat2;
        public static string fat;
        public static Color[] avlblColrs= { Colors.Crimson,Colors.DarkCyan,Colors.DeepSkyBlue,Colors.Firebrick,Colors.Green,Colors.IndianRed,Colors.Yellow,Colors.Tomato,Colors.SaddleBrown,Colors.SlateBlue};
        public static ObservableCollection<todo> myTasks = new ObservableCollection<todo>();

        public static Subject searchByCode(string code, string type)
        {
            foreach (var i in attCol)
            {
                if (i.code == code && i.type == type)
                {
                    return i;
                }
            }
            return null;
        }
        //Save the data to phone............
        public static async void sdtp()
        {

            string timeTableString = JsonConvert.SerializeObject(timeTable) + " " + JsonConvert.SerializeObject(attCol);
            byte[] timeTableByte = Encoding.UTF8.GetBytes(timeTableString.ToCharArray());
            
            StorageFile subjectStorage = await ApplicationData.Current.LocalFolder.CreateFileAsync("subjectStorage", CreationCollisionOption.ReplaceExisting);
            using (var stream = await subjectStorage.OpenStreamForWriteAsync())
            {
                stream.Write(timeTableByte, 0, timeTableByte.Length);
            }
        }

        public async static void createNotification()
        {
            DateTime now = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day);
            try {
                for (int i = 0; i < 5; i++)
                {
                    switch (now.DayOfWeek.ToString())
                    {
                        case "Monday":
                            foreach (var subject in timeTable[0])
                            {
                                setToast(subject.title, subject.time, subject.room, time(subject.time.Substring(0, 7), now));
                            }
                            break;
                        case "Tuesday":
                            foreach (var subject in timeTable[1])
                            {
                                setToast(subject.title, subject.time, subject.room, time(subject.time.Substring(0, 7), now));
                            }
                            break;
                        case "Wednesday":
                            foreach (var subject in timeTable[2])
                            {
                                setToast(subject.title, subject.time, subject.room, time(subject.time.Substring(0, 7), now));
                            }
                            break;
                        case "Thursday":
                            foreach (var subject in timeTable[3])
                            {
                                setToast(subject.title, subject.time, subject.room, time(subject.time.Substring(0, 7), now));
                            }
                            break;
                        case "Friday":
                            foreach (var subject in timeTable[4])
                            {
                                setToast(subject.title, subject.time, subject.room, time(subject.time.Substring(0, 7), now));
                            }
                            break;
                    }
                    now = now.AddDays(1);
                }
            }
            catch (Exception e)
            {
               await new MessageDialog(e.ToString()).ShowAsync();
            }
        }

        //Toast designer
        private static void setToast(string Name, string time, string room, DateTime dueTime)
        {

            try
            {
                ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
                XmlNodeList textElements = toastXml.GetElementsByTagName("text");
                textElements[0].AppendChild(toastXml.CreateTextNode(Name));
                textElements[1].AppendChild(toastXml.CreateTextNode(time + " " + room));
                string t = dueTime.ToString();
                ScheduledToastNotification scheduledToast = new ScheduledToastNotification(toastXml, dueTime);
                toastNotifier.AddToSchedule(scheduledToast);

            }
            catch(Exception e)
            {
                //await new MessageDialog(Name+" "+dueTime.DayOfWeek.ToString()+"  "+e.ToString()).ShowAsync();
                //Skip the subject
            }
        }
        private static DateTime time(string hour, DateTime now)
        {
                int h = Convert.ToInt32(hour.Substring(0, 2));

                string meridian = hour.Substring(5, 2);
                if (meridian == "PM")
                {
                    h = (12 + h);
                }
                int m = Convert.ToInt32(hour.Substring(3, 2));
                m = m - 10;
                if (m < 0)
                {
                    m = 60 + m;
                    h = h - 1;
                }
                return new DateTime(now.Year, now.Month, now.Day, h, m, 0);
        }
        
    }
    public class Subject
    {
        public string code { get; set; }
        public string title { get; set; }
        public string teacher { get; set; }
        public int att { get; set; }
        public string attString { get; set; }
        public string room { get; set; }
        public int ctd { get; set; }
        public string time { get; set; }
        public string type { get; set; }
        public SolidColorBrush myColor { get; set; }
        //Constructor
        public Subject()
        {
            code = null;
            title = null;
            teacher = null;
            room = null;
            ctd = 0;
            att = 0;
        }
    }

    public class todo
    {
        public string title { get; set; }
        public DateTime deadline { get; set; }
        public DateTime nextPoke { get; set; }
        public string description { get; set; }
    }
}
    
