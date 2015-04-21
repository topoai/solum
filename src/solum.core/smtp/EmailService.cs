using solum.extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.ComponentModel;
using System.Net;

namespace solum.core.smtp
{
    public class EmailService : Service
    {
        public EmailService(string name) : base(name)
        {

        }

        #region Configuration Properties
        [JsonProperty("smtp-host", Required=Required.Always)]
        public string SmtpHost { get; private set; }
        [JsonProperty("smtp-port", Required=Required.Always)]        
        public int SmtpPort { get; private set; }
        [JsonProperty("ssl-enabled", DefaultValueHandling=DefaultValueHandling.Populate)]
        [DefaultValue(false)]
        public bool SSLEnabled { get; private set; }
        [JsonProperty("user-name")]
        public string UserName { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
        [JsonProperty("default-from-address")]
        public string DefaultFromAddress { get; private set; }
        #endregion

        SmtpClient m_client;
        ActionBlock<MailMessage> m_message_queue;

        protected override void OnLoad()
        {
            // ** Validate Configuration Settings
            if (SSLEnabled)
            {
                // ** Require UserName/Password
                if (string.IsNullOrEmpty(UserName))
                    throw new ArgumentNullException("user-name is required when SSL is enabled.");
                if (string.IsNullOrEmpty(Password))
                    throw new ArgumentNullException("password is required when SSL is enabled.");
            }

            Log.Debug("Loading SMTP client on host={0} port={1}...".format(SmtpHost, SmtpPort));
            m_client = new SmtpClient(SmtpHost, SmtpPort);
            m_client.DeliveryMethod = SmtpDeliveryMethod.Network;

            if (SSLEnabled)
            {
                Log.Debug("Enabling SSL on SMTP client with username/password...");
                m_client.EnableSsl = SSLEnabled;
                m_client.Credentials = new NetworkCredential(UserName, Password);
            }

            if (string.IsNullOrEmpty(DefaultFromAddress))
                Log.Warn("No default from address specified... All messages must specify a From address to send a message.");
            else
                Log.Debug("Default From Address: {0}".format(DefaultFromAddress));

            base.OnLoad();
        }

        protected override void OnStart()
        {
            Log.Debug("Enabling mail message queue...");
            m_message_queue = new ActionBlock<MailMessage>(message => sendMessage(message));
            
            base.OnStart();
        }

        protected override void OnStop()
        {
            // ** Stop receiving messages            
            m_message_queue.Complete();

            // ** Complete sending all messages
            Log.Info("Waiting for {0} queued messages to send...".format(m_message_queue.InputCount));
            m_message_queue.Completion.Wait();

            // ** TODO: Save any pending/failed messages to disk


            base.OnStop();
        }

        /// <summary>
        /// Generate an email with the default sender (from)
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public void Email(string to, string subject, string body)
        {
            Email(DefaultFromAddress, to, subject, body);
        }

        /// <summary>
        /// Creates an email message from the parameters and queues it for delievery
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public void Email(string from, string to, string subject, string body)
        {
            if (Status != ServiceStatus.Started)
                throw new Exception("The email service is not started.  status={0}".format(Status));

            var message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;

            if (!m_message_queue.Post(message))
                throw new Exception("Mail queue full!");
        }        

        void sendMessage(MailMessage message)
        {
            try
            {
                m_client.Send(message);
            }
            catch (Exception ex)
            {
                Log.ErrorException("Error sending email message: {0}".format(message.ToJson(indent: true)), ex);

                // ** TODO: Save to failure queue or try message again...
                //    TODO: Add failure retry limit...
                // throw ex;
            }
        }
    }
}
