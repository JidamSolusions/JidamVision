namespace MessagingLibrary
{
	public enum BindingType
	{
		Tcp,
		Pipe
	}

	public enum MlErrorCode
    {

		None = 0,
		TimeOut = 1,
		ConnectError = 2,
		AlreadyConnect = 3,
	}

	public enum CommunicationErrorType
	{
		None,
		CannotConnect,
		Disconnected
	}
}
