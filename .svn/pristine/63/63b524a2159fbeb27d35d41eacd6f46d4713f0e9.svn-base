using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZslCustomsAssist.Server.Rest;
using ZslCustomsAssist.Runtime;
using ZslCustomsAssist.Utils;
using ZslCustomsAssist.Utils.Log;

namespace ZslCustomsAssist.Runtime.Config
{
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class DownloadConfig : AbstractLog, ICloneable
    {
        public string entId { get; set; }

        public string dxpId { get; set; }

        public string entName { get; set; }

        public string entCode { get; set; }

        public string httpapiKey { get; set; }

        public string rsaPrivate { get; set; }

        public string ediCode { get; set; }

        public string receiveThread { get; set; }

        public string receivePeriod { get; set; }

        public string sendThread { get; set; }

        public string sendPeriod { get; set; }

        public string sendNumber { get; set; }

        public List<string> receiptMessageTypes { get; set; } = new List<string>();

        public List<ReceiveMessageType> receiveMessageTypes { get; set; }

        public List<DataResourceType> dataResourceTypes { get; set; }

        public string writingReceiveThread { get; set; }

        public string writingReceivePeriod { get; set; }

        public List<XmlSignType> xmlSignTypes { get; set; }

        public string xmlSignVersion { get; set; }

        public List<TransportProcotol> transportProcotols { get; set; }

        public string tranVersion { get; set; }

        public int GetReceiveThread()
        {
            try
            {
                int num = DateHelper.GetUInt(int.Parse(receiveThread));
                int minSendThreads = ServerCore.MinSendThreads;
                return num < minSendThreads ? minSendThreads : num;
            }
            catch (Exception ex)
            {
                receiveThread = "10";
                return 10;
            }
        }

        public int GetReceivePeriod()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(receivePeriod));
            }
            catch (Exception ex)
            {
                receivePeriod = "200";
                return 200;
            }
        }

        public int GetSendThread()
        {
            try
            {
                int num = DateHelper.GetUInt(int.Parse(sendThread));
                int minReceiveThreads = ServerCore.MinReceiveThreads;
                return num < minReceiveThreads ? minReceiveThreads : num;
            }
            catch (Exception ex)
            {
                sendThread = "1";
                return 1;
            }
        }

        public int GetSendPeriod()
        {
            try
            {
                return int.Parse(sendPeriod);
            }
            catch (Exception ex)
            {
                sendPeriod = "3000";
                return 3000;
            }
        }

        public int GetSendNumber()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(sendNumber));
            }
            catch (Exception ex)
            {
                sendNumber = "10";
                return 10;
            }
        }

        public int GetWritingReceiveThread()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(writingReceiveThread));
            }
            catch (Exception ex)
            {
                writingReceiveThread = "10";
                return 10;
            }
        }

        public int GetWritingReceivePeriod()
        {
            try
            {
                return DateHelper.GetUInt(int.Parse(writingReceivePeriod));
            }
            catch (Exception ex)
            {
                writingReceivePeriod = "1000";
                return 1000;
            }
        }

        public object Clone() => MemberwiseClone();
    }
}
