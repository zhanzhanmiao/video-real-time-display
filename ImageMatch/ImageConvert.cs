using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageMatch
{
    public class ImageConvert
    {
        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(int Destination, int add, int Length);

        public static void HObject2Bpp8(HObject image, out Bitmap res)
        {
            HTuple hpoint, type, width, height;

            const int Alpha = 255;
            int[] ptr = new int[2];
            HOperatorSet.GetImagePointer1(image, out hpoint, out type, out width, out height);

            res = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette pal = res.Palette;
            for (int i = 0; i <= 255; i++)
            {
                pal.Entries[i] = Color.FromArgb(Alpha, i, i, i);
            }
            res.Palette = pal;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bitmapData = res.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            int PixelSize = Bitmap.GetPixelFormatSize(bitmapData.PixelFormat) / 8;
            ptr[0] = bitmapData.Scan0.ToInt32();
            ptr[1] = hpoint.I;
            if (width % 4 == 0)
                CopyMemory(ptr[0], ptr[1], width * height * PixelSize);
            else
            {
                for (int i = 0; i < height - 1; i++)
                {
                    ptr[1] += width;
                    CopyMemory(ptr[0], ptr[1], width * PixelSize);
                    ptr[0] += bitmapData.Stride;
                }
            }
            res.UnlockBits(bitmapData);

        }

        public static void Bitmap2HObjectBpp24(Bitmap original_image, out HObject ho_image)
        {
            try
            {
                HOperatorSet.GenEmptyObj(out ho_image);
                Point po = new Point(0, 0);
                Size so = new Size(original_image.Width, original_image.Height);//template.Width, template.Height
                Rectangle ro = new Rectangle(po, so);

                Bitmap bmp_image = original_image.Clone(ro, PixelFormat.Format24bppRgb);

                Rectangle rect = new Rectangle(0, 0, bmp_image.Width, bmp_image.Height);

                BitmapData srcBmpData = bmp_image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                int stride = srcBmpData.Stride;
                int width = srcBmpData.Width;
                int height = srcBmpData.Height;

                unsafe
                {
                    int count = width * height;
                    byte[] dataRed = new byte[count];
                    byte[] dataGreen = new byte[count];
                    byte[] dataBlue = new byte[count];
                    byte* bptr = (byte*)srcBmpData.Scan0;
                    fixed (byte* pDataRed = dataRed, pDataGreen = dataGreen, pDataBlue = dataBlue)
                    {
                        for (int i = 0; i < height; i++)
                            for (int j = 0; j < width; j++)
                            {
                                dataRed[i * width + j] = bptr[i * stride + 3 * j];
                                dataGreen[i * width + j] = bptr[i * stride + 3 * j + 1];
                                dataBlue[i * width + j] = bptr[i * stride + 3 * j + 2];
                            }

                        HOperatorSet.GenImage3(out ho_image, "byte", srcBmpData.Width, srcBmpData.Height, new IntPtr(pDataBlue), new IntPtr(pDataGreen), new IntPtr(pDataRed));
                    }
                    //HOperatorSet.GenImageInterleaved(out ho_image, srcBmpData.Scan0, "rgb", bmp_image.Width, bmp_image.Height, 0, "byte",
                    //0, 0, 0, 0, -1, 0);
                }
                bmp_image.UnlockBits(srcBmpData);
            }
            catch (Exception ex)
            {
                ho_image = null;
            }
        }

        public static void Bitmap2HObjectBpp8(Bitmap DstImage, out HObject Hobj)
        {
            
            HOperatorSet.GenEmptyObj(out Hobj);

            //Point po = new Point(0, 0);
            //Size so = new Size(SrcImage.Width, SrcImage.Height);//template.Width, template.Height
            //Rectangle ro = new Rectangle(po, so);

            //Bitmap DstImage = new Bitmap(SrcImage.Width, SrcImage.Height, PixelFormat.Format8bppIndexed);
            //DstImage = SrcImage.Clone(ro, PixelFormat.Format8bppIndexed);

            int width = DstImage.Width;
            int height = DstImage.Height;

            Rectangle rect = new Rectangle(0, 0, width, height);
            System.Drawing.Imaging.BitmapData dstBmpData =
                DstImage.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);//pImage.PixelFormat
            int PixelSize = Bitmap.GetPixelFormatSize(dstBmpData.PixelFormat) / 8;
            int stride = dstBmpData.Stride;

            IntPtr Bptr = dstBmpData.Scan0;

            unsafe
            {
                int count = height * width;
                byte[] data = new byte[count];
                byte* bptr = (byte*)dstBmpData.Scan0;
                fixed (byte* pData = data)
                {
                    for (int i = 0; i < height; i++)
                        for (int j = 0; j < width; j++)
                        {
                            data[i * width + j] = bptr[i * stride + j];
                        }
                    HOperatorSet.GenImage1(out Hobj, "byte", width, height, new IntPtr(pData));
                }
            }

            DstImage.UnlockBits(dstBmpData);

            }
    }
}
