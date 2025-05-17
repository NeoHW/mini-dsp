using System.Collections.Concurrent;

namespace Shared.Models;

public interface IUserStore
{
    UserData? GetUserById(string userId);
    UserData AddUser(UserData user);
    List<UserData> GetAllUsersToList();
}
public class UserStore : IUserStore 
{
   private readonly ConcurrentDictionary<string, UserData> _users = new();
   
   public UserStore(IEnumerable<UserData> users)
   {
       _users = new ConcurrentDictionary<string, UserData>(
           users.Select(u => new KeyValuePair<string, UserData>(u.UserId, u))
       );
   }
   
   public UserData? GetUserById(string userId) => _users.GetValueOrDefault(userId);
   
   public UserData AddUser(UserData user)
   {
       return _users.AddOrUpdate(
           user.UserId,
           user,
           (_, _) => user
       );
   }
   
   public List<UserData> GetAllUsersToList()
   {
       return _users.Values.ToList();
   }
}