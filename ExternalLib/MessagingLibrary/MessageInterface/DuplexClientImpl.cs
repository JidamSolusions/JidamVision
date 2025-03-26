using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MessagingLibrary.MessageInterface
{
	/// <summary>
	/// DuplexClientServer를 구현한 클래스
	/// </summary>
	internal class DuplexClientImpl : DuplexClientBase<IMessageDuplexService>, IMessageDuplexService
	{
		public event EventHandler Closed = delegate { };
		public event EventHandler Opened = delegate { };

		public DuplexClientImpl(InstanceContext callbackInstance, Binding binding, EndpointAddress endpointAddress)
			: base(callbackInstance, binding, endpointAddress)
		{
			base.ChannelFactory.Opened += ChannelFactory_Opened;
			base.ChannelFactory.Closed += ChannelFactory_Closed;
			base.InnerChannel.Closed += InnerChannel_Closed;
			base.InnerChannel.Faulted += InnerChannel_Faulted;
		}

		private void InnerChannel_Faulted(object sender, EventArgs e)
		{
			Closed(this, e);
		}

		private void InnerChannel_Closed(object sender, EventArgs e)
		{
			Closed(this, e);
		}

		private void ChannelFactory_Closed(object sender, EventArgs e)
		{
			Closed(this, e);
		}

		public void Run()
		{
			Channel.Run();
		}

		public void Connect()
		{
			Channel.Connect();
		}

		private void ChannelFactory_Opened(object sender, EventArgs e)
		{
			Opened(this, e);
		}

		public void TakeMessage(MmiMessageInfo item)
		{
			Channel.TakeMessage(item);
		}

		public void Disconnect()
		{
			Channel.Disconnect();
		}
	}
}
