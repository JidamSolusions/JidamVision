using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using OpenCvSharp;
using JidamVision.Core;
using JidamVision.Teach;
using System.Security.Policy;

namespace JidamVision.Algorithm
{
    /*
    #ABSTRACT ALGORITHM# - <<<검사 알고리즘 추상화 개발>>> 
     */

    //#MATCH PROP#1 InspAlgorithm 클래스를 추가, 여러 알고리즘을 추상화하기 위함
    //추가 내용은 나중에 개발하고, 현재는 비어 있는 상태로 만들것

    //#MODEL SAVE#7 Xml Serialize를 위해서, 아래 코드 추가
    //XmlSerialize는 추상화된 상태를 알수 없어, 상속된 클래스를 명시적으로 포함해야 함.
    [XmlInclude(typeof(MatchAlgorithm))]
    [XmlInclude(typeof(BlobAlgorithm))]
    public abstract class InspAlgorithm
    {
        //#ABSTRACT ALGORITHM#1 검사 알고리즘을 추상화하여, 공통된 값이나, 함수 정의
        
        //알고리즘 타입 정의
        public InspectType InspectType { get; set; } = InspectType.InspNone;

        //알고지즘을 사용할지 여부 결정
        public bool IsUse { get; set; } = true;
        //검사가 완료되었는지를 판단
        public bool IsInspected { get; set; } = false;

        public Rect TeachRect { get; set; }
        public Rect InspRect { get; set; }

        public eImageChannel ImageChannel { get; set; } = eImageChannel.Gray;

        //검사할 원본 이미지
        protected Mat _srcImage = null;

        public List<string> ResultString { get; set; } = new List<string>();

        public bool IsDefect { get; set; }

        //검사에 필요한 정보를 설정, 각 알고리즘이 함수를 상속받아서, 필요한 정보를 추가로 입력받는다.
        public virtual void SetInspData(Mat srcImage)
        {
            _srcImage = srcImage;
        }

        //검사 함수로, 상속 받는 클래스는 필수로 구현해야한다.
        public abstract bool DoInspect();

        public virtual void ResetResult()
        {
            IsInspected = false;
            IsDefect = false;
            ResultString.Clear();
        }

        //검사 결과가 Rect정보로 출력이 가능하다면, 이 함수를 상속 받아서, 정보 반환
        public virtual int GetResultRect(out List<Rect> resultArea)
        {
            resultArea  = null;
            return 0;
        }

    }
}
