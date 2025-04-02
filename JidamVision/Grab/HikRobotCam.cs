using JidamVision.Util;
using MvCamCtrl.NET;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MvCamCtrl.NET.MyCamera;

namespace JidamVision.Grab
{

    internal class HikRobotCam : GrabModel
    {
        private cbOutputExdelegate ImageCallback;

        private MyCamera _camera = null;

        private void ImageCallbackFunc(IntPtr pData, ref MV_FRAME_OUT_INFO_EX pFrameInfo, IntPtr pUser)
        {
            Console.WriteLine("Get one frame: Width[" + Convert.ToString(pFrameInfo.nWidth) + "] , Height[" + Convert.ToString(pFrameInfo.nHeight)
                                + "] , FrameNum[" + Convert.ToString(pFrameInfo.nFrameNum) + "]");

            OnGrabCompleted(BufferIndex);

            if (_userImageBuffer[BufferIndex].ImageBuffer != null)
            {
                if (pFrameInfo.enPixelType == MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    if (_userImageBuffer[BufferIndex].ImageBuffer != null)
                        Marshal.Copy(pData, _userImageBuffer[BufferIndex].ImageBuffer, 0, (int)pFrameInfo.nFrameLen);
                }
                else
                {
                    MV_PIXEL_CONVERT_PARAM _pixelConvertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                    _pixelConvertParam.nWidth = pFrameInfo.nWidth;
                    _pixelConvertParam.nHeight = pFrameInfo.nHeight;
                    _pixelConvertParam.pSrcData = pData;
                    _pixelConvertParam.nSrcDataLen = pFrameInfo.nFrameLen;
                    _pixelConvertParam.enSrcPixelType = pFrameInfo.enPixelType;
                    _pixelConvertParam.enDstPixelType = MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                    _pixelConvertParam.pDstBuffer = _userImageBuffer[BufferIndex].ImageBufferPtr;
                    _pixelConvertParam.nDstBufferSize = pFrameInfo.nFrameLen * 3;

                    int nRet = _camera.MV_CC_ConvertPixelType_NET(ref _pixelConvertParam);
                    if (MyCamera.MV_OK != nRet)
                    {
                        SLogger.Write($"Convert pixel type Failed! [{nRet:x8}]",SLogger.LogType.Error);
                        return;
                    }
                }
            }

            _grabDoneEvent.Set(); // 이벤트 발생시킴
            OnTransferCompleted(BufferIndex);

            //IO 트리거 촬상시 최대 버퍼를 넘으면 첫번째 버퍼로 변경
            if (IncreaseBufferIndex)
            {
                BufferIndex++;
                if (BufferIndex >= _userImageBuffer.Count())
                    BufferIndex = 0;
            }
        }



        #region Private Field
        private bool _disposed = false;
        #endregion

        #region Method


        internal override bool Create(string strIpAddr = null)
        {
            Environment.SetEnvironmentVariable("PYLON_GIGE_HEARTBEAT", "5000" /*ms*/);

            _strIpAddr = strIpAddr;

            try
            {
                Int32 nDevIndex = 0;

                int nRet = MyCamera.MV_OK;

                // Enum deivce
                MyCamera.MV_CC_DEVICE_INFO_LIST stDevList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
                nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE, ref stDevList);
                if (MyCamera.MV_OK != nRet)
                {
                    SLogger.Write($"Enum device failed! [{nRet:x8}]",SLogger.LogType.Error);
                    return false;
                }
                Console.WriteLine("Enum device count :{0}", stDevList.nDeviceNum);
                if (0 == stDevList.nDeviceNum)
                {
                    return false;
                }

                MyCamera.MV_CC_DEVICE_INFO stDevInfo;

                // ch:打印设备信息 | en:Print device info
                for (Int32 i = 0; i < stDevList.nDeviceNum; i++)
                {
                    stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));

                    if (MyCamera.MV_GIGE_DEVICE == stDevInfo.nTLayerType)
                    {
                        MyCamera.MV_GIGE_DEVICE_INFO stGigEDeviceInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(stDevInfo.SpecialInfo.stGigEInfo, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                        uint nIp1 = ((stGigEDeviceInfo.nCurrentIp & 0xff000000) >> 24);
                        uint nIp2 = ((stGigEDeviceInfo.nCurrentIp & 0x00ff0000) >> 16);
                        uint nIp3 = ((stGigEDeviceInfo.nCurrentIp & 0x0000ff00) >> 8);
                        uint nIp4 = (stGigEDeviceInfo.nCurrentIp & 0x000000ff);

                        SLogger.Write($"[device {i}]:");
                        SLogger.Write($"DevIP:{nIp1}.{nIp2}.{nIp3}.{nIp4}");
                        SLogger.Write("UserDefineName:" + stGigEDeviceInfo.chUserDefinedName);

                        string strDevice = "[device " + i.ToString() + "]:";
                        string strIP = nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4;


                        if (strIP == strIpAddr)
                        {
                            nDevIndex = i;
                            break;
                        }
                    }
                }

                if (nDevIndex < 0 || nDevIndex > stDevList.nDeviceNum - 1)
                {
                    SLogger.Write($"Invalid selected device number:{nDevIndex}",SLogger.LogType.Error);
                    return false;
                }

                // Open device
                if (_camera == null)
                {
                    _camera = new MyCamera();
                }

                stDevInfo = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(stDevList.pDeviceInfo[nDevIndex], typeof(MyCamera.MV_CC_DEVICE_INFO));

                // Create device
                nRet = _camera.MV_CC_CreateDevice_NET(ref stDevInfo);
                if (MyCamera.MV_OK != nRet)
                {
                    SLogger.Write($"Create device failed! [{nRet:x8}]");
                    return false;
                }

                _disposed = false;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ex.ToString();
                return false;
            }
            return true;
        }

        internal override bool Grab(int bufferIndex, bool waitDone)
        {
            if (_camera == null)
                return false;

            BufferIndex = bufferIndex;
            bool err = true;

            _grabDoneEvent.Reset();

            if (!HardwareTrigger)
            {
                try
                {
                    int nRet = _camera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                    if (MyCamera.MV_OK != nRet)
                    {
                        err = false;
                    }
                }
                catch
                {
                    err = false;
                }
            }

            if(waitDone)
            {
                // 콜백이 호출될 때까지 최대 3초 대기
                if (!_grabDoneEvent.WaitOne(3000))
                {
                    Console.WriteLine("Grab timeout!");
                    return false;
                }
            }

            return err;
        }

        internal override bool Close()
        {
            if (_camera != null)
            {
                _camera.MV_CC_StopGrabbing_NET();
                _camera.MV_CC_CloseDevice_NET();
            }

            return true;
        }

        internal override bool Open()
        {
            try
            {
                if (_camera == null)
                    return false;

                if (!_camera.MV_CC_IsDeviceConnected_NET())
                {
                    int nRet = _camera.MV_CC_OpenDevice_NET();
                    if (MyCamera.MV_OK != nRet)
                    {
                        _camera.MV_CC_DestroyDevice_NET();
                        SLogger.Write($"Device open fail! [{nRet:x8}]",SLogger.LogType.Error);
                        MessageBox.Show($"Device open fail! {nRet:X8}");
                        return false;
                    }

                    //Detection network optimal package size(It only works for the GigE camera)
                    int nPacketSize = _camera.MV_CC_GetOptimalPacketSize_NET();
                    if (nPacketSize > 0)
                    {
                        nRet = _camera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                        if (nRet != MyCamera.MV_OK)
                        {
                            SLogger.Write($"Set Packet Size failed! [{nRet:x8}]", SLogger.LogType.Error);
                        }
                    }

                    _camera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)MyCamera.MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_ON);

                    if (HardwareTrigger)
                    {
                        _camera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
                    }
                    else
                    {
                        _camera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
                    }

                    //Register image callback
                    ImageCallback = new cbOutputExdelegate(ImageCallbackFunc);
                    nRet = _camera.MV_CC_RegisterImageCallBackEx_NET(ImageCallback, IntPtr.Zero);
                    if (MyCamera.MV_OK != nRet)
                    {
                        SLogger.Write($"Register image callback failed! [{nRet:x8}]", SLogger.LogType.Error);
                        return false;
                    }

                    // start grab image
                    nRet = _camera.MV_CC_StartGrabbing_NET();
                    if (MyCamera.MV_OK != nRet)
                    {
                        SLogger.Write($"Start grabbing failed! [{nRet:x8}]", SLogger.LogType.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                SLogger.Write(ex.ToString(),SLogger.LogType.Error);
                return false;
            }

            return true;
        }

        internal override bool Reconnect()
        {
            if (_camera is null)
            {
                SLogger.Write("_camera is null",SLogger.LogType.Error);
                return false;
            }
            Close();

            return Open();
        }

        internal override bool GetPixelBpp(out int pixelBpp)
        {
            pixelBpp = 8;
            if (_camera == null)
                return false;

            //Get Pixel Format
            MyCamera.MVCC_ENUMVALUE stEnumValue = new MyCamera.MVCC_ENUMVALUE();
            int nRet = _camera.MV_CC_GetEnumValue_NET("PixelFormat", ref stEnumValue);
            if (MyCamera.MV_OK != nRet)
            {
                SLogger.Write($"Get PixelFormat failed! [{nRet:x8}]", SLogger.LogType.Error);
                return false;
            }

            MyCamera.MvGvspPixelType ePixelFormat = (MyCamera.MvGvspPixelType)stEnumValue.nCurValue;

            if (ePixelFormat == MvGvspPixelType.PixelType_Gvsp_Mono8)
                pixelBpp = 8;
            else
                pixelBpp = 24;

            return true;
        }
        #endregion

        #region Parameter Setting
        internal override bool SetExposureTime(long exposure)
        {
            if (_camera == null)
                return false;

            _camera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);

            int nRet = _camera.MV_CC_SetFloatValue_NET("ExposureTime", exposure);
            if (nRet != MyCamera.MV_OK)
            {
                SLogger.Write($"Set Exposure Time Fail! [{nRet:x8}]", SLogger.LogType.Error);
                return false;
            }

            return true;
        }

        internal override bool GetExposureTime(out long exposure)
        {
            exposure = 0;
            if (_camera == null)
                return false;

            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = _camera.MV_CC_GetFloatValue_NET("ExposureTime", ref stParam);
            if (MyCamera.MV_OK == nRet)
            {
                exposure = (long)stParam.fCurValue;
            }
            return true;
        }

        internal override bool SetGain(float gain)
        {
            if (_camera == null)
                return false;

            _camera.MV_CC_SetEnumValue_NET("GainAuto", 0);

            int nRet = _camera.MV_CC_SetFloatValue_NET("Gain", gain);
            if (nRet != MyCamera.MV_OK)
            {
                SLogger.Write($"Set Gain Time Fail! [{nRet:x8}]", SLogger.LogType.Error);
                return false;
            }

            return true;
        }

        internal override bool GetGain(out float gain)
        {
            gain = 0;
            if (_camera == null)
                return false;

            MyCamera.MVCC_FLOATVALUE stParam = new MyCamera.MVCC_FLOATVALUE();
            int nRet = _camera.MV_CC_GetFloatValue_NET("Gain", ref stParam);
            if (MyCamera.MV_OK == nRet)
            {
                gain = (long)stParam.fCurValue;
            }
            return true;
        }

        internal override bool GetResolution(out int width, out int height, out int stride)
        {
            width = 0;
            height = 0;
            stride = 0;

            if (_camera == null)
                return false;

            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            int nRet = _camera.MV_CC_GetIntValue_NET("Width", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                SLogger.Write($"Get Width Fail! [{nRet:x8}]", SLogger.LogType.Error);
                return false;
            }
            width = (ushort)stParam.nCurValue;

            nRet = _camera.MV_CC_GetIntValue_NET("Height", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                SLogger.Write($"Get Height Fail! [{nRet:x8}]", SLogger.LogType.Error);
                return false;
            }
            height = (ushort)stParam.nCurValue;

            MyCamera.MVCC_ENUMVALUE stEnumValue = new MyCamera.MVCC_ENUMVALUE();
            nRet = _camera.MV_CC_GetEnumValue_NET("PixelFormat", ref stEnumValue);
            if (MyCamera.MV_OK != nRet)
            {
                SLogger.Write($"Get PixelFormat Fail! [{nRet:x8}]", SLogger.LogType.Error);
                return false;
            }

            if ((MvGvspPixelType)stEnumValue.nCurValue == MvGvspPixelType.PixelType_Gvsp_Mono8)
                stride = width * 1;
            else
                stride = width * 3;

            return true;
        }

        internal override bool SetTriggerMode(bool hardwareTrigger)
        {
            if (_camera is null)
                return false;

            HardwareTrigger = hardwareTrigger;

            if (HardwareTrigger)
            {
                _camera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_LINE0);
            }
            else
            {
                _camera.MV_CC_SetEnumValue_NET("TriggerSource", (uint)MyCamera.MV_CAM_TRIGGER_SOURCE.MV_TRIGGER_SOURCE_SOFTWARE);
            }

            return true;
        }

        internal override bool SetWhiteBalance(bool auto, float redGain = 1.0f, float blueGain = 1.0f)
        {
            if (_camera == null)
                return false;

            if (auto)
            {
                int nRet = _camera.MV_CC_SetBalanceWhiteAuto_NET(1);

                //// 자동 화이트 밸런스 설정
                //int nRet = _camera.MV_CC_SetEnumValue_NET("BalanceWhiteAuto",
                //    (uint)MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_ONCE);

                if (MyCamera.MV_OK != nRet)
                {
                    SLogger.Write("Failed to enable auto white balance!",SLogger.LogType.Error);
                    return false;
                }

                // **화이트 밸런스 적용을 위한 추가 트리거**
                _camera.MV_CC_SetCommandValue_NET("TriggerSoftware");
            }
            else
            {
                // 수동 모드로 설정
                int nRet = _camera.MV_CC_SetEnumValue_NET("BalanceWhiteAuto",
                    (uint)MyCamera.MV_CAM_BALANCEWHITE_AUTO.MV_BALANCEWHITE_AUTO_OFF);

                if (MyCamera.MV_OK != nRet)
                {
                    SLogger.Write("Failed to disable auto white balance!", SLogger.LogType.Error);
                    return false;
                }

                // 수동 모드에서 Red 및 Blue 게인 설정
                nRet = _camera.MV_CC_SetFloatValue_NET("BalanceRatioRed", redGain);
                if (MyCamera.MV_OK != nRet)
                {
                    SLogger.Write("Failed to set Red gain!",SLogger.LogType.Error);
                    return false;
                }

                nRet = _camera.MV_CC_SetFloatValue_NET("BalanceRatioBlue", blueGain);
                if (MyCamera.MV_OK != nRet)
                {
                    SLogger.Write("Failed to set Blue gain!",SLogger.LogType.Error);
                    return false;
                }
            }

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
                Close();

                if(_camera != null)
                    _camera.MV_CC_DestroyDevice_NET();
            }
            _disposed = true;
        }

        ~HikRobotCam()
        {
            Dispose(disposing: true);
        }
        #endregion
    }
}
