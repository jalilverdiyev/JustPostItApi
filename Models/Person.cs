namespace JustPostItAPI.Models;

public enum PersonStatus
{
    None = 0,
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}

public class Person
{
    public string Name { get; set; }
    public int PersonId { get; set; }
    public string Profile_Photo { get; set; }
    public PersonStatus Status { get; set; }

    public bool IsValid { get; set; }
    public Person()
    {
        Name = "";
        Profile_Photo = "";
        IsValid = false;
    }

    public Person(string name, int id, string profilePhoto, PersonStatus status, bool isValid)
    {
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(profilePhoto))
        {
            Name = name;
            PersonId = id;
            Profile_Photo = profilePhoto;
            Status = status;
            IsValid = isValid;
        }
        else
        {
            Name = "";
            Profile_Photo = "";
        }
    }
}