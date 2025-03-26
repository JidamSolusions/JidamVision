using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Cressem.MessagingLibrary.ABCGenerator
{
	public class TcpGenerator : IABCGenerator
	{
		private const int SERVER_PORT = 7900;
		private const int CLIENT_PORT = 7901;

		private bool _isServer;
		private string _remoteIpAddress;

		private readonly Dictionary<bool, Tuple<int, string>> _dicAddresses = new Dictionary<bool, Tuple<int, string>>()
		{
			{ true,  new Tuple<int, string>( SERVER_PORT, "net.tcp://{0}:{1}/Server")},
			{ false, new Tuple<int, string>( CLIENT_PORT, "net.tcp://{0}:{1}/Client")},
		};


		public TcpGenerator(bool isServer, string remoteIpAddress = "")
		{
			_isServer = isServer;
			_remoteIpAddress = remoteIpAddress;
		}

		public string GetHostAddress()
		{
			string localIpAddress = GetLocalIPAddress();

			string address = String.Format(_dicAddresses[_isServer].Item2, localIpAddress, _dicAddresses[_isServer].Item1);

			return address;
		}

		public Binding GetHostBinding()
		{
			NetTcpBinding binding = new NetTcpBinding();
			binding.Security.Mode = SecurityMode.None;
			binding.SendTimeout = new TimeSpan(0, 0, 0, 0, 100);

			return binding;
		}

		public string GetRemoteAddress()
		{
			string localIpAddress = GetLocalIPAddress();

			string address = String.Format(_dicAddresses[!_isServer].Item2, localIpAddress, _dicAddresses[!_isServer].Item1);

			return address;
		}

		public Binding GetRemoteBinding()
		{
			NetTcpBinding binding = new NetTcpBinding();
			binding.Security.Mode = SecurityMode.None;
			binding.SendTimeout = new TimeSpan(0, 0, 0, 0, 100);
			return binding;
		}

		private static string GetLocalIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}

		public string LocalIpAddress { get { return GetLocalIPAddress(); } }
	}
}
