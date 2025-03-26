using JidamVision.Setting;
using MessagingLibrary;
using MessagingLibrary.MessageInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;
using static MessagingLibrary.Message;

namespace JidamVision.Sequence
{
    public enum SeqCmd
    {
        None = 0,
        OpenRecipe,
        InspReady,
        VisionReady,
        InspStart,
        InspDone
    }

    public enum Vision2Mmi
    {
        None = 0,
        InspReady,
        InspDone,
        Error
    }

    public enum MmiSeq
    {
        None = 0,
        InspStart,
        InspDone,
        Error

    }

    public class Sequence : IDisposable
    {
        private static Sequence _sequence = null;

        public static Sequence Inst
        {
            get
            {
                if (_sequence == null)
                {
                    _sequence = new Sequence();
                    _sequence.InitSequence();
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
        private MmiSeq _mmiState = MmiSeq.None;

        private string _lastErrMsg;

        public Sequence()
        {

        }


        private void InitSequence()
        {
            //통신 초기화
            //통신 이벤트 등록
            _message.MachineName = SettingXml.Inst.MachineName;

            _sequenceThread = new Thread(SequenceThread);
            _sequenceThread.IsBackground = true;
            _sequenceThread.Start();
        }

        public int CreateHandler(Communicator communicator)
        {
            try
            {
                if (communicator != null)
                {
                    _communicator = communicator;
                    _communicator.ReceiveMessage += Communicator_ReceiveMessage;
                }
            }
            catch (Exception ex)
            {
                _lastErrMsg = ex.Message;
                return -1;
            }

            return 0;
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

        private void UpdateSeqState()
        {
            switch (_mmiState)
            {
                case MmiSeq.None:
                    break;
                case MmiSeq.InspStart:
                    SeqCommand(this, SeqCmd.InspStart, null);
                    break;
                case MmiSeq.InspDone:
                    SeqCommand(this, SeqCmd.InspDone, null);
                    break;
                case MmiSeq.Error:
                    SeqCommand(this, SeqCmd.InspDone, null);
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
                        SeqCommand(this, SeqCmd.OpenRecipe, null);
                    }
                    break;
                case Message.MessageCommand.Loaded:
                    {
                        SeqCommand(this, SeqCmd.InspReady, null);
                    }
                    break;
            }
        }

        public void VisionCommand(Vision2Mmi visionCmd, Object e)
        {
            switch (visionCmd)
            {
                case Vision2Mmi.InspReady:
                    {
                        string errMsg = (string)e;
                        if (errMsg != "")
                        {
                            _lastErrMsg = errMsg;
                            _mmiState = MmiSeq.Error;
                            break;
                        }

                        _message.Command = Message.MessageCommand.OpenRecipe;
                        _message.Status = CommandStatus.Success;
                        SendMessage(_message);
                    }
                    break;
                case Vision2Mmi.InspDone:
                    {
                        string errMsg = (string)e;

                        if (errMsg != "")
                        {
                            _lastErrMsg = errMsg;
                            _mmiState = MmiSeq.Error;
                            break;
                        }

                        _message.Command = Message.MessageCommand.Loaded;
                        _message.Status = CommandStatus.Success;
                        SendMessage(_message);

                        if (_mmiState == MmiSeq.None)
                        {
                            _mmiState = MmiSeq.InspStart;
                        }
                    }
                    break;
            }
        }

        private void ResetSequence()
        {
            _mmiState = MmiSeq.None;
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
                        _communicator.ReceiveMessage -= Communicator_ReceiveMessage;
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
