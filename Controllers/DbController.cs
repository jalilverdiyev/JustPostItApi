using System.Data;
using JustPostItAPI.Models;
using MySql.Data.MySqlClient;

namespace JustPostItAPI.Controllers;

public static class DbController
{
    private static IConfiguration _config = null!;

    public static IConfiguration Configuration
    {
        get
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            _config = builder.Build();
            return _config;
        }
    }

    private static readonly MySqlConnection Conn = new MySqlConnection(Configuration.GetConnectionString("JPIDatabase"));

    private static string HashPass(string input)
    {
        string salt = BCrypt.Net.BCrypt.GenerateSalt();
        string result = BCrypt.Net.BCrypt.HashPassword(input, salt);
        return result;
    }

    //User CRUD operations
    public static List<Person> GetPeople(int id)
    {
        List<Person> friends = GetFriends(id);
        List<Person> people = new List<Person>();
        try
        {
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            string query = $"SELECT UserName,Id,ProfilePhoto FROM Users Where Id !={id}";

            MySqlCommand command = new MySqlCommand(query, Conn);
            MySqlDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                string name = (string)dataReader[0];
                int personId = (int)dataReader[1];
                string profilePhoto = (string)dataReader[2];
                PersonStatus status;
                try
                {
                    status = friends.First(x => x.PersonId == (int)dataReader[1]).Status;
                }
                catch (InvalidOperationException)
                {
                    status = PersonStatus.None;
                }

                people.Add(new Person()
                {
                    Name = name,
                    PersonId = personId,
                    Profile_Photo = profilePhoto,
                    Status = status
                });
            }
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (Conn.State != ConnectionState.Closed)
            {
                Conn.Close();
            }
        }

        return people;
    }

    public static List<Person> GetFriends(int id)
    {
        List<Person> friends = new List<Person>();
        try
        {
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            string query = $"SELECT Users.Id,Users.UserName,Users.ProfilePhoto,Friendship.Status From Users " +
                           $"INNER JOIN Friendship ON Friendship.FriendId=Users.Id WHERE Friendship.UserId={id} " +
                           $"UNION Select Users.Id,Users.UserName,Users.ProfilePhoto,Friendship.Status From Users " +
                           $"INNER JOIN Friendship ON Friendship.UserId=Users.Id WHERE Friendship.FriendId={id}";

            MySqlCommand command = new MySqlCommand(query, Conn);
            MySqlDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                friends.Add(new Person()
                {
                    PersonId = (int)dataReader[0],
                    Name = (string)dataReader[1],
                    Profile_Photo = (string)dataReader[2],
                    Status = (PersonStatus)dataReader[3]
                });
            }
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (Conn.State != ConnectionState.Closed)
            {
                Conn.Close();
            }
        }

        return friends;
    }

    public static List<string> GetEmails()
    {
        List<string> emails = new List<string>();
        try
        {
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            string query = "SELECT Email FROM Users";
            MySqlCommand cmd = new MySqlCommand(query, Conn);
            MySqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                emails.Add((string)dr[0]);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (Conn.State != ConnectionState.Closed)
            {
                Conn.Close();
            }   
        }

        return emails;
    }
    
    public static List<Person> GetFriendRequests(int id)
    {
        List<Person> friends = new List<Person>();
        try
        {
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            string query =
                $"SELECT Users.Id,Users.UserName,Users.ProfilePhoto,Friendship.Status From Users INNER JOIN Friendship ON Friendship.UserId=Users.Id WHERE Friendship.FriendId={id};";

            MySqlCommand command = new MySqlCommand(query, Conn);
            MySqlDataReader dataReader = command.ExecuteReader();
            while (dataReader.Read())
            {
                friends.Add(new Person()
                {
                    PersonId = (int)dataReader[0],
                    Name = (string)dataReader[1],
                    Profile_Photo = (string)dataReader[2],
                    Status = (PersonStatus)dataReader[3]
                });
            }
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (Conn.State != ConnectionState.Closed)
            {
                Conn.Close();
            }
        }

        return friends;
    }

    /*public static string GetProfilePhotoById(int id)
    {
        string photo="";
        try
        {
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            string query = $"SELECT ProfilePhoto FROM Users WHERE Id={id}";
            MySqlCommand cmd = new MySqlCommand(query, Conn);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                photo = (string)dataReader[0];
            }
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (Conn.State != ConnectionState.Closed)
            {
                Conn.Close();
            }
        }

        return photo;
    }*/

    public static bool Add(User user)
    {
        int result = 0;
        if (GetEmails().Count(x => x == user.Email) > 0)
        {
            return false;
        }
        if (user.Password != null)
        {
            string hashed = HashPass(user.Password);
            try
            {
                string profilePhoto;
                if (user.ProfilePhoto != null)
                {
                    List<IFormFile> arr = new List<IFormFile> { user.ProfilePhoto };
                    profilePhoto = FileManager.SaveFiles(arr, ImageType.Profile)[0];
                }
                else
                {
                    profilePhoto = "default.png";
                }

                if (Conn.State != ConnectionState.Open)
                {
                    Conn.Open();
                }

                string query =
                    $"INSERT INTO Users (UserName,Email,Password,ProfilePhoto) VALUES('{user.UserName}','{user.Email}','{hashed}','{profilePhoto}')";
                MySqlCommand command = new MySqlCommand(query, Conn);
                result = command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                if (e.Number == 1062) // Unique key exception
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (Conn.State != ConnectionState.Closed)
                {
                    Conn.Close();
                }
            }
        }

        return result > 0;
    }

    public static bool AddFriend(int id, Person person)
    {
        int result;
        List<Person> friends = GetFriends(id);
        int count = friends.Count(x => x.PersonId == person.PersonId);
        if (count > 0)
        {
            return false;
        }

        try
        {
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            string query =
                $"INSERT INTO Friendship(UserId,FriendId,Status,DateAdded,DateModified) VALUES({id},{person.PersonId},{(int)person.Status},NOW(),NOW())";
            MySqlCommand updateCommand = new MySqlCommand(query, Conn);
            result = updateCommand.ExecuteNonQuery();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (Conn.State != ConnectionState.Closed)
                Conn.Close();
        }

        return result > 0;
    }

    public static bool UpdateFriend(int id, Person person)
    {
        int result;
        if (Conn.State != ConnectionState.Open)
        {
            Conn.Open();
        }

        try
        {
            string query =
                $"UPDATE Friendship SET Status={(int)person.Status} WHERE UserId ={person.PersonId} AND FriendId={id}";

            MySqlCommand command = new MySqlCommand(query, Conn);
            result = command.ExecuteNonQuery();
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (Conn.State != ConnectionState.Closed)
            {
                Conn.Close();
            }
        }

        return result > 0;
    }
    // public static bool Modify(params string[] moreColumns){}

    //Post CRUD operations
    public static bool Add(Post post)
    {
        int result = 0;
        try
        {
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            string queryPost =
                $"INSERT INTO Posts(OwnerId,Text) VALUES({post.OwnerId},'{post.Text}'); SELECT last_insert_id()";
            MySqlCommand commandPost = new MySqlCommand(queryPost, Conn);
            post.Id = Convert.ToInt32(commandPost.ExecuteScalar());
            result += post.Id != 0 ? 1 : 0;
            if (post.Photos != null)
            {
                List<string> paths = FileManager.SaveFiles(post.Photos, ImageType.Post);
                foreach (var order in post.Orders!)
                {
                    string queryPhoto = $"INSERT INTO PostPhotos(PostId,Path) VALUES({post.Id},'{paths[order]}')";
                    using (MySqlCommand commandPhoto = new MySqlCommand(queryPhoto, Conn))
                    {
                        result += commandPhoto.ExecuteNonQuery();
                    }
                }
            }
        }
        catch (MySqlException e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (Conn.State != ConnectionState.Closed)
            {
                Conn.Close();
            }
        }

        return result > 0;
    }

    public enum GetPostType
    {
        None = 0,
        Self = 1,
        People = 2
    }

    public static List<Post> GetPosts(int id, GetPostType type)
    {
        List<Post> posts = new List<Post>();
        Dictionary<string, int> photos = new Dictionary<string, int>();
        try
        {
            if (Conn.State != ConnectionState.Open)
            {
                Conn.Open();
            }

            string query;
            
            switch (type)
            {
                case GetPostType.None:
                    return posts;
                case GetPostType.Self:
                     query = $"SELECT Posts.Id,Posts.Text,NULL as Path,Posts.OwnerId,Users.UserName,Users.ProfilePhoto,Posts.DateAdded FROM Posts " +
                                   $"INNER JOIN Users ON Posts.OwnerId = Users.Id WHERE Posts.OwnerId = {id} " +
                                   $"UNION SELECT Posts.Id,Posts.Text,PostPhotos.Path,Posts.OwnerId,Users.UserName,Users.ProfilePhoto,Posts.DateAdded FROM PostPhotos " +
                                   $"INNER JOIN Posts ON PostPhotos.PostId=Posts.Id AND Posts.Id " +
                                   $"INNER JOIN Users On Posts.OwnerId = Users.Id WHERE Posts.OwnerId = {id} ORDER BY DateAdded DESC";
                    break;
                case GetPostType.People:
                    query = $"SELECT Posts.Id,Posts.Text,NULL as Path,Posts.OwnerId,Users.UserName,Users.ProfilePhoto,Posts.DateAdded FROM Posts " +
                            $"INNER JOIN Users ON Posts.OwnerId = Users.Id WHERE Posts.OwnerId != {id} " +
                            $"UNION SELECT Posts.Id,Posts.Text,PostPhotos.Path,Posts.OwnerId,Users.UserName,Users.ProfilePhoto,Posts.DateAdded FROM PostPhotos " +
                            $"INNER JOIN Posts ON PostPhotos.PostId=Posts.Id AND Posts.Id " +
                            $"INNER JOIN Users On Posts.OwnerId = Users.Id WHERE Posts.OwnerId != {id} ORDER BY DateAdded DESC";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            MySqlCommand cmd = new MySqlCommand(query, Conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (posts.FirstOrDefault(x => x?.Id == (int)reader[0], null) == null)
                {
                    posts.Add(new Post()
                    {
                        Id = (int)reader[0],
                        Text = (string)reader[1],
                        OwnerId = (int)reader[3],
                        Owner = (string)reader[4],
                        OwnerPhoto = (string)reader[5]
                    });
                }

                if (!(reader[2] is DBNull))
                {
                    photos.Add((string)reader[2], (int)reader[0]);
                }
            }

            foreach (var post in posts)
            {
                List<string> paths = new List<string>();
                var photoOfPost = photos.Where(x => x.Value == post.Id);
                foreach (var photo in photoOfPost)
                {
                    paths.Add(photo.Key);
                }

                post.Paths = paths;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (Conn.State != ConnectionState.Closed)
            {
                Conn.Close();
            }
        }

        return posts;
    }

    //Login 
    public static Person Authenticate(User user)
    {
        Person person = new Person();
        if (user.Password != null)
        {
            try
            {
                if (Conn.State != ConnectionState.Open)
                {
                    Conn.Open();
                }

                string query = $"SELECT Password,Id,ProfilePhoto FROM Users WHERE UserName = '{user.UserName}'";
                MySqlCommand command = new MySqlCommand(query, Conn);
                MySqlDataReader reader = command.ExecuteReader();
                string pass = "";
                person.Name = user.UserName!;
                person.Status = PersonStatus.None;
                while (reader.Read())
                {
                    pass = (string)reader[0];
                    person.PersonId = (int)reader[1];
                    person.Profile_Photo = (string)reader[2];
                }

                if (BCrypt.Net.BCrypt.Verify(user.Password, pass))
                {
                    person.IsValid = true;
                }
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                Conn.Close();
            }
        }

        return person;
    }
}