using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using HalconDotNet;
using GxIAPINET;
//using GxIAPINET.Common;
using System.Runtime.InteropServices;
using ImageProcess;
using Camera;

namespace ImageMatch
{
    public partial class Run : Form
    {
        IGXFactory m_objIGXFactory = null;
        GxCamera camera;

        bool m_bIsOpen = false;
        bool m_bIsSnap = false;
        bool m_bIsCorrect = false;
        bool m_bIsScreenShot = false;
        bool m_bIsChange = false;
        bool m_bWait = false;

        int m_correctNum = 0;
        int m_shotNum = 0;
        double TerminalHeight = 500.0;
        double TerminalWidth = 250.0;
        HTuple hv_PRow, hv_PCol;
        HTuple SS_Col, SS_Row;

        HObject rectangle, ho_ScreenShot;

        public Run()
        {
            InitializeComponent();
        }

        private void UpdateUI()
        {
            m_btnOpenCamera.Enabled = !m_bIsOpen;
            m_btnCloseCamera.Enabled = m_bIsOpen;
            m_btnStartGrab.Enabled = m_bIsOpen && !m_bIsSnap;
            m_btnStopGrab.Enabled = m_bIsOpen && m_bIsSnap;
            m_btnCorrect.Enabled = m_bIsOpen && m_bIsSnap;
            //m_btnMatch.Enabled = m_bIsOpen && m_bIsSnap;
        }

        private void m_btnOpenCamera_Click(object sender, EventArgs e)
        {
            try
            {
                List<IGXDeviceInfo> listGXDeviceInfo = new List<IGXDeviceInfo>();
                m_objIGXFactory = IGXFactory.GetInstance();
                m_objIGXFactory.Init();
                m_objIGXFactory.UpdateAllDeviceList(200, listGXDeviceInfo);
                camera = new GxCamera();

                //ImageAlg testalg = new ImageAlg();
                //Order order = new Order();

                if (listGXDeviceInfo.Count <= 0)
                {
                    MessageBox.Show("未发现设备！");
                    //int[,] p = new int[,] { { 4, 2 }, { 1, 1 }, { 2, 5 }, { 6, 7 } };
                    //Point[] points = new Point[4];
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    points[i].X = p[i, 0];
                    //    points[i].Y = p[i, 1];
                    //}
                    //Point[] sorted = ImageAlg.SortPoints(points);
                    return;
                }

                String strUserID = listGXDeviceInfo[0].GetUserID();
                
                camera.strUserID = strUserID;
                //camera = new GxCamera();

                /////////////////////////////////

                camera.OpenCamera(m_pic_ShowImage);
                m_btnCloseCamera.Focus();

                m_bIsOpen = true;
                UpdateUI();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void m_btnCloseCamera_Click(object sender, EventArgs e)
        {
            try
            {
                camera.CloseAll(m_bIsSnap);
                m_bIsSnap = false;
                m_bIsOpen = false;
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void m_btnStartGrab_Click(object sender, EventArgs e)
        {
            try
            {
                camera.StartGrab();
                camera.ShowOriginalImageEvent += ShowOriginalImageEvent;
                m_bIsSnap = true;
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void m_btnStopGrab_Click(object sender, EventArgs e)
        {
            try
            {
                camera.StopGrab();
                m_bIsSnap = false;
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void m_btnCorrect_Click(object sender, EventArgs e)
        {
            try
            {
                HTuple picBoxHeight = m_pic_ShowImage.Height;
                HTuple picBoxWidth = m_pic_ShowImage.Width;
                if (hv_PCol.Length == 4)
                {
                    camera.StartCorrect(hv_PCol, hv_PRow, picBoxWidth, picBoxHeight);
                }
                camera.CorrectImageEvent += CorrectEvent;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void m_btnWaitResponse_Click(object sender, EventArgs e)
        {
            try
            {
                camera.m_bWait = true;
                camera.WaitResponseEvent += WaitResponseEvent;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void m_btnScreenShot_Click(object sender, EventArgs e)
        {
            try
            {
                m_bIsScreenShot = true;
                m_shotNum = 0;
                SS_Col = new HTuple();
                SS_Row = new HTuple();
                MessageBox.Show("选择截图点");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        Bitmap ScreenShot;
        private void m_btnSaveScreenShot_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (m_shotNum == 2)
                {
                    HOperatorSet.GenRectangle1(out rectangle, SS_Row[0], SS_Col[0], SS_Row[1], SS_Col[1]);
                    //Bitmap grabimage = (Bitmap)m_picShowCorrectImage.InitialImage;
                    HObject grab;
                    ImageConvert.Bitmap2HObjectBpp24(TransImage, out grab);
                    HOperatorSet.ReduceDomain(grab, rectangle, out ho_ScreenShot);
                    HOperatorSet.CropDomain(ho_ScreenShot, out ho_ScreenShot);

                    ImageConvert.HObject2Bpp8(ho_ScreenShot, out ScreenShot);
                    //先传递给camera,camera再传递给imagealg
                    camera.TargetLocation[0].X = (int)SS_Col[0].D;
                    camera.TargetLocation[0].Y = (int)SS_Row[0].D;

                    camera.TargetLocation[1].X = (int)SS_Col[1].D;
                    camera.TargetLocation[1].Y = (int)SS_Row[1].D;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        } 

        
        private void m_btnMatch_Click(object sender, EventArgs e)
        {
            HTuple hv_Column, hv_Score, hv_Row;

            //Bitmap temp = (Bitmap)m_picShowCorrectImage.InitialImage;
            HObject ho_Match;
            ImageConvert.Bitmap2HObjectBpp24(TransImage, out ho_Match);
            //ImageConvert.Bitmap2HObjectBpp8(ScreenShot, out ho_ScreenShot);
            MatchAlg.ImageLocation(ho_ScreenShot, ho_Match, out hv_Row, out hv_Column, out hv_Score);

            if (hv_Score > 0.5)
            {             
                pictureBox2.Refresh();
                pictureBox2.Image = TransImage;

                Graphics g = pictureBox2.CreateGraphics();
                Pen p = new  Pen(Color.Red, 2);

                HTuple height = SS_Row[1] - SS_Row[0];
                HTuple width = SS_Col[1] - SS_Col[0];

                width = width * pictureBox2.Width / TerminalWidth;
                height = height * pictureBox2.Height / TerminalHeight;
                hv_Column = hv_Column * pictureBox2.Width / TerminalWidth - width / 2;
                hv_Row = hv_Row * pictureBox2.Height / TerminalHeight - height / 2;

                //以矩形左上角的点为基准
                pictureBox2.Update();
                g.DrawRectangle(p, hv_Column.TupleInt(), hv_Row.TupleInt(), width.TupleInt(), height.TupleInt());
            }
            else
            {
                MessageBox.Show("无法定位！");
                pictureBox2.Refresh();
            }
        }
        
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            m_bIsChange = !m_bIsChange;
            if (m_bIsChange == true)
            {
                try
                {
                    m_bIsCorrect = true;
                    m_correctNum = 0;
                    hv_PCol = new HTuple();
                    hv_PRow = new HTuple();
                    MessageBox.Show("选择校正点");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //点击图像显示控件触发的动作
        private void m_pic_ShowImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_bIsCorrect == true && m_correctNum < 4 && m_bIsChange == true)
            {   //窗口与图像大小的缩放比例
                MessageBox.Show(e.Location.ToString());
                hv_PCol[m_correctNum] = e.X;
                hv_PRow[m_correctNum] = e.Y;
                m_correctNum += 1;
            }
        }

        private void m_picShowCorrectImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (m_bIsScreenShot == true && m_shotNum < 2)
            {   //窗口与图像大小的缩放比例300,600:129,223
                MessageBox.Show(e.Location.ToString());
                SS_Col[m_shotNum] = e.X * TerminalWidth / (double)m_picShowCorrectImage.Width;
                SS_Row[m_shotNum] = e.Y * TerminalHeight / (double)m_picShowCorrectImage.Height;
                m_shotNum += 1;
            }
        }
        
        void ShowOriginalImageEvent(Bitmap m_GrabImage)
        {
            //m_pic_ShowImage.Refresh();
            m_pic_ShowImage.Image = m_GrabImage;
        }

        Bitmap TransImage;
        void CorrectEvent(Bitmap m_GrabImage)
        {
            TransImage = camera.imagealg.DynamicCorrect(m_GrabImage);
            m_picShowCorrectImage.Image = TransImage;
            
        }

        void WaitResponseEvent()
        {
            Bitmap ScreenShot1 = (Bitmap)ScreenShot.Clone();
            Bitmap TransImage1 = (Bitmap)TransImage.Clone();
            
            bool flag = false;
            flag = camera.imagealg.ObjectMatch(ScreenShot1, TransImage1,ImageAlg.MATCH_STRATEGY.MULTI_SCALE_OBJECT_MATCH);

            //if (flag != m_bWait)
            //{
            //    m_txtboxTargetStatus.Refresh();
            //    m_bWait = flag;
            //    m_txtboxTargetStatus.Text = m_bWait.ToString();
            //}
        }
    }
}
