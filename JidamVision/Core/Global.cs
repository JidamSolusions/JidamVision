using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace JidamVision.Core
{
    //싱글톤을 이용하여, 전역적으로 쉽게 접근하는 클래스
    public class Global : IDisposable
    {
        #region Singleton Instance
        private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

        public static Global Inst
        {
            get
            {
                return _instance.Value;
            }
        }
        #endregion

        private InspStage _stage = new InspStage();

        public InspStage InspStage
        {
            get { return _stage; }
        }

        public Global()
        {
        }

        public void Initialize()
        {
            _stage.Initialize();

        }

        public void Dispose()
        {



        }
    }
}
