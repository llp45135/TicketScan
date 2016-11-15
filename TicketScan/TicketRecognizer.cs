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
        private FiltersSequence commonSeq, extractCodeSeq, extractQRCodeSeq;                          //Commonly filter sequence to be used 

        public TicketRecognizer()
        {
            commonSeq = new FiltersSequence();
            extractCodeSeq = new FiltersSequence();
            extractQRCodeSeq = new FiltersSequence();
            commonSeq.Add(new GrayscaleBT709());                            //灰度化
            commonSeq.Add(new SISThreshold());                              //二值化
            commonSeq.Add(new Invert());
            extractCodeSeq.Add(new Mean());                                 //均值滤波
            //extractCodeSeq.Add(new Invert());                            //黑白翻转
            extractQRCodeSeq.Add(new GrayscaleBT709());
            extractQRCodeSeq.Add(new SISThreshold());
            extractQRCodeSeq.Add(new DifferenceEdgeDetector());
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


            foreach (Blob b in blobs)
            {
                if (b.Image.Height >= minHeight)
                {
                    codeImgDic.Add(b.Rectangle.X, b);
                }
            }

            var rectYList = from o in codeImgDic.Values orderby o.Rectangle.Y select o;
            int maxY = rectYList.Last().Rectangle.Y;

            Bitmap bmp = new Bitmap(charWidth * 21, charHeight + 4);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, charWidth * 21, charHeight + 4);
            int offset = 0;

            IList<KeyValuePair<int, Blob>> list = new List<KeyValuePair<int, Blob>>();
            var dicFilter = from objDic in codeImgDic where Math.Abs(objDic.Value.Rectangle.Y - maxY) <= charHeight orderby objDic.Key select objDic;
            list = dicFilter.ToList<KeyValuePair<int, Blob>>();


            for (int i = 0; i < list.Count; i++)
            {
                KeyValuePair<int, Blob> kvp = list[i];
                Blob blob = kvp.Value;



                if (i > 0)
                {
                    KeyValuePair<int, Blob> preKvp = list[i - 1];
                    Blob preBlob = preKvp.Value;
                    if (Math.Abs(blob.Rectangle.X - preBlob.Rectangle.X - preBlob.Rectangle.Width) <= charWidth)
                    {
                        Crop c = new Crop(blob.Rectangle);
                        Bitmap b = c.Apply(tempBin);
                        PointF p = new PointF(offset, 2);
                        g.DrawImage(b, p);
                        offset += blob.Rectangle.Width + 1;
                    }
                    else
                    {
                        Bitmap b = blob.Image.ToManagedImage();
                        Bitmap pb = preBlob.Image.ToManagedImage();

                        Console.WriteLine(Math.Abs(blob.Rectangle.X - preBlob.Rectangle.X - preBlob.Rectangle.Width));

                    }
                }else
                {
                    Crop c = new Crop(blob.Rectangle);
                    Bitmap b = c.Apply(tempBin);
                    PointF p = new PointF(offset, 2);
                    g.DrawImage(b, p);
                    offset += blob.Rectangle.Width + 1;

                }
            }
            g.Dispose();
            return commonSeq.Apply(bmp);
            //return bmp;
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
                Rectangle rect = new Rectangle(leftX, topY - h, w, h);
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


        /// <summary>
        /// 截取二维码区域
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ticketType"></param>
        /// <returns></returns>
        public Bitmap ExtractQRCodeImage(Bitmap source, int ticketType)
        {
            Bitmap temp = (Bitmap)source.Clone();
            Bitmap retImg = null;
            if (Config.BLUE_TICKET == ticketType)
            {
                int leftX = Convert.ToInt16(temp.Width * Config.BLUE_QRCODE_X_COPR_RATIO);
                int topY = Convert.ToInt16(temp.Height * Config.BLUE_QRCODE_Y_COPR_RATIO);
                int w = Convert.ToInt16(temp.Width * Config.BLUE_QRCODE_W_COPR_RATIO);
                int h = Convert.ToInt16(temp.Height * Config.BLUE_QRCODE_H_COPR_RATIO);
                Rectangle rect = new Rectangle(leftX, topY, w, h);
                Crop corp = new Crop(rect);
                retImg = corp.Apply(temp);
            }
            else
            {
                int leftX = Convert.ToInt16(temp.Width * Config.RED_QRCODE_X_COPR_RATIO);
                int topY = Convert.ToInt16(temp.Height * Config.RED_QRCODE_Y_COPR_RATIO);
                int w = Convert.ToInt16(temp.Width * Config.RED_QRCODE_W_COPR_RATIO);
                int h = Convert.ToInt16(temp.Height * Config.RED_QRCODE_H_COPR_RATIO);
                Rectangle rect = new Rectangle(leftX, topY, w, h);
                Crop corp = new Crop(rect);
                retImg = corp.Apply(temp);
            }
            return retImg.Clone(new Rectangle(0, 0, retImg.Width, retImg.Height), PixelFormat.Format24bppRgb);

        }



        /// <summary>
        /// 检测是否存在二维码区域
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool IsExistQRCode(Bitmap source, int ticketType)
        {
            bool isQRCode = false;

            int qrCodeW = 0, qrCodeH = 0;
            if (Config.BLUE_TICKET == ticketType)
            {
                qrCodeW = Convert.ToInt16(Config.BLUE_QRCODE_W_COPR_RATIO * Config.BLUE_TICKET_WIDTH);
                qrCodeH = Convert.ToInt16(Config.BLUE_QRCODE_H_COPR_RATIO * Config.BLUE_TICKET_HEIGHT);
            }
            else
            {
                qrCodeW = Convert.ToInt16(Config.RED_QRCODE_W_COPR_RATIO * Config.RED_TICKET_WIDTH);
                qrCodeH = Convert.ToInt16(Config.RED_QRCODE_H_COPR_RATIO * Config.RED_TICKET_HEIGHT);

            }
            Bitmap temp = extractQRCodeSeq.Apply(source);
            BlobCounter blobCounter = new BlobCounter(temp); //把图片上的联通物体都分离开来
            Blob[] blobs = blobCounter.GetObjects(temp, false);

            foreach (Blob b in blobs)
            {
                if (b.Image.Height >= qrCodeH * 0.7 && b.Image.Width >= qrCodeW * 0.7)
                    isQRCode = true;
            }

            return isQRCode;

        }



        public Bitmap DetectQRCode(Bitmap source, int ticketType)
        {

            Bitmap temp = ExtractQRCodeImage(source, ticketType);

            if (IsExistQRCode(temp, ticketType))
            {
                return temp;
            }
            else
            {
                source.RotateFlip(RotateFlipType.Rotate180FlipX);
                Bitmap temp1 = ExtractQRCodeImage(source, ticketType);
                if (IsExistQRCode(temp1, ticketType))
                {
                    return temp1;
                }
            }
            return null;
        }


        public Bitmap Prepare(Bitmap source)
        {
            return commonSeq.Apply(source);
        }
    }
}
