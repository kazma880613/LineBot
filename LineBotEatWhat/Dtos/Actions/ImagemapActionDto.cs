﻿using LineBot.Dtos.Message;

namespace LineBot.Dtos.Actions
{
    public class ImagemapActionDto
    {
        public string Type { get; set; }
        public string? Label { get; set; }

        // Message action
        public string? Text { get; set; }

        // Uri action
        public string? LinkUri { get; set; }

        public ImagemapAreaDto Area { get; set; }
    }
}
