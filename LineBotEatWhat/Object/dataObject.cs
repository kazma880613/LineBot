using LineBot.Dtos.Message;

namespace LineBot.Object
{
    public class ChannelSetting
    {
        public string ChanelchannelAccessToken = "xyhiewvQ/6qlHC5MD3K+vnk0pWRKO7vSB4IvaNUjl2OsMTTn/AEyNRb1LDlDGwy4bIvdjNhYWJ/PcdPx6+1Iky+hZmNIGa1RFZ+v2WT6RZ38rQfg8NSVBtoTBcjxEKDv30Q3xbyHU4Q/4TGN2Ot82QdB04t89/1O/w1cDnyilFU=";
        public string ChannelSecret = "0c478b3349408e2b229c9ffa93a3a72f";
    }

    public class DBConfig
    {
        public string dbIP = "localhost";
        public int dbPort = 3306;
        public string dbUsername = "root";
        public string dbPassword = "kazma1999";
        public string dbName = "catering";
    }

    public class Response
    {
        public double distance;
        public string station;
        public string address;
        public double longitude;
        public double latitude;
    }

    public class Carouseldata
    {
        public string name;
        public string address;
        public string tel;
        public string opentime;
        public string picture;
    }
}
