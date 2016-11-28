using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using ZXing;
using ZXing.Common;

namespace TicketScan
{
    public class QRCode
    {
        private static readonly BarcodeReader barcodeReader = new BarcodeReader();
        private static readonly IList<ResultPoint> resultPoints = new List<ResultPoint>();
        private EncodingOptions EncodingOptions { get; set; }

        public static String DecodeByZXing(Bitmap image)
        {
            barcodeReader.AutoRotate = true;
            resultPoints.Clear();
            String resultString = String.Empty;

            var timerStart = DateTime.Now.Ticks;
            Result[] results = null;
            barcodeReader.Options.PossibleFormats = new[] { BarcodeFormat.QR_CODE };
            var result = barcodeReader.Decode(image);
            if (result != null)
            {
                results = new[] { result };
            }
            var timerStop = DateTime.Now.Ticks;

            if (results == null)
            {
                resultString = null;
            }
            else
            {
                resultString = result.Text;
            }

            return resultString;


        }

        public static string DecodeByTh(Bitmap image)
        {
            QRCodeDecoder decoder = new QRCodeDecoder();
            var result = String.Empty;
            try
            {
                result = decoder.decode(new QRCodeBitmapImage(image));

            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return result;

        }
    }
}
