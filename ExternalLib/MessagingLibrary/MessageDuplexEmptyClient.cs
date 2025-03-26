using MessagingLibrary.MessageInterface;
using System;
using System.ServiceModel;

namespace MessagingLibrary
{
	public class MessageDuplexEmptyClient : IMessageDuplexClient
	{
		private CommunicationState _state;
		public override CommunicationState State
		{
			get
			{
				return _state;
			}
		}

		public override void Close()
		{
			_state = CommunicationState.Closed;
			OnClosed(this, new EventArgs());
		}

		public override void Abort()
		{

		}

		public override bool Connect()
		{
			_state = CommunicationState.Opened;
			OnOpened(this, new EventArgs());
			return true;
		}

		public override CommunicationErrorType SendMessage(MmiMessageInfo info)
		{
			return CommunicationErrorType.None;
		}

		public string Test()
		{
			return "Test";
		}
	}
}
