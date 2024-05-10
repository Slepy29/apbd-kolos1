using s28168.Models.DTOs;

namespace s28168.Repository;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
    Task<bool> DoesBookExist(string title);
    Task<bool> DoesGenreExist(int id);
    Task<BookDTO> GetBook(int id);
    Task<int> AddNewBookWithGenres(NewBookWithGenresDTO newBookWithGenresDto);
}