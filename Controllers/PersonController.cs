using JustPostItAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace JustPostItAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PersonController : ControllerBase
{
    private readonly ILogger<PersonController> _logger;

    public PersonController(ILogger<PersonController> logger)
    {
        _logger = logger;
    }
    [HttpGet("GetPeople")]
    public List<Person> GetPeople(int id)
    {
        return DbController.GetPeople(id);
    }

    [HttpGet("GetFriends")]
    public List<Person> GetFriends(int id)
    {
        return DbController.GetFriends(id).FindAll(friend => friend.Status == PersonStatus.Accepted);
    }

    [HttpGet("GetFriendsRequests")]
    public List<Person> GetFriendRequests(int id)
    {
        return DbController.GetFriendRequests(id).FindAll(friend => friend.Status == PersonStatus.Pending);
    }

    [HttpPost("AddFriend")]
    public bool AddFriend(int id, Person person)
    {
        return DbController.AddFriend(id, person);
    }

    [HttpPut("UpdateFriend")]
    public bool UpdateFriend(int id, Person person)
    {
        return DbController.UpdateFriend(id, person);
    }
}

