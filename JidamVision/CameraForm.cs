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
using JidamVision.Util;
using System.Diagnostics.Eventing.Reader;

namespace JidamVision
{
    public partial class CameraForm : DockContent
    {
        //# SAVE ROI#1 현재 선택된 이미지 채널 저장을 위한 변수
        eImageChannel _currentImageChannel = eImageChannel.Color;

        public CameraForm()
        {
            InitializeComponent();

            this.FormClosed += CameraForm_FormClosed;

            imageViewer.DiagramEntityEvent += ImageViewer_DiagramEntityEvent;
            rbtnColor.Checked = true;
        }

        private void ImageViewer_DiagramEntityEvent(object sender, DiagramEntityEventArgs e)
        {
            SLogger.Write($"ImageViewer Action {e.ActionType.ToString()}");
            switch (e.ActionType)
            {
                case EntityActionType.Select:
                    Global.Inst.InspStage.SelectInspWindow(e.InspWindow);
                    imageViewer.Focus();
                    break;
                case EntityActionType.Inspect:
                    Global.Inst.InspStage.TryInspection(e.InspWindow);
                    break;
                case EntityActionType.Add:
                    Global.Inst.InspStage.AddInspWindow(e.WindowType, e.Rect);
                    break;
                case EntityActionType.Move:
                    Global.Inst.InspStage.MoveInspWindow(e.InspWindow, e.OffsetMove);
                    break;
                case EntityActionType.Resize:
                    Global.Inst.InspStage.ModifyInspWindow(e.InspWindow, e.Rect);
                    break;
                case EntityActionType.Delete:
                    Global.Inst.InspStage.DelInspWindow(e.InspWindow);
                    break;
                case EntityActionType.DeleteList:
                    Global.Inst.InspStage.DelInspWindow(e.InspWindowList);
                    break;
                case EntityActionType.AddGroup:
                    Global.Inst.InspStage.CreateGroupWindow(e.InspWindowList);
                    break;
                case EntityActionType.Break:
                    Global.Inst.InspStage.BreakGroupWindow(e.InspWindow);
                    break;
                case EntityActionType.UpdateImage:
                    Global.Inst.InspStage.SetTeachingImage(e.InspWindow);
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
            btnAutoRun.Location = new System.Drawing.Point(xPos, btnAutoRun.Location.Y);
            btnInspect.Location = new System.Drawing.Point(xPos, btnInspect.Location.Y);
            btnStop.Location = new System.Drawing.Point(xPos, btnStop.Location.Y);
            chkCycle.Location = new System.Drawing.Point(xPos, chkCycle.Location.Y);
            chkPreview.Location = new System.Drawing.Point(xPos, chkPreview.Location.Y);
            chkShowROI.Location = new System.Drawing.Point(xPos, chkShowROI.Location.Y);

            int yPos = this.Height - margin - groupBox1.Height;
            groupBox1.Location = new System.Drawing.Point(xPos, yPos);

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

        #region Select Channel
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
        #endregion

        //#MATCH PROP#14 템플릿 매칭 위치 입력 받는 함수
        public void AddRect(List<Rect> rects)
        {
            //#BINARY FILTER#18 imageViewer는 Rectangle 타입으로 그래픽을 그리므로, 
            //아래 코드를 이용해, Rect -> Rectangle로 변환하는 람다식
            var rectangles = rects.Select(r => new Rectangle(r.X, r.Y, r.Width, r.Height)).ToList();
            imageViewer.AddRect(rectangles);

        }

        public void AddRoi(InspWindowType inspWindowType)
        {
            imageViewer.NewRoi(inspWindowType);
        }

        //#MODEL#13 모델 정보를 이용해, ROI 갱신
        public void UpdateDiagramEntity()
        {
            Model model = Global.Inst.InspStage.CurModel;
            List<DiagramEntity> diagramEntityList = new List<DiagramEntity>();

            foreach (InspWindow window in model.InspWindowList)
            {
                if (window is null)
                    continue;

                if (window is GroupWindow group)
                {
                    foreach (InspWindow member in group.Members)
                    {
                        DiagramEntity entity = new DiagramEntity()
                        {
                            LinkedWindow = member,
                            EntityROI = new Rectangle(
                                member.WindowArea.X, member.WindowArea.Y,
                                member.WindowArea.Width, member.WindowArea.Height),
                            EntityColor = imageViewer.GetWindowColor(member.InspWindowType),
                            IsHold = member.IsTeach,
                        };
                        diagramEntityList.Add(entity);
                    }
                }
                else if (window.Parent == null)
                {
                    DiagramEntity entity = new DiagramEntity()
                    {
                        LinkedWindow = window,
                        EntityROI = new Rectangle(
                            window.WindowArea.X, window.WindowArea.Y,
                                window.WindowArea.Width, window.WindowArea.Height),
                        EntityColor = imageViewer.GetWindowColor(window.InspWindowType),
                        IsHold = window.IsTeach
                    };
                    diagramEntityList.Add(entity);
                }
            }

            imageViewer.SetDiagramEntityList(diagramEntityList);
        }
        public void SelectDiagramEntity(InspWindow window)
        {
            imageViewer.SelectDiagramEntity(window);
        }

        public void UpdateImageViewer()
        {
            imageViewer.Invalidate();
        }

        private void CameraForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            imageViewer.DiagramEntityEvent -= ImageViewer_DiagramEntityEvent;

            this.FormClosed -= CameraForm_FormClosed;
        }

        //#INSP WORKER#8 CaearaForm에 검사 버튼을 추가하고, 전체 검사 함수 추가
        private void btnInspect_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.CycleInspect(chkCycle.Checked);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.StopCycle();

        }

        private void chkPreview_CheckedChanged(object sender, EventArgs e)
        {
            Global.Inst.InspStage.PreView.SetPreview(chkPreview.Checked);
        }

        private void chkShowROI_CheckedChanged(object sender, EventArgs e)
        {
            if(chkShowROI.Checked)
            {
                UpdateDiagramEntity();
            }
            else
            {
                imageViewer.ResetEntity();
            }
        }

        private void btnAutoRun_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.StartAutoRun();
        }
    }
}
