using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketScan
{
    public class TicketRecognizer
    {
        private FiltersSequence commonSeq,extractCodeSeq;                          //Commonly filter sequence to be used 

        public TicketRecognizer()
        {
            commonSeq = new FiltersSequence();
            extractCodeSeq = new FiltersSequence();
            commonSeq.Add(new GrayscaleBT709());                            //灰度化
            commonSeq.Add(new SISThreshold());                              //二值化
            commonSeq.Add(new Invert());
            extractCodeSeq.Add(new Mean());                              //均值滤波
            //extractCodeSeq.Add(new Invert());                            //黑白翻转

        }


        /// <summary>
        /// 提取21位码图像
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ticketType"></param>
        /// <returns></returns>
        public Bitmap GetTicketCodeImgs(Bitmap source, int ticketType)
        {
            Bitmap tempBin = source.Clone() as Bitmap; //Clone image to keep original image

            int minHeight, maxHeight, charWidth, charHeight, charOffset;
            if (Config.BLUE_TICKET == ticketType)
            {
                minHeight = Convert.ToInt16(Config.BLUE_CODE_CHAR_HEIGHT_RATIO * 0.7 * Config.BLUE_TICKET_HEIGHT);
                maxHeight = Convert.ToInt16(Config.BLUE_CODE_CHAR_HEIGHT_RATIO * 1.1 * Config.BLUE_TICKET_HEIGHT);
                charHeight = Convert.ToInt16(Config.BLUE_CODE_CHAR_HEIGHT_RATIO * Config.BLUE_TICKET_HEIGHT);
                charWidth = Convert.ToInt16(Config.BLUE_CODE_CHAR_WIDTH_RATIO * Config.BLUE_TICKET_WIDTH);
                charOffset = Convert.ToInt16(Config.BLUE_CODE_CHAR_OFFSET_RATIO * Config.RED_TICKET_WIDTH);

            }
            else
            {
                minHeight = Convert.ToInt16(Config.RED_CODE_CHAR_HEIGHT_RATIO * 0.7 * Config.RED_TICKET_HEIGHT);
                maxHeight = Convert.ToInt16(Config.RED_CODE_CHAR_HEIGHT_RATIO * 1.1 * Config.RED_TICKET_HEIGHT);
                charHeight = Convert.ToInt16(Config.RED_CODE_CHAR_HEIGHT_RATIO * Config.RED_TICKET_HEIGHT);
                charWidth = Convert.ToInt16(Config.RED_CODE_CHAR_WIDTH_RATIO * Config.RED_TICKET_WIDTH);
                charOffset = Convert.ToInt16(Config.RED_CODE_CHAR_OFFSET_RATIO * Config.RED_TICKET_WIDTH);
            }
            tempBin = commonSeq.Apply(source); // Apply filters on source image
            Bitmap temp = tempBin.Clone() as Bitmap;
            temp = extractCodeSeq.Apply(temp);

            BlobCounter blobCounter = new BlobCounter(temp); //把图片上的联通物体都分离开来
            Blob[] blobs = blobCounter.GetObjects(temp, false);
            IList<Bitmap> codeImgs = new List<Bitmap>();
            Dictionary<int, Blob> codeImgDic = new Dictionary<int, Blob>();

            int minX = 0 , minY = 0;
            foreach (Blob b in blobs)
            {
                if (b.Image.Height >= minHeight)
                {
                    if (b.Rectangle.X < minX) minX = b.Rectangle.X;
                    if (b.Rectangle.Y < minY) minY = b.Rectangle.Y;
                    codeImgDic.Add(b.Rectangle.X, b);
                }else
                {
                    Console.WriteLine("  height=" + b.Image.Height + " width=" + b.Image.Width + "  X=" + b.Rectangle.X);
                }
            }


            Bitmap bmp = new Bitmap(charWidth * 21 , charHeight + 4);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, charWidth * 21 , charHeight + 4);
            int offset = 0, preX = 0, preY = 0, x = 0, y = 0;
            var dicSort = from objDic in codeImgDic orderby objDic.Key select objDic;
            IList<KeyValuePair<int, Blob>> list = new List<KeyValuePair<int, Blob>>();
            list = dicSort.ToList<KeyValuePair<int, Blob>>();



            for (int i = 0; i < list.Count; i++)
            {
                KeyValuePair<int, Blob> kvp = list[i];
                Blob blob = kvp.Value;
                Crop c = new Crop(blob.Rectangle);
                Bitmap b = c.Apply(tempBin);
                PointF p = new PointF(offset, 2);
                g.DrawImage(b, p);
                offset += blob.Rectangle.Width;
            }




            g.Dispose();
            //return commonSeq.Apply(bmp);
            return bmp;
        }

        /// <summary>
        /// 切割21位码的区域
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ticketType"></param>
        /// <returns></returns>
        public Bitmap ExtractTicketCodeImage(Bitmap source, int ticketType)
        {
            Bitmap temp = (Bitmap)source.Clone();
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
            return retImg.Clone(new Rectangle(0, 0, retImg.Width, retImg.Height), PixelFormat.Format24bppRgb);
        }



        public Bitmap Prepare(Bitmap source)
        {
            return commonSeq.Apply(source);
        }
    }
}
