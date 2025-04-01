using JidamVision.Algorithm;
using JidamVision.Core;
using JidamVision.Teach;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.MonthCalendar;

namespace JidamVision.Property
{
    /*
    #BINARY FILTER# - <<<이진화 검사 개발>>> 
    입력된 lower, upper 임계값을 이용해, 영상을 이진화한 후, Filter(area)등을 이용해, 원하는 영역을 찾는다.
     */

    //#BINARY FILTER#7 이진화 하이라이트, 이외에, 이진화 이미지를 보기 위한 옵션
    public enum ShowBinaryMode
    {
        ShowBinaryNone = 0,             //이진화 하이라이트 끄기
        ShowBinaryHighlight,            //이진화 하이라이트 보기
        ShowBinaryOnly                  //배경 없이 이진화 이미지만 보기
    }

    public partial class BinaryInspProp : UserControl
    {
        public event EventHandler<EventArgs> PropertyChanged;
        public event EventHandler<RangeChangedEventArgs> RangeChanged;

        BlobAlgorithm _blobAlgo = null;

        /* NOTE
        public int LowerValue
        {
            get { return trackBarLower.Value; }
        }
        C# 6부터는 위 코드를 더 간결하게 람다(=>) 문법을 사용하여 표현
        */

        // 속성값을 이용하여 이진화 임계값 설정
        public int LowerValue => trackBarLower.Value;
        public int UpperValue => trackBarUpper.Value;

        public BinaryInspProp()
        {
            InitializeComponent();

            // TrackBar 초기 설정
            trackBarLower.ValueChanged += OnValueChanged;
            trackBarUpper.ValueChanged += OnValueChanged;
            
            txtAreaMin.Leave += OnFilterChanged;
            txtAreaMax.Leave += OnFilterChanged;
            
            txtWidthMin.Leave += OnFilterChanged;
            txtWidthMax.Leave += OnFilterChanged;
            
            txtHeightMin.Leave += OnFilterChanged;
            txtHeightMax.Leave += OnFilterChanged;

            txtCount.Leave += OnFilterChanged;

            trackBarLower.Value = 0;
            trackBarUpper.Value = 128;
        }

        public void SetAlgorithm(BlobAlgorithm blobAlgo)
        {
            _blobAlgo = blobAlgo;
            SetProperty();
        }

        public void SetProperty()
        {
            if (_blobAlgo is null)
                return;

            BinaryThreshold threshold = _blobAlgo.BinThreshold;
            trackBarLower.Value = threshold.lower;
            trackBarUpper.Value = threshold.upper;
            chkInvert.Checked = threshold.invert;

            txtAreaMin.Text = _blobAlgo.AreaMin.ToString();
            txtAreaMax.Text = _blobAlgo.AreaMax.ToString();
            txtWidthMin.Text = _blobAlgo.WidthMin.ToString();
            txtWidthMax.Text = _blobAlgo.WidthMax.ToString();
            txtHeightMin.Text = _blobAlgo.HeightMin.ToString();
            txtHeightMax.Text = _blobAlgo.HeightMax.ToString();
            txtCount.Text = _blobAlgo.BlobCount.ToString();
        }

        public void GetProperty()
        {
            if (_blobAlgo is null)
                return;

            BinaryThreshold threshold = new BinaryThreshold();
            threshold.upper = UpperValue;
            threshold.lower = LowerValue;
            threshold.invert = chkInvert.Checked;

            _blobAlgo.BinThreshold = threshold;

            if (txtAreaMin.Text != "")
            {
                int areaMin = int.Parse(txtAreaMin.Text);
                _blobAlgo.AreaMin = areaMin;
            }
            if (txtAreaMax.Text != "")
            {
                int areaMax = int.Parse(txtAreaMax.Text);
                _blobAlgo.AreaMax = areaMax;
            }

            if (txtWidthMin.Text != "")
            {
                int widthMin = int.Parse(txtWidthMin.Text);
                _blobAlgo.WidthMin = widthMin;
            }
            if (txtWidthMax.Text != "")
            {
                int widthMax = int.Parse(txtWidthMax.Text);
                _blobAlgo.WidthMax = widthMax;
            }

            if (txtHeightMin.Text != "")
            {
                int heightMin = int.Parse(txtHeightMin.Text);
                _blobAlgo.HeightMin = heightMin;
            }
            if (txtHeightMax.Text != "")
            {
                int heightMax = int.Parse(txtHeightMax.Text);
                _blobAlgo.HeightMax = heightMax;
            }

            if (txtCount.Text != "")
            {
                int blobCount = int.Parse(txtCount.Text);
                _blobAlgo.BlobCount = blobCount;
            }
        }

        //#BINARY FILTER#10 이진화 옵션을 선택할때마다, 이진화 이미지가 갱신되도록 하는 함수
        private void UpdateBinary()
        {
            GetProperty();

            bool invert = chkInvert.Checked;
            bool highlight = chkHighlight.Checked;

            ShowBinaryMode showBinaryMode = ShowBinaryMode.ShowBinaryNone;
            if (highlight)
            {
                showBinaryMode = ShowBinaryMode.ShowBinaryHighlight;

                bool showBinary = chkShowBinary.Checked;

                if (showBinary)
                    showBinaryMode = ShowBinaryMode.ShowBinaryOnly;
            }

            RangeChanged?.Invoke(this, new RangeChangedEventArgs(LowerValue, UpperValue, invert, showBinaryMode));
        }

        //#BINARY FILTER#11 GUI 이벤트와 UpdateBinary함수 연동
        private void OnValueChanged(object sender, EventArgs e)
        {
            UpdateBinary();
        }

        private void chkInvert_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBinary();
        }

        private void chkBinaryOnly_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBinary();
        }

        private void chkHighlight_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBinary();
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            if (_blobAlgo == null) 
                return;

            if (int.TryParse(txtAreaMin.Text, out int areaMin))
            {
                _blobAlgo.AreaMin = areaMin;
                PropertyChanged?.Invoke(this, null);
            }
            else
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                txtAreaMin.Text = _blobAlgo.AreaMin.ToString(); // 기존 값 복원
            }

            if (int.TryParse(txtAreaMax.Text, out int areaMax))
            {
                _blobAlgo.AreaMax = areaMax;
                PropertyChanged?.Invoke(this, null);
            }
            else
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                txtAreaMax.Text = _blobAlgo.AreaMax.ToString(); // 기존 값 복원
            }

            if (int.TryParse(txtWidthMin.Text, out int widthMin))
            {
                _blobAlgo.WidthMin = widthMin;
                PropertyChanged?.Invoke(this, null);
            }
            else
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                txtWidthMin.Text = _blobAlgo.WidthMin.ToString(); // 기존 값 복원
            }

            if (int.TryParse(txtWidthMax.Text, out int widthMax))
            {
                _blobAlgo.WidthMax = widthMax;
                PropertyChanged?.Invoke(this, null);
            }
            else
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                txtWidthMax.Text = _blobAlgo.WidthMax.ToString(); // 기존 값 복원
            }

            if (int.TryParse(txtHeightMin.Text, out int heightMin))
            {
                _blobAlgo.HeightMin = heightMin;
                PropertyChanged?.Invoke(this, null);
            }
            else
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                txtHeightMin.Text = _blobAlgo.HeightMin.ToString(); // 기존 값 복원
            }

            if (int.TryParse(txtHeightMax.Text, out int heightMax))
            {
                _blobAlgo.HeightMax = heightMax;
                PropertyChanged?.Invoke(this, null);
            }
            else
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                txtHeightMax.Text = _blobAlgo.HeightMax.ToString(); // 기존 값 복원
            }

            if (int.TryParse(txtCount.Text, out int blobCount))
            {
                _blobAlgo.BlobCount = blobCount;
                PropertyChanged?.Invoke(this, null);
            }
            else
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                txtCount.Text = _blobAlgo.BlobCount.ToString(); // 기존 값 복원
            }
        }
    }

    //#BINARY FILTER#9 이진화 관련 이벤트 발생시, 전달할 값 추가
    public class RangeChangedEventArgs : EventArgs
    {
        public int LowerValue { get; }
        public int UpperValue { get; }
        public bool Invert { get; }
        public ShowBinaryMode ShowBinMode { get; }

        public RangeChangedEventArgs(int lowerValue, int upperValue, bool invert, ShowBinaryMode showBinaryMode)
        {
            LowerValue = lowerValue;
            UpperValue = upperValue;
            Invert = invert;
            ShowBinMode = showBinaryMode;
        }
    }
}
