using Kolokwium.Models;

namespace Kolokwium.Repository;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
    Task<GetBook> GetAuthors(int bookId);

    Task<int> DoesAuthorExist(string firstName, string lastName);
    Task<int> PostBook(string title);
    Task PostBooksAuthors(int bookId, int authorId);
}