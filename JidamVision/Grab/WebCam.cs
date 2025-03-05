using MvCamCtrl.NET;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JidamVision.Grab
{
    internal class WebCam : GrabModel
    {
        private VideoCapture _capture = null;
        private Mat _frame = null;

        #region Private Field
        private bool _disposed = false;
        #endregion

        #region Method

        internal override bool Create(string strIpAddr = null)
        {
            //_capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW); // 0번 카메라 (기본 웹캠)
            _capture = new VideoCapture(0); // 0번 카메라 (기본 웹캠)
            if (_capture == null)
                return false;

            return true;
        }

        internal override bool Grab(int bufferIndex, bool waitDone)
        {
            if (_frame is null)
                _frame = new Mat();

            _capture.Read(_frame);
            if (!_frame.Empty())
            {
                OnGrabCompleted(BufferIndex);

                int bufSize = (int)(_frame.Total() * _frame.ElemSize());

                if (_userImageBuffer != null && _userImageBuffer.Length > BufferIndex)
                {
                    if (_userImageBuffer[BufferIndex].ImageBuffer.Length >= bufSize)
                    {
                        Marshal.Copy(_frame.Data, _userImageBuffer[BufferIndex].ImageBuffer, 0, bufSize); // Mat의 데이터를 byte 배열로 복사
                    }
                    else
                    {
                        Console.WriteLine("Error: Buffer size is too small.");
                    }
                }

                OnTransferCompleted(BufferIndex);

                if (IncreaseBufferIndex)
                {
                    BufferIndex++;
                    if (BufferIndex >= _userImageBuffer.Count())
                        BufferIndex = 0;
                }
            }
            return true;
        }

        internal override bool Close()
        {
            if (_capture != null)
                _capture.Release();
            
            return true;
        }

        internal override bool Open()
        {
            if (_capture == null)
                return false;

            //if (!_capture.Open(0))
            //{
            //    Console.WriteLine("Failed open the webcam!");
            //    return false;
            //}

            // BGR 포맷을 강제 설정 (산업용 카메라나 특정 드라이버에 따라 가능)

            // BGR24 포맷 (컬러)
            int fourccBGR3 = VideoWriter.FourCC('B', 'G', 'R', '3');
            _capture.Set(VideoCaptureProperties.CodecPixelFormat, fourccBGR3);


            return true;
        }

        internal override bool Reconnect()
        {
            Close();
            return Open();
        }

        internal override bool GetPixelBpp(out int pixelBpp)
        {
            pixelBpp = 8;

            if (_capture == null)
                return false;

            if (_frame is null)
            {
                _frame = new Mat();
                _capture.Read(_frame); // 프레임 캡처
            }

            pixelBpp = _frame.ElemSize() * 8; // 픽셀당 비트수 계산

            return true;
        }
        #endregion

        #region Parameter Setting
        internal override bool SetExposureTime(long exposure)
        {
            if (_capture == null)
                return false;

            _capture.Set(VideoCaptureProperties.Exposure, exposure);
            return true;
        }

        internal override bool GetExposureTime(out long exposure)
        {
            exposure = 0;

            if (_capture == null)
                return false; 
            
            exposure = (long)_capture.Get(VideoCaptureProperties.Exposure);
            return true;
        }

        internal override bool SetGain(long gain)
        {
            if (_capture == null)
                return false;
         
            _capture.Set(VideoCaptureProperties.Gain, gain);
            return true;
        }

        internal override bool GetGain(out long gain)
        {
            gain = 0;
            if (_capture == null)
                return false;

            gain = (long)_capture.Get(VideoCaptureProperties.Gain);
            return true;
        }

        internal override bool GetResolution(out int width, out int height, out int stride)
        {
            width = 0;
            height = 0;
            stride = 0;

            if (_capture == null)
                return false;

            width = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
            height = (int)_capture.Get(VideoCaptureProperties.FrameHeight);

            int bpp = 8;
            GetPixelBpp(out bpp);

            if (bpp == 8)
                stride = width * 1;
            else
                stride = width * 3;

            return true;
        }

        internal override bool SetTriggerMode(bool hardwareTrigger)
        {
            if (_capture is null)
                return false;

            HardwareTrigger = hardwareTrigger;

            return true;
        }

        #endregion

        #region Dispose
        internal override void Dispose()
        {
            Dispose(disposing: true);
        }

        internal void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_capture != null)
                    _capture.Release();
            }
            _disposed = true;
        }

        ~WebCam()
        {
            Dispose(disposing: false);
        }
        #endregion
    }
}
