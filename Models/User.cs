namespace JustPostItAPI.Models;

public class User
{
    public string? UserName { get; set; }
    public string? Email { get; set; } 
    public string? Password { get; set; }
    public int? Id { get; set; }
    public IFormFile? ProfilePhoto { get; set; }
    public bool IsValid { get; set; }
}