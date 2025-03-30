using JidamVision.Algorithm;
using JidamVision.Core;
using JidamVision.Teach;
using JidamVision.Util;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static JidamVision.Core.ImageSpace;

namespace JidamVision.Inspect
{
    /*
    #INSP WORKER# - <<<검사 알고리즘 통합 및 검사 관리 클래스 추가>>> 
    검사 관리 클래스 : 전체 검사 또는 개별 검사 동작
    검사 알고리즘 추상화
     */

    //검사 관련 처리 클래스
    public class InspWorker
    {
        private readonly ConcurrentQueue<InspWindow> _jobQueue = new ConcurrentQueue<InspWindow>();
        private readonly List<Task> _workerTasks = new List<Task>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private int _threadCount = 1;
        private SynchronizationContext _uiContext;

        private InspectBoard _inspectBoard = new InspectBoard();

        public InspWorker(int threadCount)
        {
            _threadCount = threadCount;
        }

        public InspWorker()
        {
        }

        public void AddJob(InspWindow job)
        {
            _jobQueue.Enqueue(job);
        }

        public void StartAsync()
        {
            _cts = new CancellationTokenSource();

            for (int i = 0; i < _threadCount; i++)
            {
                var task = Task.Run(() => DoWork(_cts.Token));
                _workerTasks.Add(task);
            }
        }
        public void Stop()
        {
            _cts.Cancel();
            Task.WaitAll(_workerTasks.ToArray());
            _workerTasks.Clear();
        }

        private void DoWork(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_jobQueue.TryDequeue(out var inspWindow))
                {
                    RunInspWindow(inspWindow);
                }
                else
                {
                    Thread.Sleep(10); // 잠깐 대기
                }
            }
        }

        private void RunInspWindow(InspWindow inspWindow)
        {
            if (inspWindow == null)
                return;

            if (!UpdateInspData(inspWindow))
                return;

            inspWindow.DoInpsect(InspectType.InspNone);
        }

        public void StartCycleInspectImage()
        {
            _cts = new CancellationTokenSource();
            _uiContext = SynchronizationContext.Current;

            Task.Run(() => InspectionLoop(this, _cts.Token));
        }

        private void InspectionLoop(InspWorker inspWorker, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Global.Inst.InspStage.OneCycle();

                //Thread.Sleep(200); // 주기 설정
            }
        }

        //#INSP WORKER#2 InspStage내의 모든 InspWindow들을 검사하는 함수
        public bool RunInspect(out bool isDefect)
        {
            isDefect = false;
            Model curMode = Global.Inst.InspStage.CurModel;
            List<InspWindow> inspWindowList = curMode.InspWindowList;
            foreach (var inspWindow in inspWindowList)
            {
                if (inspWindow is null)
                    continue;

                UpdateInspData(inspWindow);
            }

            _inspectBoard.InspectWindowList(inspWindowList);

            foreach (var inspWindow in inspWindowList)
            {
                if (!isDefect && inspWindow.IsDefect())
                    isDefect = true;

                DisplayResult(inspWindow, InspectType.InspNone);
            }

            return true;
        }

        //#INSP WORKER#5 특정 InspWindow에 대한 검사 진행
        //inspType이 있다면 그것만을 검사하고, 없다면 InpsWindow내의 모든 알고리즘 검사
        public bool TryInspect(InspWindow inspObj, InspectType inspType)
        {
            if (inspObj != null)
            {
                if (!UpdateInspData(inspObj))
                    return false;

                _inspectBoard.Inspect(inspObj);

                DisplayResult(inspObj, inspType);
            }
            else
            {
                bool isDefect = false;
                RunInspect(out isDefect);
            }

            ResultForm resultForm = MainForm.GetDockForm<ResultForm>();
            if (resultForm != null)
            {
                if (inspObj != null)
                    resultForm.AddWindowResult(inspObj);
                else
                {
                    Model curMode = Global.Inst.InspStage.CurModel;
                    resultForm.AddModelResult(curMode);
                }
            }

            return true;
        }

        //#INSP WORKER#3 각 알고리즘 타입 별로 검사에 필요한 데이터를 입력하는 함수
        private bool UpdateInspData(InspWindow inspWindow)
        {
            if (inspWindow is null)
                return false;

            Rect windowArea = inspWindow.WindowArea;

            inspWindow.PatternLearn();

            foreach (var inspAlgo in inspWindow.AlgorithmList)
            {
                //검사 영역 초기화
                inspAlgo.TeachRect = windowArea;
                inspAlgo.InspRect = windowArea;

                InspectType inspType = inspAlgo.InspectType;

                switch (inspType)
                {
                    case InspectType.InspBinary:
                        {
                            BlobAlgorithm blobAlgo = (BlobAlgorithm)inspAlgo;

                            Mat srcImage = Global.Inst.InspStage.GetMat(0, blobAlgo.ImageChannel);
                            blobAlgo.SetInspData(srcImage);
                            break;
                        }

                    case InspectType.InspMatch:
                        {
                            MatchAlgorithm matchAlgo = (MatchAlgorithm)inspAlgo;

                            Mat srcImage = Global.Inst.InspStage.GetMat(0, matchAlgo.ImageChannel);
                            matchAlgo.SetInspData(srcImage);
                            break;
                        }
                    default:
                        {
                            SLogger.Write($"Not support inspection type : {inspType}", SLogger.LogType.Error);
                            return false;
                        }
                }
            }

            return true;
        }

        //#INSP WORKER#4 InspWindow내의 알고리즘 중에서, 인자로 입력된 알고리즘과 같거나,
        //인자가 None이면 모든 알고리즘의 검사 결과(Rect 영역)를 얻어, cameraForm에 출력한다.
        private bool DisplayResult(InspWindow inspObj, InspectType inspType)
        {
            if (inspObj is null)
                return false;

            List<Rect> totalArea = new List<Rect>();

            List<InspAlgorithm> inspAlgorithmList = inspObj.AlgorithmList;
            foreach (var algorithm in inspAlgorithmList)
            {
                if (algorithm.InspectType != inspType && inspType != InspectType.InspNone)
                    continue;

                List<Rect> resultArea = new List<Rect>();
                int resultCnt = algorithm.GetResultRect(out resultArea);
                if (resultCnt > 0)
                {
                    totalArea.AddRange(resultArea);
                }
            }

            if (totalArea.Count > 0)
            {
                //찾은 위치를 이미지상에서 표시
                var cameraForm = MainForm.GetDockForm<CameraForm>();
                if (cameraForm != null)
                {
                    cameraForm.AddRect(totalArea);
                }
            }

            return true;
        }
    }
}
