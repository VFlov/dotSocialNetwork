using dotSocialNetwork.Data;
using dotSocialNetwork.Models;
using Microsoft.EntityFrameworkCore;

namespace dotSocialNetwork.Repository
{
    public class MessageRepository : Repository<Message>
    {
        public MessageRepository(dotSocialNetworkContext context) : base(context) 
        {

        }
        public List<Message> GetMessages(User sender, User recipient)
        {
            Set.Include(x => x.RecipientId);
            Set.Include(x => x.SenderId);
            var from = Set.AsEnumerable().Where(x => int.Parse(x.SenderId) == sender.Id && int.Parse(x.RecipientId) == recipient.Id).ToList();
            var to = Set.AsEnumerable().Where(x => int.Parse(x.SenderId) == recipient.Id && int.Parse(x.RecipientId) == sender.Id).ToList();  
            var result = new List<Message>();
            result.AddRange(from);
            result.AddRange(to);
            result.OrderBy(x => x.Id);
            return result;
        }
    }
}
