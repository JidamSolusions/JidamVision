using MessagingLibrary.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace MessagingLibrary
{
	[Serializable]
	[XmlRoot("ITSMessage")]
	public class Message : MessageInterface.MmiMessageInfo, ICloneable
	{
		// 번호 적혀있는건 기존에 쓰고 있는 스트링이라 바꾸면 안됨
		public enum MessageCommand : int
		{
            None = -1,
			HandShake,
            OpenRecipe,
			MmiReady,
			MmiStart,
			MmiStop,
            InspReady,
			InspStart,
			InspDone,
            InspResult,
            InspEnd,
            Reset,
            Error,
            Alarm,
            MachineName,
            Loaded
        }

        public enum CommandStatus : int
		{
			None,
			Success,
			Fail,
			Ready,
			Completed,
			Running,
			Reset,
			Error,
			Start,
			End,
			Good,
			Ng,
            Retry,
        }

		public enum MsgModeCommand : int
		{
			Normal,
			Calibration,

		}
		public enum CommandType
		{
			Move,
			Etc,
		}

		[XmlElement("Ack")]
		public object Ack { get; set; }

		/// <summary>
		/// 검사 Lot Number
		/// </summary>
		[XmlElement("LotNumber")]
		public string LotNumber { get; set; }

		/// <summary>
		/// 검사 Strip ID
		/// </summary>
		[XmlElement("SerialID")]
		public string SerialID { get; set; }

		/// <summary>
		/// Barcode
		/// </summary>
		[XmlElement("Barcode")]
		public string Barcode { get; set; }


		/// <summary>
		/// 이동 좌표 - Teach 
		/// </summary>
		[XmlElement("MovePoint")]
		public PointF MovePoint { get; set; }

        /// <summary>
        /// 이동 좌표 - Teach 
        /// </summary>
        [XmlElement("ZPos")]
        public float ZPos { get; set; }

		/// <summary>
		/// 에러 메시지
		/// </summary>
		[XmlElement("Error")]
		public string ErrorMessage { get; set; }

		/// <summary>
		/// 메세지 전송 시간
		/// yyyy-MM-dd:ss:fff
		/// </summary>
		[XmlElement("Time")]
		public string Time { get; set; }

		/// <summary>
		/// Device ID
		/// </summary>
		[XmlElement("Device")]
		public string Device { get; set; }

		/// <summary>
		/// Tool ID
		/// </summary>
		[XmlElement("ToolID")]
		public string Tool { get; set; }

		[XmlElement("MachineName")]
		public string MachineName { get; set; }

		/// <summary>
		/// AGB(ALL 양품) or BAD(All 불량) Type
		/// </summary>
		[XmlElement("BoardType")]
		public string BoardType { get; set; }

		/// <summary>
		/// Customer
		/// </summary>
		[XmlElement("Customer")]
		public string Customer { get; set; }

		/// <summary>
		/// CS(앞면) or SS (뒷면)
		/// </summary>
		[XmlElement("UserSide")]
		public string UserSide { get; set; }

		/// <summary>
		/// RevisionNum
		/// </summary>
		[XmlElement("RevisionNum")]
		public string RevisionNum { get; set; }


		/// <summary>
		/// DeviceColor
		/// </summary>
		[XmlElement("DeviceColor")]
		public string DeviceColor { get; set; }

        // Review Side Info (Top, Bottom)
        [XmlElement("Side")]
        public string Side { get; set; }

        /// <summary>
        /// Review Tray ID Info
        /// </summary>
        [XmlArrayItem(ElementName = "TrayID", Type = typeof(string))]
		public string[] TrayID { get; set; }


		/// <summary>
		/// 전송 명령어
		/// </summary>
		[XmlElement("MessageCommand")]
		public MessageCommand Command { get; set; }

		[XmlElement("Status")]
		public CommandStatus Status { get; set; }

		[XmlElement("ModeCommand")]
		public MsgModeCommand ModeCommand { get; set; }

		public override string ToString()
		{
			string inputLotString = String.Empty;
			string inputToolString = String.Empty;

			return String.Format($"LotNumber : {LotNumber}");
		}

        public override string ToXmlContent()
		{
			return XmlHelper.ObjectToXmlString(this);
		}

		public static Message CreateMachineNameMessage(string machineName)
		{
			Message message = new Message
			{
				Command = MessageCommand.MachineName,
				MachineName = machineName
			};

			return message;
		}

		public object Clone()
		{
			Message message = new Message()
			{
				Ack = this.Ack,
				LotNumber = this.LotNumber,
				SerialID = this.SerialID,
				Barcode = this.Barcode,
				MovePoint = this.MovePoint,
				ErrorMessage = this.ErrorMessage,
				Time = this.Time,
				Device = this.Device,
				Tool = this.Tool,
				MachineName = this.MachineName,
				BoardType = this.BoardType,
				Customer = this.Customer,
				UserSide = this.UserSide,
				RevisionNum = this.RevisionNum,
				Side = this.Side,
				TrayID = this.TrayID,
				Command = this.Command,
				Status = this.Status,
				ModeCommand = this.ModeCommand,
			};

			return message;
		}
	}
}
