using System.ComponentModel.DataAnnotations;

namespace dotSocialNetwork.Server.Models;
public class Message
{
    public int Id { get; set; }
    public int DialogId { get; set; }
    public Dialog Dialog { get; set; }

    [Required]
    public int SenderId { get; set; }
    public User Sender { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string? AttachmentUrl { get; set; }
}