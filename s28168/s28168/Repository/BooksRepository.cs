using Microsoft.Data.SqlClient;
using s28168.Models.DTOs;

namespace s28168.Repository;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;
    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
     public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 FROM books WHERE PK = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }
    
    public async Task<bool> DoesBookExist(string title)
    {
	    var query = "SELECT 1 FROM books WHERE title = @Title";

	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = query;
	    command.Parameters.AddWithValue("@Title", title);

	    await connection.OpenAsync();

	    var res = await command.ExecuteScalarAsync();

	    return res is not null;
    }

    public async Task<bool> DoesGenreExist(int id)
    {
        var query = "SELECT 1 FROM genres WHERE PK = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<BookDTO> GetBook(int id)
    {
        var query = @"SELECT 
						    books.PK AS BookID,
						    books.title AS BookTitle,
						    genres.name AS GenreName
						FROM books
						JOIN books_genres ON books_genres.FK_book = books.PK
						JOIN genres ON genres.PK = books_genres.FK_genre
						WHERE books.PK = @ID";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = query;
	    command.Parameters.AddWithValue("@ID", id);
	    
	    await connection.OpenAsync();

	    var reader = await command.ExecuteReaderAsync();

	    var bookIdOrdinal = reader.GetOrdinal("BookID");
	    var bookTitleOrdinal = reader.GetOrdinal("BookTitle");
	    var genreNameOrdinal = reader.GetOrdinal("GenreName");

	    BookDTO bookDto = null;

	    while (await reader.ReadAsync())
	    {
		    if (bookDto is not null)
		    {
			    bookDto.Genres.Add(reader.GetString(genreNameOrdinal));
		    }
		    else
		    {
			    bookDto = new BookDTO()
			    {
				    Pk = reader.GetInt32(bookIdOrdinal),
				    Title = reader.GetString(bookTitleOrdinal),
				    Genres = new List<string>()
				    {
					    reader.GetString(genreNameOrdinal)
				    }
			    };
		    }
	    }

	    if (bookDto is null) throw new Exception();
        
        return bookDto;
    }

    public async Task<int> AddNewBookWithGenres(NewBookWithGenresDTO newBookWithGenresDTO)
    {
	    var insert = @"INSERT INTO books VALUES(@BookTitle);
					   SELECT @@IDENTITY AS ID;";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();
	    
	    command.Connection = connection;
	    command.CommandText = insert;
	    
	    command.Parameters.AddWithValue("@BookTitle", newBookWithGenresDTO.Title);
	    
	    await connection.OpenAsync();

	    var transaction = await connection.BeginTransactionAsync();
	    command.Transaction = transaction as SqlTransaction;
	    
	    try
	    {
		    var id = await command.ExecuteScalarAsync();
    
		    foreach (var genreId in newBookWithGenresDTO.Genres)
		    {
			    command.Parameters.Clear();
			    command.CommandText = "INSERT INTO books_genres VALUES(@FK_book, @Fk_genre);";
			    command.Parameters.AddWithValue("@FK_book", id);
			    command.Parameters.AddWithValue("@Fk_genre", genreId);

			    await command.ExecuteNonQueryAsync();
		    }

		    await transaction.CommitAsync();


		    return Convert.ToInt32(id);
	    }
	    catch (Exception)
	    {
		    await transaction.RollbackAsync();
		    throw;
	    }
    }
}