using OpenPop.Mime;
using OpenPop.Pop3;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SMTP_Client_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //SMTP Edge Connector Hostname
            string hostname = "smtptesting.datamotion.com";

            //SMTP Edge Connector Port
            int port = 25;

            //SMTP Edge Connector SSL
            bool ssl = true;

            //Enter your SMTP Edge Connector credentials
            string username = "<username>";
            string password = "<password>";

            //Create thread for polling messages
            Thread pollThread = StartTheThread(hostname, 995, ssl, username, password);
            
            //On testing SMTP Edge Connector system, ignore SSL error checks (you could also download and install the cert)
            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErros) => true);
            var client = new SmtpClient(hostname, port);
            client.UseDefaultCredentials = false;
            client.EnableSsl = ssl;
            
            //Set SMTP Client's credentials
            client.Credentials = new NetworkCredential(username, password);
            
            //Construct new MailMessage (from, to)
            MailMessage message = new MailMessage(username, "<target>");
            message.Headers.Add("Disposition-Notification-Options", "Change This To An Acceptable Option");
            message.Body = "TEST MESSAGE";
            message.Subject = "TEST FROM DM";
            client.Send(message);
            Console.ReadLine();
        }

        private static Thread StartTheThread(string hostname, int port, bool ssl, string username, string password)
        {
            //Create a new Thread for the FetchMessages function and start it
            var t = new Thread(() => FetchMessages(hostname, port, ssl, username, password));
            t.Start();
            return t;
        }

        private static void FetchMessages(string host, int port, bool ssl, string username, string password)
        {
            //3rd party Pop3 library
            using (Pop3Client client = new Pop3Client())
            {
                //Connect and authenticate to Pop3 specifying our own certificate validator function to bypass self signed certificate
                client.Connect(host, port, ssl, 5000, 5000, certificateValidator:certificateValidator);
                client.Authenticate(username, password);

                //Get number of messages in the inbox
                int messageCount = client.GetMessageCount();

                //Create List of type Message to hold the messages
                List<Message> allMessages = new List<Message>(messageCount);
                if (messageCount == 0)
                {
                    //If there are no messages
                    Console.WriteLine("No messages in inbox");
                }
                for(int i = messageCount; i > 0; i--)
                {
                    //For every message add it to our List
                    allMessages.Add(client.GetMessage(i));
                }

                foreach(Message message in allMessages)
                {
                    //For every message in our list convert it to a .NET MailMessage and print the Subject 
                    Console.WriteLine("Found Message:" + message.ToMailMessage().Subject);
                }
            }
        }

        //Certificate validator function to bypass self signed certificate
        private static bool certificateValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
