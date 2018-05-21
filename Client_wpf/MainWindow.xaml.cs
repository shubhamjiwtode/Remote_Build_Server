/////////////////////////////////////////////////////////////////////
// ChildProc - demonstrate creation of multiple .net processes     //
//                                                                 //
// Author: SHUBHAM JIWTODE                                         //
// Application: CSE681-Software Modeling and Analysis Demo         //
// Environment: C# console                                         //
// Source: Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * Handles the GUI
 * 
 * 
 * Interfaces:
 * ------------------
 *  private void CRQ_Click(object sender, RoutedEventArgs e) //Creates Build Request on click on Create_Request button
 *  private void START_CLICK(object sender, RoutedEventArgs e) //Creates Build Request on click on Create_Request button
 *  private void SRQ_CLICK(object sender, RoutedEventArgs e)//on click sends request to Mother process
 *  private void Test_files()//updates the Testfiles listbox
 *  private void Test_driver()//updates Test Driver files list box
 *  private void Test_request()//updates Test req list box
 *  private void TextBox_TextChanged(object sender, TextChangedEventArgs e)// Takes integer value from 1 to 9, which determines No of child to be Spwaned
 *  private void Quit_CLICK(object sender, RoutedEventArgs e)// On killProcess button click initiated Mother and repo kill
 *  void DataWindow_Closing(object sender, CancelEventArgs e)  // On Closing GUI window initiated Mother and repo kill  
 *  public static void gettestFiles()//gets test files list from repo
 *  public static void gettestDriver()//gets test driver list from repo
 *  public static void gettestlogs()//gets test logs list from repo
 *  public static void getBuildlogs()//gets buil logs list from repo
 *  void initializeMessageDispatcher()//updates the list box on gui from message from the repo
 * 
 * Required Files:
 * ---------------
 * Client.cs
 * 
 * Maintenance History:
 * --------------------
 ver 2.0      12/05/2017 
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client_namespace;
using System.Diagnostics;
using System.Threading;
using Clienthelp;
using System.Xml.Linq;
using MessagePassingComm;

namespace Client_wpf
{
    
    public partial class MainWindow : Window
    {
        Client client = new Client();
        private static XDocument doc = new XDocument();
        private static Receiver recv;
        Thread rcvThread = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        private static Sender sendRepo = new Sender("http://localhost", 6060);
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Project3";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
          
            Test_request();
            NofCP.IsEnabled = false;
            initializeMessageDispatcher();
            recv = new Receiver();
            recv.start("http://localhost", 7070);
            rcvThread = new Thread(rcvProc);
            rcvThread.Start();
            gettestFiles();
            gettestDriver();
            gettestLogs();
            getbuildLogs();
        }

        void rcvProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = recv.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution
                 
                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }



        void initializeMessageDispatcher()
        {
            // load test files list with files from folder
            messageDispatcher["updateTestDriverList"] = (CommMessage msg) =>
            {
                Testdriver.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    Testdriver.Items.Add(dir);
                }
            };
            // load test files list with files from folder
            messageDispatcher["updateTestFileList"] = (CommMessage msg) =>
            {
                Testfiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    Testfiles.Items.Add(file);
                }
            };
            // load test request listbox with files from folder
            // load test files list with files from folder
            messageDispatcher["updateBuildLogList"] = (CommMessage msg) =>
            {
                Build_Logs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    Build_Logs.Items.Add(dir);
                }
            };
            // load test files list with files from folder
            messageDispatcher["updateTestLogList"] = (CommMessage msg) =>
            {
                Test_Logs.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    Test_Logs.Items.Add(file);
                }
            };
        }




        /////////////////////////////////////////////////////////////// get test logs from repo
        public static void gettestLogs()
        {
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.command = "getfiles";
            sndMsg.author = "SHUBHAM RAMESH JIWTODE";
            sndMsg.from = "7070";
            sndMsg.to = "http://localhost:6060/IPluggableComm";
            List<string> ls = new List<string>();
            ls.Add("gettestlogs");
            sndMsg.arguments = ls;
            sendRepo.postMessage(sndMsg);

        }


        /////////////////////////////////////////////////////////////// get build logs from repo
        public static void getbuildLogs()
        {
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.command = "getfiles";
            sndMsg.author = "SHUBHAM RAMESH JIWTODE";
            sndMsg.from = "7070";
            sndMsg.to = "http://localhost:6060/IPluggableComm";
            List<string> ls = new List<string>();
            ls.Add("getbuildLogs");
            sndMsg.arguments = ls;
            sendRepo.postMessage(sndMsg);

        }





        /////////////////////////////////////////////////////////////// get test file info from repo
        public static void gettestFiles()
        {
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.command = "getfiles";
            sndMsg.author = "SHUBHAM RAMESH JIWTODE";
            sndMsg.from = "GUI";
            sndMsg.to = "http://localhost:6060/IPluggableComm";
            List<string> ls = new List<string>();
            ls.Add("gettestFiles");
            sndMsg.arguments=ls;
            sendRepo.postMessage(sndMsg);

        }


        /////////////////////////////////////////////////////////////// get test Driver info from repo
        public static void gettestDriver()
        {
            CommMessage sndMsg = new CommMessage(CommMessage.MessageType.request);
            sndMsg.command = "getfiles";
            sndMsg.author = "SHUBHAM RAMESH JIWTODE";
            sndMsg.from = "GUI";
            sndMsg.to = "http://localhost:6060/IPluggableComm";
            List<string> ls = new List<string>();
            ls.Add("gettestDriver");
            sndMsg.arguments = ls;
            sendRepo.postMessage(sndMsg);

        }
        





        /////////////////////////////////////////////////////////////// Creates Build Request on click on Create_Request button
        //also updates the test request list box
        private void CRQ_Click(object sender, RoutedEventArgs e)
        {
            
            foreach (var file in Testfiles.SelectedItems) {
                client.testedFiles.Add(file.ToString());
            }
            foreach (var file in Testdriver.SelectedItems)
            {
                client.testDriver.Add(file.ToString());
            }
            Testfiles.SelectedIndex = -1;
            Testdriver.SelectedIndex = -1;
            client.makeRequest();
            Testfiles.SelectedItems.Clear();
            Testdriver.SelectedItems.Clear();

            client.testedFiles.Clear();
            client.testDriver.Clear();
            TestRQ.Items.Clear();
            Test_request();//update test request list box


        }

        ///////////////////////////////////////////////////////////////upadates logs on button click
        private void GetLOGS_click(object sender, RoutedEventArgs e) {
            getbuildLogs();
            gettestLogs();
        }

        public void automation(int x) {
            client.createProcess(x);
            client.sendR2MP("TRQ_1205035546.xml");
            client.sendR2MP("TRQ_1201135519.xml");
        }

        ///////////////////////////////////////////////////////////////spawn child process depending on the number on child process request. 
        public void START_CLICK(object sender, RoutedEventArgs e)
        {
            
            string i = NofCP.Text;
           
                client.createProcess(Int32.Parse(i));
            Thread.Sleep(500);
            Start_Process.IsEnabled = false;
            NofCP.IsEnabled = false;
        }


        /////////////////////////////////////////////////////////////// on click sends request to Mother process.
        private void SRQ_CLICK(object sender, RoutedEventArgs e)
        {
            if (TestRQ.SelectedItem != null)
            {
                var file = TestRQ.SelectedItem;
                client.sendR2MP(file.ToString());
            }
        }



        

        /////////////////////////////////////////////////////////////// updates the Testfiles listbox
        private void Test_files()
        {
                        String testFilespath = "..\\..\\..\\ClientFileStore\\TestFiles";
                        DirectoryInfo dinfo = new DirectoryInfo(testFilespath);
                        FileInfo[] files = dinfo.GetFiles("*.*");
                        foreach (FileInfo file in files)
                        {

                            Testfiles.Items.Add(file.Name);

                        }   
        }


        
        ///////////////////////////////////////////////////////////////updates TestDriver list box
        private void Test_driver()
        {

            String testDriverspath = "..\\..\\..\\ClientFileStore\\TestDrivers";
            DirectoryInfo dinfo1 = new DirectoryInfo(testDriverspath);
            FileInfo[] fileDrivers = dinfo1.GetFiles("*.*");
            foreach (FileInfo file in fileDrivers)
            {

                Testdriver.Items.Add(file.Name);
               
            }

        }

        ///////////////////////////////////////////////////////////////updates Test req list box
        private void Test_request()
        {
            
                        String testreq = "..\\..\\..\\ClientFileStore";
                        DirectoryInfo dinfo1 = new DirectoryInfo(testreq);
                        FileInfo[] testfiles = dinfo1.GetFiles("*.xml");
                        foreach (FileInfo file in testfiles)
                        {

                            TestRQ.Items.Add(file.Name);

                        }
        }

        /////////////////////////////////////////////////////////////// Takes integer value from 1 to 9, which determines No of child to be Spwaned
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var h = NofCP.Text;
            try
            {
                if (Int32.Parse(h) < 10 && Int32.Parse(h)!=0)
                {
                    Start_Process.IsEnabled = true;
                }
            }
            catch (Exception ex) {
                NofCP.Clear() ;
                string x = "enter int value";
                x = ex.Message;
            }
            
        }

        
        private void Buildlogs_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void Testlogs_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void Testfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void Testdriver_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

           
        }

        private void TestRQ_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          
        }
        /////////////////////////////////////////////////////////////// On killProcess button click initiated Mother and repo kill
        private void Quit_CLICK(object sender, RoutedEventArgs e)
        {
            client.KillMP();
            client.KillRepo();
            client.KillTH();
            Start_Process.IsEnabled = true;
            NofCP.IsEnabled = true;
        }
        /////////////////////////////////////////////////////////////// On Closing GUI window initiated Mother and repo kill
        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            client.KillMP();
            Thread.Sleep(500);
            client.KillRepo();
            Thread.Sleep(500);
            client.KillTH();
            Thread.Sleep(500);
            Process.GetCurrentProcess().Kill();
        }
        ///////////////////////////////////////////////////////////////function to add multiple test request
        private void ADD_click(object sender, RoutedEventArgs e)
        {
            List<string> tstfiles = new List<string>();
            string testDriver = null;
            string testRequest = "";

            if (Testfiles.SelectedItems.Count < 1 || Testdriver.SelectedItem == null || TestRQ == null) {
                MessageBox.Show("/////////////////");
            }
            foreach (var item in Testfiles.SelectedItems)
            {
                if (item != null)
                {
                    tstfiles.Add(item.ToString());
                }
            }
            if (Testdriver.SelectedItem != null)
            {
                testDriver = Testdriver.SelectedItem.ToString();
            }
            if (TestRQ.SelectedItem != null)
            {
                testRequest = TestRQ.SelectedItem.ToString();
            }
            addTestHandler(testDriver, tstfiles, testRequest);
        }


        public static void addTestHandler(String testDriver, List<String> testedFiles, String testRequestName)
        {
            try
            {
                Class1 tr = new Class1();

                string testRequestPath = "../../../RepoFileStore";
                loadXml(System.IO.Path.Combine(testRequestPath, testRequestName));

                XElement root = doc.Element("testRequest");
                IEnumerable<XElement> rows = root.Descendants("test");
                XElement firstRow = rows.First();
                firstRow.AddBeforeSelf(
                   new XElement("test",
                   new XElement("testDriver", testDriver),
                   testedFiles.Select(i => new XElement("tested", i))
                   ));
                
                saveXml(testRequestPath, testRequestName);
                MessageBox.Show("Added test to " + testRequestName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public static bool loadXml(string path)
        {
            try
            {
                doc = XDocument.Load(path);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }
        }

        /*--<functionality to save the xml document when adding new test to build req-->*/
        public static bool saveXml(string path, string fileName)
        {

            string savePath = System.IO.Path.Combine(path, fileName);
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                doc.Save(System.IO.Path.Combine(path, fileName));
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }

        }

       
    }
}
