using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketScan
{
    public class ImageProcess
    {

        //背景近似度阀值
        private static double BGThreshold = 80;
        //每个字符最小宽度
        private static int MinWidthPerChar = 30;
        //每个字符最大宽度
        private static int MaxWidthPerChar = 40;
        //每个字符最小高度
        private static int MinHeightPerChar = 40;
        //色差阀值 越小消除的杂色越多
        private static double colorDiffThreshold = 150;

        private static double BlackBlobThreshold = 80;
        //与印刷票号颜色相似度的阈值
        private static double TicketNoColorDiffThreshold = 80;
        //印刷票号的基准颜色
        private static Color TicketNoColor = Color.FromArgb(149, 73, 83);


        




        /// <summary>
        /// 提取印刷票号图像
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ticketType"></param>
        /// <returns></returns>
        public static Bitmap ExtractTicketNoImage(Bitmap source,int ticketType)
        {
            Bitmap b = null;
            if (Config.BLUE_TICKET == ticketType)
            {
                int leftX = Convert.ToInt16(source.Width * Config.BLUE_TICKETNO_X_CORP_RATIO);
                int topY  = Convert.ToInt16(source.Height * Config.BLUE_TICKETNO_Y_CORP_RATIO);
                int w = Convert.ToInt16(source.Width * Config.BLUE_TICKETNO_W_CORP_RATIO);
                int h = Convert.ToInt16(source.Height * Config.BLUE_TICKETNO_H_CORP_RATIO);
                Rectangle rect = new Rectangle(leftX, topY, w, h);
                Crop corp = new Crop(rect);
                b = corp.Apply(source);
            }else
            {
                int leftX = Convert.ToInt16(source.Width * Config.BLUE_TICKETNO_X_CORP_RATIO);
                int topY = Convert.ToInt16(source.Height * Config.BLUE_TICKETNO_Y_CORP_RATIO);
                int w = Convert.ToInt16(source.Width * Config.BLUE_TICKETNO_W_CORP_RATIO);
                int h = Convert.ToInt16(source.Height * Config.BLUE_TICKETNO_H_CORP_RATIO);
                Rectangle rect = new Rectangle(leftX, topY, w, h);
                Crop corp = new Crop(rect);
                b = corp.Apply(source);

            }
            return b;
        }

        /// <summary>
        /// 提取21位码的图像
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ticketType"></param>
        /// <returns></returns>
        public static Bitmap ExtractCodeImage(Bitmap source, int ticketType)
        {
            Bitmap b = null;
            if (Config.BLUE_TICKET == ticketType)
            {

                int leftX = Convert.ToInt16(source.Width * Config.BLUE_CODE_X_CORP_RATIO);
                int topY = Convert.ToInt16(source.Height * Config.BLUE_CODE_Y_CORP_RATIO);
                int w = Convert.ToInt16(source.Width * Config.BLUE_CODE_W_CORP_RATIO);
                int h = Convert.ToInt16(source.Height * Config.BLUE_CODE_H_CORP_RATIO);
                Rectangle rect = new Rectangle(leftX, topY, w, h);
                Crop corp = new Crop(rect);
                b = corp.Apply(source);
            }
            return b;
        }



        public static  IList<Bitmap> ExtractTicketCode(Bitmap source,int imgWidth,int imgHeight)
        {
            IList<Bitmap> imgList = new List<Bitmap>();
            // 从图像中提取宽度和高度大于21位码长宽的blob
            BlobCounter extractor = new BlobCounter();
            //extractor.FilterBlobs = true;
            extractor.MinWidth = Convert.ToInt16(Config.BLUE_CODE_NUM_WIDTH_RATIO * imgWidth * 0.7);
            extractor.MaxWidth = Convert.ToInt16(Config.BLUE_CODE_NUM_WIDTH_RATIO * imgWidth);
            extractor.MinHeight = Convert.ToInt16(Config.BLUE_CODE_CHAR_HEIGHT_RATIO * imgHeight * 0.7);
            extractor.MaxHeight = Convert.ToInt16(Config.BLUE_CODE_CHAR_HEIGHT_RATIO * imgHeight);

            extractor.ProcessImage(source);
            QuadrilateralTransformation quadTransformer = new QuadrilateralTransformation();
            foreach (Blob blob in extractor.GetObjectsInformation())
            {
                // 获取边缘点
                List<IntPoint> edgePoints = extractor.GetBlobsEdgePoints(blob);
                // 利用边缘点，在原始图像上找到四角
                List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(edgePoints);
                if (corners.Count < 4) continue;

                quadTransformer.SourceQuadrilateral = corners; //Set corners for transforming card 
                quadTransformer.AutomaticSizeCalculaton = true;

                Bitmap codeImg = quadTransformer.Apply(source); //Extract(transform) card image
                imgList.Add(codeImg);

            }
            return imgList;
        }


        /// <summary>
        /// 二值化
        /// </summary>
        /// <param name="map"></param>
        public static void Binarizate(Bitmap map)
        {
            int tv = ComputeThresholdValue(map);
            int x = map.Width;
            int y = map.Height;
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (map.GetPixel(i, j).R >= tv)
                    {
                        map.SetPixel(i, j, Color.FromArgb(0xff, 0xff, 0xff));
                    }
                    else
                    {
                        map.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    }
                }
            }
        }

        /// <summary>
        /// 计算二值化阈值
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private static int ComputeThresholdValue(Bitmap img)
        {
            int i;
            int k;
            double csum;
            int thresholdValue = 1;
            int[] ihist = new int[0x100];
            for (i = 0; i < 0x100; i++)
            {
                ihist[i] = 0;
            }
            int gmin = 0xff;
            int gmax = 0;
            for (i = 1; i < (img.Width - 1); i++)
            {
                for (int j = 1; j < (img.Height - 1); j++)
                {
                    int cn = img.GetPixel(i, j).R;
                    ihist[cn]++;
                    if (cn > gmax)
                    {
                        gmax = cn;
                    }
                    if (cn < gmin)
                    {
                        gmin = cn;
                    }
                }
            }
            double sum = csum = 0.0;
            int n = 0;
            for (k = 0; k <= 0xff; k++)
            {
                sum += k * ihist[k];
                n += ihist[k];
            }
            if (n == 0)
            {
                return 60;
            }
            double fmax = -1.0;
            int n1 = 0;
            for (k = 0; k < 0xff; k++)
            {
                n1 += ihist[k];
                if (n1 != 0)
                {
                    int n2 = n - n1;
                    if (n2 == 0)
                    {
                        return thresholdValue;
                    }
                    csum += k * ihist[k];
                    double m1 = csum / ((double)n1);
                    double m2 = (sum - csum) / ((double)n2);
                    double sb = ((n1 * n2) * (m1 - m2)) * (m1 - m2);
                    if (sb > fmax)
                    {
                        fmax = sb;
                        thresholdValue = k;
                    }
                }
            }
            return thresholdValue;
        }




        public static Bitmap FilterBalckBlob(Bitmap b)
        {
            Bitmap ret = (Bitmap)b.Clone();
            //获取图片中每个像素的灰度
            for (var x = 0; x < ret.Width; x++)
            {
                for (var y = 0; y < ret.Height; y++)
                {
                    var color = ret.GetPixel(x, y);
                    if(GetColorDif(color,Color.Black)> colorDiffThreshold)
                        ret.SetPixel(x, y, Color.White);
                }
            }
            return ret;
        }


        /// <summary>
        /// 去除背景
        /// </summary>
        /// <param name="b"></param>
        public static void FilterBackground(Bitmap b)
        {
            //key 颜色  value颜色对应的数量
            Dictionary<Color, int> colorDic = new Dictionary<Color, int>();
            //获取图片中每个颜色的数量
            for (var x = 0; x < b.Width; x++)
            {
                for (var y = 0; y < b.Height; y++)
                {
                    //删除边框
                    if (y == 0 || y == b.Height)
                    {
                        b.SetPixel(x, y, Color.White);
                    }

                    var color = b.GetPixel(x, y);
                    var colorRGB = color.ToArgb();

                    if (colorDic.ContainsKey(color))
                    {
                        colorDic[color] = colorDic[color] + 1;
                    }
                    else
                    {
                        colorDic[color] = 1;
                    }
                }
            }
            //图片中最多的颜色
            Color maxColor = colorDic.OrderByDescending(o => o.Value).FirstOrDefault().Key;
            //图片中最少的颜色
            Color minColor = colorDic.OrderBy(o => o.Value).FirstOrDefault().Key;

            Dictionary<int[], double> maxColorDifDic = new Dictionary<int[], double>();
            //查找 maxColor 最接近颜色
            for (var x = 0; x < b.Width; x++)
            {
                for (var y = 0; y < b.Height; y++)
                {
                    maxColorDifDic.Add(new int[] { x, y }, GetColorDif(b.GetPixel(x, y), maxColor));
                }
            }
            //去掉和maxColor接近的颜色 即 替换成白色
            var maxColorDifList = maxColorDifDic.OrderBy(o => o.Value).Where(o => o.Value < BGThreshold).ToArray();
            foreach (var kv in maxColorDifList)
            {
                b.SetPixel(kv.Key[0], kv.Key[1], Color.White);
            }
        }

        private static double GetColorDif(Color color1, Color color2)
        {
            return Math.Sqrt((Math.Pow((color1.R - color2.R), 2) +
                Math.Pow((color1.G - color2.G), 2) +
                Math.Pow((color1.B - color2.B), 2)));
        }

        /// <summary>
        /// 剪切图片四周空白区域
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap CutBlankEdge(Bitmap bmp)
        {
            int leftX, rightX, topY, bottomY;
            leftX = GetLeftXCutPoint(bmp);
            rightX = GetRightXCutPoint(bmp);
            topY = GetTopYCutPoint(bmp);
            bottomY = GetBottomYCutPoint(bmp);
            Rectangle rect = new Rectangle(leftX, topY, rightX - leftX, bottomY - topY);

            Crop corp = new Crop(rect);
            return corp.Apply(bmp);
        }

        /// <summary>
        /// 获取左边空白切割点 X坐标
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private static int GetLeftXCutPoint(Bitmap img)
        {
            int startX = -1;//-1表示在寻找开始节点
            for (var x = 0; x < img.Width; x++)
            {
                if (startX == -1)//开始点
                {
                    int blackPXCount = GetBlackPXCountInX(x, 3, img);
                    //如果大于有效像素则是开始节点
                    if (blackPXCount > 6)
                    {
                        startX = x;
                        break;
                    }
                }
            }
            return startX;
        }


        /// <summary>
        /// 获取右边空白切割点 X坐标
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private static int GetRightXCutPoint(Bitmap img)
        {
            int startX = -1;//-1表示在寻找开始节点
            for (var x = img.Width; x > 0; x--)
            {
                if (startX == -1)//开始点
                {
                    int blackPXCount = GetBlackPXCountInX(x, 3, img);
                    //如果大于有效像素则是开始节点
                    if (blackPXCount > 6)
                    {
                        startX = x;
                        break;
                    }
                }
            }
            return startX + 3;
        }


        /// <summary>
        /// 获取上边空白切割点 X坐标
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private static int GetTopYCutPoint(Bitmap img)
        {
            int startY = -1;//-1表示在寻找开始节点
            for (var y = 0; y < img.Height; y++)
            {
                if (startY == -1)//开始点
                {
                    int blackPXCount = GetBlackPXCountInY(y, 3, img);
                    //如果大于有效像素则是开始节点
                    if (blackPXCount > 6)
                    {
                        startY = y;
                        break;
                    }
                }
            }
            return startY;
        }


        /// <summary>
        /// 获取下边空白切割点 X坐标
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private static int GetBottomYCutPoint(Bitmap img)
        {
            int startY = -1;//-1表示在寻找开始节点
            for (var y = img.Height; y > 0; y--)
            {
                int blackPXCount = GetBlackPXCountInY(y - 3, 3, img);
                //如果大于有效像素则是开始节点
                if (blackPXCount > 6)
                {
                    startY = y;
                    break;
                }
            }
            return startY;
        }

        /// <summary>
        /// 获取一个垂直区域内的黑色像素
        /// </summary>
        /// <param name="startX">开始x</param>
        /// <param name="offset">左偏移像素</param>
        /// <returns></returns>
        private static int GetBlackPXCountInX(int startX, int offset, Bitmap img)
        {
            int blackPXCount = 0;
            for (int x = startX; x < startX + offset; x++)
            {
                if (x >= img.Width)
                {
                    continue;
                }
                for (var y = 0; y < img.Height; y++)
                {
                    if (img.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                    {
                        blackPXCount++;
                    }
                }
            }
            return blackPXCount;
        }

        /// <summary>
        /// 获取一个横向区域内的黑色像素
        /// </summary>
        /// <param name="startY">开始Y</param>
        /// <param name="offset">上下偏移像素</param>
        /// <returns></returns>
        private static int GetBlackPXCountInY(int startY, int offset, Bitmap img)
        {
            int blackPYCount = 0;
            for (int y = startY; y < startY + offset; y++)
            {
                if (y >= img.Height)
                {
                    continue;
                }
                for (var x = 0; x < img.Width; x++)
                {
                    if (img.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                    {
                        blackPYCount++;
                    }
                }
            }
            return blackPYCount;
        }


        /// <summary>
        /// 等距分割印刷票号
        /// </summary>
        /// <param name="img"></param>
        /// <param name="sourceWidth"></param>
        /// <param name="sourceHeight"></param>
        /// <returns></returns>
        public static IList<Bitmap> SplitTicketNoByDefinedWidth(Bitmap img,int sourceWidth,int sourceHeight)
        {
            IList<Bitmap> bmpList = new List<Bitmap>();
            int charW = Convert.ToInt16(sourceWidth * Config.BLUE_TICKETNO_CHAR_WIDTH_RATIO);
            int h = Convert.ToInt16(sourceHeight * Config.BLUE_TICKETNO_H_CORP_RATIO);
            Rectangle rect = new Rectangle(0, 0, charW, h);
            Crop c = new Crop(rect);
            bmpList.Add(c.Apply(img));
            int numW = (img.Width - charW) / 6;
            for (int i = 0; i < Config.TICKET_NO_NUM_COUNT; i++)
            {
                int x = charW + numW * i;
                rect = new Rectangle(x, 0, numW, h);
                c = new Crop(rect);
                bmpList.Add(c.Apply(img));
            }
            return bmpList;
        }


        /// <summary>
        /// 等距分割21位码
        /// </summary>
        /// <param name="img"></param>
        /// <param name="sourceWidth"></param>
        /// <param name="sourceHeight"></param>
        /// <returns></returns>
        public static IList<Bitmap> SplitTicketCodeByDefinedWidth(Bitmap img, int sourceWidth, int sourceHeight)
        {
            IList<Bitmap> bmpList = new List<Bitmap>();
            int charW = Convert.ToInt16(sourceWidth * Config.BLUE_CODE_CHAR_WIDTH_RATIO);
            int h = img.Height;
            Rectangle rect = new Rectangle(0, 0, charW, h);
            Crop c;
            int numW = (img.Width - charW) / 20;
            for (int i = 0; i < 21; i++)
            {
                if(i < 14)
                {
                    int x = numW * i;
                    rect = new Rectangle(x, 0, numW, h);
                    c = new Crop(rect);
                    bmpList.Add(c.Apply(img));

                }else if(i == 14)
                {
                    int x = numW * i;
                    rect = new Rectangle(x, 0, numW + 3, h);
                    c = new Crop(rect);
                    bmpList.Add(c.Apply(img));

                }else
                {
                    int x = numW * i  + 3;
                    rect = new Rectangle(x, 0, numW , h);
                    c = new Crop(rect);
                    bmpList.Add(c.Apply(img));

                }
            }
            return bmpList;
        }



        /// <summary>
        /// 图片分割
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static IList<Bitmap> Split(Bitmap img, int charCount)
        {
            IList<Bitmap> bmpList = new List<Bitmap>();
            if (img == null) return bmpList;
            List<int[]> xCutPointList = GetXCutPointList(img);
            List<int[]> yCutPointList = GetYCutPointList(xCutPointList, img);
            for (int i = 0; i < xCutPointList.Count(); i++)
            {
                int xStart = xCutPointList[i][0];
                int xEnd = xCutPointList[i][1];
                int yStart = yCutPointList[i][0];
                int yEnd = yCutPointList[i][1];
                if (i >= charCount) break;
                Rectangle rect = new Rectangle(xStart, yStart, xEnd - xStart + 1, yEnd - yStart + 1);
                Crop c = new Crop(rect);
                bmpList.Add(c.Apply(img));
            }

            bmpList = ToResizeAndCenterIt(bmpList);
            return bmpList;
        }


        /// <summary>
        /// 获取竖向分割点
        /// </summary>
        /// <param name="img"></param>
        /// <returns>List int[xstart xend]</returns>
        private static List<int[]> GetXCutPointList(Bitmap img)
        {
            //分割点  List<int[xstart xend]>
            List<int[]> xCutList = new List<int[]>();
            int startX = -1;//-1表示在寻找开始节点
            for (var x = 0; x < img.Width; x++)
            {
                if (startX == -1)//开始点
                {
                    int blackPXCount = GetBlackPXCountInX(x, 3, img);
                    //如果大于有效像素则是开始节点
                    if (blackPXCount > 7)
                    {
                        startX = x;
                    }
                }
                else//结束点
                {
                    if (x == img.Width - 1)//判断是否最后一列
                    {
                        xCutList.Add(new int[] { startX, x });
                        break;
                    }
                    else if (x >= startX + MinWidthPerChar)//隔开一定距离才能结束分割
                    {
                        int blackPXCount = GetBlackPXCountInX(x, 3, img);
                        //小于等于阀值则是结束节点
                        if (blackPXCount <= 2)
                        {
                            if (x > startX + MaxWidthPerChar)
                            {
                                //大于最大字符的宽度应该是两个字符粘连到一块了 从中间分开
                                int middleX = startX + (x - startX) / 2;
                                xCutList.Add(new int[] { startX, middleX });
                                xCutList.Add(new int[] { middleX + 1, x });
                            }
                            else
                            {
                                //验证黑色像素是否太少
                                blackPXCount = GetBlackPXCountInX(startX, x - startX, img);
                                if (blackPXCount <= 10)
                                {
                                    startX = -1;
                                }
                                else
                                {
                                    xCutList.Add(new int[] { startX, x });
                                }
                            }
                            startX = -1;
                        }
                    }
                }
            }
            return xCutList;
        }

        /// <summary>
        /// 根据XCutPointList获取每个字符左上、右下坐标用于剪切
        /// 1、从中间查找 上下交替 找到有两行空白为止
        /// </summary>
        /// <param name="xCutPointList">List int[xstart xend]</param>
        /// <returns>每个字符上、下坐标 List int[ystart yend]</returns>
        private static List<int[]> GetYCutPointList(List<int[]> xCutPointList, Bitmap img)
        {
            List<int[]> list = new List<int[]>();
            int yMiddle = img.Height / 2;
            //某区域内黑色像素最小阀值
            int minBlackPXInY = 1;
            //y开始区域黑色像素数量
            int yStartBlackPointCount = 100;
            //y结束区域黑色像素数量
            int yEndBlackPointCount = 100;
            int yStart = 0;//y开始坐标
            int yEnd = 0;//y结束坐标

            foreach (var xPoint in xCutPointList)
            {
                //重置为100否则出错
                yStartBlackPointCount = 100;
                yEndBlackPointCount = 100;
                //从中间往两边偏移 直到像素小于阀值
                for (var i = 0; i < yMiddle; i++)
                {
                    if (yStartBlackPointCount > minBlackPXInY)
                    {
                        yStart = yMiddle - i;
                        yStartBlackPointCount = GetBlackPXCountInY(yStart, 2, xPoint[0], xPoint[1], img);
                    }
                    if (yEndBlackPointCount > minBlackPXInY || yEnd - yStart < MinHeightPerChar)
                    {
                        yEnd = yMiddle + i;
                        yEndBlackPointCount = GetBlackPXCountInY(yEnd, 2, xPoint[0], xPoint[1], img);
                    }
                    if (yStartBlackPointCount <= minBlackPXInY
                        && yEndBlackPointCount <= minBlackPXInY
                        && yEnd - yStart >= MinHeightPerChar)
                    {
                        break;
                    }
                }

                list.Add(new int[] { yStart, yEnd });
            }
            return list;
        }

        /// <summary>
        /// 获取分割后某区域的黑色像素
        /// </summary>
        /// <param name="startY"></param>
        /// <param name="offset"></param>
        /// <param name="startX"></param>
        /// <param name="endX"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        private static int GetBlackPXCountInY(int startY, int offset, int startX, int endX, Bitmap img)
        {
            int blackPXCount = 0;
            int startY1 = offset > 0 ? startY : startY + offset;
            int offset1 = offset > 0 ? startY + offset : startY;
            for (var x = startX; x <= endX; x++)
            {
                for (var y = startY1; y < offset1; y++)
                {
                    if (y >= img.Height)
                    {
                        continue;
                    }
                    if (img.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                    {
                        blackPXCount++;
                    }
                }
            }
            return blackPXCount;
        }


        /// <summary>
        /// 去除干扰线
        /// </summary>
        /// <param name="img"></param>
        public static void FilterDisturb(Bitmap img)
        {
            byte[] p = new byte[9]; //最小处理窗口3*3
            //去干扰线
            for (var x = 0; x < img.Width; x++)
            {
                for (var y = 0; y < img.Height; y++)
                {
                    Color currentColor = img.GetPixel(x, y);
                    int color = currentColor.ToArgb();

                    if (x > 0 && y > 0 && x < img.Width - 1 && y < img.Height - 1)
                    {
                        #region 中值滤波效果不好
                        ////取9个点的值
                        //p[0] = img.GetPixel(x - 1, y - 1).R;
                        //p[1] = img.GetPixel(x, y - 1).R;
                        //p[2] = img.GetPixel(x + 1, y - 1).R;
                        //p[3] = img.GetPixel(x - 1, y).R;
                        //p[4] = img.GetPixel(x, y).R;
                        //p[5] = img.GetPixel(x + 1, y).R;
                        //p[6] = img.GetPixel(x - 1, y + 1).R;
                        //p[7] = img.GetPixel(x, y + 1).R;
                        //p[8] = img.GetPixel(x + 1, y + 1).R;
                        ////计算中值
                        //for (int j = 0; j < 5; j++)
                        //{
                        //    for (int i = j + 1; i < 9; i++)
                        //    {
                        //        if (p[j] > p[i])
                        //        {
                        //            s = p[j];
                        //            p[j] = p[i];
                        //            p[i] = s;
                        //        }
                        //    }
                        //}
                        ////      if (img.GetPixel(x, y).R < dgGrayValue)
                        //img.SetPixel(x, y, Color.FromArgb(p[4], p[4], p[4]));    //给有效值付中值
                        #endregion

                        //上 x y+1
                        double upDif = GetColorDif(currentColor, img.GetPixel(x, y + 1));
                        //下 x y-1
                        double downDif = GetColorDif(currentColor, img.GetPixel(x, y - 1));
                        //左 x-1 y
                        double leftDif = GetColorDif(currentColor, img.GetPixel(x - 1, y));
                        //右 x+1 y
                        double rightDif = GetColorDif(currentColor, img.GetPixel(x + 1, y));
                        //左上
                        double upLeftDif = GetColorDif(currentColor, img.GetPixel(x - 1, y + 1));
                        //右上
                        double upRightDif = GetColorDif(currentColor, img.GetPixel(x + 1, y + 1));
                        //左下
                        double downLeftDif = GetColorDif(currentColor, img.GetPixel(x - 1, y - 1));
                        //右下
                        double downRightDif = GetColorDif(currentColor, img.GetPixel(x + 1, y - 1));

                        ////四面色差较大
                        //if (upDif > threshold && downDif > threshold && leftDif > threshold && rightDif > threshold)
                        //{
                        //    img.SetPixel(x, y, Color.White);
                        //}
                        //三面色差较大
                        if ((upDif > colorDiffThreshold && downDif > colorDiffThreshold && leftDif > colorDiffThreshold)
                            || (downDif > colorDiffThreshold && leftDif > colorDiffThreshold && rightDif > colorDiffThreshold)
                            || (upDif > colorDiffThreshold && leftDif > colorDiffThreshold && rightDif > colorDiffThreshold)
                            || (upDif > colorDiffThreshold && downDif > colorDiffThreshold && rightDif > colorDiffThreshold))
                        {
                            img.SetPixel(x, y, Color.White);
                        }

                        List<int[]> xLine = new List<int[]>();
                        //去横向干扰线  原理 如果这个点上下有很多白色像素则认为是干扰
                        for (var x1 = x + 1; x1 < x + 10; x1++)
                        {
                            if (x1 >= img.Width)
                            {
                                break;
                            }

                            if (img.GetPixel(x1, y + 1).ToArgb() == Color.White.ToArgb()
                                && img.GetPixel(x1, y - 1).ToArgb() == Color.White.ToArgb())
                            {
                                xLine.Add(new int[] { x1, y });
                            }
                        }
                        if (xLine.Count() >= 4)
                        {
                            foreach (var xpoint in xLine)
                            {
                                img.SetPixel(xpoint[0], xpoint[1], Color.White);
                            }
                        }

                        //去竖向干扰线

                    }
                }
            }
        }

        /// <summary>
        /// 图片归一化
        /// </summary>
        /// 对分割好的图片进行大小归一，居中处理
        /// <param name="list"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static IList<Bitmap> ToResizeAndCenterIt(IList<Bitmap> list, int w = 40, int h = 50)
        {
            IList<Bitmap> resizeList = new List<Bitmap>();


            for (int i = 0; i < list.Count; i++)
            {
                //反转一下图片
                list[i] = new Invert().Apply(list[i]);

                int sw = list[i].Width;
                int sh = list[i].Height;

                Crop corpFilter = new Crop(new Rectangle(0, 0, w, h));

                list[i] = corpFilter.Apply(list[i]);

                //再反转回去
                list[i] = new Invert().Apply(list[i]);

                //计算中心位置
                int centerX = (w - sw) / 2;
                int centerY = (h - sh) / 2;

                list[i] = new CanvasMove(new IntPoint(centerX, centerY), Color.White).Apply(list[i]);

                resizeList.Add(list[i]);
            }

            return resizeList;
        }

        public static Bitmap ToResizeAndCenterIt(Bitmap img, int w = 40, int h = 50)
        {
 

                //反转一下图片
                img = new Invert().Apply(img);

                int sw = img.Width;
                int sh = img.Height;

                Crop corpFilter = new Crop(new Rectangle(0, 0, w, h));

                img = corpFilter.Apply(img);

                //再反转回去
                img = new Invert().Apply(img);

                //计算中心位置
                int centerX = (w - sw) / 2;
                int centerY = (h - sh) / 2;

                img = new CanvasMove(new IntPoint(centerX, centerY), Color.White).Apply(img);


            return img;
        }


        /// <summary>
        /// 返回两图比较的相似度 最大1
        /// </summary>
        /// <param name="compareImg">对比图</param>
        /// <param name="mainImg">要识别的图</param>
        /// <returns></returns>
        private static double CompareImg(Bitmap compareImg, Bitmap mainImg)
        {
            int img1x = compareImg.Width;
            int img1y = compareImg.Height;
            int img2x = mainImg.Width;
            int img2y = mainImg.Height;
            //最小宽度
            double min_x = img1x > img2x ? img2x : img1x;
            //最小高度
            double min_y = img1y > img2y ? img2y : img1y;

            double score = 0;
            //重叠的黑色像素
            for (var x = 0; x < min_x; x++)
            {
                for (var y = 0; y < min_y; y++)
                {
                    if (compareImg.GetPixel(x, y).ToArgb() == Color.Black.ToArgb()
                        && compareImg.GetPixel(x, y).ToArgb() == mainImg.GetPixel(x, y).ToArgb())
                    {
                        score++;
                    }
                }
            }
            double originalBlackCount = 0;
            //对比图片的黑色像素
            for (var x = 0; x < img1x; x++)
            {
                for (var y = 0; y < img1y; y++)
                {
                    if (Color.Black.ToArgb() == compareImg.GetPixel(x, y).ToArgb())
                    {
                        originalBlackCount++;
                    }
                }
            }
            return score / originalBlackCount;
        }


        /// <summary>
        /// 用所有的学习的图片对比当前图片
        /// 最后取黑色重叠区域最多的
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static String Recognize(IList<Bitmap> imgs,String templatePath)
        {
            string[] detailPathList = Directory.GetDirectories(templatePath);
            if (detailPathList == null || detailPathList.Length == 0)
            {
                return "";
            }
            //config.txt 文件中指定了识别字母的顺序
            string configPath = templatePath + "config.txt";
            if (!File.Exists(configPath))
            {
                return "";
            }

            String resultS = String.Empty;
            foreach (Bitmap img in imgs)
            {
                string resultString = string.Empty;
                string configString = File.ReadAllText(configPath);
                double maxRate = 0;//相似度  最大1
                foreach (char resultChar in configString)
                {
                    string charPath = templatePath + resultChar.ToString();
                    if (!Directory.Exists(charPath))
                    {
                        continue;
                    }
                    string[] fileNameList = Directory.GetFiles(charPath);
                    if (fileNameList == null || fileNameList.Length == 0)
                    {
                        continue;
                    }


                    foreach (string filename in fileNameList)
                    {
                        Bitmap imgSample = new Bitmap(filename);
                        //过滤宽高相差太大的
                        if (Math.Abs(imgSample.Width - img.Width) >= 6
                            || Math.Abs(imgSample.Height - img.Height) >= 6)
                        {
                            continue;
                        }
                        //当前相似度
                        double currentRate = CompareImg(imgSample, img);
                        if (currentRate > maxRate)
                        {
                            maxRate = currentRate;
                            resultString = resultChar.ToString();
                        }
                        imgSample.Dispose();
                    }
                }
                resultS += resultString;
            }
            return resultS;
        }

        

    }
}
