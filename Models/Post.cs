using Microsoft.AspNetCore.Mvc;

namespace JustPostItAPI.Models;

public class Post
{
    public string? Text { get; set; }
    [HiddenInput]
    public List<int>? Orders { get; set; } 
    public List<IFormFile>? Photos { get; set; }
    public List<string>? Paths { get; set; }
    public int? Id { get; set; }
    public string? Owner { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerPhoto { get; set; }
}