using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Cressem.MessagingLibrary.ABCGenerator
{
	public class PipeGenerator : IABCGenerator
	{
		private bool _isServer;

		private readonly Dictionary<bool, string> _dicAddresses = new Dictionary<bool, string>()
		{
			{ true,  "net.pipe://localhost/Server"},
			{ false, "net.pipe://localhost/Client"},
		};

		public PipeGenerator(bool isServer)
		{
			_isServer = isServer;
		}

		public string GetHostAddress()
		{
			string address = _dicAddresses[_isServer];
			return address;
		}

		public Binding GetHostBinding()
		{
			NetNamedPipeBinding binding = new NetNamedPipeBinding();
			binding.SendTimeout = new TimeSpan(0, 0, 0, 0, 100);
			return new NetNamedPipeBinding();
		}

		public string GetRemoteAddress()
		{
			string address = _dicAddresses[!_isServer];
			return address;
		}

		public Binding GetRemoteBinding()
		{
			NetNamedPipeBinding binding = new NetNamedPipeBinding();
			binding.SendTimeout = new TimeSpan(0, 0, 0, 0, 100);
			return new NetNamedPipeBinding();
		}

		public string LocalIpAddress { get { return ""; } }
	}
}
