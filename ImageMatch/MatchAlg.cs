using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HalconDotNet;
using System.Drawing;
using System.Drawing.Imaging;

namespace ImageMatch
{
    class MatchAlg
    {
        public HObject OutCorrectedImage(HObject ho_Image, HTuple hv_PRow, HTuple hv_PCol)
        {
            //HTuple hv_AcqHandle = null;
            //HObject ho_Image = null;

            ////获取照片
            //HOperatorSet.OpenFramegrabber("GigEVision2", 0, 0, 0, 0, 0, 0, "progressive",
            //    -1, "default", -1, "false", "default", "default", 0, -1, out hv_AcqHandle);
            //HOperatorSet.GrabImageStart(hv_AcqHandle, -1);

            //ho_Image.Dispose();
            //HOperatorSet.GrabImageAsync(out ho_Image, hv_AcqHandle, -1);                

            //标定点的选择，待修改
            HTuple hv_QRow, hv_QCol;

            //校正之后的图像大小
            hv_QRow = new HTuple();
            hv_QRow[0] = 0;
            hv_QRow[1] = 0;
            hv_QRow[2] = 600;
            hv_QRow[3] = 600;
            hv_QCol = new HTuple();
            hv_QCol[0] = 0;
            hv_QCol[1] = 300;
            hv_QCol[2] = 300;
            hv_QCol[3] = 0;

            HTuple hv_FirstHomMat2D = null;
            
            HTuple hv_covariance = null;
            HObject ho_TransImage;
            HOperatorSet.GenEmptyObj(out ho_TransImage);

            HOperatorSet.VectorToProjHomMat2d(hv_PRow + 0.5, hv_PCol + 0.5, hv_QRow + 0.5, hv_QCol + 0.5,
                 "normalized_dlt", new HTuple(), new HTuple(), new HTuple(), new HTuple(),
                 new HTuple(), new HTuple(), out hv_FirstHomMat2D, out hv_covariance);
            ho_TransImage.Dispose();
            HOperatorSet.ProjectiveTransImageSize(ho_Image, out ho_TransImage, hv_FirstHomMat2D,
                "bilinear", 300, 600, "false");

            //Bitmap CorrectImage = new Bitmap(hv_QRow[3], hv_QCol[1], System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            //GenerateRGBBitmap(ho_Image, out CorrectImage);
            return ho_TransImage;
        }

        public static void CorrectImage(HObject ho_Image, HTuple hv_row, HTuple hv_col, out HTuple hv_FirstHomMat2D, out HTuple hv_SecondHomMat2D)
        {
            // Local iconic variables 

            HObject ho_Region, ho_ImageReduced;
            HObject ho_transImage, ho_ImagePart, ho_Edges, ho_UnionContours;
            HObject ho_SelectedContours1, ho_ObjectSelected, ho_ClosedContours;
            HObject ho_ContoursSplit, ho_RegressContours, ho_HorizontalEdges;
            HObject ho_VerticalEdges, ho_FieldBorder;
            HObject ho_TransImage;

            // Local control variables 

            HTuple hv_ccol = null;
            HTuple hv_crow = null, hv_Covariance = null;
            HTuple hv_Length = null, hv_Indices = null, hv_Num = null;
            HTuple hv_RowBegHor = null, hv_ColBegHor = null, hv_RowEndHor = null;
            HTuple hv_ColEndHor = null, hv_NrHor = null, hv_NcHor = null;
            HTuple hv_DistHor = null, hv_IndexHor = null, hv_RowBegVer = null;
            HTuple hv_ColBegVer = null, hv_RowEndVer = null, hv_ColEndVer = null;
            HTuple hv_NrVer = null, hv_NcVer = null, hv_DistVer = null;
            HTuple hv_IndexVer = null, hv_RowUL = null, hv_ColUL = null;
            HTuple hv_IsOverlapping = null, hv_RowUR = null, hv_ColUR = null;
            HTuple hv_RowLL = null, hv_ColLL = null, hv_RowLR = null;
            HTuple hv_ColLR = null, hv_dstWidth = null, hv_dstHeight = null;
            // Initialize local and output iconic variables 
            
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_transImage);
            HOperatorSet.GenEmptyObj(out ho_ImagePart);
            HOperatorSet.GenEmptyObj(out ho_Edges);
            HOperatorSet.GenEmptyObj(out ho_UnionContours);
            HOperatorSet.GenEmptyObj(out ho_SelectedContours1);
            HOperatorSet.GenEmptyObj(out ho_ObjectSelected);
            HOperatorSet.GenEmptyObj(out ho_ClosedContours);
            HOperatorSet.GenEmptyObj(out ho_ContoursSplit);
            HOperatorSet.GenEmptyObj(out ho_RegressContours);
            HOperatorSet.GenEmptyObj(out ho_HorizontalEdges);
            HOperatorSet.GenEmptyObj(out ho_VerticalEdges);
            HOperatorSet.GenEmptyObj(out ho_FieldBorder);
            HOperatorSet.GenEmptyObj(out ho_TransImage);

            hv_ccol = new HTuple();
            hv_ccol[0] = 0;
            hv_ccol[1] = 300;
            hv_ccol[2] = 300;
            hv_ccol[3] = 0;
            hv_crow = new HTuple();
            hv_crow[0] = 0;
            hv_crow[1] = 0;
            hv_crow[2] = 600;
            hv_crow[3] = 600;

            ho_Region.Dispose();
            HOperatorSet.GenRegionPolygonFilled(out ho_Region, hv_row, hv_col);
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_Image, ho_Region, out ho_ImageReduced);
            //ho_Image.WriteObject("E://projects//周报//original.bmp");
            //ho_ImageReduced.WriteObject("E://projects//周报//reduced.bmp"); 

            HOperatorSet.VectorToProjHomMat2d(hv_row, hv_col, hv_crow, hv_ccol, "normalized_dlt",
                new HTuple(), new HTuple(), new HTuple(), new HTuple(), new HTuple(), new HTuple(),
                out hv_FirstHomMat2D, out hv_Covariance);
            ho_transImage.Dispose();
            HOperatorSet.ProjectiveTransImageSize(ho_ImageReduced, out ho_transImage, hv_FirstHomMat2D,
                "bilinear", 300, 600, "false");

            ho_transImage.WriteObject("E://projects//周报//firstcorrect.bmp");

            ho_ImagePart.Dispose();
            HOperatorSet.CropDomain(ho_transImage, out ho_ImagePart);

            ho_Edges.Dispose();
            HOperatorSet.EdgesSubPix(ho_transImage, out ho_Edges, "canny", 2, 5, 50);

            ho_UnionContours.Dispose();
            HOperatorSet.UnionCollinearContoursXld(ho_Edges, out ho_UnionContours, 10, 1,
                8, 0.4, "attr_keep");
            ho_SelectedContours1.Dispose();
            HOperatorSet.UnionAdjacentContoursXld(ho_UnionContours, out ho_SelectedContours1,
                10, 1, "attr_keep");

            HOperatorSet.LengthXld(ho_SelectedContours1, out hv_Length);
            HOperatorSet.TupleSortIndex(hv_Length, out hv_Indices);
            hv_Num = new HTuple(hv_Indices.TupleLength());
            ho_ObjectSelected.Dispose();
            HOperatorSet.SelectObj(ho_SelectedContours1, out ho_ObjectSelected, (hv_Indices.TupleSelect(
                hv_Num - 1)) + 1);

            //ho_ClosedContours.Dispose();
            //HOperatorSet.CloseContoursXld(ho_ObjectSelected, out ho_ClosedContours);
            ho_ContoursSplit.Dispose();
            //HOperatorSet.SegmentContoursXld(ho_ClosedContours, out ho_ContoursSplit, "lines",
            //    3, 20, 10);
            HOperatorSet.SegmentContoursXld(ho_ObjectSelected, out ho_ContoursSplit, "lines",
                3, 20, 10);
            ho_RegressContours.Dispose();
            HOperatorSet.RegressContoursXld(ho_ContoursSplit, out ho_RegressContours, "no",
                1);

            ho_HorizontalEdges.Dispose();
            HOperatorSet.SelectShapeXld(ho_RegressContours, out ho_HorizontalEdges, "rect2_phi",
                "and", (new HTuple(-20)).TupleRad(), (new HTuple(20)).TupleRad());
            ho_VerticalEdges.Dispose();
            HOperatorSet.SelectShapeXld(ho_RegressContours, out ho_VerticalEdges, (new HTuple("rect2_phi")).TupleConcat(
                "rect2_phi"), "or", (new HTuple((new HTuple(-90)).TupleRad())).TupleConcat(
                (new HTuple(70)).TupleRad()), (new HTuple((new HTuple(-70)).TupleRad())).TupleConcat(
                (new HTuple(90)).TupleRad()));
            HOperatorSet.FitLineContourXld(ho_HorizontalEdges, "tukey", -1, 0, 10, 2, out hv_RowBegHor,
                out hv_ColBegHor, out hv_RowEndHor, out hv_ColEndHor, out hv_NrHor, out hv_NcHor,
                out hv_DistHor);
            hv_IndexHor = hv_RowBegHor.TupleSortIndex();
            HOperatorSet.FitLineContourXld(ho_VerticalEdges, "tukey", -1, 0, 10, 2, out hv_RowBegVer,
                out hv_ColBegVer, out hv_RowEndVer, out hv_ColEndVer, out hv_NrVer, out hv_NcVer,
                out hv_DistVer);
            hv_IndexVer = hv_ColBegVer.TupleSortIndex();
            HOperatorSet.IntersectionLines(hv_RowBegHor.TupleSelect(hv_IndexHor.TupleSelect(
                0)), hv_ColBegHor.TupleSelect(hv_IndexHor.TupleSelect(0)), hv_RowEndHor.TupleSelect(
                hv_IndexHor.TupleSelect(0)), hv_ColEndHor.TupleSelect(hv_IndexHor.TupleSelect(
                0)), hv_RowBegVer.TupleSelect(hv_IndexVer.TupleSelect(0)), hv_ColBegVer.TupleSelect(
                hv_IndexVer.TupleSelect(0)), hv_RowEndVer.TupleSelect(hv_IndexVer.TupleSelect(
                0)), hv_ColEndVer.TupleSelect(hv_IndexVer.TupleSelect(0)), out hv_RowUL,
                out hv_ColUL, out hv_IsOverlapping);
            HOperatorSet.IntersectionLines(hv_RowBegHor.TupleSelect(hv_IndexHor.TupleSelect(
                0)), hv_ColBegHor.TupleSelect(hv_IndexHor.TupleSelect(0)), hv_RowEndHor.TupleSelect(
                hv_IndexHor.TupleSelect(0)), hv_ColEndHor.TupleSelect(hv_IndexHor.TupleSelect(
                0)), hv_RowBegVer.TupleSelect(hv_IndexVer.TupleSelect(1)), hv_ColBegVer.TupleSelect(
                hv_IndexVer.TupleSelect(1)), hv_RowEndVer.TupleSelect(hv_IndexVer.TupleSelect(
                1)), hv_ColEndVer.TupleSelect(hv_IndexVer.TupleSelect(1)), out hv_RowUR,
                out hv_ColUR, out hv_IsOverlapping);
            HOperatorSet.IntersectionLines(hv_RowBegHor.TupleSelect(hv_IndexHor.TupleSelect(
                1)), hv_ColBegHor.TupleSelect(hv_IndexHor.TupleSelect(1)), hv_RowEndHor.TupleSelect(
                hv_IndexHor.TupleSelect(1)), hv_ColEndHor.TupleSelect(hv_IndexHor.TupleSelect(
                1)), hv_RowBegVer.TupleSelect(hv_IndexVer.TupleSelect(0)), hv_ColBegVer.TupleSelect(
                hv_IndexVer.TupleSelect(0)), hv_RowEndVer.TupleSelect(hv_IndexVer.TupleSelect(
                0)), hv_ColEndVer.TupleSelect(hv_IndexVer.TupleSelect(0)), out hv_RowLL,
                out hv_ColLL, out hv_IsOverlapping);
            HOperatorSet.IntersectionLines(hv_RowBegHor.TupleSelect(hv_IndexHor.TupleSelect(
                1)), hv_ColBegHor.TupleSelect(hv_IndexHor.TupleSelect(1)), hv_RowEndHor.TupleSelect(
                hv_IndexHor.TupleSelect(1)), hv_ColEndHor.TupleSelect(hv_IndexHor.TupleSelect(
                1)), hv_RowBegVer.TupleSelect(hv_IndexVer.TupleSelect(1)), hv_ColBegVer.TupleSelect(
                hv_IndexVer.TupleSelect(1)), hv_RowEndVer.TupleSelect(hv_IndexVer.TupleSelect(
                1)), hv_ColEndVer.TupleSelect(hv_IndexVer.TupleSelect(1)), out hv_RowLR,
                out hv_ColLR, out hv_IsOverlapping);

            ho_FieldBorder.Dispose();
            HOperatorSet.GenContourPolygonXld(out ho_FieldBorder, ((((((hv_RowUL.TupleConcat(
                hv_RowUR))).TupleConcat(hv_RowLR))).TupleConcat(hv_RowLL))).TupleConcat(hv_RowUL),
                ((((((hv_ColUL.TupleConcat(hv_ColUR))).TupleConcat(hv_ColLR))).TupleConcat(
                hv_ColLL))).TupleConcat(hv_ColUL));

            hv_dstWidth = (((hv_ColUR - hv_ColUL) + hv_ColLR) - hv_ColLL) / 2;
            hv_dstHeight = (((hv_RowLL - hv_RowUL) + hv_RowLR) - hv_RowUR) / 2;
            HOperatorSet.VectorToProjHomMat2d((((((hv_RowUL.TupleConcat(hv_RowUR))).TupleConcat(
                hv_RowLR))).TupleConcat(hv_RowLL)) + 0.5, (((((hv_ColUL.TupleConcat(hv_ColUR))).TupleConcat(
                hv_ColLR))).TupleConcat(hv_ColLL)) + 0.5, hv_crow + 0.5, hv_ccol + 0.5, "normalized_dlt",
                new HTuple(), new HTuple(), new HTuple(), new HTuple(), new HTuple(), new HTuple(),
                out hv_SecondHomMat2D, out hv_Covariance);

            ho_TransImage.Dispose();
            HOperatorSet.ProjectiveTransImageSize(ho_transImage, out ho_TransImage, hv_SecondHomMat2D,
                "bilinear", 300, 600, "false");

            

        }

        public static void ImageLocation(HObject ScreenShot, HObject CorrectImage, out HTuple hv_Row, out HTuple hv_Column, out HTuple hv_Score)
        {

            HTuple hv_Angle = null, hv_ModelID = null;
            
            HOperatorSet.CreateShapeModel(ScreenShot, "auto", -0.39, 0.79, "auto", "auto",
                    "use_polarity", "auto", "auto", out hv_ModelID);
            HOperatorSet.FindShapeModel(CorrectImage, hv_ModelID, -0.39, 0.79, 0.5, 1, 0.5, "least_squares",
                    0, 0.9, out hv_Row, out hv_Column, out hv_Angle, out hv_Score);
        }
    }
}
