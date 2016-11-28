using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketScan
{
    public class Config
    {
        public static double BLUE_TICKETNO_X_CORP_RATIO = 62.0 / 750.0;
        public static double BLUE_TICKETNO_Y_CORP_RATIO = 24.0 / 465.0;
        public static double BLUE_TICKETNO_W_CORP_RATIO = 153.0 / 750.0;
        public static double BLUE_TICKETNO_H_CORP_RATIO = 32.0 / 465.0;
        public static double BLUE_TICKETNO_CHAR_WIDTH_RATIO = 25.0 / 750.0;
        public static double BLUE_TICKETNO_NUM_WIDTH_RATIO = 21.3 / 750.0;
        public static int TICKET_NO_NUM_COUNT = 6;
        public static int BLUE_TICKET_WIDTH = 750;
        public static int BLUE_TICKET_HEIGHT = 465;


        public static double BLUE_CODE_X_CORP_RATIO = 45.0 / BLUE_TICKET_WIDTH;
        public static double BLUE_CODE_Y_CORP_RATIO = 460.0 / BLUE_TICKET_HEIGHT;
        public static double BLUE_CODE_W_CORP_RATIO = 310.0 / BLUE_TICKET_WIDTH;
        public static double BLUE_CODE_H_CORP_RATIO = 80.0 / BLUE_TICKET_HEIGHT;
        public static double BLUE_CODE_NUM_WIDTH_RATIO = 12.0 / BLUE_TICKET_WIDTH;
        public static double BLUE_CODE_CHAR_WIDTH_RATIO = 14.0 / BLUE_TICKET_WIDTH;
        public static double BLUE_CODE_CHAR_HEIGHT_RATIO = 24.0 / BLUE_TICKET_HEIGHT;
        public static double BLUE_CODE_CHAR_OFFSET_RATIO = 3.0 / BLUE_TICKET_WIDTH;

        public static double BLUE_QRCODE_X_COPR_RATIO = 540.0 / BLUE_TICKET_WIDTH;
        public static double BLUE_QRCODE_Y_COPR_RATIO = 255.0 / BLUE_TICKET_HEIGHT;
        public static double BLUE_QRCODE_W_COPR_RATIO = 230.0 / BLUE_TICKET_WIDTH;
        public static double BLUE_QRCODE_H_COPR_RATIO = 180.0 / BLUE_TICKET_HEIGHT;


        public static int RED_TICKET_WIDTH = 710;
        public static int RED_TICKET_HEIGHT = 490;
        public static double RED_CODE_X_CORP_RATIO = 30.0 / RED_TICKET_WIDTH;
        public static double RED_CODE_Y_CORP_RATIO = 460.0 / RED_TICKET_HEIGHT;
        public static double RED_CODE_W_CORP_RATIO = 290.0 / RED_TICKET_WIDTH;
        public static double RED_CODE_H_CORP_RATIO = 70.0 / RED_TICKET_HEIGHT;
        public static double RED_CODE_NUM_WIDTH_RATIO = 11.0 / RED_TICKET_WIDTH;
        public static double RED_CODE_CHAR_WIDTH_RATIO = 15.0 / RED_TICKET_WIDTH;
        public static double RED_CODE_CHAR_HEIGHT_RATIO = 25.0 / RED_TICKET_HEIGHT;
        public static double RED_CODE_CHAR_OFFSET_RATIO = 9.0 / RED_TICKET_WIDTH;

        public static double RED_QRCODE_X_COPR_RATIO = 470.0 / RED_TICKET_WIDTH;
        public static double RED_QRCODE_Y_COPR_RATIO = 260.0 / RED_TICKET_HEIGHT;
        public static double RED_QRCODE_W_COPR_RATIO = 200.0 / RED_TICKET_WIDTH;
        public static double RED_QRCODE_H_COPR_RATIO = 200.0 / RED_TICKET_HEIGHT;

        public static int Normalize_QRCodeWidth = 300;
        public static int Normalize_QRCodeHeight = 300;



        public static int BLUE_TICKET = 1;
        public static int RED_TICKET = 2;

        public static int CODE_OFFSET = 4;

        public static int QRCODE_WIDTH = 100, QRCODE_HIGHT = 100;

        public static int Is_Blank_Ticket = 1;
        public static int Is_Recognition_QRCode = 2;
        public static int Is_Detected_QRCode = 3;



    }
}
