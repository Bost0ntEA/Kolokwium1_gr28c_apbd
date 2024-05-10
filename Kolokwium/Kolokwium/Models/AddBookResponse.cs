namespace Kolokwium.Models;

public class AddBookResponse
{
    public int bookId { get; set; }
    public string title { get; set; }
    public List<AddAuthor> authorsList { get; set; }
}