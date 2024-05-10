namespace Kolokwium.Models;

public class AddBook
{
    public string title { get; set; }
    public List<AddAuthor> authorsList { get; set; }
}