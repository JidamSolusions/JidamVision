using MessagingLibrary.MessageInterface;
using System;
using System.ComponentModel;
using System.ServiceModel;
using System.Threading;

namespace MessagingLibrary
{
	/// <summary>
	/// IMessageDuplexCallback을 구현한 콜백 클래스
	/// </summary>
	[CallbackBehavior(UseSynchronizationContext = false)]
	public class MessageDuplexCallback : IMessageDuplexCallback
	{
		#region Event

		public event EventHandler<MessageEventArgs> ServiceCallbackEvent;

		#endregion

		#region Variables

		private SynchronizationContext _syncContext = AsyncOperationManager.SynchronizationContext;

		#endregion

		#region Public Methods

		public void TakeMessage(MmiMessageInfo items)
		{
			_syncContext.Send(new SendOrPostCallback(OnServiceCallbackEvent), new MessageEventArgs(items));
		}

		#endregion

		#region Private Method

		private void OnServiceCallbackEvent(object state)
		{
			EventHandler<MessageEventArgs> handler = ServiceCallbackEvent;
			MessageEventArgs e = state as MessageEventArgs;

			if (handler != null)
			{
				handler(this, e);
			}
		}

		#endregion

	}
}
