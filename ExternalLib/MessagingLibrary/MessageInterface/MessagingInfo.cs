namespace MessagingLibrary.MessageInterface
{

	public class MmiMessageInfo
	{
		public string Message { get; set; }

		public virtual string ToXmlContent()
		{
			return Message;
		}
	}
}
