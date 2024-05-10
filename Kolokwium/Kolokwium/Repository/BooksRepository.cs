using Kolokwium.Models;
using Microsoft.Data.SqlClient;

namespace Kolokwium.Repository;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;

    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesBookExist(int id)
    {
        {
            var query = @"SELECT 1 FROM books WHERE PK = @ID";

            await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await using SqlCommand command = new SqlCommand();

            command.Connection = connection;
            command.CommandText = query;
            command.Parameters.AddWithValue("@ID", id);

            await connection.OpenAsync();

            var res = await command.ExecuteScalarAsync();

            return res is not null;
        }
    }

    public async Task<GetBook> GetAuthors(int bookId)
    {
        var query = @"SELECT a.PK, a.first_na as firstName, a.last_na as lastName, b.title as title FROM authors a
        JOIN books_authors ba on a.PK = bk.FK_author
        JOIN books b ON b.PK=ba.FK_book WHERE b.FK_book=@bookid";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@bookid", bookId);

        await connection.OpenAsync();
        
        var reader = await command.ExecuteReaderAsync();

        var authorIDOrdinal = reader.GetOrdinal("PK");
        var bookTitleOrdinal = reader.GetOrdinal("title");
        var authorFirstNameOrdinal = reader.GetOrdinal("firstName");
        var authorLastNameOrdinal = reader.GetOrdinal("lastName");

        GetBook book = null;

        while (await reader.ReadAsync())
        {
            if (book is null)
            {
                book = new GetBook()
                {
                    bookId = bookId,
                    title = reader.GetString(bookTitleOrdinal),
                    authorsList = new List<GetAuthor>()
                    {
                        new GetAuthor()
                        {
                            firstName = reader.GetString(authorFirstNameOrdinal),
                            lastName = reader.GetString(authorLastNameOrdinal)
                        }
                    }
                };
            }
            else
            {
                book.authorsList.Add(new GetAuthor()
                {
                    firstName = reader.GetString(authorFirstNameOrdinal),
                    lastName = reader.GetString(authorLastNameOrdinal)
                });
            }
        }

        if (book is null)
        {
            throw new Exception("cos jest nie tak z bazą");
        }

        return book;
    }

    public async Task<int> DoesAuthorExist(string firstName, string lastName)
    {
        var query = "SELECT PK FROM authors WHERE first_na = @first AND last_na = @last";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@first", firstName);
        command.Parameters.AddWithValue("@last", lastName);

        await connection.OpenAsync();
        
        var res = await command.ExecuteScalarAsync();
        
        if (res == null || res == DBNull.Value)
            return -1;
        return Convert.ToInt32(res);
        
    }
    

    public async Task<int> PostBook(string title)
    {
        var query = "INSERT INTO books(title) VALUES (@title); SELECT @@IDENTITY AS ID;";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@title", title);

        await connection.OpenAsync();
        
        var res = await command.ExecuteScalarAsync();
        
        return Convert.ToInt32(res);
    }

    public async Task PostBooksAuthors(int bookId, int authorId)
    {
        var query = "INSERT INTO books_authors VALUES (@idBook, @idAuthor);";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@idBook", bookId);
        command.Parameters.AddWithValue("@idAuthor", authorId);

        await connection.OpenAsync();
        
        await command.ExecuteNonQueryAsync();
    }
}
