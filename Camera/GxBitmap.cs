using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using GxIAPINET;

namespace GxIAPINET.Common
{
    public class GxBitmap
    {

        IGXDevice m_objIGXDevice = null;                ///<设备对像
        PictureBox m_pic_ShowImage = null;                ///<图片显示控件
        bool m_bIsColor = false;               ///<是否支持彩色相机
        byte[] m_byMonoBuffer = null;                ///<黑白相机buffer
        byte[] m_byColorBuffer = null;                ///<彩色相机buffer
        byte[] m_byRawBuffer = null;                ///<用于存储Raw图的Buffer
        int m_nPayloadSize = 0;                   ///<图像数据大小
        int m_nWidth = 0;                   ///<图像宽度
        int m_nHeigh = 0;                   ///<图像高度
        Bitmap m_bitmap = null;                ///<bitmap对象
        Bitmap m_Showbitmap = null;                ///<要显示的bitmap对象
        const uint PIXEL_FORMATE_BIT = 0x00FF0000;          ///<用于与当前的数据格式进行与运算得到当前的数据位数
        const uint GX_PIXEL_8BIT = 0x00080000;          ///<8位数据图像格式

        /// <summary>
        /// 构造函数用于初始化设备对象与PictureBox控件对象
        /// </summary>
        /// <param name="objIGXDevice">设备对象</param>
        /// <param name="objPictureBox">图像显示控件</param>
        public GxBitmap(IGXDevice objIGXDevice, PictureBox objPictureBox)
        {
            m_objIGXDevice = objIGXDevice;
            m_pic_ShowImage = objPictureBox;
            string strValue = null;
            if (null != objIGXDevice)
            {
                //获得图像原始数据大小、宽度、高度等
                m_nPayloadSize = (int)objIGXDevice.GetRemoteFeatureControl().GetIntFeature("PayloadSize").GetValue();
                m_nWidth = (int)objIGXDevice.GetRemoteFeatureControl().GetIntFeature("Width").GetValue();
                m_nHeigh = (int)objIGXDevice.GetRemoteFeatureControl().GetIntFeature("Height").GetValue();

                //获取是否为彩色相机
                if (objIGXDevice.GetRemoteFeatureControl().IsImplemented("PixelColorFilter"))
                {
                    strValue = objIGXDevice.GetRemoteFeatureControl().GetEnumFeature("PixelColorFilter").GetValue();

                    if ("None" != strValue)
                    {
                        m_bIsColor = true;
                    }
                }
            }

            //申请用于缓存图像数据的buffer
            m_byRawBuffer = new byte[m_nPayloadSize];
            m_byMonoBuffer = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];
            m_byColorBuffer = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];

            __CreateBitmap(out m_bitmap, m_nWidth, m_nHeigh, m_bIsColor);

            if (null != objPictureBox)
            {
                __CreateBitmap(out m_Showbitmap, m_nWidth, m_nHeigh, m_bIsColor);
            }
        }

        /// <summary>
        /// 用于图像处理后并显示图像
        /// </summary>
        /// <param name="objCfg">图像处理参数配置对象</param>
        /// <param name="objIBaseData">图像数据对象</param>
        public void ShowImageProcess(IImageProcessConfig objCfg, IBaseData objIBaseData)
        {
            //检查图像是否改变并更新Buffer
            __UpdateBufferSize(objIBaseData);

            if (null != objIBaseData)
            {
                if (GX_FRAME_STATUS_LIST.GX_FRAME_STATUS_SUCCESS == objIBaseData.GetStatus())
                {
                    if (m_bIsColor)
                    {
                        IntPtr pBufferColor = objIBaseData.ImageProcess(objCfg);
                        Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
                        __ShowImage(m_byColorBuffer);
                    }
                    else
                    {
                        IntPtr pBufferMono = objIBaseData.ImageProcess(objCfg);
                        Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
                        __ShowImage(m_byMonoBuffer);
                    }
                }
            }
        }


        /// <summary>
        /// 用于显示图像
        /// </summary>
        /// <param name="objIBaseData">图像数据对象</param>
        public void Show(IBaseData objIBaseData)
        {
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;

            //检查图像是否改变并更新Buffer
            __UpdateBufferSize(objIBaseData);


            if (null != objIBaseData)
            {
                emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                if (GX_FRAME_STATUS_LIST.GX_FRAME_STATUS_SUCCESS == objIBaseData.GetStatus())
                {
                    if (m_bIsColor)
                    {
                        IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);
                        Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
                        __ShowImage(m_byColorBuffer);
                    }
                    else
                    {
                        IntPtr pBufferMono = IntPtr.Zero;
                        if (__IsPixelFormat8(objIBaseData.GetPixelFormat()))
                        {
                            pBufferMono = objIBaseData.GetBuffer();
                        }
                        else
                        {
                            pBufferMono = objIBaseData.ConvertToRaw8(emValidBits);
                        }
                        Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
                        __ShowImage(m_byMonoBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// 存储图像
        /// </summary>
        /// <param name="objIBaseData">图像数据对象</param>
        /// <param name="strFilePath">显示图像文件名</param>
        public void SaveBmp(IBaseData objIBaseData, string strFilePath)
        {
            GX_VALID_BIT_LIST emValidBits = GX_VALID_BIT_LIST.GX_BIT_0_7;

            //检查图像是否改变并更新Buffer
            __UpdateBufferSize(objIBaseData);

            if (null != objIBaseData)
            {
                emValidBits = __GetBestValudBit(objIBaseData.GetPixelFormat());
                if (m_bIsColor)
                {
                    IntPtr pBufferColor = objIBaseData.ConvertToRGB24(emValidBits, GX_BAYER_CONVERT_TYPE_LIST.GX_RAW2RGB_NEIGHBOUR, false);
                    Marshal.Copy(pBufferColor, m_byColorBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
                    __UpdateImage(m_byColorBuffer);
                }
                else
                {
                    IntPtr pBufferMono = IntPtr.Zero;
                    if (__IsPixelFormat8(objIBaseData.GetPixelFormat()))
                    {
                        pBufferMono = objIBaseData.GetBuffer();
                    }
                    else
                    {
                        pBufferMono = objIBaseData.ConvertToRaw8(emValidBits);
                    }
                    Marshal.Copy(pBufferMono, m_byMonoBuffer, 0, __GetStride(m_nWidth, m_bIsColor) * m_nHeigh);
                    __UpdateImage(m_byMonoBuffer);
                }
                m_bitmap.Save(strFilePath, ImageFormat.Bmp);
            }
        }

        /// <summary>
        /// 存储Raw图像
        /// </summary>
        /// <param name="objIBaseData">图像数据对象</param>
        /// <param name="strFilePath">显示图像文件名</param>
        public void SaveRaw(IBaseData objIBaseData, string strFilePath)
        {
            Stream objFileStream = new FileStream(strFilePath, FileMode.Create);
            BinaryWriter objSW = new BinaryWriter(objFileStream);

            //检查图像是否改变并更新Buffer
            __UpdateBufferSize(objIBaseData);

            if (null != objIBaseData)
            {
                IntPtr pBufferRaw = objIBaseData.GetBuffer();
                Marshal.Copy(pBufferRaw, m_byRawBuffer, 0, m_nPayloadSize);
            }

            objSW.Write(m_byRawBuffer);
            objSW.Close();
            objFileStream.Close();
        }

        /// <summary>
        /// 检查图像是否改变并更新Buffer
        /// </summary>
        /// <param name="objIBaseData">图像数据对象</param>
        private void __UpdateBufferSize(IBaseData objIBaseData)
        {
            if (null != objIBaseData)
            {
                if (__IsCompatible(m_bitmap, m_nWidth, m_nHeigh, m_bIsColor))
                {
                    m_nPayloadSize = (int)objIBaseData.GetPayloadSize();
                    m_nWidth = (int)objIBaseData.GetWidth();
                    m_nHeigh = (int)objIBaseData.GetHeight();
                }
                else
                {
                    m_nPayloadSize = (int)objIBaseData.GetPayloadSize();
                    m_nWidth = (int)objIBaseData.GetWidth();
                    m_nHeigh = (int)objIBaseData.GetHeight();

                    m_byRawBuffer = new byte[m_nPayloadSize];
                    m_byMonoBuffer = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];
                    m_byColorBuffer = new byte[__GetStride(m_nWidth, m_bIsColor) * m_nHeigh];
                }
            }
        }

        /// <summary>
        /// 更新存储数据
        /// </summary>
        /// <param name="byBuffer">图像buffer</param>
        private void __UpdateImage(byte[] byBuffer)
        {
            if (__IsCompatible(m_bitmap, m_nWidth, m_nHeigh, m_bIsColor))
            {
                __UpdateBitmap(m_bitmap, byBuffer, m_nWidth, m_nHeigh, m_bIsColor);
            }
            else
            {
                __CreateBitmap(out m_bitmap, m_nWidth, m_nHeigh, m_bIsColor);
                __UpdateBitmap(m_bitmap, byBuffer, m_nWidth, m_nHeigh, m_bIsColor);
            }
        }

        /// <summary>
        /// 显示图像处理
        /// </summary>
        /// <param name="byBuffer">图像数据buffer</param>
        private void __ShowImage(byte[] byBuffer)
        {
            //更新存储数据
            __UpdateImage(byBuffer);

            if (null != m_pic_ShowImage)
            {
                m_Showbitmap = new Bitmap(m_bitmap, m_pic_ShowImage.Width, m_pic_ShowImage.Height);

                Bitmap bitmap = m_pic_ShowImage.Image as Bitmap;

                m_pic_ShowImage.Image = m_Showbitmap;
                if (bitmap != null)
                {
                    bitmap.Dispose();
                }

                m_pic_ShowImage.Refresh();
            }
        }

        /// <summary>
        /// 判断PixelFormat是否为8位
        /// </summary>
        /// <param name="emPixelFormatEntry">图像数据格式</param>
        /// <returns>true为8为数据，false为非8位数据</returns>
        private bool __IsPixelFormat8(GX_PIXEL_FORMAT_ENTRY emPixelFormatEntry)
        {
            bool bIsPixelFormat8 = false;
            uint uiPixelFormatEntry = (uint)emPixelFormatEntry;
            if ((uiPixelFormatEntry & PIXEL_FORMATE_BIT) == GX_PIXEL_8BIT)
            {
                bIsPixelFormat8 = true;
            }
            return bIsPixelFormat8;
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

        /// <summary>
        /// 获取图像显示格式
        /// </summary>
        /// <param name="bIsColor">是否为彩色相机</param>
        /// <returns>图像的数据格式</returns>
        private PixelFormat __GetFormat(bool bIsColor)
        {
            return bIsColor ? PixelFormat.Format24bppRgb : PixelFormat.Format8bppIndexed;
        }

        /// <summary>
        /// 计算宽度所占的字节数
        /// </summary>
        /// <param name="nWidth">图像宽度</param>
        /// <param name="bIsColor">是否是彩色相机</param>
        /// <returns>图像一行所占的字节数</returns>
        private int __GetStride(int nWidth, bool bIsColor)
        {
            return bIsColor ? nWidth * 3 : nWidth;
        }

        /// <summary>
        /// 判断是否兼容
        /// </summary>
        /// <param name="bitmap">Bitmap对象</param>
        /// <param name="nWidth">图像宽度</param>
        /// <param name="nHeight">图像高度</param>
        /// <param name="bIsColor">是否是彩色相机</param>
        /// <returns>true为一样，false不一样</returns>
        private bool __IsCompatible(Bitmap bitmap, int nWidth, int nHeight, bool bIsColor)
        {
            if (bitmap == null
                || bitmap.Height != nHeight
                || bitmap.Width != nWidth
                || bitmap.PixelFormat != __GetFormat(bIsColor)
             )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 创建Bitmap
        /// </summary>
        /// <param name="bitmap">Bitmap对象</param>
        /// <param name="nWidth">图像宽度</param>
        /// <param name="nHeight">图像高度</param>
        /// <param name="bIsColor">是否是彩色相机</param>
        private void __CreateBitmap(out Bitmap bitmap, int nWidth, int nHeight, bool bIsColor)
        {
            bitmap = new Bitmap(nWidth, nHeight, __GetFormat(bIsColor));
            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                ColorPalette colorPalette = bitmap.Palette;
                for (int i = 0; i < 256; i++)
                {
                    colorPalette.Entries[i] = Color.FromArgb(i, i, i);
                }
                bitmap.Palette = colorPalette;
            }
        }

        /// <summary>
        /// 更新和复制图像数据到Bitmap的buffer
        /// </summary>
        /// <param name="bitmap">Bitmap对象</param>
        /// <param name="nWidth">图像宽度</param>
        /// <param name="nHeight">图像高度</param>
        /// <param name="bIsColor">是否是彩色相机</param>
        private void __UpdateBitmap(Bitmap bitmap, byte[] byBuffer, int nWidth, int nHeight, bool bIsColor)
        {
            //给BitmapData加锁
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            //得到一个指向Bitmap的buffer指针
            IntPtr ptrBmp = bmpData.Scan0;
            int nImageStride = __GetStride(m_nWidth, bIsColor);
            //图像宽能够被4整除直接copy
            if (nImageStride == bmpData.Stride)
            {
                Marshal.Copy(byBuffer, 0, ptrBmp, bmpData.Stride * bitmap.Height);
            }
            else//图像宽不能够被4整除按照行copy
            {
                for (int i = 0; i < bitmap.Height; ++i)
                {
                    Marshal.Copy(byBuffer, i * nImageStride, new IntPtr(ptrBmp.ToInt64() + i * bmpData.Stride), m_nWidth);
                }
            }
            //BitmapData解锁
            bitmap.UnlockBits(bmpData);
        }
    }
}
