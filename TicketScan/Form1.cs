using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tesseract;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace TicketScan
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        TicketScan ts = new TicketScan();

        private Bitmap exteactTicketNoImg;
        private Bitmap exteactTicketCodeImg;
        private Bitmap bmpWork;
        private Bitmap bmpTicket;
        private Bitmap bmpQRCode;
        int ticketType = 0;

        private TicketRecognizer ticketRecognizer = new TicketRecognizer();

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {


            bmpTicket = AForge.Imaging.Image.FromFile(openFileDialog.FileName);

            //bmpTicket = ImageProcess.FilterBalckBlob(bmpTicket);
            //bmpTicket = new Grayscale(0.2125, 0.7154, 0.0721).Apply(bmpTicket);
            //bmpTicket = bmpTicket.Clone(new Rectangle(0, 0, bmpTicket.Width, bmpTicket.Height), PixelFormat.Format24bppRgb);
            //ImageProcess.Binarizate(bmpTicket);
            //bmpTicket = ImageProcess.CutBlankEdge(bmpTicket);

            //bmpTicket = ticketRecognizer.Prepare(bmpTicket);

            if (ticketType == Config.BLUE_TICKET)
            {
                ResizeBilinear resizer = new ResizeBilinear(Config.BLUE_TICKET_WIDTH, Config.BLUE_TICKET_HEIGHT);
                bmpTicket = resizer.Apply(bmpTicket);

            }
            else
            {
                ResizeBilinear resizer = new ResizeBilinear(Config.RED_TICKET_WIDTH, Config.RED_TICKET_HEIGHT);
                bmpTicket = resizer.Apply(bmpTicket);


            }


            var result = ticketRecognizer.Prepare(bmpTicket, ticketType);

            bmpQRCode = result.Item2;
            bmpWork = result.Item3;
            bmpTicket = result.Item4;

            if (result.Item1 == Config.Is_Detected_QRCode)
            {

             }else
            {

            }

            pictureBox_ticket.Image = bmpTicket;
            pictureBox_code.Image = bmpWork;
            pictureBox_QRCode.Image = bmpQRCode;







            //bmpSource = AForge.Imaging.Image.FromFile(openFileDialog.FileName);

            //pictureBox_source.Image = bmpSource;
            if (pictureBox_work1.Image != null)
            {
                pictureBox_work1.Image.Dispose();
                pictureBox_work1.Image = null;
            }
            if (pictureBox_work2.Image != null)
            {
                pictureBox_work2.Image.Dispose();
                pictureBox_work2.Image = null;

            }
            if (pictureBox_work3.Image != null)
            {
                pictureBox_work3.Image.Dispose();
                pictureBox_work3.Image = null;

            }



        }

        private void button2_Click(object sender, EventArgs e)
        {
            bmpWork = new Grayscale(0.2125, 0.7154, 0.0721).Apply(exteactTicketCodeImg);
            bmpWork = bmpWork.Clone(new Rectangle(0, 0, bmpWork.Width, bmpWork.Height), PixelFormat.Format24bppRgb);

            //            bmpWork = new Grayscale(0.3, 0.59, 0.11).Apply(bmpSource);
            pictureBox_work1.Image = bmpWork;


        }

        private void button3_Click(object sender, EventArgs e)
        {

            ImageProcess.Binarizate(bmpWork);
            bmpWork = ImageProcess.CutBlankEdge(bmpWork);
            pictureBox_work2.Image = bmpWork;

        }









        IList<Bitmap> splitBmpList;




        private void button4_Click(object sender, EventArgs e)
        {

            splitBmpList = ImageProcess.SplitTicketNoByDefinedWidth(bmpWork, bmpTicket.Width, bmpTicket.Height);
            PictureBox[] pxs = { pictureBox_slpit1, pictureBox_slpit2, pictureBox_slpit3, pictureBox_slpit4, pictureBox_slpit5, pictureBox_slpit6, pictureBox_slpit7 };
            for (int i = 0; i < splitBmpList.Count; i++)
            {
                pxs[i].Image = splitBmpList[i];
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (splitBmpList == null)
            {
                label_result.Text = "";
                return;
            }
            label_result.Text = OCRTicketNo(splitBmpList);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            //exteactTicketCodeImg = ticketRecognizer.ExtractTicketCodeImageArea(bmpTicket, ticketType);
            //bmpWork = exteactTicketCodeImg;
            //if (pictureBox_code.Image != null)
            //{
            //    pictureBox_code.Image.Dispose();
            //    pictureBox_code.Image = null;
            //    pictureBox_code.Image = bmpWork;
            //}
            //else
            //{
            //    pictureBox_code.Image = bmpWork;
            //}

        }

        private void button7_Click(object sender, EventArgs e)
        {
            //Bitmap qrCode = ticketRecognizer.DetectQRCode(bmpTicket,ticketType);
            //if(qrCode != null) pictureBox_QRCode.Image = qrCode;


            //Bitmap temp = ticketRecognizer.ExtractQRCodeImageArea(bmpTicket, ticketType);
            //var result = ticketRecognizer.IsExistQRCode(temp, ticketType);
            //if (result.Item1)
            //{
            //    pictureBox_QRCode.Image = temp;
            //}
            //else
            //{
            //    bmpTicket.RotateFlip(RotateFlipType.Rotate180FlipNone);
            //    pictureBox_ticket.Image = bmpTicket;
            //    Bitmap temp1 = ticketRecognizer.ExtractQRCodeImageArea(bmpTicket, ticketType);
            //    var result2 = ticketRecognizer.IsExistQRCode(temp1, ticketType);
            //    if (result2.Item1)
            //    {
            //        pictureBox_QRCode.Image = temp1;
            //    }
            //}

        }

        private void button8_Click(object sender, EventArgs e)
        {
            process();

        }

        private void process()
        {
            bmpWork = new Grayscale(0.2125, 0.7154, 0.0721).Apply(exteactTicketNoImg);
            bmpWork = bmpWork.Clone(new Rectangle(0, 0, exteactTicketNoImg.Width, exteactTicketNoImg.Height), PixelFormat.Format24bppRgb);
            pictureBox_work1.Image = bmpWork;


            ImageProcess.FilterBackground(bmpWork);
            pictureBox_work1.Image = bmpWork;


            ImageProcess.FilterDisturb(bmpWork);
            pictureBox_work2.Image = bmpWork;

            ImageProcess.Binarizate(bmpWork);
            bmpWork = ImageProcess.CutBlankEdge(bmpWork);
            pictureBox_work3.Image = bmpWork;

            splitBmpList = ImageProcess.Split(bmpWork, 7);
            PictureBox[] pxs = { pictureBox_slpit1, pictureBox_slpit2, pictureBox_slpit3, pictureBox_slpit4, pictureBox_slpit5, pictureBox_slpit6, pictureBox_slpit7 };
            for (int i = 0; i < splitBmpList.Count; i++)
            {
                pxs[i].Image = splitBmpList[i];
            }

            if (splitBmpList == null)
            {
                label_result.Text = "";
                return;
            }
            label_result.Text = OCRTicketNo(splitBmpList);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (bmpWork != null)
            {
                System.DateTime currentTime = System.DateTime.Now;
                String path = Environment.CurrentDirectory + "\\Template\\";
                DirectoryInfo df = Directory.CreateDirectory(path);
                Bitmap b = bmpWork.Clone(new Rectangle(0, 0, bmpWork.Width, bmpWork.Height), PixelFormat.Format1bppIndexed);

                //b.Save(path + "\\" + currentTime.Ticks.ToString() + ".tif");

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.InitialDirectory = path;
                saveFileDialog1.Filter = "*.tif|*.tif";
                saveFileDialog1.Title = "保存图像";
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    string temp = saveFileDialog1.FileName;
                    b.Save(temp, System.Drawing.Imaging.ImageFormat.Tiff);
                }
            }
        }

        /// <summary>
        /// 识别印刷票号
        /// </summary>
        /// <param name="imgs"></param>
        /// <returns></returns>
        private string OCRTicketNo(IList<Bitmap> imgs)
        {
            string res = "";

            using (var engineLetter = new TesseractEngine(@"tessdata", "eng", EngineMode.TesseractOnly))
            {
                engineLetter.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                engineLetter.SetVariable("tessedit_unrej_any_wd", true);
                engineLetter.DefaultPageSegMode = PageSegMode.SingleChar;
                using (var page = engineLetter.Process(imgs[0], PageSegMode.SingleChar))
                    res += page.GetText().Substring(0, 1);
            }

            imgs.RemoveAt(0);
            using (var engine = new TesseractEngine(@"tessdata", "eng", EngineMode.TesseractOnly))
            {
                engine.SetVariable("tessedit_char_whitelist", "1234567890");
                engine.SetVariable("tessedit_unrej_any_wd", true);
                engine.DefaultPageSegMode = PageSegMode.SingleChar;

                foreach (Bitmap img in imgs)
                {
                    using (var page = engine.Process(img, PageSegMode.SingleChar))
                        res += page.GetText().Substring(0, 1);

                }
            }

            Console.WriteLine("OCR Result = " + res);
            return res;
        }

        /// <summary>
        /// 识别21位码
        /// </summary>
        /// <param name="imgs"></param>
        /// <returns></returns>
        private string OCRTicketCode(IList<Bitmap> imgs)
        {
            string charStr = "";

            using (var engineLetter = new TesseractEngine(@"tessdata", "TicketCode", EngineMode.TesseractOnly))
            {
                engineLetter.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                engineLetter.SetVariable("tessedit_unrej_any_wd", true);
                engineLetter.DefaultPageSegMode = PageSegMode.SingleChar;
                using (var page = engineLetter.Process(imgs[14], PageSegMode.SingleChar))
                {
                    String s = page.GetText();
                    if (s.Length > 0)
                        charStr = page.GetText().Substring(0, 1);

                }
            }

            IList<Bitmap> num1 = new List<Bitmap>();
            IList<Bitmap> num2 = new List<Bitmap>();

            for (int i = 0; i < imgs.Count; i++)
            {
                if (i < 14)
                {
                    num1.Add(imgs[i]);
                }
                else if (i == 14) continue;
                else
                {
                    num2.Add(imgs[i]);
                }
            }

            String num1Str = String.Empty;
            String num2Str = String.Empty;

            using (var engine = new TesseractEngine(@"tessdata", "TicketCode", EngineMode.TesseractOnly))
            {
                engine.SetVariable("tessedit_char_whitelist", "1234567890");
                engine.SetVariable("tessedit_unrej_any_wd", true);
                engine.DefaultPageSegMode = PageSegMode.SingleChar;

                foreach (Bitmap img in num1)
                {
                    using (var page = engine.Process(img, PageSegMode.SingleChar))
                    {
                        String s = page.GetText();
                        if (s.Length > 0)
                            num1Str += page.GetText().Substring(0, 1);
                    }


                }

                foreach (Bitmap img in num2)
                {
                    using (var page = engine.Process(img, PageSegMode.SingleChar))
                    {
                        String s = page.GetText();
                        if (s.Length > 0)
                            num2Str += page.GetText().Substring(0, 1);

                    }
                }
            }
            String res = num1Str + charStr + num2Str;
            Console.WriteLine("OCR Result = " + res);
            return res;
        }






        private void button10_Click(object sender, EventArgs e)
        {
            DateTime beginTime = DateTime.Now;
            for (int i = 0; i < 1000; i++)
            {
                DateTime oneLoopBegin = DateTime.Now;
                process();
                Console.WriteLine("loop : " + i + " Using time = " + ((DateTime.Now.Ticks - oneLoopBegin.Ticks) / 10000));
            }
            Console.WriteLine("Totle use time = " + ((DateTime.Now.Ticks - beginTime.Ticks) / 10000));
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (exteactTicketCodeImg != null)
            {
                Bitmap b = ImageProcess.FilterBalckBlob(exteactTicketCodeImg);
                if (pictureBox_code.Image != null)
                {
                    pictureBox_code.Image.Dispose();
                    pictureBox_code.Image = null;
                    pictureBox_code.Image = b;
                }
                else
                {
                    pictureBox_code.Image = b;
                }

            }


        }


        IList<Bitmap> codeImgList = new List<Bitmap>();
        private void button12_Click(object sender, EventArgs e)
        {
            //codeImgList = ImageProcess.ExtractTicketCode(bmpWork,bmpTicket.Width,bmpTicket.Height);
            //            codeImgList = ts.SplitTicketCodeByDefinedWidth(bmpWork, ticketType);
            codeImgList = ts.SplitTicketCode(bmpWork);
            flowLayoutPanel_codeimgs.Controls.Clear();
            foreach (Bitmap img in codeImgList)
            {
                PictureBox pb = new PictureBox();
                pb.Image = img;
                flowLayoutPanel_codeimgs.Controls.Add(pb);
            }

        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (codeImgList == null || codeImgList.Count != 21)
            {
                label_result.Text = "";
                return;
            }
            label_result.Text = OCRTicketCode(codeImgList);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            ticketType = Config.RED_TICKET;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            ticketType = Config.BLUE_TICKET;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            bmpWork = ticketRecognizer.GetTicketCodeImgs(bmpWork, ticketType);

            //bmpWork = ticketRecognizer.FilterTicketCodeWithHeight(bmpWork, ticketType);
            //bmpWork = ts.CutTicketCodeEdge(bmpWork, ticketType);
            pictureBox_work3.Image = bmpWork;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            using (var engine = new TesseractEngine(@"tessdata", "TicketCode", EngineMode.TesseractOnly))
            {
                //engine.SetVariable("tessedit_char_whitelist", "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                //engine.SetVariable("tessedit_unrej_any_wd", true);
                engine.DefaultPageSegMode = PageSegMode.SingleBlock;
                using (var page = engine.Process(bmpWork, PageSegMode.SingleBlock))
                {
                    String s = page.GetText();
                    Console.WriteLine(s);
                    label_result.Text = s;
                }

            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            QRCodeReader reader = new QRCodeReader();
            //BarcodeReader reader = new BarcodeReader();
            Result result;
            try{
                Bitmap image = (Bitmap)pictureBox_QRCode.Image;
                //result = reader.Decode(bitmap);
                LuminanceSource source;
                source = new BitmapLuminanceSource(image);
                BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));
                result = new MultiFormatReader().decode(bitmap);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }


            if (result != null)
            {
                Console.WriteLine(result.BarcodeFormat.ToString());
                string instr = result.Text; //显示解析结果  

                if (instr.Length != 144)
                {
                    MessageBox.Show(DateTime.Now.ToString() + "  扫描票面二维码出错：\r\n");
                    return ;
                }
                int i = instr.Length;
                long nowYear = DateTime.Now.Year;
                StringBuilder outStr = new StringBuilder(117);
                long returnStatus = UnsecurityDll.uncompress(instr, outStr, nowYear);

                string sb = outStr.ToString();
                if (sb.Length == 0)
                {
                    MessageBox.Show(DateTime.Now.ToString() + "  扫描票面二维码解密出错：\r\n");
                    return ;
                }
                string ticketNo = sb.Substring(0, 7);
                string fromStationCode = sb.Substring(7, 3);
                string toStationCode = sb.Substring(10, 3);
                string seatCode = sb.Substring(29, 4);
                string trainCodeTmp = sb.Substring(16, 8).Trim();
                char[] trainLetter = trainCodeTmp.Substring(0, 1).ToCharArray();
                string trainCode = "";
                if (Char.IsLetter(trainLetter[0]))    
                {
                    trainCode = trainCodeTmp.Substring(0, 1) + Convert.ToInt16(trainCodeTmp.Substring(2)).ToString();
                }
                else
                {
                    trainCode = Convert.ToInt16(trainCodeTmp).ToString();
                }
            
                int ticketPrice = Convert.ToInt16(sb.Substring(33, 5))/ 10;
                string trainDate = sb.Substring(38, 8);
                string officeNo = sb.Substring(50, 7);
                int windowNo = Convert.ToInt16(sb.Substring(57, 3));
                string statisticsDate = sb.Substring(60, 8);
                string cardType = sb.Substring(68, 2);
                string id = sb.Substring(72, 12) +"****"+ sb.Substring(86, 4);
                string name = sb.Substring(90, 18).Trim();

                MessageBox.Show(DateTime.Now.ToString() + "**姓名：" + name + ";身份证号：" + id + ";票号：" + ticketNo);
            
            }
            else
            {
                MessageBox.Show(DateTime.Now.ToString() + "**未识别！！" );
            }
        }
    }
}

 




