using System.Transactions;
using Kolokwium.Models;
using Kolokwium.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Kolokwium.Controllers;
[ApiController]
public class BookController: ControllerBase
{
    private readonly IBooksRepository _booksRepository;

    public BookController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }

    [HttpGet]
    [Route("api/book/{id:int}/authors")]
    public async Task<IActionResult> GetBook(int id)
    {
        if (!await _booksRepository.DoesBookExist(id))
        {
            return NotFound($"brak ksiazki o id {id}");
        }

        return Ok(await _booksRepository.GetAuthors(id));
    }

    [HttpPost]
    [Route("api/books")]
    public async Task<IActionResult> POSTBook(AddBook addBook)
    {
        
        AddBookResponse abr = new AddBookResponse();
        int idOfBook;
        using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            idOfBook = await _booksRepository.PostBook(addBook.title);
            abr.bookId = idOfBook;
            abr.title = addBook.title;
            foreach (var author in addBook.authorsList)
            {
                var autorId = await _booksRepository.DoesAuthorExist(author.firstName, author.lastName);
                if (autorId == -1)
                {
                    return NotFound($"nie istnieje autor o imieniu {author.firstName} i nazwisku {author.lastName}");
                } 
                abr.authorsList.Add(new AddAuthor()
                {
                firstName = author.firstName,
                lastName = author.lastName
                });
                await _booksRepository.PostBooksAuthors(idOfBook,autorId);
            }
            scope.Complete();
        }
        return Created(Request.Path.Value ?? $"api/books", abr);
    }
}