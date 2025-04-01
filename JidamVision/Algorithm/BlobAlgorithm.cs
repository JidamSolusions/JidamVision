using JidamVision.Core;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;

namespace JidamVision.Algorithm
{
    //#BINARY FILTER#1 이진화 필터를 위한 클래스


    //이진화 임계값 설정을 구조체로 만들기
    public struct BinaryThreshold
    {
        public int lower;
        public int upper;
        public bool invert;
    }

    public class BlobAlgorithm : InspAlgorithm
    {
        //이진화 필터로 찾은 영역
        private List<Rect> _findArea;

        public BinaryThreshold BinThreshold { get; set; } = new BinaryThreshold();

        //픽셀 영역으로 이진화 필터
        public int AreaMin { get; set; } = 50;
        public int AreaMax { get; set; } = 500;

        public int WidthMin { get; set; } = 0;
        public int WidthMax { get; set; } = 0;

        public int HeightMin { get; set; } = 0;
        public int HeightMax { get; set; } = 0;
        public int BlobCount { get; set; } = 0;
        public int OutBlobCount { get; set; } = 0;

        public BlobAlgorithm()
        {
            //#ABSTRACT ALGORITHM#5 각 함수마다 자신의 알고리즘 타입 설정
            InspectType = InspectType.InspBinary;
        }

        //#BINARY FILTER#2 이진화 후, 필터를 이용해 원하는 영역을 얻음 

        //#ABSTRACT ALGORITHM#6 
        //InspAlgorithm을 상속받아, 구현하고, 인자로 입력받던 것을 부모의 _srcImage 이미지 사용
        //검사 시작전 IsInspected = false로 초기화하고, 검사가 정상적으로 완료되면,IsInspected = true로 설정
        public override bool DoInspect()
        {
            ResetResult();
            OutBlobCount = 0;

            if (_srcImage == null)
                return false;

            Mat targetImage = _srcImage[InspRect];

            Mat grayImage = new Mat();
            if (targetImage.Type() == MatType.CV_8UC3)
                Cv2.CvtColor(targetImage, grayImage, ColorConversionCodes.BGR2GRAY);
            else
                grayImage = targetImage;

            Mat binaryImage = new Mat();
            //Cv2.Threshold(grayImage, binaryMask, lowerValue, upperValue, ThresholdTypes.Binary);
            Cv2.InRange(grayImage, BinThreshold.lower, BinThreshold.upper, binaryImage);

            if (BinThreshold.invert)
                binaryImage = ~binaryImage;

            if (AreaMin > 0 || AreaMax > 0 || WidthMin > 0 || WidthMax > 0 || HeightMin > 0 || HeightMax > 0)
            {
                if (!BlobFilter(binaryImage, AreaMin, AreaMax, WidthMin, WidthMax, HeightMin, HeightMax))
                    return false;
            }

            IsInspected = true;

            return true;
        }

        //#BINARY FILTER#3 이진화 필터처리 함수
        private bool BlobFilter(Mat binImage, int areaMin, int areaMax, int widthMin, int widthMax, int heightMin, int heightMax)
        {
            // 컨투어 찾기
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(binImage, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            // 필터링된 객체를 담을 리스트
            Mat filteredImage = Mat.Zeros(binImage.Size(), MatType.CV_8UC1);

            if (_findArea is null)
                _findArea = new List<Rect>();

            _findArea.Clear();

            int findBlobCount = 0;

            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);
                if (area <= 0)
                    continue;

                if (areaMin > 0 && area < areaMin)
                    continue;

                if (areaMax > 0 && area > areaMax)
                    continue;

                // RotatedRect 정보 계산
                //RotatedRect rotatedRect = Cv2.MinAreaRect(contour);
                Rect boundingRect = Cv2.BoundingRect(contour);

                if (widthMin > 0 && boundingRect.Width < widthMin)
                    continue;

                if (widthMax > 0 && boundingRect.Width > widthMax)
                    continue;

                if (heightMin > 0 && boundingRect.Height < heightMin)
                    continue;

                if (heightMax > 0 && boundingRect.Height > heightMax)
                    continue;

                // 필터링된 객체를 이미지에 그림
                //Cv2.DrawContours(filteredImage, new Point[][] { contour }, -1, Scalar.White, -1);

                findBlobCount++;
                Rect blobRect = boundingRect + InspRect.TopLeft;

                string blobInfo;
                blobInfo = $"Blob X:{blobRect.X}, Y:{blobRect.Y}, Size({blobRect.Width},{blobRect.Height})";
                ResultString.Add(blobInfo);

                _findArea.Add(blobRect);
            }

            OutBlobCount = findBlobCount;

            IsDefect = false;
            if (BlobCount > 0)
            {
                string result = "OK";
                if (findBlobCount != BlobCount)
                {
                    result = "NG";
                    IsDefect = true;
                }
                string resultInfo = "";
                resultInfo = $"[{result}] match blob count [in : {BlobCount},out : {findBlobCount}]";
                ResultString.Add(resultInfo);
            }

            return true;
        }

        //#BINARY FILTER#4 이진화 영역 반환
        public override int GetResultRect(out List<Rect> resultArea)
        {
            resultArea = null;

            //#ABSTRACT ALGORITHM#7 검사가 완료되지 않았다면, 리턴
            if (!IsInspected)
                return -1;

            if (_findArea is null || _findArea.Count <= 0)
                return -1;

            resultArea = _findArea;
            return resultArea.Count;
        }
    }
}
