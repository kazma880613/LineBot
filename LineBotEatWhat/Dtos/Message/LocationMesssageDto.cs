using LineBot.Enum;

namespace LineBot.Dtos.Message
{
    public class LocationMessageDto : BaseMessageDto
    {
        public LocationMessageDto()
        {
            Type = MessageEnum.Location;
        }

        public string Title { get; set; }
        public string Address { get; set; }

        public double Latitude { get; set; } // 緯度
        public double Longitude { get; set; } // 經度

        public string Function { get; set; }
    }
}
