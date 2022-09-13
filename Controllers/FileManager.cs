namespace JustPostItAPI.Controllers;

public enum ImageType
{
    None = 0,
    Profile = 1,
    Post = 2
}

public static class FileManager
{
    public static List<string> SaveFiles(List<IFormFile> files,ImageType type)
    {
        List<string> uniqueNames = new List<string>();
        string dir = Directory.GetCurrentDirectory(); 
        switch (type)
        {
            case ImageType.None:
                return uniqueNames;
            case ImageType.Profile:
                dir += "/wwwroot/images/profilePhotos"; 
                break;
            case ImageType.Post:
                dir += "/wwwroot/images/postPhotos";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.FileName);
            var uniqueName = Guid.NewGuid() + ext;
            uniqueNames.Add(uniqueName);
            var path = Path.Combine(dir, uniqueName);
            var stream = new FileStream(path,FileMode.Create);
            file.CopyTo(stream);
        }
        return uniqueNames;
    }
}