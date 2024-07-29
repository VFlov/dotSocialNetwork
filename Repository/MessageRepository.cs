using dotSocialNetwork.Data;
using dotSocialNetwork.Models;
using GigaChatAdapter;
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
        public string Answer(string prompt)
        {
            string authData = "NDMzNGQzM2MtMWRkYS00YjA2LWI5NTItMGFiNWFlMTAwYzU5OjdiMjY3M2M3LWM2NTctNGE1NC1iZTkxLTRhYzQyNGE3MzExOQ==";
            Authorization auth = new Authorization(authData, GigaChatAdapter.Auth.RateScope.GIGACHAT_API_PERS);
            var authResult = auth.SendRequest();
            if (authResult.Result.AuthorizationSuccess)
            {
                Completion completion = new Completion();
                //Console.WriteLine("Напишите запрос к модели. В ином случае закройте окно, если дальнейшую работу с чатботом необходимо прекратить.");
                auth.UpdateToken();
                var result = completion.SendRequest(auth.LastResponse.GigaChatAuthorizationResponse?.AccessToken, prompt, false);
                if (result.Result.RequestSuccessed)
                {
                    return result.Result.GigaChatCompletionResponse.Choices.LastOrDefault().Message.Content;
                }
                else
                {
                    return result.Result.ErrorTextIfFailed;
                }
            }
            return "Error";
        }
    }
}
