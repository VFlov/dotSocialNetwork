namespace dotSocialNetwork.Models.ViewModels
{
    public class ChatViewModel
    {
        public User User { get; set; }
        public User Interlocutor { get; set; }
        public List<Message> History { get; set; }
        public MessageViewModel NewMessage { get; set; }
        public ChatViewModel()
        {
            NewMessage = new MessageViewModel();
        }
    }
}
