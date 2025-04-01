using Common.Util.Helpers;
using JidamVision.Setting;
using JidamVision.Util;
using MessagingLibrary;
using MessagingLibrary.MessageInterface;
using MessagingLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace JidamVision.Sequence
{
    public enum CommunicatorType
    {
        None,
        WCF,
    }

    public delegate bool CallBack(object sender, IMessageDuplexCallback channel,
        MmiMessageInfo message, ClientInfo clinet);
    public class Communicator
    {
        public event EventHandler Closed = delegate { };
        public event EventHandler Opened = delegate { };
        public event EventHandler<Message> ReceiveMessage = delegate { };

        private Message _currentMessage;
        private IMessageDuplexClient _clinet;

        public CommunicationState State
        {
            get
            {
                if (_clinet == null)
                    return CommunicationState.Faulted;
                else
                    return _clinet.State;
            }
        }

        public void Create(CommunicatorType type, params object[] parameters)
        {
            switch (type)
            {
                case CommunicatorType.None:
                    _clinet = new MessageDuplexEmptyClient();
                    break;
                case CommunicatorType.WCF:
                    string ip = parameters[0] as string;
                    _clinet = new MessageDuplexClient(BindingType.Tcp, ip);
                    //if (NetHelper.Ping(ip))
                    //{
                    //    _clinet = new MessageDuplexClient(BindingType.Tcp, ip);
                    //}
                    //else
                    //{
                    //    return;
                    //}
                    break;
            }

            _clinet.WcfReceivedMessage += Client_WcfReceivedMessage;
            _clinet.Closed += Client_Closed;
            _clinet.Opened += Client_Opened;
            _clinet.Connect();
        }

        public void Close()
        {
            if (_clinet != null)
            {
                _clinet.Close();
            }
        }

        public bool Connect()
        {
            if (_clinet is null)
                return false;

            return _clinet.Connect();
        }

        public bool SendMessage(MmiMessageInfo message)
        {
            if (_clinet == null)
                return false;
            var result = _clinet.SendMessage(new MmiMessageInfo() { Message = message.ToXmlContent() });
            if (result != CommunicationErrorType.None)
            {
                SLogger.Write($"SendMessage FAiled : {result}", SLogger.LogType.Error);
            }

            return true;
        }

        public void SendMachineInfo()
        {
            //#WCF_FSM#1 서버와 통신을 하기 위해서는, 약속된 클라이언트 여야함.
            //현재 정의된 클라이언트는 아래 중에서 하나를 선택하여 MachineName으로 설정해야함
            //VISION01
            //VISION02
            //VISION03
            //환경설정에서 SettingXml.Inst.MachineName에 값을 설정하도록 할것
            Message content = new Message
            {
                Command = Message.MessageCommand.HandShake,
                MachineName = SettingXml.Inst.MachineName
            };

            MmiMessageInfo messageInfo = new MmiMessageInfo
            {
                Message = content.ToXmlContent()
            };

            SendMessage(messageInfo);
        }

        private void Client_Opened(object sender, EventArgs e)
        {
            Opened(this, e);
        }

        private void Client_Closed(object sender, EventArgs e)
        {
            Closed(this, e);

            if (_clinet != null)
            {
                _clinet.WcfReceivedMessage -= Client_WcfReceivedMessage;
                _clinet.Closed -= Client_Closed;
                _clinet.Opened -= Client_Opened;
                _clinet = null;
            }
        }

        private void Client_WcfReceivedMessage(object sender, IMessageDuplexCallback channel,
            MmiMessageInfo message, ClientInfo clinet)
        {
            _currentMessage = XmlHelper.XmlDeserialize<Message>(message.Message);
            SLogger.Write($"--Receive Message--\nTime : {_currentMessage.Time}\nCommand : {_currentMessage.Command}", SLogger.LogType.Info);
            switch (_currentMessage.Command)
            {
                case Message.MessageCommand.Loaded:

                    break;
            }

            ReceiveMessage(this, _currentMessage);
        }
    }
}

