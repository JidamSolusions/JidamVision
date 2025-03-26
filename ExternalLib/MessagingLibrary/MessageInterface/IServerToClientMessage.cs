using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Cressem.MessagingLibrary.MessageInterface
{
	[ServiceContract(SessionMode = SessionMode.Allowed)]
	internal interface IServerToClientMessage
	{
		[OperationContract(IsOneWay = true)]
		void TakeMessage(MessagingInfo message);
	}
}
