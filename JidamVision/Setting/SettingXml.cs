using Common.Util.Helpers;
using JidamVision.Grab;
using JidamVision.Sequence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace JidamVision.Setting
{
    /*
    #SETUP# - <<<환경설정 정보를 저장하기 위한 클래스>>> 
    환정설정 정보를 XML방식으로 저장하고, Singleton 방식으로 어디서나 호출하여 사용
    환경정설 파일은 실행파일 폴더 안에 Setup/Setting.xml로 저장
    1)프로젝트 내 Util/XmlHelper.cs 파일 복사하여 사용할것
    2)프로젝트 내에 Setting 폴더를 생성
    3)WindowForm SetupForm 이름으로 생성
    4)UserControl CameraSetting 이름으로 생성
    5)UserControl PathSetting 이름으로 생성
    */

    //#SETUP#1 환경설정 클래스 구현 
    public class SettingXml
    {
        //환경설정 파일 저장 경로
        private const string SETTING_DIR = "Setup";
        private const string SETTING_FILE_NAME = @"Setup\Setting.xml";

        #region Singleton Instance
        private static SettingXml _setting;

        public static SettingXml Inst
        {
            get
            {
                if (_setting is null)
                    Load();

                return _setting;
            }
        }
        #endregion

        //환경설정 로딩
        public static void Load()
        {
            if (_setting != null)
                return;

            //환경설정 경로 생성
            string settingFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, SETTING_FILE_NAME);
            if (File.Exists(settingFilePath) == true)
            {
                //환경설정 파일이 있다면 XmlHelper를 이용해 로딩
                _setting = XmlHelper.LoadXml<SettingXml>(settingFilePath);
            }

            if(_setting is null)
            {
                //환경설정 파일이 없다면 새로 생성
                _setting = CreateDefaultInstance();
            }
        }

        //환경설정 저장
        public static void Save()
        {
            string settingFilePath = Path.Combine(Environment.CurrentDirectory, SETTING_FILE_NAME);
            if (!File.Exists(settingFilePath))
            {
                //Setup 폴더가 없다면 생성
                string setupDir = Path.Combine(Environment.CurrentDirectory, SETTING_DIR);

                if (!Directory.Exists(setupDir))
                    Directory.CreateDirectory(setupDir);

                //Setting.xml 파일이 없다면 생성
                FileStream fs = File.Create(settingFilePath);
                fs.Close();
            }

            //XmlHelper를 이용해 Xml로 환경설정 정보 저장
            XmlHelper.SaveXml(settingFilePath, Inst);
        }

        //최초 환경설정 파일 생성
        private static SettingXml CreateDefaultInstance()
        {
            SettingXml setting = new SettingXml();
            setting.ModelDir = @"d:\model";
            return setting;
        }



        public SettingXml() { }

        public string MachineName { get; set; } = "Jidam";

        public string ModelDir { get; set; } = "";

        public CameraType CamType { get; set; } = CameraType.WebCam;

        public CommunicatorType CommType { get; set; }
        public string CommIP {  get; set; }

    }
}
