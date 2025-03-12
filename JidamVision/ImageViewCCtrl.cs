using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace JidamVision
{
    public partial class ImageViewCCtrl : UserControl
    {
        private System.Drawing.Point _roiStart = System.Drawing.Point.Empty;
        private Rectangle _roiRect = Rectangle.Empty;
        private bool _isSelectingRoi = false;
        private bool _isResizingRoi = false;
        private bool _isMovingRoi = false;
        private System.Drawing.Point _resizeStart = System.Drawing.Point.Empty;
        private System.Drawing.Point _moveStart = System.Drawing.Point.Empty;
        private int _resizeDirection = -1;
        private const int _ResizeHandleSize = 10;

        // 마우스 클릭 위치 저장
        private System.Drawing.Point RightClick = System.Drawing.Point.Empty;

        // 현재 이미지 이동을 위한 오프셋 값
        private System.Drawing.Point Offset = System.Drawing.Point.Empty;

        // 마지막 오프셋 값을 저장하여 마우스 이동을 연속적으로 처리
        private System.Drawing.Point LastOffset = new System.Drawing.Point(0, 0);

        // 현재 로드된 이미지
        private Bitmap Bitmap = null;

        // 더블 버퍼링을 위한 캔버스
        // 더블버퍼링 : 화면 깜빡임을 방지하고 부드러운 펜더링위해 사용
        // Onpaint() 메서드는 화면을 즉시 그리는 방식 사용
        // 사용자가 빠르게 작동할 경우 Flickering 발생
        private Bitmap Canvas = null;

        // 캔버스 크기
        private Rectangle CanvasSize = new Rectangle(0, 0, 0, 0);

        // 화면에 표시될 이미지의 크기 및 위치
        // 부동 소수점(float) 좌표를 사용하는 사각형 구조체
        private RectangleF ImageRect = new RectangleF(0, 0, 0, 0);

        // 현재 줌 배율
        private float ZoomFactor = 1.0f;

        // 최소 및 최대 줌 제한 값
        private const float MinZoom = 1f;
        private const float MaxZoom = 200.0f;

        //줌아웃위하 초기값
        private float InitialCenterX;  // 초기 이미지 중심 X
        private float InitialCenterY;  // 초기 이미지 중심 Y
        private float InitialStartX;    //resize 위한 초기 X값 저장
        private float InitialStartY;    //resize 위한 초기 Y값 저장
        private float InitialWidth;    // 초기 이미지 너비
        private float InitialHeight;   // 초기 이미지 높이

        //#MATCH PROP#11 템플릿 매칭 결과 출력을 위해 Rectangle 리스트 변수 설정
        private List<Rectangle> _rectangles = new List<Rectangle>();

        //#SETROI#1 ROI 그리기 모드
        public bool RoiMode { get; set; } = false;

        public ImageViewCCtrl()
        {
            InitializeComponent();
            InitializeCanvas();

            // 마우스 휠 이벤트를 등록하여 줌 기능 추가
            // MouseWheel+= : 같은 이벤트에 여러 개의 핸들러 등록가능. 이전 핸들러 삭제않고 추가됨
            // UserControl1_MouseWheel 메서드를 MouseEventHandler 델리게이트(delegate) 형식으로 변환
            // MouseWheel += UserControl1_MouseWheel; 델리게이트 직접 지정없이해도 자동변환됨
            MouseWheel += new MouseEventHandler(ImageViewCCtrl_MouseWheel);
        }

        private void InitializeCanvas()
        {
            // 캔버스를 UserControl 크기만큼 생성
            ResizeCanas();

            // 화면 깜빡임을 방지하기 위한 더블 버퍼링 설정
            DoubleBuffered = true;
        }

        private void ResizeCanas()
        {
            // 캔버스를 UserControl 크기만큼 생성
            Canvas = new Bitmap(Width, Height);
            CanvasSize.Width = Width;
            CanvasSize.Height = Height;

            if (Bitmap == null) return;

            // UserControl 크기에 맞춰 이미지 비율 유지하여 크기 조정
            float WidthRatio = (float)Width / Bitmap.Width;  //UserControl1 Width/Bitmap.Width
            float HeightRatio = (float)Height / Bitmap.Height;
            float Scale = Math.Min(WidthRatio, HeightRatio); // 더 작은 값을 선택하여 비율 유지

            float NewWidth = Bitmap.Width * Scale;
            float NewHeight = Bitmap.Height * Scale;

            if (InitialStartX == 0 || InitialStartY == 0)
            {
                InitialStartX = ImageRect.X;
                InitialStartY = ImageRect.Y;
            }

            // 이미지가 UserControl 중앙에 배치되도록 정렬
            ImageRect = new RectangleF(
                (Width - NewWidth) / 2, // UserControl 너비에서 이미지 너비를 뺀 후, 절반을 왼쪽 여백으로 설정하여 중앙 정렬
                (Height - NewHeight) / 2,
                NewWidth,
                NewHeight
            );

            UpdateROI();

            //줌아웃위한 초기값 저장
            InitialCenterX = ImageRect.X + (ImageRect.Width / 2);
            InitialCenterY = ImageRect.Y + (ImageRect.Height / 2);

            InitialStartX = ImageRect.X;
            InitialStartY = ImageRect.Y;
            InitialWidth = NewWidth;
            InitialHeight = NewHeight;
        }

        public void LoadBitmap(Bitmap bitmap)
        {
            // 기존에 로드된 이미지가 있다면 해제 후 초기화, 메모리누수 방지
            if (Bitmap != null)
            {
                Bitmap.Dispose(); // Bitmap 객체가 사용하던 메모리 리소스를 해제
                Bitmap = null;  //객체를 해제하여 가비지 컬렉션(GC)이 수집할 수 있도록 설정
            }

            // 새로운 이미지 로드
            Bitmap = bitmap;


            // UserControl 크기에 맞춰 이미지 비율 유지하여 크기 조정
            float WidthRatio = (float)Width / Bitmap.Width;  //UserControl1 Width/Bitmap.Width
            float HeightRatio = (float)Height / Bitmap.Height;
            float Scale = Math.Min(WidthRatio, HeightRatio); // 더 작은 값을 선택하여 비율 유지

            float NewWidth = Bitmap.Width * Scale;
            float NewHeight = Bitmap.Height * Scale;

            // 이미지가 UserControl 중앙에 배치되도록 정렬
            ImageRect = new RectangleF(
                (Width - NewWidth) / 2, // UserControl 너비에서 이미지 너비를 뺀 후, 절반을 왼쪽 여백으로 설정하여 중앙 정렬
                (Height - NewHeight) / 2,
                NewWidth,
                NewHeight
            );

            //줌아웃위한 초기값 저장
            InitialCenterX = ImageRect.X + (ImageRect.Width / 2);
            InitialCenterY = ImageRect.Y + (ImageRect.Height / 2);
            InitialWidth = NewWidth;
            InitialHeight = NewHeight;

            // 줌 초기화
            ZoomFactor = 1.0f;

            // 이미지 이동을 위한 오프셋 값 초기화
            Offset = new System.Drawing.Point((int)ImageRect.X, (int)ImageRect.Y);  //이미지 왼쪽상단(Top-Left)의 시작 좌표
            LastOffset = Offset;

            // 변경된 화면을 다시 그리도록 요청
            Invalidate();
        }

        public void LoadImage(string path)
        {
            // 기존에 로드된 이미지가 있다면 해제 후 초기화, 메모리누수 방지
            if (Bitmap != null)
            {
                Bitmap.Dispose(); // Bitmap 객체가 사용하던 메모리 리소스를 해제
                Bitmap = null;  //객체를 해제하여 가비지 컬렉션(GC)이 수집할 수 있도록 설정
            }

            // 새로운 이미지 로드
            Bitmap = (Bitmap)Image.FromFile(path);

            // UserControl 크기에 맞춰 이미지 비율 유지하여 크기 조정
            float WidthRatio = (float)Width / Bitmap.Width;  //UserControl1 Width/Bitmap.Width
            float HeightRatio = (float)Height / Bitmap.Height;
            float Scale = Math.Min(WidthRatio, HeightRatio); // 더 작은 값을 선택하여 비율 유지

            float NewWidth = Bitmap.Width * Scale;
            float NewHeight = Bitmap.Height * Scale;

            // 이미지가 UserControl 중앙에 배치되도록 정렬
            ImageRect = new RectangleF(
                (Width - NewWidth) / 2, // UserControl 너비에서 이미지 너비를 뺀 후, 절반을 왼쪽 여백으로 설정하여 중앙 정렬
                (Height - NewHeight) / 2,
                NewWidth,
                NewHeight
            );

            // 줌 초기화
            ZoomFactor = 1.0f;

            // 이미지 이동을 위한 오프셋 값 초기화
            Offset = new System.Drawing.Point((int)ImageRect.X, (int)ImageRect.Y);  //이미지 왼쪽상단(Top-Left)의 시작 좌표
            LastOffset = Offset;

            // 변경된 화면을 다시 그리도록 요청
            Invalidate();
        }

        public Bitmap GetRoiImage()
        {
            if (Bitmap == null || _roiRect.IsEmpty)
                return null;

            // 원본 이미지에서 ROI 크롭
            Bitmap roiBitmap = new Bitmap(_roiRect.Width, _roiRect.Height);
            using (Graphics g = Graphics.FromImage(roiBitmap))
            {
                g.DrawImage(Bitmap, new Rectangle(0, 0, _roiRect.Width, _roiRect.Height), _roiRect, GraphicsUnit.Pixel);
            }

            return roiBitmap;
        }

        public void SaveROI(string savePath)
        {
            if (Bitmap == null || _roiRect.IsEmpty)
                return;

            // 원본 이미지에서 ROI 크롭
            using (Bitmap roiBitmap = new Bitmap(_roiRect.Width, _roiRect.Height))
            {
                using (Graphics g = Graphics.FromImage(roiBitmap))
                {
                    g.DrawImage(Bitmap, new Rectangle(0, 0, _roiRect.Width, _roiRect.Height), _roiRect, GraphicsUnit.Pixel);
                }
                roiBitmap.Save(savePath, ImageFormat.Png);
            }
        }

        // Windows Forms에서 컨트롤이 다시 그려질 때 자동으로 호출되는 메서드
        // 화면새로고침(Invalidate()), 창 크기변경, 컨트롤이 숨겨졌다가 나타날때 실행
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Bitmap != null)
            {
                // 캔버스를 초기화하고 이미지 그리기
                using (Graphics g = Graphics.FromImage(Canvas))  // 메모리누수방지
                {
                    g.Clear(Color.Transparent); // 배경을 투명하게 설정

                    //이미지 확대or축소때 화질 최적화 방식(Interpolation Mode) 설정                    
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.DrawImage(Bitmap, ImageRect);

                    /* Interpolation Mode********************************************
                     * NearestNeighbor	빠르지만 품질이 낮음 (픽셀이 깨질 수 있음)
                     * Bicubic	Bilinear보다 더 부드러움, 그러나 속도가 느릴 수 있음
                     * HighQualityBicubic	가장 부드럽고 고품질, 그러나 가장 느림
                     * HighQualityBilinear	Bilinear보다 품질이 높고 Bicubic보다 빠름
                     ****************************************************************/

                    //#MATCH PROP#12 템플릿 매칭 위치 그리기
                    float scaleX = ImageRect.Width / Bitmap.Width;
                    float scaleY = ImageRect.Height / Bitmap.Height;

                    // 이미지 좌표 → 화면 좌표 변환 후 사각형 그리기
                    if (_rectangles != null)
                    {
                        using (Pen pen = new Pen(Color.LightCoral, 2))
                        {
                            foreach (var rect in _rectangles)
                            {
                                // 이미지 좌표를 화면 좌표로 변환
                                int screenX = (int)(rect.X * scaleX + ImageRect.X);
                                int screenY = (int)(rect.Y * scaleY + ImageRect.Y);
                                int screenWidth = (int)(rect.Width * scaleX);
                                int screenHeight = (int)(rect.Height * scaleY);

                                g.DrawRectangle(pen, screenX, screenY, screenWidth, screenHeight);
                            }
                        }
                    }

                    //#SETROI#6 ROI 그리기
                    if (RoiMode && !_roiRect.IsEmpty)
                    {
                        Rectangle rect = _roiRect;
                        using (Pen pen = new Pen(Color.LightGreen, 2))
                        {
                            g.DrawRectangle(pen, rect);
                        }

                        // 리사이즈 핸들 그리기 (8개 포인트: 4 모서리 + 4 변 중간)
                        using (Brush brush = new SolidBrush(Color.LightBlue))
                        {
                            System.Drawing.Point[] resizeHandles = GetResizeHandles(rect);
                            foreach (System.Drawing.Point handle in resizeHandles)
                            {
                                g.FillRectangle(brush, handle.X - _ResizeHandleSize / 2, handle.Y - _ResizeHandleSize / 2, _ResizeHandleSize, _ResizeHandleSize);
                            }
                        }
                    }

                    // 캔버스를 UserControl 화면에 표시
                    e.Graphics.DrawImage(Canvas, 0, 0);
                }
            }
        }

        private void ImageViewCCtrl_MouseDown(object sender, MouseEventArgs e)
        {
            //#SETROI#3 ROI 모드에서 ROI 그리기 시작 또는 ROI 크기 이동/변경 모드 설정
            if (RoiMode && e.Button == MouseButtons.Left)
            {
                //마우스 클릭 위치가 ROI 크기 변경을 하기 위한 위치(모서리,엣지)인지 여부 판단
                _resizeDirection = GetResizeHandleIndex(e.Location);
                if (_resizeDirection != -1)
                {
                    _isResizingRoi = true;
                    _resizeStart = e.Location;
                }
                //ROI 크기 변경 이외에 이동을 위해, 입력좌표가 ROI 안에 있는지 여부 판단
                else if (_roiRect.Contains(e.Location))
                {
                    _isMovingRoi = true;
                    _moveStart = e.Location;
                }
                else
                {
                    //새로운 ROI 그리기 시작 위치 설저어
                    _roiStart = e.Location;
                    _isSelectingRoi = true;
                }
            }

            // 마우스 오른쪽 버튼이 눌렸을 때 클릭 위치 저장
            if (e.Button == MouseButtons.Right)
            {
                RightClick = e.Location;

                // UserControl이 포커스를 받아야 마우스 휠이 정상적으로 동작함
                Focus();
            }
        }

        private void ImageViewCCtrl_MouseMove(object sender, MouseEventArgs e)
        {
            //#SETROI#4 ROI 크기 변경 또는 이동 진행
            if (RoiMode)
            {
                //최초 ROI 생성하여 그리기
                if (_isSelectingRoi && e.Button == MouseButtons.Left)
                {
                    int x = Math.Min(_roiStart.X, e.X);
                    int y = Math.Min(_roiStart.Y, e.Y);
                    int width = Math.Abs(e.X - _roiStart.X);
                    int height = Math.Abs(e.Y - _roiStart.Y);
                    _roiRect = new Rectangle(x, y, width, height);
                    Invalidate();
                }
                //기존 ROI 크기 변경
                else if (_isResizingRoi && e.Button == MouseButtons.Left)
                {
                    ResizeROI(e.Location);
                    _resizeStart = e.Location;
                    Invalidate();
                }
                //ROI 위치 이동
                else if (_isMovingRoi && e.Button == MouseButtons.Left)
                {
                    int dx = e.X - _moveStart.X;
                    int dy = e.Y - _moveStart.Y;
                    _roiRect.X += dx;
                    _roiRect.Y += dy;
                    _moveStart = e.Location;
                    Invalidate();
                }
                //마우스 클릭없이, 위치만 이동시에, 커서의 위치가 크기변경또는 이동 위치일때, 커서 변경
                else
                {
                    int index = GetResizeHandleIndex(e.Location);
                    if (index != -1)
                    {
                        Cursor = GetCursorForHandle(index);
                    }
                    else if (_roiRect.Contains(e.Location))
                    {
                        Cursor = Cursors.SizeAll; // ROI 내부 이동
                    }
                    else
                    {
                        Cursor = Cursors.Default;
                    }
                }
            }

            // 마우스 오른쪽 버튼이 눌린 상태에서만 이동 처리
            if (e.Button == MouseButtons.Right)
            {
                // 현재 마우스 위치와 이전 클릭 위치를 비교하여 이동 거리 계산
                Offset.X = e.Location.X - RightClick.X + LastOffset.X;
                Offset.Y = e.Location.Y - RightClick.Y + LastOffset.Y;

                // 이미지 위치 업데이트
                ImageRect.X = Offset.X;
                ImageRect.Y = Offset.Y;

                // 변경된 화면을 다시 그리도록 요청
                Invalidate();
            }
        }

        private void ImageViewCCtrl_MouseUp(object sender, MouseEventArgs e)
        {
            //#SETROI#5 ROI 크기 변경 또는 이동 완료
            if (e.Button == MouseButtons.Left)
            {
                if (_isSelectingRoi)
                {
                    _isSelectingRoi = false;
                }
                else if (_isResizingRoi)
                {
                    _isResizingRoi = false;
                }
                else if (_isMovingRoi)
                {
                    _isMovingRoi = false;
                }
            }

            // 마우스를 떼면 마지막 오프셋 값을 저장하여 이후 이동을 연속적으로 처리
            if (e.Button == MouseButtons.Right)
            {
                LastOffset = Offset;
            }
        }

        //마우스 위치가 ROI 크기 변경을 위한 여부를 확인하기 위해, 4개 모서리와 사각형 라인의 중간 위치 반환
        private System.Drawing.Point[] GetResizeHandles(Rectangle rect)
        {
            return new System.Drawing.Point[]
            {
                new System.Drawing.Point(rect.Left, rect.Top), // 좌상
                new System.Drawing.Point(rect.Right, rect.Top), // 우상
                new System.Drawing.Point(rect.Left, rect.Bottom), // 좌하
                new System.Drawing.Point(rect.Right, rect.Bottom), // 우하
                new System.Drawing.Point(rect.Left + rect.Width / 2, rect.Top), // 상 중간
                new System.Drawing.Point(rect.Left + rect.Width / 2, rect.Bottom), // 하 중간
                new System.Drawing.Point(rect.Left, rect.Top + rect.Height / 2), // 좌 중간
                new System.Drawing.Point(rect.Right, rect.Top + rect.Height / 2) // 우 중간
            };
        }

        //마우스 위치가 크기 변경 위치에 해당하는 지를, 위치 인덱스로 반환
        private int GetResizeHandleIndex(System.Drawing.Point mousePos)
        {
            System.Drawing.Point[] handles = GetResizeHandles(_roiRect);
            for (int i = 0; i < handles.Length; i++)
            {
                Rectangle handleRect = new Rectangle(handles[i].X - _ResizeHandleSize / 2, handles[i].Y - _ResizeHandleSize / 2, _ResizeHandleSize, _ResizeHandleSize);
                if (handleRect.Contains(mousePos)) return i;
            }
            return -1;
        }

        //사각 모서리와 중간 지점을 인덱스로 설정하여, 해당 위치에 따른 커서 타입 반환
        private Cursor GetCursorForHandle(int handleIndex)
        {
            switch (handleIndex)
            {
                case 0: case 3: return Cursors.SizeNWSE;
                case 1: case 2: return Cursors.SizeNESW;
                case 4: case 5: return Cursors.SizeNS;
                case 6: case 7: return Cursors.SizeWE;
                default: return Cursors.Default;
            }
        }

        //ROI 크기 변경시, 마우스 위치를 입력받아, ROI 크기 변경
        private void ResizeROI(System.Drawing.Point mousePos)
        {
            switch (_resizeDirection)
            {
                case 0: _roiRect.X = mousePos.X; _roiRect.Y = mousePos.Y; _roiRect.Width -= (mousePos.X - _resizeStart.X); _roiRect.Height -= (mousePos.Y - _resizeStart.Y); break;
                case 1: _roiRect.Width = mousePos.X - _roiRect.X; _roiRect.Y = mousePos.Y; _roiRect.Height -= (mousePos.Y - _resizeStart.Y); break;
                case 2: _roiRect.X = mousePos.X; _roiRect.Width -= (mousePos.X - _resizeStart.X); _roiRect.Height = mousePos.Y - _roiRect.Y; break;
                case 3: _roiRect.Width = mousePos.X - _roiRect.X; _roiRect.Height = mousePos.Y - _roiRect.Y; break;
                case 4: _roiRect.Y = mousePos.Y; _roiRect.Height -= (mousePos.Y - _resizeStart.Y); break;
                case 5: _roiRect.Height = mousePos.Y - _roiRect.Y; break;
                case 6: _roiRect.X = mousePos.X; _roiRect.Width -= (mousePos.X - _resizeStart.X); break;
                case 7: _roiRect.Width = mousePos.X - _roiRect.X; break;
            }
        }

        private void ImageViewCCtrl_MouseWheel(object sender, MouseEventArgs e)
        {
            // 마우스 휠 위(줌 인) 또는 아래(줌 아웃) 이벤트 처리
            float ZoomChange = e.Delta > 0 ? 1.1f : 0.9f; //마우스휠 위로(+) 1.1배,아래로(-) 0.9배 축소
            float NewZoomFactor = ZoomFactor * ZoomChange; //현재줌배율*새로운줌배율

            // MaxZoom, MinZoom 값 범위내에서 줌
            if (NewZoomFactor > MaxZoom)
            {
                NewZoomFactor = MaxZoom;
            }
            if (NewZoomFactor < MinZoom)
            {
                NewZoomFactor = MinZoom;
            }


            // 줌 아웃 시 점진적으로 초기 크기와 중심위치로 복귀
            if (ZoomChange < 1.0f)
            {

                /******************************************************************************************************
                 * Lerp(Linear Interpolation, 선형 보간)은 두 값 사이를 일정한 비율로 보간하여 중간 값을 계산하는 기법
                 * 두 값 A와 B 사이의 중간 값을 단계적으로 계산하여 부드러운 변화 효과
                 * Lerp(A, B, t) = A * (1 - t) + B * t
                 * A: 시작 값 (현재 값), B: 목표 값 (최종 값), t: 보간 비율 (0.0f ~ 1.0f, 값이 클수록 더 빠르게 B에 가까워짐)
                 *******************************************************************************************************/


                float t = 0.5f; // 이미지 점진적으로 줄어들도록 보간

                // 현재 크기를 점진적으로 초기 크기로 조정
                float NewWidth = ImageRect.Width * (1 - t) + InitialWidth * t;
                float NewHeight = ImageRect.Height * (1 - t) + InitialHeight * t;

                // 현재 중심에서 점진적으로 초기 중심으로 이동
                float CurrentCenterX = ImageRect.X + (ImageRect.Width / 2);
                float CurrentCenterY = ImageRect.Y + (ImageRect.Height / 2);

                float NewCenterX = CurrentCenterX * (1 - t) + InitialCenterX * t;
                float NewCenterY = CurrentCenterY * (1 - t) + InitialCenterY * t;

                // 새로운 이미지 위치 반영 (점진적으로 초기 상태로 회귀)
                ImageRect = new RectangleF(
                    NewCenterX - (NewWidth / 2),
                    NewCenterY - (NewHeight / 2),
                    NewWidth,
                    NewHeight
                );

                ZoomFactor = ZoomFactor * (1 - t) + 1.0f * t;
            }
            else  // 줌 인 시 마우스 위치 기준 확대
            {
                // 마우스 위치를 기준으로 줌 좌표 변환
                float MouseXRatio = (e.X - ImageRect.X) / ImageRect.Width;
                float MouseYRatio = (e.Y - ImageRect.Y) / ImageRect.Height;

                // 새로운 이미지 크기 계산
                float NewWidth = ImageRect.Width * ZoomChange;
                float NewHeight = ImageRect.Height * ZoomChange;

                // 마우스 포인터 위치를 유지하면서 새로운 이미지 위치 설정
                float NewX = e.X - (MouseXRatio * NewWidth);
                float NewY = e.Y - (MouseYRatio * NewHeight);

                // 마우스 포인터를 중심으로 새로운 이미지 위치 반영
                ImageRect = new RectangleF(NewX, NewY, NewWidth, NewHeight);


                // 배율 업데이트
                ZoomFactor = NewZoomFactor;
            }

            // 줌 후 이동할 때 중심을 기준으로 좌표 갱신
            Offset = new System.Drawing.Point((int)ImageRect.X, (int)ImageRect.Y);
            LastOffset = Offset;

            // 다시 그리기 요청
            Invalidate();
        }

        private void ImageViewCCtrl_Resize(object sender, EventArgs e)
        {
            ResizeCanas();
            Invalidate();
        }

        // 창 resize ROI 업데이트
        private void UpdateROI()
        {
            if (Bitmap == null || _roiRect.IsEmpty || InitialWidth == 0 || InitialHeight == 0)
                return;

            // 기존 ROI 좌표를 원본 ImageRect 기준으로 변환 (비율)
            float roiX_ratio = (_roiRect.X - InitialStartX) / InitialWidth;
            float roiY_ratio = (_roiRect.Y - InitialStartY) / InitialHeight;
            float roiW_ratio = _roiRect.Width / InitialWidth;
            float roiH_ratio = _roiRect.Height / InitialHeight;

            // 새로운 ImageRect 크기에 맞춰 ROI 조정
            _roiRect.X = (int)(ImageRect.X + roiX_ratio * ImageRect.Width);
            _roiRect.Y = (int)(ImageRect.Y + roiY_ratio * ImageRect.Height);
            _roiRect.Width = (int)(roiW_ratio * ImageRect.Width);
            _roiRect.Height = (int)(roiH_ratio * ImageRect.Height);

            // 새로운 초기 크기 갱신
            InitialStartX = ImageRect.X;
            InitialStartY = ImageRect.Y;
            InitialWidth = ImageRect.Width;
            InitialHeight = ImageRect.Height;
        }

        public Rectangle GetRoiRect()
        {
            if (Bitmap == null || _roiRect.IsEmpty)
                return new Rectangle();

            // UserControl 좌표계에서 원본 이미지 좌표계로 변환하는 비율 계산
            float scaleX = (float)Bitmap.Width / ImageRect.Width;
            float scaleY = (float)Bitmap.Height / ImageRect.Height;

            // 변환된 ROI 좌표 계산
            int roiX = (int)((_roiRect.X - ImageRect.X) * scaleX);
            int roiY = (int)((_roiRect.Y - ImageRect.Y) * scaleY);
            int roiWidth = (int)(_roiRect.Width * scaleX);
            int roiHeight = (int)(_roiRect.Height * scaleY);

            // ROI가 원본 이미지 범위를 초과하지 않도록 조정
            roiX = Math.Max(0, roiX);
            roiY = Math.Max(0, roiY);
            roiWidth = Math.Min(Bitmap.Width - roiX, roiWidth);
            roiHeight = Math.Min(Bitmap.Height - roiY, roiHeight);

            if (roiWidth <= 0 || roiHeight <= 0)
                return new Rectangle(); // 유효하지 않은 ROI

            // 원본 이미지에서 ROI 부분을 추출
            Rectangle roi = new Rectangle(roiX, roiY, roiWidth, roiHeight);
            return roi;
        }

        //#MATCH PROP#13 템플릿 매칭 위치 입력 받는 함수
        public void AddRect(List<Rectangle> rectangles)
        {
            _rectangles = rectangles;
            Invalidate();
        }
    }
}
