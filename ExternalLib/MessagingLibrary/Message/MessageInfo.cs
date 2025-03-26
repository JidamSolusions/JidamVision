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
            None,
            HandShake,                  // (? ms 마다)						/ Parameter					/ 모두	/ 모두
            Reset,                      // 시퀀스 리셋						/							/ 제어	/ 모두
            Stop,                       // 중지								/							/ 모두	/ 모두
            Error,                      // 에러 발생						/ 내용						/ 비전	/ 모두
            Alarm,                      // 알람 울릴 필요성이 있을 때		/ 내용						/ 비전	/ 모두

            //***************************
            //새로변경된 규약
            ControlStart,
            ControlEnd,

            MovePosition,
            CurrentPosition,
            //***************************

            MovePoint_On,				// 비젼 위치 이동 모드 ON			/ 내용						/ 비전	/ 모두
            MovePoint_Move,				// 비젼 위치 이동 요청				/ 내용						/ 비전	/ 모두
            MovePoint_Off,              // 비젼 위치 이동 모드 OFF			/ 내용						/ 비전	/ 모두
            MovePoint_Done,				// 비젼 위치 이동 완료				/ 내용						/ 제어	/ 모두
            MovePoint_Pos,              // 현재 위치 요청					/ 내용						/ 비젼	/ 모두

            MovePoint_Teaching,         // 이동
            MovePoint_PRS,                   // 이동 - lot/serial
            MovePoint_Defect,           // 이동 - lot/serial
                                        //MovePosition,               // 위치 이동						/ 좌표
            MoveResolution,             // 해상도 변경						/ 해상도					/ 비전	/ 모두
            MotorValue,                 // 현재 모터값						/ 모터 값					/ 제어  / 모두

            ScanPositionPRS,            // N개 보정마크 위치 전송
            MovePositionPRS,            // N개 보정마크 이동 + 촬상 시작

            ScanPositionDefect,         // N개 불량 위치 전송
            MovePositionDefect,         // N개 불량 위치 이동 + 촬상 시작

            Manual,
            Loaded,                     // 스캔 준비 완료					/ Side,Tool,Lot,Serial		/ 제어	/ 모두
            VisionReady,                // 비전 준비 확인					/ 해상도, 이미지 개수		/ 비전	/ 모두
            ControlReady,               // 제어 준비 확인					/ 해상도, 이미지 개수		/ 제어	/ 모두
            InspectionEnd,              // 비전 시퀀스	(핸드쉐이크 안함)	/							/ 비전	/ 모두
            LotEnd,                     // 랏 종료							/ Side, Tool, Lot			/ 제어	/ 모두
            Complete,                   // 배출 - lot/serial

            ReviewInfo,                 // Side, TrayID 전달
            CreateModel,                // 모델 생성 
            OpenRecipe,                 // 모델 로드 

            MovePoint_Manual,
            LotStart,
            Loading,
            LineScan,
            ModeChange,

            MachineName,                // 장비 이름
            LotInfo,                    // Lot 정보

            Inspect,                             // 검사

            #region Area Sequence

            AreascanStart,              // 그랩 시작						/ 시작, 종료 좌표,	 번호	/ 비전	/ Area
            AreascanEnd,                // 그랩 종료						/ Shot 번호					/ 비전	/ Area
            AreascanFinish,             // 자재 시퀀스 종료					/ Lot						/ 비전	/ Area

            #endregion

            #region Linescan Squence

            LinescanReady,              // 그랩 준비, 미리 좌표 이동		/ 시작, 종료 좌표, 속도
            LinescanStart,              // 그랩 시작						/ 번호						/ 비전	/ Line
            LinescanEnd,                // 그랩 종료						/ Swath 번호				/ 제어	/ Line
            LinescanFinish,             // 자재 시퀀스 종료					/ Serial					/ 비전	/ Line

            #endregion

            #region Camera

            LiveOn,                          // Camera Live On
            LiveOff,                    // Camera Live Off
            Grab,                       // Camera Grab
            #endregion

            #region InspectType

            Align,                      // Align 검사 진행
            Barcode,                    // Barcode 검사 진행
            Measure,                    // 거리 측정 진행
            Result,

            #endregion
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
        /// 실제 모터 값 - Teach 
        /// </summary>
        [XmlElement("MotorValue")]
		public PointF MotorValue { get; set; }


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

		public CommandType GetCommandType()
		{
			if (this.Command == MessageCommand.MovePoint_Teaching)
				return CommandType.Move;
			else
				return CommandType.Etc;
		}
		public override string ToString()
		{
			string inputLotString = String.Empty;
			string inputToolString = String.Empty;

			return String.Format($"LotNumber : {LotNumber}");
		}

        [XmlElement("ScanPoint")]
        public List<PointF> ScanPoint { get; set; }

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
				MotorValue = this.MotorValue,
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
