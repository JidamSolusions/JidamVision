using JidamVision.Algorithm;
using JidamVision.Grab;
using JidamVision.Inspect;
using JidamVision.Sequence;
using JidamVision.Setting;
using JidamVision.Teach;
using JidamVision.Util;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JidamVision.Core
{
    //검사와 관련된 클래스를 관리하는 클래스
    public class InspStage : IDisposable
    {
        public static readonly int MAX_GRAB_BUF = 5;

        private ImageSpace _imageSpace = null;
        private GrabModel _grabManager = null;
        private CameraType _camType = CameraType.None;
        private PreviewImage _previewImage = null;

        //#INSP WORKER#6 InspWorker 변수 추가 
        private InspWorker _inspWorker = null;

        //#MODEL#6 모델 변수 선언
        private Model _model = null;

        private ImageLoader _imageLoader = null;

        private string _lotNumber;
        private string _serialID;

        public ImageSpace ImageSpace
        {
            get => _imageSpace;
        }

        public PreviewImage PreView
        {
            get => _previewImage;
        }

        //#INSP WORKER#7 InspWorker 프로퍼티 추가
        public InspWorker InspWorker
        {
            get => _inspWorker;
        }

        //#MODEL#7 모델 프로퍼티 만들기
        public Model CurModel
        {
            get => _model;
        }

        public ImageLoader ImageLoader
        {
            get
            {
                return _imageLoader;
            }
        }

        //#INSP WORKER#1 1개만 있던 InspWindow를 리스트로 변경하여, 여러개의 ROI를 관리하도록 개선
        public List<InspWindow> InspWindowList { get; set; } = new List<InspWindow>();

        public bool LiveMode { get; set; } = false;

        public int SelBufferIndex { get; set; } = 0;
        public eImageChannel SelImageChannel { get; set; } = eImageChannel.Gray;

        public bool UseCamera { get; set; } = false;

        public InspStage() { }

        public bool Initialize()
        {
            SLogger.Write("InspStage 초기화!");

            _imageSpace = new ImageSpace();
            _previewImage = new PreviewImage();
            _inspWorker = new InspWorker();

            //#MODEL#8 모델 인스턴스 생성
            _model = new Model();

            _imageLoader = new ImageLoader();

            //#SETUP#7 환경설정에서 설정값 가져오기
            LoadSetting();

            switch (_camType)
            {
                case CameraType.WebCam:
                    {
                        _grabManager = new WebCam();
                        break;
                    }
                case CameraType.HikRobotCam:
                    {
                        _grabManager = new HikRobotCam();
                        break;
                    }
                default:
                    {
                        SLogger.Write("Not supported camera type!", SLogger.LogType.Error);
                        return false;
                    }
            }

            if (_grabManager.InitGrab() == true)
            {
                _grabManager.TransferCompleted += _multiGrab_TransferCompleted;

                InitModelGrab(MAX_GRAB_BUF);
            }

            VisionSequence.Inst.InitSequence();
            VisionSequence.Inst.SeqCommand += SeqCommand;


            return true;
        }

        //#SETUP#6 환경설정에서 설정값 가져오기
        private void LoadSetting()
        {
            //카메라 설정 타입 얻기
            _camType = SettingXml.Inst.CamType;
        }

        public void InitModelGrab(int bufferCount)
        {
            if (_grabManager == null)
                return;

            int pixelBpp = 8;
            _grabManager.GetPixelBpp(out pixelBpp);

            int imageWidth;
            int imageHeight;
            int imageStride;
            _grabManager.GetResolution(out imageWidth, out imageHeight, out imageStride);

            if (_imageSpace != null)
            {
                _imageSpace.SetImageInfo(pixelBpp, imageWidth, imageHeight, imageStride);
            }

            SetBuffer(bufferCount);

            if (_camType == CameraType.HikRobotCam)
            {
                _grabManager.SetExposureTime(20000);
                _grabManager.SetGain(1.4f);
                _grabManager.Grab(0);

                _grabManager.SetWhiteBalance(true);
            }

        }
        public void SetImageBuffer(string filePath)
        {
            if (_grabManager == null)
                return;

            SLogger.Write($"Load Image : {filePath}");

            Mat matImage = Cv2.ImRead(filePath);

            int pixelBpp = 8;
            int imageWidth;
            int imageHeight;
            int imageStride;

            if (matImage.Type() == MatType.CV_8UC3)
                pixelBpp = 24;

            imageWidth = (matImage.Width + 3) / 4 * 4;
            imageHeight = matImage.Height;

            // 4바이트 정렬된 새로운 Mat 생성
            Mat alignedMat = new Mat();
            Cv2.CopyMakeBorder(matImage, alignedMat, 0, 0, 0, imageWidth - matImage.Width, BorderTypes.Constant, Scalar.Black);

            imageStride = imageWidth * matImage.ElemSize();

            if (_imageSpace != null)
            {
                _imageSpace.SetImageInfo(pixelBpp, imageWidth, imageHeight, imageStride);
            }

            SetBuffer(1);

            int bufferIndex = 0;

            // Mat의 데이터를 byte 배열로 복사
            int bufSize = (int)(alignedMat.Total() * alignedMat.ElemSize());
            Marshal.Copy(alignedMat.Data, ImageSpace.GetInspectionBuffer(bufferIndex), 0, bufSize);

            _imageSpace.Split(bufferIndex);

            DisplayGrabImage(bufferIndex);

            if (_previewImage != null)
            {
                Bitmap bitmap = ImageSpace.GetBitmap(0);
                _previewImage.SetImage(BitmapConverter.ToMat(bitmap));
            }
        }

        public void SetBuffer(int bufferCount)
        {
            if (_grabManager == null)
                return;

            _imageSpace.InitImageSpace(bufferCount);
            _grabManager.InitBuffer(bufferCount);

            for (int i = 0; i < bufferCount; i++)
            {
                _grabManager.SetBuffer(
                    _imageSpace.GetInspectionBuffer(i),
                    _imageSpace.GetnspectionBufferPtr(i),
                    _imageSpace.GetInspectionBufferHandle(i),
                    i);
            }

            SLogger.Write("버퍼 초기화 성공!");
        }

        public bool Grab(int bufferIndex)
        {
            if (_grabManager == null)
                return false;

            if(!_grabManager.Grab(bufferIndex, true))
                return false;

            return true;
        }

        // NOTE
        // async / await란?
        // async / await는 비동기 프로그래밍(Asynchronous Programming)을 쉽게 구현할 수 있도록 도와주는 키워드입니다.
        //기본 개념은 작업(Task)이 끝날 때까지 기다리지 않고 다른 작업을 진행할 수 있도록 하는 것입니다.
        //이를 통해 UI가 멈추지 않으며(프리징 방지), 응답성이 높은 프로그램을 만들 수 있습니다.
        private async void _multiGrab_TransferCompleted(object sender, object e)
        {
            int bufferIndex = (int)e;
            Console.WriteLine($"_multiGrab_TransferCompleted {bufferIndex}");

            _imageSpace.Split(bufferIndex);

            DisplayGrabImage(bufferIndex);

            if (_previewImage != null)
            {
                Bitmap bitmap = ImageSpace.GetBitmap(0);
                _previewImage.SetImage(BitmapConverter.ToMat(bitmap));
            }

            if (LiveMode)
            {
                SLogger.Write("Grab");
                await Task.Delay(100);  // 비동기 대기
                _grabManager.Grab(bufferIndex, true);  // 다음 촬영 시작
            }
        }

        private void DisplayGrabImage(int bufferIndex)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                if (cameraForm.InvokeRequired)
                {
                    cameraForm.Invoke((MethodInvoker)(() =>
                    {
                        cameraForm.UpdateDisplay();
                    }));
                }
                else
                {
                    cameraForm.UpdateDisplay();
                }
            }
        }

        public void SaveCurrentImage(string filePath)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                Mat displayImage = cameraForm.GetDisplayImage();
                Cv2.ImWrite(filePath, displayImage);
            }
        }

        public Bitmap GetBitmap(int bufferIndex = -1, eImageChannel imageChannel = eImageChannel.None)
        {
            if (bufferIndex >= 0)
                SelBufferIndex = bufferIndex;

            //#BINARY FILTER#13 채널 정보가 유지되도록, eImageChannel.None 타입을 추가
            if (imageChannel != eImageChannel.None)
                SelImageChannel = imageChannel;

            if (Global.Inst.InspStage.ImageSpace is null)
                return null;

            return Global.Inst.InspStage.ImageSpace.GetBitmap(SelBufferIndex, SelImageChannel);
        }
        public Mat GetMat(int bufferIndex = -1, eImageChannel imageChannel = eImageChannel.None)
        {
            if (bufferIndex >= 0)
                SelBufferIndex = bufferIndex;

            //#BINARY FILTER#14 채널 정보가 유지되도록, eImageChannel.None 타입을 추가
            if (imageChannel != eImageChannel.None)
                SelImageChannel = imageChannel;

            return Global.Inst.InspStage.ImageSpace.GetMat(SelBufferIndex, SelImageChannel);
        }


        public void TryInspection(InspWindow inspWindow)
        {
            InspWorker.TryInspect(inspWindow, InspectType.InspNone);
        }

        public void SelectInspWindow(InspWindow inspWindow)
        {
            var propForm = MainForm.GetDockForm<PropertiesForm>();
            if (propForm != null)
            {
                if (inspWindow is null)
                {
                    propForm.ResetProperty();
                    return;
                }

                propForm.ShowProperty(inspWindow);
            }

            UpdateProperty(inspWindow);

            Global.Inst.InspStage.PreView.SetInspWindow(inspWindow);
        }

        //#MODEL#9 ImageViwer에서 ROI를 추가하여, InspWindow생성하는 함수
        public void AddInspWindow(InspWindowType windowType, Rect rect)
        {
            InspWindow inspWindow = _model.AddInspWindow(windowType);
            if (inspWindow is null)
                return;

            inspWindow.WindowArea = rect;
            inspWindow.IsTeach = false;
            SetTeachingImage(inspWindow);
            UpdateProperty(inspWindow);
            UpdateDiagramEntity();

            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.SelectDiagramEntity(inspWindow);
                SelectInspWindow(inspWindow);
            }
        }

        //입력된 윈도우 이동
        public void MoveInspWindow(InspWindow inspWindow, OpenCvSharp.Point offset)
        {
            if (inspWindow == null)
                return;

            //그룹이 있다면 해당 그룹을 이동
            GroupWindow group = (GroupWindow)inspWindow.Parent;
            if (group != null)
                group.OffsetMove(offset);
            else
            {
                inspWindow.OffsetMove(offset);
                UpdateProperty(inspWindow);
            }
        }

        //#MODEL#10 기존 ROI 수정되었을때, 그 정보를 InspWindow에 반영
        public void ModifyInspWindow(InspWindow inspWindow, Rect rect)
        {
            if (inspWindow == null)
                return;

            inspWindow.WindowArea = rect;
            inspWindow.IsTeach = false;
            SetTeachingImage(inspWindow);

            UpdateProperty(inspWindow);
        }

        //#MODEL#11 InspWindow 삭제하기
        public void DelInspWindow(InspWindow inspWindow)
        {
            _model.DelInspWindow(inspWindow);
            UpdateDiagramEntity();
        }


        public void DelInspWindow(List<InspWindow> inspWindowList)
        {
            _model.DelInspWindowList(inspWindowList);
            UpdateDiagramEntity();
        }

        //GroupWindow 생성
        public void CreateGroupWindow(List<InspWindow> inspWindowList)
        {
            if (_model is null)
                return;

            _model.AddGroupWindow(inspWindowList);

            UpdateDiagramEntity();
        }

        //GroupWindow 해제
        public void BreakGroupWindow(InspWindow window)
        {
            if (window is null)
                return;

            GroupWindow group = null;
            if (window.InspWindowType == InspWindowType.Group)
            {
                group = (GroupWindow)window;
            }
            else
            {
                group = (GroupWindow)window.Parent;
            }

            if (group == null)
            {
                MessageBox.Show("그룹윈도우가 아닙니다!");
                return;
            }

            _model.BreakGroupWindow(group);
            UpdateDiagramEntity();
        }

        private void UpdateProperty(InspWindow inspWindow)
        {
            if (inspWindow is null)
                return;

            PropertiesForm propertiesForm = MainForm.GetDockForm<PropertiesForm>();
            if (propertiesForm is null)
                return;

            propertiesForm.UpdateProperty(inspWindow);
        }

        public void SetTeachingImage(InspWindow inspWindow)
        {
            if (inspWindow is null)
                return;

            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm is null)
                return;

            Mat curImage = cameraForm.GetDisplayImage();
            if (curImage is null)
                return;

            Mat windowImage = curImage[inspWindow.WindowArea];
            inspWindow.WindowImage = windowImage;

            MatchAlgorithm matchAlgo = (MatchAlgorithm)inspWindow.FindInspAlgorithm(InspectType.InspMatch);
            if (matchAlgo != null)
            {
                matchAlgo.SetTemplateImage(windowImage);
            }
        }

        //#MODEL#15 변경된 모델 정보 갱신하여, ImageViewer와 모델트리에 반영
        public void UpdateDiagramEntity()
        {
            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.UpdateDiagramEntity();
            }

            ModelTreeForm modelTreeForm = MainForm.GetDockForm<ModelTreeForm>();
            if (modelTreeForm != null)
            {
                modelTreeForm.UpdateDiagramEntity();
            }
        }

        public void RedrawMainView()
        {
            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.UpdateImageViewer();
            }
        }

        //#MODEL SAVE#3 Mainform에서 호출되는 모델 열기와 저장 함수
        public bool LoadModel(string filePath)
        {
            SLogger.Write($"모델 로딩:{filePath}");

            _model = _model.Load(filePath);

            if (_model is null)
            {
                SLogger.Write($"모델 로딩 실패:{filePath}");
                return false;
            }

            string inspImagePath = _model.InspectImagePath;
            if (File.Exists(inspImagePath))
            {
                Global.Inst.InspStage.SetImageBuffer(inspImagePath);
            }

            UpdateDiagramEntity();

            return true;
        }

        public void SaveModel(string filePath)
        {
            SLogger.Write($"모델 저장:{filePath}");

            //입력 경로가 없으면 현재 모델 저장
            if (string.IsNullOrEmpty(filePath))
                Global.Inst.InspStage.CurModel.Save();
            else
                Global.Inst.InspStage.CurModel.SaveAs(filePath);
        }

        public void CycleInspect(bool isCycle)
        {
            string inspImagePath = CurModel.InspectImagePath;
            if (inspImagePath == "")
                return;

            string inspImageDir = Path.GetDirectoryName(inspImagePath);
            if (!Directory.Exists(inspImageDir))
                return;

            if (!_imageLoader.IsLoadedImages())
                _imageLoader.LoadImages(inspImageDir);

            if (isCycle)
                _inspWorker.StartCycleInspectImage();
            else
                OneCycle();
        }

        public bool OneCycle()
        {
            if(UseCamera)
            {
                if(!Grab(0))
                    return false;
            }
            else
            {
                if (!VirtualGrab())
                    return false;
            }

            bool isDefect = false;
            if (!_inspWorker.RunInspect(out isDefect))
                return false;

            return true;
        }

        public void StopCycle()
        {
            if (_inspWorker != null)
                _inspWorker.Stop();

            VisionSequence.Inst.StopAutoRun();
        }

        public bool VirtualGrab()
        {
            if (_imageLoader is null)
                return false;

            string imagePath = _imageLoader.GetNextImagePath();
            if (imagePath == "")
                return false;

            Global.Inst.InspStage.SetImageBuffer(imagePath);

            _imageSpace.Split(0);

            DisplayGrabImage(0);

            return true;
        }

        private void SeqCommand(object sender, SeqCmd seqCmd, object Param)
        {
            switch (seqCmd)
            {
                //case SeqCmd.OpenRecipe:
                //    {
                //        SLogger.Write("MMI : OpenRecipe", SLogger.LogType.Info);

                //        string modelName = (string)Param;
                //        string modelPath = Path.Combine(SettingXml.Inst.ModelDir, modelName, modelName + ".xml");

                //        string errMsg = "";

                //        if (File.Exists(modelPath))
                //        {
                //            if (!LoadModel(modelPath))
                //                errMsg = "모델 열기 실패!";
                //        }
                //        else
                //        {
                //            errMsg = $"{modelName}이 존재하지 않습니다!";
                //        }

                //        VisionSequence.Inst.VisionCommand(Vision2Mmi.ModeLoaded, errMsg);
                //    }
                //    break;
                //case SeqCmd.InspReady:
                //    {
                //        SLogger.Write("MMI : InspReady", SLogger.LogType.Info);

                //        //검사 모드 진입
                //        string errMsg = "";

                //        if(Param != null)
                //        {
                //            MessagingLibrary.Message msg = (MessagingLibrary.Message)Param;
                //            if (!InspectReady(msg.LotNumber, msg.SerialID))
                //            {
                //                errMsg = string.Format("Inspection not ready");
                //                SLogger.Write(errMsg, SLogger.LogType.Error);
                //            }
                //        }

                //        VisionSequence.Inst.VisionCommand(Vision2Mmi.InspReady, errMsg);
                //    }
                //    break;
                case SeqCmd.InspStart:
                    {
                        //#WCF_FSM#5 카메라 촬상 후, 검사 진행
                        SLogger.Write("MMI : InspStart", SLogger.LogType.Info);

                        //검사 시작
                        string errMsg = "";
                        
                        if (UseCamera)
                        {
                            if (!Grab(0))
                            {
                                errMsg = string.Format("Failed to grab");
                                SLogger.Write(errMsg, SLogger.LogType.Error);
                            }
                        }

                        bool isDefect = false;
                        if (!_inspWorker.RunInspect(out isDefect))
                        {
                            errMsg = string.Format("Failed to inspect");
                            SLogger.Write(errMsg, SLogger.LogType.Error);
                        }

                        //#WCF_FSM#6 비젼 -> 제어에 검사 완료 및 결과 전송
                        VisionSequence.Inst.VisionCommand(Vision2Mmi.InspDone, isDefect);
                    }
                    break;
                case SeqCmd.InspEnd:
                    {
                        SLogger.Write("MMI : InspEnd", SLogger.LogType.Info);

                        //모든 검사 종료
                        string errMsg = "";

                        //검사 완료에 대한 처리
                        SLogger.Write("검사 종료");

                        VisionSequence.Inst.VisionCommand(Vision2Mmi.InspEnd, errMsg);
                    }
                    break;
            }
        }

        //검사를 위한 준비 작업
        private bool InspectReady(string lotNumber, string serialID)
        {
            _lotNumber = lotNumber;
            _serialID = serialID;

            LiveMode = false;
            UseCamera = SettingXml.Inst.CamType != CameraType.None ? true : false;

            return true;
        }

        public bool StartAutoRun()
        {
            SLogger.Write("Action : StartAutoRun");

            string modelPath = CurModel.ModelPath;
            if (modelPath == "")
            {
                SLogger.Write("열려진 모델이 없습니다!", SLogger.LogType.Error);
                MessageBox.Show("열려진 모델이 없습니다!");
                return false;
            }

            if (_grabManager is null)
            {
                SLogger.Write("카메라가 설정되지 않았습니다!", SLogger.LogType.Error);
                MessageBox.Show("카메라가 설정되지 않았습니다!");
                return false;
            }

            LiveMode = false;
            UseCamera = SettingXml.Inst.CamType != CameraType.None ? true : false;

            string modelName = Path.GetFileNameWithoutExtension(modelPath);
            VisionSequence.Inst.StartAutoRun(modelName);
            return true;
        }

        #region Disposable

        private bool disposed = false; // to detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    VisionSequence.Inst.SeqCommand -= SeqCommand;

                    if (_imageSpace != null)
                        _imageSpace.Dispose();

                    if (_imageLoader != null)
                        _imageLoader.Dispose();
                }

                // Dispose unmanaged managed resources.

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion //Disposable
    }
}
