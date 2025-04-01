using JidamVision.Setting;
using JidamVision.Util;
using MessagingLibrary;
using MessagingLibrary.MessageInterface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using static MessagingLibrary.Message;

namespace JidamVision.Sequence
{
    /*
    #WCF_FSM# - <<<비젼 - 제어 통신을 이용한 검사 시퀸스 개발>>> 
    제어(서버) - 비젼(클라이언트) 간의 통신
    비젼에서 "StartAutoRun"함수를 이용해 전체 사이클을 반복하도록 명련
    정지할때는 "StopAutoRun"함수를 이용해 제어에 정지 요청
    */

    //시퀀스에서 비젼 내부에, 특정 기능을 요청
    public enum SeqCmd
    {
        None = 0,
        //OpenRecipe,         //제어 -> 비젼으로 모델 열기 명령
        //InspReady,  
        InspStart,
        InspEnd,
    }

    public enum Vision2Mmi
    {
        None = 0,
        InspDone,               //비젼 -> 제어 : 검사 완료 및 결과 반환
        InspEnd,                
        Error
    }

    public enum VisionSeq
    {
        None = 0,
        OpenRecipe,             //비젼 -> 제어 : 모델 열기 요청
        MmiStart,                  //비젼 -> 제어 : 전체 사이클 검사 요청
        MmiStop,                  //비젼 -> 제어 : 전체 사이클 검사 정지 요청
        Error

    }

    public class VisionSequence : IDisposable
    {
        private static VisionSequence _sequence = null;

        public static VisionSequence Inst
        {
            get
            {
                if (_sequence == null)
                {
                    _sequence = new VisionSequence();
                }
                return _sequence;
            }
        }

        #region Event
        public delegate void XEventHandler<T1, T2>(object sender, T1 e1, T2 e2);
        public event XEventHandler<SeqCmd, object> SeqCommand = delegate { };
        #endregion

        private Message _message = new Message();
        private Communicator _communicator = null;

        private Thread _sequenceThread = null;
        private bool _isRun = true;
        private VisionSeq _visionState = VisionSeq.None;
        private string _modelName = "";

        private string _lastErrMsg;

        private bool _mmiOpenRecipe = false;

        public bool IsMmiConnected { get; set; } = false;

        public VisionSequence()
        {

        }

        private bool InitCommunicator()
        {
            if (SettingXml.Inst.CommType == CommunicatorType.WCF)
            {
                SLogger.Write("WCF 통신 초기화!");

                string ipAddr = SettingXml.Inst.CommIP;

                #region WCF

                _communicator = new Communicator();
                _communicator.ReceiveMessage += Communicator_ReceiveMessage;
                _communicator.Closed += Communicator_Closed;
                _communicator.Opened += Communicator_Opened;

                _communicator.Create(CommunicatorType.WCF, ipAddr);

                if (_communicator.State == System.ServiceModel.CommunicationState.Opened)
                    _communicator.SendMachineInfo();

                if (_communicator.State != System.ServiceModel.CommunicationState.Opened)
                {
                    SLogger.Write("MMI 연결 실패!", SLogger.LogType.Error);
                    return false;
                }

                #endregion WCF
            }
            else
            {
                return false;
            }

            return true;
        }

        public void InitSequence()
        {
            if (!InitCommunicator())
                return;

            //통신 초기화
            //통신 이벤트 등록
            _message.MachineName = SettingXml.Inst.MachineName;

            _sequenceThread = new Thread(SequenceThread);
            _sequenceThread.IsBackground = true;
            _sequenceThread.Start();
        }

        public void ResetCommunicator(Communicator communicator)
        {
            if (_communicator is null)
                return;

            _communicator.ReceiveMessage -= Communicator_ReceiveMessage;
            _communicator = communicator;
            _communicator.ReceiveMessage += Communicator_ReceiveMessage;
        }

        private bool SendMessage(MmiMessageInfo message)
        {
            if (_communicator is null)
                return false;

            _message.Time = string.Format($"{DateTime.Now:HH:mm:ss:fff}");
            return _communicator.SendMessage(message);
        }

        private void SequenceThread()
        {
            while (_isRun)
            {
                UpdateSeqState();
                Thread.Sleep(1);
            }
        }

        //#WCF_FSM#2 비젼 -> 제어에 자동 검사 요청
        public void StartAutoRun(string modelName)
        {
            _visionState = VisionSeq.OpenRecipe;
            _modelName = modelName;
        }

        //#WCF_FSM#8 비젼 -> 제어에 자동 검사 정지 요청
        public void StopAutoRun()
        {
            _visionState = VisionSeq.MmiStop;
        }

        public void SetVisionSeq(VisionSeq visionSeq, object param)
        {
            _visionState = visionSeq;
        }

        private void UpdateSeqState()
        {
            switch (_visionState)
            {
                case VisionSeq.None:
                    {
                    }
                    break;
                case VisionSeq.OpenRecipe:
                    {
                        //#WCF_FSM#3 비젼 -> 제어에 모델 열기 요청

                        if (_modelName == "")
                        {
                            _visionState = VisionSeq.None;
                            break;
                        }

                        SLogger.Write("Vision Seq : " + _visionState.ToString());
                        _mmiOpenRecipe = false;
                        //OpenRecipe 명령 전달
                        _message.Command = Message.MessageCommand.OpenRecipe;
                        //_modelName 모델명 전달
                        _message.Tool = _modelName;
                        _message.Status = CommandStatus.None;
                        _message.ErrorMessage = "";
                        SendMessage(_message);

                        _visionState = VisionSeq.MmiStart;
                    }
                    break;
                case VisionSeq.MmiStart:
                    {
                        if (!_mmiOpenRecipe)
                            break;

                        SLogger.Write("Vision Seq : " + _visionState.ToString());

                        //#WCF_FSM#3 비젼 -> 제어에 전체 검사 요청
                        _message.Command = Message.MessageCommand.MmiStart;
                        _message.Status = CommandStatus.None;
                        _message.ErrorMessage = "";
                        SendMessage(_message);

                        _visionState = VisionSeq.None;
                    }
                    break;
                case VisionSeq.MmiStop:
                    {
                        //#WCF_FSM#9 제어에 자동 검사 정지 요청
                        SLogger.Write("Vision Seq : " + _visionState.ToString());

                        _message.Command = Message.MessageCommand.MmiStop;
                        _message.Status = CommandStatus.None;
                        _message.ErrorMessage = "";
                        SendMessage(_message);

                        _visionState = VisionSeq.None;
                    }
                    break;
                case VisionSeq.Error:
                    {
                        _message.Command = Message.MessageCommand.Error;
                        _message.Status = CommandStatus.Fail;
                        _message.ErrorMessage = _lastErrMsg;
                        SendMessage(_message);

                        _visionState = VisionSeq.None;
                    }
                    break;
            }
        }

        private void Communicator_ReceiveMessage(object sender, Message e)
        {
            switch (e.Command)
            {
                case Message.MessageCommand.Reset:
                    {
                        ResetSequence();
                    }
                    break;
                case Message.MessageCommand.OpenRecipe:
                    {
                        if(e.Status == Message.CommandStatus.Success)
                        {
                            //비젼의 요청에 의해, OpenRecipe가 성공한 경우
                            _mmiOpenRecipe = true;
                            break;
                        }
                        else
                        {
                            //Mmi에서 비젼에, OpenRecipe를 요청한 경우
                            //SeqCommand(this, SeqCmd.OpenRecipe, (object)e.Tool);
                        }
                    }
                    break;
                case Message.MessageCommand.MmiStart:
                    {
                        if (e.Status == Message.CommandStatus.Success)
                        {
                            //비젼의 요청에 의해, MmiStart가 성공한 경우
                            break;
                        }
                    }
                    break;
                case Message.MessageCommand.MmiStop:
                    {
                        if (e.Status == Message.CommandStatus.Success)
                        {
                            //비젼의 요청에 의해, MmiStop가 성공한 경우
                            break;
                        }
                    }
                    break;
                case Message.MessageCommand.InspStart:
                    {
                        //#WCF_FSM#4 제어 -> 비젼으로 검사 시작 요청
                        SeqCommand(this, SeqCmd.InspStart, e);
                    }
                    break;
                case Message.MessageCommand.InspEnd:
                    {
                        SeqCommand(this, SeqCmd.InspEnd, e);
                    }
                    break;
            }
        }

        //비젼 -> MMI에게 Command 전송
        public void VisionCommand(Vision2Mmi visionCmd, Object e)
        {
            switch (visionCmd)
            {
                // case Vision2Mmi.ModeLoaded:
                //    {
                //        string errMsg = (string)e;
                //        if (errMsg != "")
                //        {
                //            _lastErrMsg = errMsg;
                //            SendError();
                //            break;
                //        }

                //        _message.Command = Message.MessageCommand.OpenRecipe;
                //        _message.Status = CommandStatus.Success;
                //        SendMessage(_message);
                //    }
                //    break;
                //case Vision2Mmi.InspReady:
                //    {
                //        string errMsg = (string)e;
                //        if (errMsg != "")
                //        {
                //            _lastErrMsg = errMsg; 
                //            SendError();
                //            break;
                //        }

                //        _message.Command = Message.MessageCommand.InspReady;
                //        _message.Status = CommandStatus.Success;
                //        SendMessage(_message);
                //    }
                //    break;
                case Vision2Mmi.InspDone:
                    {
                        //#WCF_FSM#7 제어에 Ng/Good 결과를 담아 검사 완료 명령 전송
                        bool isDefect = (bool)e;

                        _message.Command = Message.MessageCommand.InspDone;
                        _message.Status = isDefect ? CommandStatus.Ng :CommandStatus.Good;
                        SendMessage(_message);
                    }
                    break;
            }
        }

        private void SendError()
        {
            _message.Command = Message.MessageCommand.Error;
            _message.Status = CommandStatus.Fail;
            _message.ErrorMessage = _lastErrMsg;
            SendMessage(_message);
        }

        private void ResetSequence()
        {
            _visionState = VisionSeq.None;
        }


        private void Communicator_Opened(object sender, EventArgs e)
        {
            SLogger.Write($"MMI에 연결되었습니다.");
            IsMmiConnected = true;
        }

        private void Communicator_Closed(object sender, EventArgs e)
        {
            SLogger.Write("서버와의 연결이 끊어졌습니다.", SLogger.LogType.Error);
            IsMmiConnected = false;
        }

        #region Disposable
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _isRun = false;
                    if (_communicator != null)
                    {
                        _communicator.ReceiveMessage -= Communicator_ReceiveMessage;
                        _communicator.Opened -= Communicator_Opened;
                        _communicator.Closed -= Communicator_Closed;
                        _communicator = null;
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion //Disposable
    }
}
