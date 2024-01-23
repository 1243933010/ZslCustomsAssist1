using IBM.WMQ;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Server.Rest;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.MQ.MQQueueProxy
{
    public class IbmMQQueueProxy : AbstractLog
    {
        private int queueNum;
        private IbmMQQueueProxy.Func queueAction;
        private MQQueueManager mqQueueManager;
        private MQQueue mqQueue;
        private string mqQueueName = "";
        private int mqQueueOpts = 0;
        private Hashtable properties;
        private int sendOpts = 8208;
        private int revOpts = 8225;
        private MQPutMessageOptions putOptions;
        private MQGetMessageOptions getOptions;
        private Queue<MQQueue> freeMQQueuePool = new Queue<MQQueue>();
        private object poolAccessLock = new object();

        private IbmMQQueueProxy()
        {
        }

        public IbmMQQueueProxy(IbmMQQueueProxy.Func func) => this.queueAction = func;

        public void Initialize(TransportProcotol mqProp)
        {
            lock (this.poolAccessLock)
                this.freeMQQueuePool.Clear();
            switch (this.queueAction)
            {
                case IbmMQQueueProxy.Func.SEND:
                    this.queueNum = mqProp.mqSendQueueNum;
                    break;
                case IbmMQQueueProxy.Func.RECEIVE:
                    this.queueNum = mqProp.mqReceiveQueueNum;
                    break;
            }
            this.CreateMQQueues(mqProp, this.queueNum);
        }

        public void HandleMessege(MQMessage message)
        {
            MQQueue mqQueue1;
            lock (this.poolAccessLock)
            {
                try
                {
                    mqQueue1 = this.freeMQQueuePool.Dequeue();
                }
                catch
                {
                    AbstractLog.logger.Info((object)"当前无可用MQ发送连接，准备重新创建MQ发送连接.");
                    mqQueue1 = this.CreateMQQueue(ServerCore.downloadConfig.transportProcotols[0]);
                    AbstractLog.logger.Info((object)"MQ发送连接(无可用MQ发送连接)创建成功.");
                    Thread.Sleep(Thread.CurrentThread.ManagedThreadId * 10);
                }
            }
            try
            {
                if (this.putOptions == null)
                    this.putOptions = new MQPutMessageOptions();
                mqQueue1.Put(message, this.putOptions);
            }
            catch (MQException ex)
            {
                Thread.Sleep(Thread.CurrentThread.ManagedThreadId * 30);
                AbstractLog.logger.Info((object)"发送消息至MQ失败！准备重新创建MQ发送连接.");
                MQQueue mqQueue2 = this.CreateMQQueue(ServerCore.downloadConfig.transportProcotols[0]);
                AbstractLog.logger.Info((object)"MQ发送连接(发送消息至MQ失败)创建成功.");
                lock (this.poolAccessLock)
                    this.freeMQQueuePool.Enqueue(mqQueue2);
                throw ex;
            }
            lock (this.poolAccessLock)
                this.freeMQQueuePool.Enqueue(mqQueue1);
        }

        public void GetMesseges(List<MQMessage> messages, int num)
        {
            MQQueue mqQueue1;
            lock (this.poolAccessLock)
            {
                try
                {
                    mqQueue1 = this.freeMQQueuePool.Dequeue();
                }
                catch
                {
                    AbstractLog.logger.Info((object)"当前无可用MQ接收连接，准备重新创建MQ接收连接.");
                    mqQueue1 = this.CreateMQQueue(ServerCore.downloadConfig.transportProcotols[0]);
                    AbstractLog.logger.Info((object)"MQ接收连接(无可用MQ接收连接)创建成功.");
                    Thread.Sleep(Thread.CurrentThread.ManagedThreadId * 10);
                }
            }
            try
            {
                if (this.getOptions == null)
                    this.getOptions = new MQGetMessageOptions();
                if (mqQueue1.CurrentDepth > 0)
                {
                    if (mqQueue1.CurrentDepth >= num)
                    {
                        for (int index = 0; index < num; ++index)
                        {
                            MQMessage message = new MQMessage();
                            try
                            {
                                mqQueue1.Get(message, this.getOptions);
                                messages.Add(message);
                            }
                            catch (MQException ex)
                            {
                                if (ex.ReasonCode != 2033)
                                    throw ex;
                            }
                        }
                    }
                    else
                    {
                        for (int index = 0; index < mqQueue1.CurrentDepth; ++index)
                        {
                            MQMessage message = new MQMessage();
                            try
                            {
                                mqQueue1.Get(message, this.getOptions);
                                messages.Add(message);
                            }
                            catch (MQException ex)
                            {
                                if (ex.ReasonCode != 2033)
                                    throw ex;
                            }
                        }
                    }
                }
            }
            catch (MQException ex)
            {
                Thread.Sleep(Thread.CurrentThread.ManagedThreadId * 30);
                AbstractLog.logger.Info((object)"接收MQ消息失败！准备重新创建MQ接收连接.");
                MQQueue mqQueue2 = this.CreateMQQueue(ServerCore.downloadConfig.transportProcotols[0]);
                AbstractLog.logger.Info((object)"MQ接收连接(接收MQ消息失败)创建成功.");
                lock (this.poolAccessLock)
                    this.freeMQQueuePool.Enqueue(mqQueue2);
                throw ex;
            }
            lock (this.poolAccessLock)
                this.freeMQQueuePool.Enqueue(mqQueue1);
        }

        private void CreateMQQueues(TransportProcotol mqProp, int createQueueNum)
        {
            switch (this.queueAction)
            {
                case IbmMQQueueProxy.Func.SEND:
                    this.mqQueueName = mqProp.mqSendQueueName;
                    this.mqQueueOpts = this.sendOpts;
                    break;
                case IbmMQQueueProxy.Func.RECEIVE:
                    this.mqQueueName = mqProp.mqReceiveQueueName;
                    this.mqQueueOpts = this.revOpts;
                    break;
            }
            this.properties = new Hashtable();
            this.properties.Add((object)"hostname", (object)mqProp.mqHostName);
            this.properties.Add((object)"port", (object)mqProp.mqPort);
            this.properties.Add((object)"channel", (object)mqProp.mqChannel);
            this.properties.Add((object)"CCSID", (object)1381);
            for (int index = 0; index < createQueueNum; ++index)
            {
                this.mqQueueManager = new MQQueueManager(mqProp.mqQueueManagerName, this.properties);
                this.mqQueue = this.mqQueueManager.AccessQueue(this.mqQueueName, this.mqQueueOpts);
                lock (this.poolAccessLock)
                    this.freeMQQueuePool.Enqueue(this.mqQueue);
            }
        }

        private MQQueue CreateMQQueue(TransportProcotol mqProp)
        {
            switch (this.queueAction)
            {
                case IbmMQQueueProxy.Func.SEND:
                    this.mqQueueName = mqProp.mqSendQueueName;
                    this.mqQueueOpts = this.sendOpts;
                    break;
                case IbmMQQueueProxy.Func.RECEIVE:
                    this.mqQueueName = mqProp.mqReceiveQueueName;
                    this.mqQueueOpts = this.revOpts;
                    break;
            }
            this.mqQueueManager = new MQQueueManager(mqProp.mqQueueManagerName, this.properties);
            return this.mqQueueManager.AccessQueue(this.mqQueueName, this.mqQueueOpts);
        }

        public static void UpdateTransportProcotol(
          string oldTranVersion,
          params IbmMQQueueProxy[] proxies)
        {
            List<TransportProcotol> transportProcotols = ServerCore.downloadConfig.transportProcotols;
            string tranVersion = ServerCore.downloadConfig.tranVersion;
            if (transportProcotols != null && transportProcotols.Count > 0)
            {
                TransportProcotol mqProp = transportProcotols[0];
                if (!"MQ".Equals(mqProp.tpName))
                {
                    ServerCore.supportedTransportProtocol = "HTTPS";
                    AbstractLog.logger.Error((object)"传输协议不合符规范要求,已使用默认HTTPS传输方式!");
                }
                else if (string.IsNullOrWhiteSpace(oldTranVersion))
                {
                    foreach (IbmMQQueueProxy proxy in proxies)
                        proxy.Initialize(mqProp);
                    ServerCore.supportedTransportProtocol = mqProp.tpName;
                }
                else if (!tranVersion.Equals(oldTranVersion))
                {
                    foreach (IbmMQQueueProxy proxy in proxies)
                        proxy.Initialize(mqProp);
                    ServerCore.supportedTransportProtocol = mqProp.tpName;
                }
            }
            else
                ServerCore.supportedTransportProtocol = "HTTPS";
        }

        public enum Func
        {
            SEND,
            RECEIVE,
        }
    }
}
