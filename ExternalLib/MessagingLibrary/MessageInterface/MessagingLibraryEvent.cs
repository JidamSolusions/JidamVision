using MessagingLibrary.Model;

namespace MessagingLibrary.MessageInterface
{
	/// <summary>
	/// Wcf에서 메시지 수신시 발생하는 이벤트 핸들러
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="message"></param>
	public delegate void WcfReceivedMessageHandler(object sender, IMessageDuplexCallback channel, MmiMessageInfo message, ClientInfo client);

	public delegate void WcfConnected(object sender, ClientInfo context);
}
