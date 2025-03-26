using MessagingLibrary.MessageInterface;
using MessagingLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MessagingLibrary
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class MessageDuplexService : IMessageDuplexService
	{
		#region Variables

		private static readonly object _sycnRoot = new object();

		private string _servicePipeAddress = "net.pipe://localhost/WCFServices";
		private string _serviceTcpAddress = "net.tcp://{0}:7900/WCFServices";

		public event WcfReceivedMessageHandler WcfReceivedMessage = delegate { };
		public event WcfConnected WcfConnectedHandler = delegate { };
		public event WcfConnected WcfDisconnectedHandler = delegate { };

		private List<Tuple<IContextChannel, IMessageDuplexCallback, ClientInfo>> _callbackChannels = new List<Tuple<IContextChannel, IMessageDuplexCallback, ClientInfo>>();

		private ServiceHost _selfHost;

		private BindingType _bindingType;

		#endregion

		#region Constructor

		public MessageDuplexService(BindingType bindingType)
		{
			_bindingType = bindingType;
		}

		#endregion

		public string ListenningIpAddress { get; set; }

		#region Public Methods

		/// <summary>
		/// 서버 실행
		/// </summary>
		public void Run()
		{
			string address = _bindingType == BindingType.Tcp ? GetTcpAddress() : GetPipeAddress();
			Uri uri = new Uri(address);

			Binding binding = _bindingType == BindingType.Tcp ? GetTcpBinding() : GetPipeBinding();
			binding.CloseTimeout = new TimeSpan(10, 0, 0, 0);
			binding.OpenTimeout = new TimeSpan(10, 0, 0, 0);
			binding.ReceiveTimeout = new TimeSpan(10, 0, 0, 0);
			binding.SendTimeout = new TimeSpan(10, 0, 0, 0);

			_selfHost = new ServiceHost(this, uri);
			_selfHost.AddServiceEndpoint(typeof(IMessageDuplexService),
						  binding,
						  uri);
			_selfHost.Open();
		}

		/// <summary>
		/// 클라이언튼에서 오는 접속 요청
		/// </summary>
		public void Connect()
		{
			try
			{
				IMessageDuplexCallback callbackChannel =
					 OperationContext.Current.GetCallbackChannel<IMessageDuplexCallback>();

				lock (_sycnRoot)
				{
					//if (!_callbackChannels.Contains(callbackChannel))
					{
						ClientInfo client = GetClient(OperationContext.Current);
						OperationContext.Current.Channel.Closed += Channel_Closed1;
						OperationContext.Current.Channel.Closing += Channel_Closing;
						_callbackChannels.Add(new Tuple<IContextChannel, IMessageDuplexCallback, ClientInfo>(OperationContext.Current.Channel, callbackChannel, client));

						WcfConnectedHandler(this, client);
					}
				}
			}
			catch
			{
				throw;
			}
		}

		/// <summary>
		/// 클리어언트에서 호출
		/// </summary>
		/// <param name="item"></param>
		public void TakeMessage(MmiMessageInfo item)
		{
			IContextChannel registeredUser = OperationContext.Current.Channel;

			var found = _callbackChannels.Where(x => x.Item1 == registeredUser).FirstOrDefault();
			if (found != null)
				WcfReceivedMessage(this, found.Item2, item, found.Item3);
		}

		/// <summary>
		/// 클라이언트에 메시지 보낼때
		/// </summary>
		/// <param name="item"></param>
		public void SendMessage(MmiMessageInfo item)
		{
			if (_callbackChannels.Count > 0)
			{
				foreach (var v in _callbackChannels)
				{
					if (v.Item1.State == CommunicationState.Opened)
						v.Item2.TakeMessage(item);
				}
			}
		}

		public bool SendMessage(string name, MmiMessageInfo item)
		{
			try
			{
				var client = _callbackChannels.Where(x => x.Item3.Name == name).FirstOrDefault();
				client.Item2.TakeMessage(item);
				System.Threading.Thread.Sleep(5);
			}
			catch { return false; }
			return true;
		}
		/// <summary>
		/// 클라이언트의 접속을 끊는다.
		/// </summary>
		public void Disconnect()
		{
			if (OperationContext.Current != null)
			{
				IMessageDuplexCallback callbackChannel = OperationContext.Current.GetCallbackChannel<IMessageDuplexCallback>();
			}

			try
			{
				lock (_sycnRoot)
				{
					//foreach(var v in _callbackChannels)
					//{
					//	v.Item1.Close();
					//}
				}
			}
			catch
			{
				throw;
			}
		}

		#endregion

		#region Private Methods

		private ClientInfo GetClient(OperationContext context)
		{
			ClientInfo client = new ClientInfo();

			MessageProperties properties = context.IncomingMessageProperties;

			RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

			client.IpAddress = endpoint.Address;
			client.Port = endpoint.Port;

			return client;
		}

		private static string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}

		private string GetTcpAddress()
		{
			string address;
			if (String.IsNullOrWhiteSpace(ListenningIpAddress) == true)
				address = String.Format(_serviceTcpAddress, GetLocalIPAddress());
			else
				address = String.Format(_serviceTcpAddress, ListenningIpAddress);
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

		private void Channel_Closed(object sender, EventArgs e)
		{
			foreach (var tuple in _callbackChannels)
			{
				if (tuple.Item1.State != CommunicationState.Opened)
				{
					_callbackChannels.Remove(tuple);
				}
			}
			//throw new NotImplementedException();
		}

		private void Channel_Faulted(object sender, EventArgs e)
		{
			//throw new NotImplementedException();
		}

		private void Channel_Closing(object sender, EventArgs e)
		{
		}

		private void Channel_Closed1(object sender, EventArgs e)
		{
			var found = _callbackChannels.Where(x => x.Item1 == sender).FirstOrDefault();
			if (found != null)
			{
				WcfDisconnectedHandler(this, found.Item3);

				_callbackChannels.Remove(found);
			}
		}

		#endregion
	}
}
