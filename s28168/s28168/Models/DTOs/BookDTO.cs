namespace s28168.Models.DTOs;

public class BookDTO
{
    public int Pk { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> Genres { get; set; }
}
