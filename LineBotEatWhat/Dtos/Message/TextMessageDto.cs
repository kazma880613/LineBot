using LineBot.Enum;

namespace LineBot.Dtos.Message
{
    public class TextMessageDto : BaseMessageDto
    {
        public TextMessageDto()
        {
            Type = MessageEnum.Text;
        }
        public string Text { get; set; }
        public List<TextMessageEmojiDto>? Emojis { get; set; }
    }

    public class TextMessageEmojiDto
    {
        public int Index { get; set; }
        public string ProductId { get; set; }
        public string EmojiId { get; set; }
    }
}
