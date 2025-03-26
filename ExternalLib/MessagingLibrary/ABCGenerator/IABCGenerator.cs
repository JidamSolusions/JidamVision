using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Cressem.MessagingLibrary.ABCGenerator
{
	/// <summary>
	/// WCF의 ABC(Address, Binding, Contract)를 생성하는 Interface
	/// </summary>
	public interface IABCGenerator
	{
		string LocalIpAddress { get; }

		string GetHostAddress();

		Binding GetHostBinding();

		string GetRemoteAddress();

		Binding GetRemoteBinding();
	}
}
