using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.MQ
{
    public class MyActiveMq
    {
        public IMessageConsumer consumer;
        private ITextMessage msg;
        private bool isTopic = false;
        private bool hasSelector = false;
        private const string ClientID = "clientid";
        private const string SelectorValue = "demo";
        private const string Selector = "filter='demo'";

        private IConnectionFactory Factory { get; set; }

        public IConnection Connection { get; set; }

        private ISession Session { get; set; }

        private IMessageProducer Producer { get; set; }

        public bool ConnectSuccess { get; set; }

        public MyActiveMq(string mqUrl)
        {
            this.ConnectSuccess = false;
            this.InitMqConnection(mqUrl);
        }

        public void InitMqConnection(string mqUrl)
        {
            try
            {
                this.Factory = (IConnectionFactory)new ConnectionFactory(mqUrl);
                this.Connection = this.Factory.CreateConnection();
                this.Connection.Start();
                this.Session = this.Connection.CreateSession(AcknowledgementMode.ClientAcknowledge);
                this.ConnectSuccess = true;
            }
            catch (Exception ex)
            {
                this.ConnectSuccess = false;
                AbstractLog.logger.Info((object)("连接失败：" + ex.StackTrace));
            }
        }

        private void Connection_ConnectionResumedListener() => throw new NotImplementedException();

        private void Connection_ExceptionListener(Exception exception) => throw new NotImplementedException();

        ~MyActiveMq()
        {
        }

        public void InitProducer(bool topic, string name, bool selector = false)
        {
            this.isTopic = topic;
            this.hasSelector = selector;
            try
            {
                this.Producer = !topic ? this.Session.CreateProducer((IDestination)new ActiveMQQueue(name)) : this.Session.CreateProducer((IDestination)new ActiveMQTopic(name));
                this.msg = this.Producer.CreateTextMessage();
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Info((object)("InitProducer：" + ex.StackTrace));
            }
        }

        public void InitConsumer(bool topic, string name, bool selector = false)
        {
            this.isTopic = topic;
            this.hasSelector = selector;
            try
            {
                this.consumer = !topic ? (!selector ? this.Session.CreateConsumer((IDestination)new ActiveMQQueue(name)) : this.Session.CreateConsumer((IDestination)new ActiveMQQueue(name), "filter='demo'")) : (!selector ? this.Session.CreateDurableConsumer((ITopic)new ActiveMQTopic(name), "clientid", (string)null, false) : this.Session.CreateDurableConsumer((ITopic)new ActiveMQTopic(name), "clientid", "filter='demo'", false));
                AbstractLog.logger.Info((object)"消费者初始化成功");
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Error((object)ex);
                AbstractLog.logger.Info((object)("InitConsumer：" + ex.StackTrace));
                throw ex;
            }
        }
        public bool SendMessage(string message, string msgId = "defult", MsgPriority priority = MsgPriority.Normal)
        {
            if (this.Producer == null)
                return false;
            this.msg.Text = message;
            return this.isTopic ? this.ProducerSubcriber(message, priority) : this.P2P(message, priority);
        }
        public ITextMessage GetMessage()
        {
            ITextMessage textMessage = (ITextMessage)null;
            try
            {
                textMessage = this.consumer.Receive(new TimeSpan(200000L)) as ITextMessage;
            }
            catch (Exception ex)
            {
                this.ConnectSuccess = false;
                AbstractLog.logger.Info((object)("GetMessage:\n" + ex.Message));
                AbstractLog.logger.Error((object)ex);
            }
            return textMessage ?? (ITextMessage)null;
        }

        private bool P2P(string message, MsgPriority priority)
        {
            try
            {
                if (this.hasSelector)
                    this.msg.Properties.SetString("filter", "demo");
                this.Producer.Send((IMessage)this.msg, MsgDeliveryMode.Persistent, priority, TimeSpan.MinValue);
                return true;
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Info((object)("P2P:\n" + ex.Message));
                return false;
            }
        }

        private bool ProducerSubcriber(string message, MsgPriority priority)
        {
            try
            {
                this.Producer.Priority = priority;
                this.Producer.DeliveryMode = MsgDeliveryMode.Persistent;
                this.Producer.Send((IMessage)this.msg, MsgDeliveryMode.Persistent, priority, TimeSpan.MinValue);
                return true;
            }
            catch (Exception ex)
            {
                AbstractLog.logger.Info((object)("ProducerSubcriber:\n" + ex.Message));
                return false;
            }
        }

        public void ShutDown()
        {
            Console.WriteLine("Close Connection and Session...");
            this.Session.Close();
            this.Connection.Close();
        }
    }
}
