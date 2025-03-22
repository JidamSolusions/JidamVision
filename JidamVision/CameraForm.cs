using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using JidamVision.Core;
using OpenCvSharp.Extensions;
using System.Web;
using JidamVision.Teach;
using System.IO;
using OpenCvSharp;

namespace JidamVision
{
    public partial class CameraForm : DockContent
    {
        //# SAVE ROI#1 현재 선택된 이미지 채널 저장을 위한 변수
        eImageChannel _currentImageChannel = eImageChannel.Color;

        public CameraForm()
        {
            InitializeComponent();

            imageViewer.ModifyROI += ImageViewer_ModifyROI;
            rbtnColor.Checked = true;
        }

        private void ImageViewer_ModifyROI(object sender, DiagramEntityEventArgs e)
        {
            switch (e.ActionType)
            {
                case EntityActionType.Add:
                    Global.Inst.InspStage.AddInspWindow(e.WindowType, e.Rect);
                    break;

                case EntityActionType.Modify:
                    Global.Inst.InspStage.ModifyInspWindow(e.InspWindow, e.Rect);
                    break;

                case EntityActionType.Delete:
                    Global.Inst.InspStage.DelInspWindow(e.InspWindow);
                    break;
            }
        }

        //# SAVE ROI#2 GUI상에서 선택된 채널 라디오 버튼에 따른 채널 정보를 반환
        private eImageChannel GetCurrentChannel()
        {
            if (rbtnRedChannel.Checked)
            {
                return eImageChannel.Red;
            }
            else if (rbtnBlueChannel.Checked)
            {
                return eImageChannel.Blue;
            }
            else if (rbtnGreenChannel.Checked)
            {
                return eImageChannel.Green;
            }
            else if (rbtnGrayChannel.Checked)
            {
                return eImageChannel.Gray;
            }

            return eImageChannel.Color;
        }

        public void UpdateDisplay(Bitmap bitmap = null)
        {
            if (bitmap == null)
            {
                //# SAVE ROI#3 채널 정보 변수에 저장
                //참고 프로젝트에서 _currentImageChannel를 모두 찾아서, 수정할것
                _currentImageChannel = GetCurrentChannel();
                bitmap = Global.Inst.InspStage.GetBitmap(0, _currentImageChannel);
                if (bitmap == null)
                    return;
            }

            imageViewer.LoadBitmap(bitmap);

            //#BINARY FILTER#12 이진화 프리뷰에서 각 채널별로 설정이 적용되도록, 현재 이미지를 프리뷰 클래스 설정
            //현재 선택된 이미지로 Previwe이미지 갱신
            Mat curImage = Global.Inst.InspStage.GetMat();
            Global.Inst.InspStage.PreView.SetImage(curImage);
        }

        public OpenCvSharp.Mat GetDisplayImage()
        {
            return Global.Inst.InspStage.ImageSpace.GetMat(0, _currentImageChannel);
        }

        private void CameraForm_Resize(object sender, EventArgs e)
        {
            int margin = 10;

            int xPos = Location.X + this.Width - btnGrab.Width - margin;

            btnGrab.Location = new System.Drawing.Point(xPos, btnGrab.Location.Y);
            btnLive.Location = new System.Drawing.Point(xPos, btnLive.Location.Y);
            btnSetRoi.Location = new System.Drawing.Point(xPos, btnSetRoi.Location.Y);
            btnSave.Location = new System.Drawing.Point(xPos, btnSave.Location.Y);
            btnInspect.Location = new System.Drawing.Point(xPos, btnInspect.Location.Y);
            groupBox1.Location = new System.Drawing.Point(xPos, groupBox1.Location.Y);

            imageViewer.Width = this.Width - btnGrab.Width - margin * 2;
            imageViewer.Height = this.Height - margin * 2;

            imageViewer.Location = new System.Drawing.Point(margin, margin);
        }

        private void btnGrab_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.Grab(0);
        }

        private void btnLive_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.LiveMode = !Global.Inst.InspStage.LiveMode;

            if (Global.Inst.InspStage.LiveMode)
                Global.Inst.InspStage.Grab(0);
        }

        private void CameraForm_Load(object sender, EventArgs e)
        {

        }

        private void rbtnColor_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void rbtnRedChannel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void rbtnBlueChannel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void rbtnGreenChannel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void rbtnGrayChannel_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDisplay();
        }

        /*
         #SETROI# - <<<ROI 설정 개발>>> 
        이미지 상에서 ROI(Region of Interest)를 설정하는 기능
         */
        private void btnSetRoi_Click(object sender, EventArgs e)
        {
            //#SETROI#2 ROI 모드 토글 설정
            imageViewer.RoiMode = !imageViewer.RoiMode;
            imageViewer.Invalidate();
        }

        /*
         #SAVE ROI# - <<<ROI 영역 이미지 파일 저장>>> 
        이미지 상에서 ROI 영역을 파일로 저장하여, 템플릿 매칭에서 사용
        */
        private void btnSave_Click(object sender, EventArgs e)
        {
            //# SAVE ROI#5 현재 채널 이미지에서, 설정된 ROI 영역을 파일로 저장
            OpenCvSharp.Mat currentImage = Global.Inst.InspStage.GetMat(0, _currentImageChannel);
            if(currentImage != null )
            {
                //현재 설정된 ROI 영역을 가져옴
                Rectangle roiRect = imageViewer.GetRoiRect();
                if (roiRect.IsEmpty == true)
                    return;

                //전체 이미지에서 ROI 영역만을 roiImage에 저장
                Mat roiImage = new Mat(currentImage, new Rect(roiRect.X, roiRect.Y, roiRect.Width, roiRect.Height));

                //현재 실행파일이 있는 경로에, 저장할 경로 만들기
                string savePath = Path.Combine(Directory.GetCurrentDirectory(), Define.ROI_IMAGE_NAME);
                //이미지 저장
                Cv2.ImWrite(savePath, roiImage);
            }
        }

        //#MATCH PROP#14 템플릿 매칭 위치 입력 받는 함수
        public void AddRect(List<Rect> rects)
        {
            //#BINARY FILTER#18 imageViewer는 Rectangle 타입으로 그래픽을 그리므로, 
            //아래 코드를 이용해, Rect -> Rectangle로 변환하는 람다식
            var rectangles = rects.Select(r => new Rectangle(r.X, r.Y, r.Width, r.Height)).ToList();
            imageViewer.AddRect(rectangles);
            
        }

        //#INSP WORKER#8 CaearaForm에 검사 버튼을 추가하고, 전체 검사 함수 추가
        private void btnInspect_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.InspWorker.RunInspect();
        }

        public void AddRoi(InspWindowType inspWindowType)
        {
            imageViewer.NewRoi(inspWindowType);
        }

        //#MODEL#13 모델 정보를 이용해, ROI 갱신
        public void UpdateDiagramEntity()
        {
            Model model = Global.Inst.InspStage.CurModel;
            List<InspWindow> windowList = model.InspWindowList;
            if (windowList.Count <= 0)
                return;

            List<DiagramEntity> diagramEntityList = new List<DiagramEntity>();

            foreach (InspWindow window in model.InspWindowList)
            {
                DiagramEntity diagramEntity = new DiagramEntity();
                Rect rect = window.WindowArea;
                diagramEntity.LinkedWindow = window;
                diagramEntity.EntityROI = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                diagramEntity.EntityColor = imageViewer.GetWindowColor(window.InspWindowType);
                diagramEntityList.Add(diagramEntity);
            }

            imageViewer.SetDiagramEntityList(diagramEntityList);
        }
    }
}
