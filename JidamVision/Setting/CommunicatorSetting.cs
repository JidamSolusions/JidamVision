using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JidamVision.Grab;
using JidamVision.Util;
using JidamVision.Sequence;

namespace JidamVision.Setting
{
    public partial class CommunicatorSetting: UserControl
    {
        public CommunicatorSetting()
        {
            InitializeComponent();

            //최초 로딩시, 환경설정 정보 로딩
            LoadSetting();
        }

        private void LoadSetting()
        {
            cbCommType.DataSource = Enum.GetValues(typeof(CommunicatorType)).Cast<CommunicatorType>().ToList();

            //환경설정에서 현재 통신 타입 얻기
            cbCommType.SelectedIndex = (int)SettingXml.Inst.CommType;

            txtIpAddr.Text = SettingXml.Inst.CommIP;
        }

        private void SaveSetting()
        {
            //환경설정에 통신 타입 설정
            SettingXml.Inst.CommType = (CommunicatorType)cbCommType.SelectedIndex;

            //통신 IP 설정
            SettingXml.Inst.CommIP = txtIpAddr.Text;
            
            //환경설정 저장
            SettingXml.Save();

            SLogger.Write($"통신 설정 저장");
        }

        //적용 버튼 선택시 저장하기
        private void btnApply_Click(object sender, EventArgs e)
        {
            SaveSetting();
        }
    }
}
