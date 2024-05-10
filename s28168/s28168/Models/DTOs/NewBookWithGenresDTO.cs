namespace s28168.Models.DTOs;

public class NewBookWithGenresDTO
{
    public string Title { get; set; }
    public IEnumerable<int> Genres { get; set; } = new List<int>();
}