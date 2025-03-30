using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JidamVision.Algorithm;
using OpenCvSharp;
using JidamVision.Core;
using System.Security.Policy;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using JidamVision.Setting;
using System.Xml.Linq;
using JidamVision.Inspect;

namespace JidamVision.Teach
{
    //#MATCH PROP#3 InspWindow 클래스 추가, ROI 관리 및 검사를 처리하는 클래스
    //검사 알고리즘를 관리하는 클래스

    public class InspWindow
    {
        //템플릿 매칭 이미지
        private Mat _teachingImage;

        public InspWindowType InspWindowType { get; set; }

        //#MODEL SAVE#5 모델 저장을 위한 Serialize를 위해서, prvate set -> set으로 변경
        //public string Name {  get; private set; }
        public string Name { get; set; }
        public string UID { get; set; }

        public Rect WindowArea { get; set; }
        public Rect InspArea { get; set; }

        public bool IsTeach { get; set; } = false;

        //#ABSTRACT ALGORITHM#9 개별 변수로 있던, MatchAlgorithm과 BlobAlgorithm을
        //InspAlgorithm으로 추상화하여 리스트로 관리하도록 변경

        //#MODEL SAVE#6 Xml Serialize를 위해서, Element을 명확하게 알려줘야 함
        [XmlElement("InspAlgorithm")]
        public List<InspAlgorithm> AlgorithmList { get; set; } = new List<InspAlgorithm>();

        //부모-자식 관계를 위한 변수 추가
        public InspWindow Parent { get; set; }

        [XmlElement("ChildWindow")]
        public List<InspWindow> Children { get; set; } = new List<InspWindow>();

        public List<InspResult> InspResultList { get; set; } = new List<InspResult>();

        [XmlIgnore]
        public Mat WindowImage { get; set; }

        public bool IsPatternLearn { get; set; } = false;

        public InspWindow()
        {
        }

        public InspWindow(InspWindowType windowType, string name)
        {
            InspWindowType = windowType;
            Name = name;
        }

        public bool SetTeachingImage(Mat image, System.Drawing.Rectangle rect)
        {
            _teachingImage = new Mat(image, new Rect(rect.X, rect.Y, rect.Width, rect.Height));
            return true;
        }

        //#MATCH PROP#4 템플릿 매칭 이미지 로딩
        public bool PatternLearn()
        {
            if (IsPatternLearn == true)
                return true;

            foreach (var algorithm in AlgorithmList)
            {
                if (algorithm.InspectType != InspectType.InspMatch)
                    continue;

                MatchAlgorithm matchAlgo = (MatchAlgorithm)algorithm;

                if (WindowImage != null)
                {
                    Mat tempImage = new Mat();
                    if (WindowImage.Type() == MatType.CV_8UC3)
                        Cv2.CvtColor(WindowImage, tempImage, ColorConversionCodes.BGR2GRAY);
                    else
                        tempImage = WindowImage;

                    matchAlgo.SetTemplateImage(tempImage);
                }
            }

            IsPatternLearn = true;

            return true;
        }

        //#ABSTRACT ALGORITHM#10 타입에 따라 알고리즘을 추가하는 함수
        public bool AddInspAlgorithm(InspectType inspType)
        {
            InspAlgorithm inspAlgo = null;

            switch (inspType)
            {
                case InspectType.InspBinary:
                    inspAlgo = new BlobAlgorithm();
                    break;
                case InspectType.InspMatch:
                    inspAlgo = new MatchAlgorithm();
                    break;
            }

            if (inspAlgo is null)
                return false;

            AlgorithmList.Add(inspAlgo);

            return true;
        }


        //#ABSTRACT ALGORITHM#11 알고리즘을 리스트로 관리하므로, 필요한 타입의 알고리즘을 찾는 함수
        public InspAlgorithm FindInspAlgorithm(InspectType inspType)
        {
            return AlgorithmList.Find(algo => algo.InspectType == inspType);
        }

        //#ABSTRACT ALGORITHM#12 클래스 내에서, 인자로 입력된 타입의 알고리즘을 검사하거나,
        ///모든 알고리즘을 검사하는 옵션을 가지는 검사 함수
        public virtual bool DoInpsect(InspectType inspType)
        {
            foreach (var inspAlgo in AlgorithmList)
            {
                if (inspAlgo.InspectType == inspType || inspType == InspectType.InspNone)
                    inspAlgo.DoInspect();
            }

            return true;
        }
        
        public bool IsDefect()
        {
            foreach (InspAlgorithm algo in AlgorithmList)
            {
                if (!algo.IsInspected)
                    continue;

                if (algo.IsDefect)
                    return true;
            }
            return false;
        }

        public virtual bool OffsetMove(OpenCvSharp.Point offset)
        {
            Rect windowRect = WindowArea;
            windowRect.X += offset.X;
            windowRect.Y += offset.Y;
            WindowArea = windowRect;
            return true;
        }

        public bool SetInspOffset(OpenCvSharp.Point offset)
        {
            InspArea = WindowArea + offset;
            AlgorithmList.ForEach(algo => algo.InspRect = algo.TeachRect + offset);
            return true;
        }

        #region 부모 - 자식 관계 관리 메서드 추가

        public void AddChild(InspWindow child)
        {
            if (child == null || Children.Contains(child))
                return;

            child.Parent = this;
            Children.Add(child);
        }

        public bool RemoveChild(InspWindow child)
        {
            if (child == null || !Children.Contains(child))
                return false;

            child.Parent = null;
            if (!Children.Remove(child))
                return false;

            return true;
        }

        public InspWindow GetRoot()
        {
            InspWindow root = this;
            while (root.Parent != null)
                root = root.Parent;

            return root;
        }
        #endregion

        public virtual bool SaveInspWindow(Model curModel)
        {
            if (curModel is null)
                return false;

            string imgDir = Path.Combine(Path.GetDirectoryName(curModel.ModelPath), "Images");
            if (!Directory.Exists(imgDir))
            {
                Directory.CreateDirectory(imgDir);
            }

            Mat windowImage = WindowImage;
            if (windowImage != null)
            {
                string targetPath = Path.Combine(imgDir, UID + ".png");
                Cv2.ImWrite(targetPath, windowImage);
            }

            return true;
        }

        public virtual bool LoadInspWindow(Model curModel)
        {
            if (curModel is null)
                return false;

            string imgDir = Path.Combine(Path.GetDirectoryName(curModel.ModelPath), "Images");

            foreach (InspAlgorithm algo in AlgorithmList)
            {
                if (algo is null)
                    continue;

                if (algo.InspectType == InspectType.InspMatch)
                {
                    MatchAlgorithm matchAlgo = algo as MatchAlgorithm;
                    string targetPath = Path.Combine(imgDir, UID + ".png");
                    if (File.Exists(targetPath))
                    {
                        Mat windowImage = Cv2.ImRead(targetPath);
                        if (windowImage != null)
                        {
                            WindowImage = windowImage;

                            Mat tempImage = new Mat();
                            if (windowImage.Type() == MatType.CV_8UC3)
                                Cv2.CvtColor(windowImage, tempImage, ColorConversionCodes.BGR2GRAY);
                            else
                                tempImage = windowImage;

                            matchAlgo.SetTemplateImage(tempImage);
                        }
                    }
                }
            }

            return true;
        }

        public void ResetInspResult()
        {
            InspResultList.Clear();
        }

        public void AddInspResult(InspResult inspResult)
        {
            InspResultList.Add(inspResult);
        }
    }
}
