/////////////////////////////////////////////////////////////////////
// Client.cs - Creates test req and Backend  for GUI               //
//                                                                 //
// Author: SHUBHAM JIWTODE                                         //
// Application: CSE681-Software Modeling and Analysis Demo         //
// Environment: C# console                                         //
// Source: Jim Fawcett                                             //
/////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * ===================
 * GUI specific operations
 * 
 * Interfaces
 * -----------
 * public void sendR2MP(string buildreq)// Sends build request to Mother process from ClientFileStrore  
 * public void KillRepo()// kills the repo on button event
 * public void KillMP()//Kills MOther process and child process on click
 * public void createProcess(int x)//creates mother and repo
 * public void makeRequest()// Builds the test request depending on selected files
 * public bool saveXml(string path)// Saves the created xml file to ClientFileStore
 * 
 * Required Files:
 * ---------------
 * Client.cs
 * program.cs
 * Repo.cs
 * MainWindows.xaml.cs
 * MPCommService.cs
 * IMPCommService.cs
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
using System.Xml.Linq;
using MessagePassingComm;
using System.Diagnostics;
using System.Threading;

namespace Client_namespace
{
    public class Client
    {
        
        
        private string path = "../../../ClientFileStore";
       
     
        private string author { get; set; } = "SHUBHAM JIWTODE";
        private string dateTime { get; set; } = "";

        public List<string> testedFiles { get; set; } = new List<string>();
        public List<string> testDriver { get; set; } = new List<string>();
        private XDocument req_doc;
        private static CommMessage rcMsg;
        private static Receiver rc;
        private static XDocument doc;
        private static List<string> tested_Files { get; set; } = new List<string>();
        private static List<string> test_Driver { get; set; } = new List<string>();

        public Client()
        {
           
        }

       




        ////////////////////////////////////////////////////////// Sends build request to Mother process from ClientFileStrore
        public void sendR2MP(string buildreq)
        {
            Sender send = new Sender("http://localhost", 8080);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "7070";
            sendMsg.to = "http://localhost:8080/IpluggableComm";
            sendMsg.command = "sendR2MP";
            List<string> ls = new List<string>();
            ls.Add(buildreq);
            sendMsg.arguments = ls;
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            Console.WriteLine("------------------------ sending {0} to Mother Process", buildreq);
            send.postMessage(sendMsg);

        }


        ////////////////////////////////////////////////////////// Sends build request files to Mother process from ClientFileStrore  

        public void sendR2R(string buildreq)
        {

            Sender send = new Sender("http://localhost", 6060);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "7070";////////////////////////////////////////
            sendMsg.to = "http://localhost:6060/IpluggableComm";
            sendMsg.command = "sendR2R";
            List<string> ls = new List<string>();
            ls.Add(buildreq);
            sendMsg.arguments = ls;
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            Console.WriteLine("------------------------ sending Build Request {0} to Repo",buildreq);
            send.postMessage(sendMsg);
        }
        
        /////////////////////////////////////////////////////////////// kills the repo on button event
       
        public void KillRepo() {
            Sender send = new Sender("http://localhost", 6060);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "6060";
            sendMsg.to = "http://localhost:6060/IpluggableComm";
            sendMsg.command = "KillRepo";
           
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            send.postMessage(sendMsg);
        }
        /////////////////////////////////////////////////////////////// kills the testharness on button event
        public void KillTH()
        {
            Sender send = new Sender("http://localhost", 9090);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "6060";
            sendMsg.to = "http://localhost:9090/IpluggableComm";
            sendMsg.command = "KillTH";

            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            send.postMessage(sendMsg);
        }
        /////////////////////////////////////////////////////////////// kills the Mother process on button event
        public void KillMP()
        {
            Sender send = new Sender("http://localhost", 8080);
            CommMessage sendMsg = new CommMessage(CommMessage.MessageType.request);
            sendMsg.from = "7070";
            sendMsg.to = "http://localhost:8080/IpluggableComm";
            sendMsg.command = "KillMP";
            sendMsg.author = "SHUBHAM RAMESH JIWTODE";
            send.postMessage(sendMsg);

        }
        /////////////////////////////////////////////////////////////// Creates mother Process and Repo
        public void createProcess(int x)
        {
            Process proc1 = new Process();
            string fileName = "..\\..\\..\\Builder\\bin\\debug\\Builder.exe";
            string absFileSpec = Path.GetFullPath(fileName);
            Console.Write("\n  attempting to start Mother build server", absFileSpec);
            try
            {
                Process.Start(fileName,x.ToString());
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);               
            }

            Process proc2 = new Process();
            string RepoName = "..\\..\\..\\Repo\\bin\\debug\\Repo.exe";
            string absFileSpec2 = Path.GetFullPath(RepoName);
            Console.Write("\n  attempting to start Repo", absFileSpec2);
            try
            {
                Process.Start(RepoName, x.ToString());
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
            }
            Process proc3 = new Process();
            string TestHarness = "..\\..\\..\\TestHarness\\bin\\debug\\TestHarness.exe";
            string absFileSpec3 = Path.GetFullPath(RepoName);
            Console.Write("\n  attempting to start Repo", absFileSpec3);
            try
            {
                Process.Start(TestHarness, x.ToString());
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
            }
            
        }

        ////////////////////////////////////////////////////////// Creates build request 

        public void makeRequest()
        {
            req_doc= new XDocument();
            XElement testRequestElem = new XElement("testRequest");
            req_doc.Add(testRequestElem);

            XElement authorElem = new XElement("author");
            authorElem.Add(author);
            testRequestElem.Add(authorElem);

            XElement dateTimeElem = new XElement("dateTime");
            dateTimeElem.Add(DateTime.Now.ToString());
            testRequestElem.Add(dateTimeElem);

            XElement testElem = new XElement("test");
            testRequestElem.Add(testElem);
            foreach (string file in testDriver)
            {
                XElement driverElem = new XElement("testDriver");
                driverElem.Add(file);
                testElem.Add(driverElem);
            }
            foreach (string file in testedFiles)
            {
                XElement testedElem = new XElement("tested");
                testedElem.Add(file);
                testElem.Add(testedElem);
            }
            
            saveXml(path);
        }

        /////////////////////////////////////////////////////////////// Saves the created xml file to ClientFileStore
        public bool saveXml(string path)
        {
            string testreqName = "TRQ_" + DateTime.Now.ToString("MMddHHmmss") + ".xml";
            try
            {
                req_doc.Save(System.IO.Path.Combine(path, testreqName));
                req_doc.Save(System.IO.Path.Combine("../../../RepoFileStore", testreqName));
                Console.WriteLine("---------------------------- saving {0} at {1}", testreqName, path);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write("\n--{0}--\n", ex.Message);
                return false;
            }

        }

#if(Test_Client)
        static void Main(string[] args)
        {
            Client client = new Client();
            int n = 3;//
            client.createProcess(n);
            client.makeRequest();
            string x = "test request name";//enter the request name (to be send to Mother Process)
            client.sendR2R(x);
            client.KillMP();
            client.KillRepo();
            
            
       
        }
#endif
    }
}
