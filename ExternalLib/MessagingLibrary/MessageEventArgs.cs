using MessagingLibrary.MessageInterface;
using System;

namespace MessagingLibrary
{
	public class MessageEventArgs : EventArgs
	{
		private MmiMessageInfo _items;

		public MmiMessageInfo Item
		{
			get { return _items; }
			set { _items = value; }
		}

		public MessageEventArgs(MmiMessageInfo items)
		{
			_items = items;
		}
	}
}
