using dotSocialNetwork.Data;
using dotSocialNetwork.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;

namespace dotSocialNetwork.Repository
{
    public class FriendsRepository : Repository<Friend>
    {
        //private readonly dotSocialNetworkContext _context;
        public FriendsRepository(dotSocialNetworkContext _context) : base(_context) 
        {

        }
        public void AddFriend(User user, User friend)
        {
            var friends = Set.AsEnumerable().FirstOrDefault(x => x.Id == user.Id && x.FriendId == friend.Id);
            if (friends == null)
            {
                var item = new Friend()
                {
                    UserId = user.Id,
                    FriendId = friend.Id
                };
                Create(item);
            }
        }
        public List<User> GetFriendsByUser(User user)
        {
            //var friendsIdd = Set.Include(x => x.UserId).AsEnumerable().Where(x => x.UserId == user.Id).Select(x => x.FriendId);
            var friendsIdd = Set
                .Where(f => f.UserId == user.Id)
                .Include(f => f.UserId) 
                .Select(f => f.FriendId); 
            var friendsId = Set.Where(f => f.UserId == user.Id).Select(f => f.FriendId);
            List<User> friends = new();
            foreach (var id in friendsId)
            {
                friends.Add(_db.User.FirstOrDefault<User>(u => u.Id == id));
            }
            return friends;
        }
        public void DeleteFriend(User user, User friend)
        {
            var delFriend = Set.AsEnumerable().FirstOrDefault(x => x.UserId == user.Id && x.FriendId == friend.Id);
            if (delFriend != null)
            {
                Delete(delFriend);
            }
        }
    }
}
