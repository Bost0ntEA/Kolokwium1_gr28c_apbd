namespace Kolokwium.Models;

public class GetBook
{
    public int bookId { get; set; }
    public string title { get; set; }
    public List<GetAuthor> authorsList { get; set; }



}