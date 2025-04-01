using Common.Util.Helpers;
using JidamVision.Algorithm;
using JidamVision.Core;
using JidamVision.Setting;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace JidamVision.Teach
{
    /*
    #MODEL# - <<<티칭 정보를 저장,관리하기 위한 클래스 만들기>>> 
    검사에 필요한 모든 데이터를 관리하는 클래스
    InspWindow 정보와 검사 알고리즘 정보를 모두 가지고 있음
    */

    //#MODEL#3 모델 클래스 생성
    public class Model
    {
        //#MODEL SAVE#1 모델 정보 저장을 위해 추가한 프로퍼티
        public string ModelName { get; set; } = "";
        public string ModelInfo { get; set; } = "";
        public string ModelPath { get; set; } = "";

        public string InspectImagePath { get; set; } = "";

        //#MODEL#1 InspStage에 있던 InspWindowList 위치를 이곳으로 변경
        [XmlElement("InspWindow")]
        public List<InspWindow> InspWindowList {  get; set; } 
            
        public Model()
        {
            InspWindowList = new List<InspWindow>();
        }

        //#MODEL#4 새로운 InspWindow를 추가할때
        public InspWindow AddInspWindow(InspWindowType windowType)
        {
            InspWindow inspWindow = InspWindowFactory.Inst.Create(windowType);
            InspWindowList.Add(inspWindow);

            return inspWindow;
        }

        //#MODEL#5 기존 InspWindow를 삭제할때
        public bool DelInspWindow(InspWindow inspWindow)
        {
            if(InspWindowList.Contains(inspWindow))
            {
                InspWindowList.Remove(inspWindow);
                return true;
            }
            return false;
        }
        public bool DelInspWindowList(List<InspWindow> inspWindowList)
        {
            int before = InspWindowList.Count;
            InspWindowList.RemoveAll(w => inspWindowList.Contains(w));
            return InspWindowList.Count < before;
        }

        //#GROUP ROI#1 GroupWindow를 만들어 모델에 추가
        public GroupWindow AddGroupWindow(List<InspWindow> inspWindowList)
        {
            bool hasParentWindow = inspWindowList.Any(m => m.Parent != null);
            if( hasParentWindow)
            {
                MessageBox.Show("이미 그룹에 속한 윈도우 입니다!");
                return null;
            }

            GroupWindow groupWindow = (GroupWindow)InspWindowFactory.Inst.Create(InspWindowType.Group);
            if (groupWindow is null)
                return null;

            foreach (var inspWindow in inspWindowList)
            {
                //그룹멤버로 추가하고, 전체리스트에서는 제거
                inspWindow.Parent = groupWindow;
                groupWindow.AddMember(inspWindow);
                DelInspWindow(inspWindow);
            }

            //그룹을 전체리스트에 추가
            InspWindowList.Add(groupWindow);

            return groupWindow;
        }

        //#GROUP ROI#2 그룹해제
        public bool BreakGroupWindow(GroupWindow groupWindow)
        {
            if (groupWindow is null)
                return false;

            //전체 리스트에서 그룹을 제거
            if(InspWindowList.Contains(groupWindow))
            {
                InspWindowList.Remove(groupWindow);
            }

            //그룹의 개별 윈도우를 전체 리스트에 추가
            foreach (var inspWindow in groupWindow.Members)
            {
                inspWindow.Parent = null;
                InspWindowList.Add(inspWindow);
            }

            return true;
        }

        //#MODEL SAVE#2 모델 생성,열기,저장을 위한 함수 구현

        //신규 모델 생성
        public void CreateModel(string path, string modelName, string modelInfo)
        {
            ModelPath = path;
            ModelName = modelName;
            ModelInfo = modelInfo;
        }

        //모델 로딩함수
        public Model Load(string path)
        {
            Model model = XmlHelper.LoadXml<Model>(path);
            if (model == null)
                return null;

            foreach (var window in model.InspWindowList)
            {
                window.LoadInspWindow(model);
            }

            return model;
        }

        //모델 저장함수
        public void Save()
        {
            if (ModelPath == "")
                return;

            XmlHelper.SaveXml(ModelPath, this);

            foreach(var window in InspWindowList)
            {
                window.SaveInspWindow(this);
            }
        }

        //모델 다른 이름으로 저장함수
        public void SaveAs(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (Directory.Exists(filePath) == false)
            {
                ModelPath = Path.Combine(filePath, fileName + ".xml");
                ModelName = fileName;
                Save();
            }
        }
    }
}
