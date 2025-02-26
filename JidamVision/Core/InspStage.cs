using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JidamVision.Core
{
    //검사와 관련된 클래스를 관리하는 클래스
    public class InspStage
    {
        public static readonly int MAX_GRAB_BUF = 5;

        private ImageSpace _imageSpace = null;

        public ImageSpace ImageSpace
        {
            get => _imageSpace;
        }

        public bool LiveMode { get; set; } = false;

        public InspStage() { }

        public bool Initialize()
        {
            _imageSpace = new ImageSpace();

            //if (_grabManager.InitGrab() == true)
            //{
            //    _grabManager.TransferCompleted += _multiGrab_TransferCompleted;

            //    InitModelGrab(MAX_GRAB_BUF);
            //}

            return true;
        }


        public void InitModelGrab(int bufferCount)
        {
            //if (_grabManager == null)
            //    return;

            //int pixelBpp = 8;
            //_grabManager.GetPixelBpp(out pixelBpp);

            //int inspectionWidth;
            //int inspectionHeight;
            //int inspectionStride;
            //_grabManager.GetResolution(out inspectionWidth, out inspectionHeight, out inspectionStride);

            //if (_imageSpace != null)
            //{
            //    _imageSpace.SetImageInfo(pixelBpp, inspectionWidth, inspectionHeight, inspectionStride);
            //}

            //SetBuffer(bufferCount);

            //_grabManager.SetExposureTime(25000);

        }

        public void SetBuffer(int bufferCount)
        {
            //if (_grabManager == null)
            //    return;

            //if (_imageSpace.BufferCount == bufferCount)
            //    return;

            //_imageSpace.InitImageSpace(bufferCount);
            //_grabManager.InitBuffer(bufferCount);

            //for (int i = 0; i < bufferCount; i++)
            //{
            //    _grabManager.SetBuffer(
            //        _imageSpace.GetInspectionBuffer(i),
            //        _imageSpace.GetnspectionBufferPtr(i),
            //        _imageSpace.GetInspectionBufferHandle(i),
            //        i);
            //}
        }

        public void Grab(int bufferIndex)
        {
            //if (_grabManager == null)
            //    return;

            //_grabManager.Grab(bufferIndex, true);
        }

        private void DisplayGrabImage(int bufferIndex)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.UpdateDisplay();
            }
        }

    }
}
