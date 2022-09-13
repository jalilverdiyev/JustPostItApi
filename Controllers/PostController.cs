using JustPostItAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace JustPostItAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostController : ControllerBase
{
    [HttpGet("GetAllPosts")]
    public List<Post> GetAllPosts(int id)
    {
        return DbController.GetPosts(id, DbController.GetPostType.People);
    }

    [HttpGet("GetFriendsPosts")]
    public List<List<Post>> GetFriendsPosts(int id)
    {
        var friends = DbController.GetFriends(id).FindAll(friend => friend.Status == PersonStatus.Accepted);
        List<List<Post>> posts = new List<List<Post>>();
        foreach (var friend in friends)
        {
            posts.Add(DbController.GetPosts(friend.PersonId,DbController.GetPostType.Self));
        }

        return posts;
    }

    [HttpGet("GetSelfPosts")]
    public List<Post> GetSelfPosts(int id)
    {
        return DbController.GetPosts(id, DbController.GetPostType.Self);
    }

    [HttpPost("AddNewPost")]
    public bool AddNewPost()
    {
        var post = new Post();
        var httpPostedFile = HttpContext.Request.Form;
        var id = httpPostedFile["Id"];
        post.OwnerId = Convert.ToInt32(id);
        var text = httpPostedFile["Text"];
        post.Text = text;
        var orders = (string)httpPostedFile["Orders"];
        if (!String.IsNullOrEmpty(orders))
        {
            post.Orders = orders.Split(',').Select(int.Parse).ToList();
            List<IFormFile> list = new List<IFormFile>();
            foreach (var file in httpPostedFile.Files)
            {
                list.Add(file);
            }
            post.Photos = list;
        }
        return DbController.Add(post);
    }

    [HttpGet("GetFriendPosts")]
    public List<Post> GetFriendPosts(int id)
    {
        return DbController.GetPosts(id, DbController.GetPostType.Self);
    }
    
    
}