// Lance Roberts 04-Mar-2010
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Cressem.MessagingLibrary.MessageInterface
{
	[ServiceContract(SessionMode = SessionMode.Allowed)]
	public interface IClientToServerMessage
	{
		// Pipe Communication용
		[OperationContract(IsOneWay = true)]
		void Register(string ipAddress = "");
		
		[OperationContract(IsOneWay = true)]
		void TakeMessage(MessagingInfo message);
	}
}
