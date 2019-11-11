using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GxIAPINET;
using GxIAPINET.Common;
using HalconDotNet;
using System.Drawing;
using System.Drawing.Imaging;
using ImageProcess;

namespace Camera
{
    public class GxCamera
    {
        #region 字段
        bool m_bIsGrab = false;
        bool m_bColorFilter = false;
        bool m_bAwbLampHouse = false;
        bool m_bCorrect = false;
        public bool m_bWait = false;

        private ManualResetEvent manualEvent;

        IGXDevice m_objIGXDevice = null;
        IGXStream m_objIGXStream = null;
        IGXFeatureControl m_objIGXFeatureControl = null;
        IImageProcessConfig m_objCfg = null;
        public ImageAlg imagealg = new ImageAlg();
        string m_strPixelColorFilter = null;

        public HTuple hv_PRow, hv_PCol;
        public HObject ho_image;
        public HObject ho_TransImage, ho_zoomImage;
        public HTuple hv_FirstHomMat2D, hv_SecondHomMat2D;
        private Bitmap m_grabBmp = null;
        private Bitmap Canvas = null;
        public Point[] TargetLocation = new Point[2];

        int ArraySize = 0;
        int m_width, m_height;

        byte[] ImageArray;
        #endregion

        #region 属性
        private String str_user_ID;
        public String strUserID
        {
            get { return str_user_ID; }
            set { str_user_ID = value; }
        }
        #endregion

        #region 构造
        public GxCamera()
        {
            manualEvent = new ManualResetEvent(false);
        }
        #endregion

        #region 相机操作
        /// <summary>
        /// 相机初始化
        /// </summary>
        public void InitCamera()
        {
            if (null != m_objIGXFeatureControl)
            {
                //设置采集模式连续采集
                m_objIGXFeatureControl.GetEnumFeature("AcquisitionMode").SetValue("Continuous");

                //设置触发模式为关
                m_objIGXFeatureControl.GetEnumFeature("TriggerMode").SetValue("Off");

                // 判断相机是否支持bayer格式
                m_bColorFilter = m_objIGXFeatureControl.IsImplemented("PixelColorFilter");

                // 获取图像Bayer格式
                if (m_bColorFilter)
                {
                    m_strPixelColorFilter = m_objIGXFeatureControl.GetEnumFeature("PixelColorFilter").GetValue();
                }
                // 判断相机是否支持白平衡光源选择
                m_bAwbLampHouse = m_objIGXFeatureControl.IsImplemented("AWBLampHouse");

            }
        }

        /// <summary>
        /// 打开设备
        /// </summary>
        public void OpenCamera(PictureBox m_pic_ShowImage)
        {
            CloseStream();
            CloseCamera();

            m_objIGXDevice = IGXFactory.GetInstance().OpenDeviceByUserID(strUserID, GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
            m_objIGXFeatureControl = m_objIGXDevice.GetRemoteFeatureControl();

            if (null != m_objIGXDevice)
            {
                m_objIGXStream = m_objIGXDevice.OpenStream(0);
            }

            InitCamera();
            m_objCfg = m_objIGXDevice.CreateImageProcessConfig();
            AllocImageData();
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        public void StartGrab()
        {
            try
            {
                //开启采集流通道
                if (null != m_objIGXStream)
                {
                    //RegisterCaptureCallback第一个参数属于用户自定参数(类型必须为引用
                    //类型)，若用户想用这个参数可以在委托函数中进行使用
                    m_objIGXStream.RegisterCaptureCallback(this, OnFrameCallbackFun);
                    m_objIGXStream.StartGrab();
                }

                //发送开采命令
                if (null != m_objIGXFeatureControl)
                {
                    m_objIGXFeatureControl.GetCommandFeature("AcquisitionStart").Execute();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public void StopGrab()
        {
            try
            {
                //发送停采命令
                if (null != m_objIGXFeatureControl)
                {
                    m_objIGXFeatureControl.GetCommandFeature("AcquisitionStop").Execute();
                }

                //关闭采集流通道
                if (null != m_objIGXStream)
                {
                    m_objIGXStream.StopGrab();
                    //注销采集回调函数
                    m_objIGXStream.UnregisterCaptureCallback();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 关闭流
        /// </summary>
        public void CloseStream()
        {
            try
            {
                //关闭流
                if (null != m_objIGXStream)
                {
                    m_objIGXStream.Close();
                    m_objIGXStream = null;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        public void CloseCamera()
        {
            try
            {
                //关闭设备
                if (null != m_objIGXDevice)
                {
                    m_objIGXDevice.Close();
                    m_objIGXDevice = null;
                }
            }
            catch (Exception)
            {
            }
        }

        public unsafe Bitmap GetImage()
        {
            try
            {
                manualEvent.Reset();
                m_bIsGrab = true;
                bool flag = manualEvent.WaitOne(1000);
                if (flag)
                    return m_grabBmp;
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 停止采集关闭设备、关闭流
        /// </summary>
        public void CloseAll(bool m_bIsSnap)
        {
            try
            {
                //如果未停采则先停止采集
                if (m_bIsSnap)
                {
                    if (null != m_objIGXFeatureControl)
                    {
                        m_objIGXFeatureControl.GetCommandFeature("AcquisitionStop").Execute();
                        m_objIGXFeatureControl = null;
                    }
                }
            }
            catch (Exception)
            {

            }
            try
            {
                //停止流通道、注销采集回调和关闭流
                if (null != m_objIGXStream)
                {
                    m_objIGXStream.StopGrab();
                    //注销采集回调函数
                    m_objIGXStream.UnregisterCaptureCallback();
                    m_objIGXStream.Close();
                    m_objIGXStream = null;
                }

            }
            catch (Exception)
            {

            }

            //关闭设备
            CloseCamera();
        }

        #endregion

        #region 图像处理
        public void StartCorrect(HTuple hv_PCol, HTuple hv_PRow, HTuple picBoxWidth, HTuple picBoxHeight)
        {
            m_bCorrect = true;
            this.hv_PCol = hv_PCol;
            this.hv_PRow = hv_PRow;
            HObject ho_image;
            //HObject ho_TransImage;
            HTuple width;
            HTuple height;

            Bitmap grab = (Bitmap)m_grabBmp.Clone();

            ImageConvert.Bitmap2HObjectBpp24(grab, out ho_image);

            //ho_image.WriteObject("E://Projects//周报//original.bmp");
            
            HOperatorSet.GetImageSize(ho_image, out width, out height);

            HTuple hv_zoomPRow = hv_PRow * (height / (1.0 * picBoxHeight));
            HTuple hv_zoomPCol = hv_PCol * (width / (1.0 * picBoxWidth));
            //MatchAlg.CorrectImage(ho_image, hv_zoomPRow, hv_zoomPCol, out hv_FirstHomMat2D, out hv_SecondHomMat2D);
            Point[] Points = new Point[4];
            for (int i = 0; i < 4; i++)
            {
                Points[i].X = (int)hv_zoomPCol[i].D;
                Points[i].Y = (int)hv_zoomPRow[i].D;
            }

            Dictionary<string, int> TerminalSize = new Dictionary<string, int>();
            Dictionary<string, double> CorrectImageSize = new Dictionary<string, double>();

            TerminalSize.Add("Width", 500);
            TerminalSize.Add("Height", 800);
            CorrectImageSize.Add("Height", 500);

            imagealg.TerminalSize = TerminalSize;
            imagealg.CorrectImageSize = CorrectImageSize;
            imagealg.CalHomMat(grab, Points, ImageAlg.CORRECT_STRATEGY.DIRECT_CORRECT);
        }
        #endregion

        /// <summary>
        /// 采集事件的委托函数
        /// </summary>
        /// <param name="objUserParam">用户私有参数</param>
        /// <param name="objIFrameData">图像信息对象</param>
        int count = 1;
        private void OnFrameCallbackFun(object objUserParam, IFrameData objIFrameData)
        {
            try
            {
                IntPtr pRaw8Buffer = objIFrameData.ConvertToRaw8(GX_VALID_BIT_LIST.GX_BIT_0_7);
                Marshal.Copy(pRaw8Buffer, ImageArray, 0, ArraySize);
                m_grabBmp = CopyToBitmap(m_width, m_height, ImageArray, PixelFormat.Format8bppIndexed);

                if (m_bIsGrab)
                    manualEvent.Set();

                Bitmap grab0 = (Bitmap)m_grabBmp.Clone();
                ShowOriginalImageEvent(grab0);

                if (m_bCorrect == true)
                {
                    Bitmap grab = (Bitmap)m_grabBmp.Clone();
                    CorrectImageEvent(grab);
                }

                if (count % 15 == 0 && m_bWait == true)
                {
                    imagealg.TargetLocation = TargetLocation;
                    WaitResponseEvent();
                    count = 1;
                }
                count += 1;
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 通过GX_PIXEL_FORMAT_ENTRY获取最优Bit位
        /// </summary>
        /// <param name="em">图像数据格式</param>
        /// <returns>最优Bit位</returns>
        private GX_VALID_BIT_LIST __GetBestValudBit(GX_PIXEL_FORMAT_ENTRY emPixelFormatEntry)
        {
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
            switch (emPixelFormatEntry)
            {
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB8:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG8:
                    {
                        emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB10:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG10:
                    {
                        emValidBits = GX_VALID_BIT_LIST.GX_BIT_2_9;
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB12:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG12:
                    {
                        emValidBits = GX_VALID_BIT_LIST.GX_BIT_4_11;
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO14:
                    {
                        //暂时没有这样的数据格式待升级
                        break;
                    }
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_MONO16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GR16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_RG16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_GB16:
                case GX_PIXEL_FORMAT_ENTRY.GX_PIXEL_FORMAT_BAYER_BG16:
                    {
                        //暂时没有这样的数据格式待升级
                        break;
                    }
                default:
                    break;
            }
            return emValidBits;
        }

        private unsafe void AllocImageData()
        {
            IIntFeature objIntWidth = m_objIGXFeatureControl.GetIntFeature("Width");
            m_width = (int)objIntWidth.GetValue();
            IIntFeature objIntHeight = m_objIGXFeatureControl.GetIntFeature("Height");
            m_height = (int)objIntHeight.GetValue();
            ImageArray = new byte[m_width * m_height];
            ArraySize = m_width * m_height;
            Canvas = new Bitmap(m_width, m_height, PixelFormat.Format8bppIndexed);
        }

        private Bitmap CopyToBitmap(int nWidth, int nHeight, Byte[] RawData, PixelFormat pixelFormat)
        {
            try
            {
                BitmapData CanvasData = Canvas.LockBits(new Rectangle(0, 0, nWidth, nHeight), ImageLockMode.WriteOnly, pixelFormat);
                IntPtr ptr = CanvasData.Scan0;
                Marshal.Copy(RawData, 0, ptr, RawData.Length);

                Canvas.UnlockBits(CanvasData);

                if (PixelFormat.Format8bppIndexed == pixelFormat)
                {
                    SetGrayscalePalette(Canvas);
                }
                return Canvas;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void SetGrayscalePalette(Bitmap Image)
        {
            ColorPalette GrayscalePalette = Image.Palette;

            for (int i = 0; i < 256; i++)
            {
                GrayscalePalette.Entries[i] = Color.FromArgb(i, i, i);
            }
            Image.Palette = GrayscalePalette;
        }

        public delegate void ShowOriginalImageEventHandler(Bitmap m_GrabImage);
        public event ShowOriginalImageEventHandler ShowOriginalImageEvent;

        public delegate void CorrectImageEventHandler(Bitmap m_GrabImage);
        public event CorrectImageEventHandler CorrectImageEvent;

        public delegate void WaitResponseHandler();
        public event WaitResponseHandler WaitResponseEvent;

    }

}

