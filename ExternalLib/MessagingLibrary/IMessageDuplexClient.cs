using MessagingLibrary.MessageInterface;
using MessagingLibrary.Model;
using System;
using System.ServiceModel;

namespace MessagingLibrary
{
	public abstract class IMessageDuplexClient
	{
		public event WcfReceivedMessageHandler WcfReceivedMessage = delegate { };

		public event EventHandler Closed = delegate { };
		public event EventHandler Opened = delegate { };

		public abstract CommunicationState State { get; }
		public abstract void Close();

		public abstract void Abort();

		public abstract bool Connect();

		public abstract CommunicationErrorType SendMessage(MmiMessageInfo info);

		protected void OnWcfReceivedMessage(object sender, IMessageDuplexCallback channel, MmiMessageInfo message, ClientInfo client)
		{
			WcfReceivedMessage(sender, channel, message, client);
		}

		protected void OnOpened(object sender, EventArgs e)
		{
			Opened(sender, e);
		}

		protected void OnClosed(object sender, EventArgs e)
		{
			Closed(sender, e);
		}
	}
}
