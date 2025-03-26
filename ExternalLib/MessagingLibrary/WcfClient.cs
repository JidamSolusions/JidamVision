using Cressem.MessagingLibrary.ABCGenerator;
using Cressem.MessagingLibrary.MessageInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Cressem.MessagingLibrary
{
	class MyCallback : IMyContractCallback
	{
		public void OnCallback()
		{
			Console.WriteLine("Callback method is called from client side.");

		}
	}

	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
	public class WcfClient 
	{
		public event WcfReceivedMessageHandler WcfReceivedMessage = delegate { };

		// lock문에 사용될 객체
		private object lockObject = new object();

		private readonly BindingType _bindingType;
		
		private Guid _clientID;
		private ServiceHost _clientHost;

		private IABCGenerator _abcGenerator;

		public WcfClient(BindingType bindingType, string serverIpAddress = "")
		{
			_clientID = Guid.NewGuid();

			_bindingType = bindingType;

			_abcGenerator = _bindingType == BindingType.Pipe ? (IABCGenerator)new PipeGenerator(false) : (IABCGenerator)new TcpGenerator(false, serverIpAddress);
		}
		
		public void TakeMessage(MessagingInfo message)
		{
			lock (lockObject)
			{
				WcfReceivedMessage(this, message);
			}
		}
		
		public void Register()
		{
			using (ChannelFactory<IClientToServerMessage> factory = new ChannelFactory<IClientToServerMessage>(_abcGenerator.GetRemoteBinding(), new EndpointAddress(_abcGenerator.GetRemoteAddress())))
			{
				IClientToServerMessage clientToServerChannel = factory.CreateChannel();
				try
				{
					clientToServerChannel.Register(_abcGenerator.LocalIpAddress);
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

		public void SendMessage(MessagingInfo message)
		{
			// 한번에 한 쓰레드만 lock블럭 실행
			lock (lockObject)
			{
				string address = _abcGenerator.GetRemoteAddress();
				using (ChannelFactory<IClientToServerMessage> factory = new ChannelFactory<IClientToServerMessage>(_abcGenerator.GetRemoteBinding(), new EndpointAddress(address)))
				//using (ChannelFactory<IClientToServerMessage> factory = new ChannelFactory<IClientToServerMessage>(tuple.Item1, tuple.Item2))
				{
					IClientToServerMessage clientToServerChannel = factory.CreateChannel();
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

		public void OpenClientWcf()
		{
			_clientHost = new ServiceHost(this);
			_clientHost.AddServiceEndpoint((typeof(IServerToClientMessage)), _abcGenerator.GetHostBinding(), _abcGenerator.GetHostAddress());
			_clientHost.Open();
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
			throw new NotImplementedException();
		}
	}
}
