using Cressem.MessagingLibrary.ABCGenerator;
using Cressem.MessagingLibrary.MessageInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Cressem.MessagingLibrary
{
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
	public class WcfServer : IClientToServerMessage, IMyContract
	{
		public event WcfReceivedMessageHandler WcfReceivedMessage = delegate { };

		// lock문에 사용될 객체
		private object lockObject = new object();

		private readonly BindingType _bindingType;

		public List<string> _registeredClients = new List<string>();

		private ServiceHost _serverHost;
		private IABCGenerator _abcGenerator;

		public WcfServer(BindingType bindingType)
		{
			_bindingType = bindingType;

			_abcGenerator = _bindingType == BindingType.Pipe ? (IABCGenerator)new PipeGenerator(true) : (IABCGenerator)new TcpGenerator(true);
		}

		public void Register(string ipAddress)
		{
			if (!_registeredClients.Contains(ipAddress))
				_registeredClients.Add(ipAddress);
		}
		
		public void SendMessage(MessagingInfo message)
		{
			// 한번에 한 쓰레드만 lock블럭 실행
			lock (lockObject)
			{
				string address = _abcGenerator.GetRemoteAddress();
				using (ChannelFactory<IServerToClientMessage> factory = new ChannelFactory<IServerToClientMessage>(_abcGenerator.GetRemoteBinding(), new EndpointAddress(address)))
				//using (ChannelFactory<IClientToServerMessage> factory = new ChannelFactory<IClientToServerMessage>(tuple.Item1, tuple.Item2))
				{
					IServerToClientMessage clientToServerChannel = factory.CreateChannel();
					try
					{
						clientToServerChannel.TakeMessage(message);
					}
					catch (Exception ex)
					{
						throw;
					}
					finally
					{
						CloseChannel((ICommunicationObject)clientToServerChannel);
					}
				}
			}
		}

		public void TakeMessage(MessagingInfo message)
		{
			lock (lockObject)
			{
				WcfReceivedMessage(this, message);
			}
		}

		public void OpenServerWcf()
		{
			_serverHost = new ServiceHost(this);
			_serverHost.AddServiceEndpoint((typeof(IClientToServerMessage)), _abcGenerator.GetHostBinding(), _abcGenerator.GetHostAddress());
			//_serverHost.AddServiceEndpoint((typeof(IFromClientToServerMessages)), new NetNamedPipeBinding(), _endpointUri);
			_serverHost.Open();
		}

		private void CloseChannel(ICommunicationObject channel)
		{
			try
			{
				channel.Close();
			}
			catch (Exception ex)
			{
				throw;
			}
			finally
			{
				channel.Abort();
			}
		}

		public void MyMethod()
		{
			//Do something            
			IMyContractCallback callbackInstance
			= OperationContext.Current.GetCallbackChannel<IMyContractCallback>();//.GetCallbackChannel();
			callbackInstance.OnCallback();
		}
	}
}
