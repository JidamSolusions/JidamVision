using System;

namespace MessagingLibrary.Model
{
	public class ClientInfo
	{
		public string IpAddress { get; set; }

		public int Port { get; set; }
		public string Name { get; set; }

		public override string ToString()
		{
			return String.Format($"IP : {IpAddress}, Port : {Port}");
		}
	}
}
