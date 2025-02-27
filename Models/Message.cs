﻿namespace dotSocialNetwork.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public string SenderId { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string RecipientId { get; set; } = "";
        public string RecipientName { get; set; } = "";
    }
}
