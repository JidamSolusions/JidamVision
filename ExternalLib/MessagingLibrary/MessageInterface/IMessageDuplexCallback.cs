using System.ServiceModel;

namespace MessagingLibrary.MessageInterface
{
	/// <summary>
	/// Duplex Wcf용 콜백 인터페이스
	/// </summary>
	public interface IMessageDuplexCallback
	{
		/// <summary>
		/// 서버에서 클릴이언트로 메시지 전송용 콜백 함수
		/// </summary>
		/// <param name="items"></param>
		[OperationContract(IsOneWay = true)]
		void TakeMessage(MmiMessageInfo items);
	}
}
