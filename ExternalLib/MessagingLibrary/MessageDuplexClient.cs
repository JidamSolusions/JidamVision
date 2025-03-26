using MessagingLibrary.MessageInterface;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace MessagingLibrary
{
	/// <summary>
	/// Duplex for WCF용 클라이언트 클래스
	/// </summary>
	public class MessageDuplexClient : IMessageDuplexClient
	{
		#region Variables

		private string _servicePipeAddress = "net.pipe://localhost/WCFServices";
		private string _serviceTcpAddress = "net.tcp://{0}:7900/WCFServices";

		private DuplexClientImpl _duplexClientImpl;

		private string _ipAddress;

		private BindingType _bindingType;

		private Task<bool> _task;
		#endregion

		#region Constructor

		public MessageDuplexClient(BindingType bindingType, string ipAddress = "")
		{
			_bindingType = bindingType;
			_ipAddress = ipAddress;
		}

		#endregion

		public override CommunicationState State
		{
			get
			{
				if (_duplexClientImpl == null)
					return CommunicationState.Closed;

				return _duplexClientImpl.State;
			}
		}

		#region Public Methods

		public override void Close()
		{
            if (_duplexClientImpl.Endpoint.Name == null)
            {
                _duplexClientImpl.Disconnect();
            }

            if (_duplexClientImpl.State != CommunicationState.Closed)
                _duplexClientImpl.Close();
        }

		public override void Abort()
		{
			_duplexClientImpl.Abort();
		}

		public override bool Connect()
		{
			_task = Task<bool>.Factory.StartNew(() =>
			{
				// 서버에서 보내온 메시지 수신 위해 콜백 메시지에서 이벤트 등록한다.
				var messageDuplexCallback = new MessageDuplexCallback();
				messageDuplexCallback.ServiceCallbackEvent += MessageDuplexCallback_ServiceCallbackEvent;

				var instanceContext = new InstanceContext(messageDuplexCallback);
				instanceContext.Opened += InstanceContext_Opened;
				instanceContext.Closed += InstanceContext_Closed;
				instanceContext.Closing += InstanceContext_Closed;

				var binding = _bindingType == BindingType.Tcp ? GetTcpBinding() : GetPipeBinding();// new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
				string address = _bindingType == BindingType.Tcp ? GetTcpAddress() : GetPipeAddress();

				Uri uri = new Uri(address);
				var endpointAddress = new EndpointAddress(uri);

				_duplexClientImpl = new DuplexClientImpl(instanceContext, binding, endpointAddress);
				_duplexClientImpl.Closed += _duplexClientImpl_Closed;
				_duplexClientImpl.Opened += _duplexClientImpl_Opened;

				try
				{
					_duplexClientImpl.Connect();
					return true;
				}
				catch (Exception ex)
				{				
					Debug.WriteLine(ex.Message); 

					OnClosed(this, null);
					return false;
				}

			}, TaskCreationOptions.LongRunning);

			_task.Wait();

			return _task.Result;
		}
		private void InstanceContext_Opened(object sender, EventArgs e)
		{
			//throw new NotImplementedException();
		}

		private void _duplexClientImpl_Opened(object sender, EventArgs e)
		{
			OnOpened(this, e);
		}

		private void _duplexClientImpl_Closed(object sender, EventArgs e)
		{
			OnClosed(this, e);
		}

		private void InstanceContext_Closed(object sender, EventArgs e)
		{
			OnClosed(this, e);
		}

		public override CommunicationErrorType SendMessage(MmiMessageInfo info)
		{
			//var task = Task< CommunicationErrorType>.Factory.StartNew(() =>
			{
				if (_duplexClientImpl is null)
					return CommunicationErrorType.Disconnected;

				// Opended상태가 아니면 서버와 연결이 끊어진 상태
				if (_duplexClientImpl.State == CommunicationState.Opened)
				{
					try
					{
						// 메시지 송신 중 예외 발생 시 서버가 끊어진 상태이다.
						_duplexClientImpl.TakeMessage(info);
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);

						OnClosed(this, new EventArgs());
					}

					return CommunicationErrorType.None;
				}
				else
				{
					return CommunicationErrorType.Disconnected;
				}
			};//);

			//var result = task.Result;

			//return result;
		}

		#endregion

		private string GetTcpAddress()
		{
			string address = String.Format(_serviceTcpAddress, _ipAddress);
			return address;
		}

		private string GetPipeAddress()
		{
			return _servicePipeAddress;
		}

		private Binding GetTcpBinding()
		{
			return new NetTcpBinding(SecurityMode.None);
		}

		private Binding GetPipeBinding()
		{
			return new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

		}

		#region Event Handlers

		private void MessageDuplexCallback_ServiceCallbackEvent(object sender, MessageEventArgs e)
		{
			OnWcfReceivedMessage(this, null, e.Item, null);
		}

		#endregion
	}
}
