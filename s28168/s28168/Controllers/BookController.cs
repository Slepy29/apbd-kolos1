using Microsoft.AspNetCore.Mvc;
using s28168.Models.DTOs;
using s28168.Repository;

namespace s28168.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;
    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }
    
    [HttpGet("{id}/genres")]
    public async Task<IActionResult> GetBook(int id)
    {
        if (!await _booksRepository.DoesBookExist(id))
            return NotFound($"Book with given ID - {id} doesn't exist");

        var book = await _booksRepository.GetBook(id);
        
        return Ok(book);
    }
    
    [HttpPost]
    public async Task<IActionResult> AddBookWithGenres(NewBookWithGenresDTO newBookWithGenresDTO)
    {
        if (await _booksRepository.DoesBookExist(newBookWithGenresDTO.Title))
            return BadRequest($"Book with given title - {newBookWithGenresDTO.Title} exists");

        foreach (var genreId in newBookWithGenresDTO.Genres)
        {
            if (!await _booksRepository.DoesGenreExist(genreId))
                return NotFound($"Genre with given ID - {genreId} doesn't exist");
        }

        int addedBookId = await _booksRepository.AddNewBookWithGenres(newBookWithGenresDTO);
        
        if (!await _booksRepository.DoesBookExist(addedBookId))
            return NotFound($"New book was not found, book id - {addedBookId}");

        var book = await _booksRepository.GetBook(addedBookId);

        return Created(Request.Path.Value ?? "api/books", book);
    }
}