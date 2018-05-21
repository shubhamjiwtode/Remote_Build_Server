/*Executive.cs-----Starts repo,builder,test harness,client
 * Author: ------ SHUBHAM RAMESH JIWTODE
 * 
  Source: Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017 
  Course:CSE 681 Software Modelling and Analysis
  Application: CSE 681 Project 4- TestHarness.cs
  Environment: C# Console
  *--------------------------------------------------------------------------------------------------------*/
/*
 * Added references to:
 * - MessagePassingComm
 *
 * This package :
 * ----------------------
 *  Invokes different processes(child, mother builder and test )
 *  
 * Interfaces
 * ---------------------
 * 1.public static void startUpSetup()                          ->start Up Setup
 * 2.public static bool startUI()                               ->function to start UI 
        
   
 * Required Files:
 ** ---------------
 * IMPCommService.cs         : Service interface and Message definition
 * MPCommService.cs 
 * Maintenance History:
 * --------------------
  *  * ver 1.0 : 27 October 2017
 * - first release
 * 
 *  ver 2.0 5th December 2017
 *      -second release
 */





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Client_namespace;
using System.Threading;

namespace Executive
{
    class Executive
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = "Executive";
            startUpSetup();

        }
        
        public static void startUpSetup()
        {
            startUI();


            Client client = new Client();
            client.createProcess(2);
            client.sendR2MP("TRQ_1207143107.xml");
           
            client.sendR2MP("TRQ_1201135519.xml");
            
             
            client.sendR2MP("TRQ_1207143334.xml");
        }

        //function to start UI 
        public static bool startUI()
        {

            Process proc = new Process();
            string fileName = "..\\..\\..\\Client_wpf\\bin\\Debug\\Client_wpf.exe";
            string absFileSpec = Path.GetFullPath(fileName);
            Console.Write("\n  attempting to start {0}", absFileSpec);
            try
            {
                Process.Start(fileName);
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;

        }
    }
}