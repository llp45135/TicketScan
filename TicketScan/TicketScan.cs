using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketScan
{
    public class TicketScan
    {
        public IList<Bitmap> SplitTicketCodeByDefinedWidth(Bitmap img, int ticketType)
        {
            //Bitmap img = CutTicketCodeEdge(sourceImg, ticketType);



            int sourceWidth = 0;
            int charW = 0;
            int numW = 0;
            int offset = 0;
            if (Config.RED_TICKET == ticketType)
            {
                sourceWidth = Config.RED_TICKET_WIDTH;
                charW = Convert.ToInt16(sourceWidth * Config.RED_CODE_CHAR_WIDTH_RATIO);
                numW = Convert.ToInt16(sourceWidth * Config.RED_CODE_NUM_WIDTH_RATIO);
                offset = Convert.ToInt16(sourceWidth * Config.RED_CODE_CHAR_OFFSET_RATIO);
            }
            else
            {
                sourceWidth = Config.BLUE_TICKET_WIDTH;
                charW = Convert.ToInt16(sourceWidth * Config.BLUE_CODE_CHAR_WIDTH_RATIO);
                numW = Convert.ToInt16(sourceWidth * Config.BLUE_CODE_NUM_WIDTH_RATIO);
                offset = Convert.ToInt16(sourceWidth * Config.BLUE_CODE_CHAR_OFFSET_RATIO);

            }

            IList<Bitmap> bmpList = new List<Bitmap>();
            int h = img.Height;
            Rectangle rect = new Rectangle(0, 0, charW, h);
            Crop c;
            //int numW = (img.Width - charW) / 20;
            for (int i = 0; i < 21; i++)
            {
                if (i < 14)
                {
                    int x = numW * i;
                    rect = new Rectangle(x, 0, numW, h);
                    c = new Crop(rect);
                    bmpList.Add(c.Apply(img));

                }
                else if (i == 14)
                {
                    int x = numW * i;
                    rect = new Rectangle(x, 0, numW + offset, h);
                    c = new Crop(rect);
                    bmpList.Add(c.Apply(img));

                }
                else
                {
                    int x = numW * i + offset;
                    rect = new Rectangle(x, 0, numW, h);
                    c = new Crop(rect);
                    bmpList.Add(c.Apply(img));

                }
            }
            return bmpList;
        }


        public IList<Bitmap> SplitTicketCode(Bitmap img)
        {
            IList<Bitmap> bmpList = new List<Bitmap>();
            if (img == null) return bmpList;
            int startx = 0, stopx = 0;
            bool isBegin = true;
            int count = 0, averageW = 0,sumW = 0;
            for(int x = 0; x < img.Width; x++)
            {
                int bc = GetBlackPXCountInX(x,1, img);
                if(bc > 0)
                {
                    if (isBegin)
                    {
                        isBegin = false;
                        startx = x;
                    }
                }else
                {
                    if (!isBegin)
                    {
                        stopx = x;
                        count++;
                        int w = stopx - startx;
                        sumW += w;
                        averageW = sumW / count;

                        if(w < averageW -2)
                        {

                        }else
                        {
                            isBegin = true;
                            bmpList.Add(cut(img, startx, w));
                        }
                    }
                }
            }

            return bmpList;
        }


        private Bitmap cut(Bitmap source,int x,int width)
        {
            Rectangle rect = new Rectangle(x, 0, width , source.Height);
            Crop c = new Crop(rect);
            return c.Apply(source);

        }


        /// <summary>
        /// 剪切按照预设的21位码高度，剪切21位码图像
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public Bitmap CutTicketCodeEdge(Bitmap bmp, int ticketType)
        {
            int codeHight = 0;
            int codeWidth = 0;
            int sourceHight = 0;
            int sourceWidth = 0;

            if (Config.RED_TICKET == ticketType)
            {
                sourceHight = Config.RED_TICKET_HEIGHT;
                sourceWidth = Config.RED_TICKET_WIDTH;
                codeHight = Convert.ToInt16(sourceHight * Config.RED_CODE_CHAR_HEIGHT_RATIO);
                codeWidth = Convert.ToInt16(sourceWidth * Config.RED_CODE_W_CORP_RATIO);
            }
            else
            {
                sourceHight = Config.BLUE_TICKET_HEIGHT;
                sourceWidth = Config.BLUE_TICKET_WIDTH;
                codeHight = Convert.ToInt16(sourceHight * Config.BLUE_CODE_CHAR_HEIGHT_RATIO);
                codeWidth = Convert.ToInt16(sourceWidth * Config.BLUE_CODE_W_CORP_RATIO);

            }
            int w = 0;
            if (codeWidth < bmp.Width)
                w = codeHight;
            else
                w = bmp.Width;
            Rectangle rect = new Rectangle(0, bmp.Height - codeHight, w, codeHight);

            Crop corp = new Crop(rect);
            return corp.Apply(bmp);
        }

        /// <summary>
        /// 获取左边空白切割点 X坐标
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private int GetLeftXCutPoint(Bitmap img)
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
        private int GetRightXCutPoint(Bitmap img)
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
        private int GetTopYCutPoint(Bitmap img)
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
        private int GetBottomYCutPoint(Bitmap img)
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
        private int GetBlackPXCountInX(int startX, int offset, Bitmap img)
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

        private int GetBlackPXCountInX(int x, Bitmap img)
        {
            int blackPXCount = 0;
            for (var y = 0; y < img.Height; y++)
            {
                if (img.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                {
                    blackPXCount++;
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
        private int GetBlackPXCountInY(int startY, int offset, Bitmap img)
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






        public Bitmap ExtractTicketCodeImage(Bitmap source, int ticketType)
        {
            Bitmap temp = (Bitmap)source.Clone();
            //            temp = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp);
            //            //temp = temp.Clone(new Rectangle(0, 0, temp.Width, temp.Height), PixelFormat.Format24bppRgb);
            //            FiltersSequence seq = new FiltersSequence();
            ////            seq.Add(Grayscale.CommonAlgorithms.Y);  // 添加灰度滤镜
            //            seq.Add(new OtsuThreshold()); // 添加二值化滤镜
            //            temp = seq.Apply(temp);

            //temp = new Grayscale(0.2125, 0.7154, 0.0721).Apply(temp);
            //Binarizate(temp);



            Bitmap retImg = null;
            if (Config.BLUE_TICKET == ticketType)
            {
                int leftX = Convert.ToInt16(temp.Width * Config.BLUE_CODE_X_CORP_RATIO);
                int topY = Convert.ToInt16(temp.Height * Config.BLUE_CODE_Y_CORP_RATIO);
                int w = Convert.ToInt16(temp.Width * Config.BLUE_CODE_W_CORP_RATIO);
                int h = Convert.ToInt16(temp.Height * Config.BLUE_CODE_H_CORP_RATIO);
                Rectangle rect = new Rectangle(leftX, topY, w, h);
                Crop corp = new Crop(rect);
                retImg = corp.Apply(temp);
            }
            else
            {
                int leftX = Convert.ToInt16(temp.Width * Config.RED_CODE_X_CORP_RATIO);
                int topY = Convert.ToInt16(temp.Height * Config.RED_CODE_Y_CORP_RATIO);
                int w = Convert.ToInt16(temp.Width * Config.RED_CODE_W_CORP_RATIO);
                int h = Convert.ToInt16(temp.Height * Config.RED_CODE_H_CORP_RATIO);
                Rectangle rect = new Rectangle(leftX, topY - h, w, h);
                Crop corp = new Crop(rect);
                retImg = corp.Apply(temp);
            }
            //bmpTicket.Clone(new Rectangle(0, 0, bmpTicket.Width, bmpTicket.Height), PixelFormat.Format24bppRgb);
            return retImg.Clone(new Rectangle(0, 0, retImg.Width, retImg.Height), PixelFormat.Format24bppRgb);
        }




        /// <summary>
        /// 二值化
        /// </summary>
        /// <param name="map"></param>
        private void Binarizate(Bitmap map)
        {
            map = map.Clone(new Rectangle(0, 0, map.Width, map.Height), PixelFormat.Format24bppRgb);
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
        private int ComputeThresholdValue(Bitmap img)
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


    }
}
