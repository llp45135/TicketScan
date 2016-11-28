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
        private FiltersSequence commonSeq, extractCodeSeq, extractQRCodeSeqForRedTicket, extractQRCodeSeqForBlueTicket, qrReturnSeq;                          //Commonly filter sequence to be used 

        public TicketRecognizer()
        {
            commonSeq = new FiltersSequence();
            extractCodeSeq = new FiltersSequence();
            extractQRCodeSeqForRedTicket = new FiltersSequence();
            extractQRCodeSeqForBlueTicket = new FiltersSequence();
            qrReturnSeq = new FiltersSequence();
            commonSeq.Add(new GrayscaleBT709());                            //灰度化
            commonSeq.Add(new SISThreshold());                              //二值化
            commonSeq.Add(new Invert());
            extractCodeSeq.Add(new Mean());                                 //均值滤波
            //extractCodeSeq.Add(new Invert());                            //黑白翻转
            extractQRCodeSeqForRedTicket.Add(new GrayscaleBT709());
            extractQRCodeSeqForRedTicket.Add(new DifferenceEdgeDetector());
            extractQRCodeSeqForRedTicket.Add(new SISThreshold());
            extractQRCodeSeqForRedTicket.Add(new Dilatation());


            extractQRCodeSeqForBlueTicket.Add(new GrayscaleBT709());
            extractQRCodeSeqForBlueTicket.Add(new DifferenceEdgeDetector());
            extractQRCodeSeqForBlueTicket.Add(new SISThreshold());
            extractQRCodeSeqForBlueTicket.Add(new Dilatation());

            qrReturnSeq.Add(new GrayscaleBT709());
            qrReturnSeq.Add(new Blur());
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
            temp = ImageProcess.CutBlankEdge(temp);

            Bitmap bmp = new Bitmap(charWidth * 21, charHeight + 4);

            BlobCounter blobCounter = new BlobCounter(temp); //把图片上的联通物体都分离开来
            Blob[] blobs = blobCounter.GetObjects(temp, false);
            IList<Bitmap> codeImgs = new List<Bitmap>();

            var blobFilters = from o in blobs where o.Image.Height >= minHeight orderby o.Rectangle.Y descending select o;

            if (blobFilters.Count<Blob>() < 1) return bmp;
            //var rectYList = from o in codeImgDic.Values orderby o.Rectangle.Y select o;
            int maxY = blobFilters.First<Blob>().Rectangle.Y;

            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, charWidth * 21, charHeight + 4);
            int offset = 0;

            var imgList = from img in blobFilters where Math.Abs(img.Rectangle.Y - maxY) <= charHeight orderby img.Rectangle.X select img;
            var list = imgList.ToList<Blob>();
            for (int i = 0; i < list.Count; i++)
            {
                Blob blob = list[i];

                if (i > 0)
                {
                    Blob preBlob = list[i - 1];
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
        public Bitmap ExtractTicketCodeImageArea(Bitmap source, int ticketType,int QRBottomY)
        {
            Bitmap temp = (Bitmap)source.Clone();
            Bitmap retImg = null;
            if (Config.BLUE_TICKET == ticketType)
            {
                int leftX = Convert.ToInt16(temp.Width * Config.BLUE_CODE_X_CORP_RATIO);
                int topY = Convert.ToInt16(temp.Height * Config.BLUE_CODE_Y_CORP_RATIO);
                int w = Convert.ToInt16(temp.Width * Config.BLUE_CODE_W_CORP_RATIO);
                int h = Convert.ToInt16(temp.Height * Config.BLUE_CODE_H_CORP_RATIO );
                Rectangle rect = new Rectangle(leftX, QRBottomY, w, h);
                Crop corp = new Crop(rect);
                retImg = corp.Apply(temp);
            }
            else
            {
                int leftX = Convert.ToInt16(temp.Width * Config.RED_CODE_X_CORP_RATIO);
                int topY = Convert.ToInt16(temp.Height * Config.RED_CODE_Y_CORP_RATIO);
                int w = Convert.ToInt16(temp.Width * Config.RED_CODE_W_CORP_RATIO);
                int h = Convert.ToInt16(temp.Height * Config.RED_CODE_H_CORP_RATIO);
                int h1 = Convert.ToInt16(h*1.5);
                Rectangle rect = new Rectangle(leftX, QRBottomY - h, w, h1);
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
        public Tuple<Bitmap,Rectangle> ExtractQRCodeImageArea(Bitmap source, int ticketType)
        {
            Bitmap temp = (Bitmap)source.Clone();
            Bitmap retImg = null;
            Rectangle rect = new Rectangle();
            if (Config.BLUE_TICKET == ticketType)
            {
                int leftX = Convert.ToInt16(temp.Width * Config.BLUE_QRCODE_X_COPR_RATIO);
                int topY = Convert.ToInt16(temp.Height * Config.BLUE_QRCODE_Y_COPR_RATIO);
                int w = Convert.ToInt16(temp.Width * Config.BLUE_QRCODE_W_COPR_RATIO);
                int h = Convert.ToInt16(temp.Height * Config.BLUE_QRCODE_H_COPR_RATIO);
                rect = new Rectangle(leftX, topY, w, h);
                Crop corp = new Crop(rect);
                retImg = corp.Apply(temp);
            }
            else
            {
                int leftX = Convert.ToInt16(temp.Width * Config.RED_QRCODE_X_COPR_RATIO);
                int topY = Convert.ToInt16(temp.Height * Config.RED_QRCODE_Y_COPR_RATIO);
                int w = Convert.ToInt16(temp.Width * Config.RED_QRCODE_W_COPR_RATIO);
                int h = Convert.ToInt16(temp.Height * Config.RED_QRCODE_H_COPR_RATIO);
                rect = new Rectangle(leftX, topY, w, h);
                Crop corp = new Crop(rect);
                retImg = corp.Apply(temp);
            }
            return Tuple.Create<Bitmap,Rectangle>(retImg,rect);

        }



        /// <summary>
        /// 检测是否存在二维码区域
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public Tuple<bool, Rectangle,Bitmap> IsExistQRCode(Bitmap source, int ticketType)
        {
            int qrCodeW = 0, qrCodeH = 0;
            Bitmap temp;
            if (Config.BLUE_TICKET == ticketType)
            {
                qrCodeW = Convert.ToInt16(Config.BLUE_QRCODE_W_COPR_RATIO * Config.BLUE_TICKET_WIDTH);
                qrCodeH = Convert.ToInt16(Config.BLUE_QRCODE_H_COPR_RATIO * Config.BLUE_TICKET_HEIGHT);
                temp = extractQRCodeSeqForBlueTicket.Apply(source);
            }
            else
            {
                qrCodeW = Convert.ToInt16(Config.RED_QRCODE_W_COPR_RATIO * Config.RED_TICKET_WIDTH);
                qrCodeH = Convert.ToInt16(Config.RED_QRCODE_H_COPR_RATIO * Config.RED_TICKET_HEIGHT);
                temp = extractQRCodeSeqForRedTicket.Apply(source);
            }
            

            
            //Bitmap temp = new GrayscaleBT709().Apply(source);
            //DifferenceEdgeDetector diffFileter = new DifferenceEdgeDetector();
            //var dff = diffFileter.FormatTranslations;
            //temp = new DifferenceEdgeDetector().Apply(temp);
            //Threshold tFilter = new Threshold();
            //tFilter.ThresholdValue = 128;
            //temp = tFilter.Apply(temp);
            //Dilatation dFilter = new Dilatation();
            //temp = dFilter.Apply(temp);
            //temp = dFilter.Apply(temp);
            //temp = dFilter.Apply(temp);

            //BlobCounter blobCounter = new BlobCounter(temp); //把图片上的联通物体都分离开来
            //Blob[] blobs = blobCounter.GetObjects(temp, false);
            //var s = from o in blobs  orderby o.Rectangle.Height* o.Rectangle.Width descending select o;


            //foreach (Blob b in s.ToList<Blob>())
            //{
            //    if (b.Image.Height >= 100 && b.Image.Width >= 100)
            //    {
            //        Crop c = new Crop(b.Rectangle);
            //        Bitmap qrcode = c.Apply(source);
            //        return Tuple.Create<bool, Rectangle,Bitmap>(true, b.Rectangle, qrcode);

            //    }
            //}


            ExtractBiggestBlob ebb = new ExtractBiggestBlob();
            Bitmap qr = ebb.Apply(temp);
            if(qr.Width >=100 && qr.Height >= 100)
            {
//                Rectangle rect = new Rectangle(ebb.BlobPosition.X,ebb.BlobPosition.Y,qr.Width,qr.Height);
                Rectangle rect = new Rectangle(ebb.BlobPosition.X -5, ebb.BlobPosition.Y -5, qr.Width + 10, qr.Height + 10);
                Crop c = new Crop(rect);
                Bitmap qrcode = c.Apply(source);
                return Tuple.Create<bool, Rectangle, Bitmap>(true, rect, qrcode);

            }



            return Tuple.Create<bool, Rectangle,Bitmap>(false,new Rectangle(),null);

        }

        /// <summary>
        /// 预处理
        /// 1、截取二维码区域，判断是否存在二维码，如果不存在，旋转180度重试
        /// 2、如果存在二维码，截取21位码图像区域
        /// </summary>
        /// <param name="source"></param>
        /// <param name="ticketType"></param>
        /// <returns></returns>
        public Tuple<int,Bitmap,Bitmap,Bitmap> Prepare(Bitmap sourceImage,int ticketType)
        {
            //var source = DocumentAlign(sourceImage);
            var source = sourceImage.Clone() as Bitmap;
            var qrImgArea = this.ExtractQRCodeImageArea(source, ticketType);
            var qrResult = this.IsExistQRCode(qrImgArea.Item1, ticketType);
            if (qrResult.Item1)
            {
                Bitmap code = this.ExtractTicketCodeImageArea(source, ticketType,qrResult.Item2.Y+ qrResult.Item2.Height + qrImgArea.Item2.Y);
                return Tuple.Create<int, Bitmap, Bitmap, Bitmap>(Config.Is_Detected_QRCode, qrResult.Item3, code,source);
            }else
            {
                source.RotateFlip(RotateFlipType.Rotate180FlipNone);
                var qrImgArea2 = this.ExtractQRCodeImageArea(source, ticketType);
                var qrResult2 = this.IsExistQRCode(qrImgArea2.Item1, ticketType);
                if (qrResult2.Item1)
                {
                    Bitmap code = this.ExtractTicketCodeImageArea(source, ticketType, qrResult2.Item2.Y + qrResult2.Item2.Height + qrImgArea2.Item2.Y);
                    return Tuple.Create<int, Bitmap, Bitmap, Bitmap>(Config.Is_Detected_QRCode, qrResult2.Item3, code, source);
                }
            }
            return Tuple.Create<int, Bitmap, Bitmap, Bitmap>(Config.Is_Blank_Ticket, qrImgArea.Item1, null, source);
        }

        public Bitmap DocumentAlign(Bitmap image)
        {
            try
            {
                // get grayscale image from current image
                Bitmap grayImage = (image.PixelFormat == PixelFormat.Format8bppIndexed) ?
                    AForge.Imaging.Image.Clone(image) :
                    AForge.Imaging.Filters.Grayscale.CommonAlgorithms.BT709.Apply(image);
                // threshold it using adaptive Otsu thresholding
                OtsuThreshold threshold = new OtsuThreshold();
                threshold.ApplyInPlace(grayImage);
                // get skew angle
                DocumentSkewChecker skewChecker = new DocumentSkewChecker();
                double angle = skewChecker.GetSkewAngle(grayImage);

                if ((angle < -skewChecker.MaxSkewToDetect) ||
                     (angle > skewChecker.MaxSkewToDetect))
                {
                    throw new ApplicationException();
                }

                // create rotation filter
                RotateBilinear rotationFilter = new RotateBilinear(-angle);
                rotationFilter.FillColor = Color.White;
                // rotate image applying the filter
                return rotationFilter.Apply(image);
            }
            catch
            {
                Console.WriteLine("Failed determining skew angle. Is it reallly a scanned document?");
                return image;
            }
        }

    }
}
