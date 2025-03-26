using System.ServiceModel;

namespace MessagingLibrary.MessageInterface
{
	/// <summary>
	/// Duplex Service 인터페이스(서버용)
	/// </summary>
	[ServiceContract(CallbackContract = typeof(IMessageDuplexCallback))]
	public interface IMessageDuplexService
	{
		/// <summary>
		/// 서버의 서비스 실행
		/// </summary>
		[OperationContract(IsOneWay = true)]
		void Run();

		/// <summary>
		/// 클라이언트에서 서버로 접속
		/// </summary>
		[OperationContract(IsOneWay = true)]
		void Connect();

		/// <summary>
		/// 클리이언트에서 서버로 메시지 전송
		/// </summary>
		/// <param name="item"></param>
		[OperationContract(IsOneWay = true)]
		void TakeMessage(MmiMessageInfo item);

		/// <summary>
		/// 클라이언트에서 서버로 연결 해제 요청
		/// </summary>
		[OperationContract(IsOneWay = true)]
		void Disconnect();
	}
}
