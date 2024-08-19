using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ballesteros_0745.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    [BindProperty]
    public required IFormFile TxtFile { get; set; }

    private static readonly char[] separator = ['\r', '\n'];

    public bool ShowDownloadButton { get; set; }

    private static readonly Dictionary<int, string> arabicToRoman = new()
    {
        {1000, "M"}, {900, "CM"}, {500, "D"}, {400, "CD"},
        {100, "C"}, {90, "XC"}, {50, "L"}, {40, "XL"},
        {10, "X"}, {9, "IX"}, {5, "V"}, {4, "IV"},
        {1, "I"}
    };

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPostAsync()
    {
        ShowDownloadButton = false;
        string FileContent;
        string[] FileLines;
        StringBuilder resultLine = new();


        if (TxtFile == null || TxtFile.Length <= 0)
        {
            return Page();
        }

        using (var reader = new StreamReader(TxtFile.OpenReadStream()))
        {
            FileContent = await reader.ReadToEndAsync();
            FileLines = FileContent
                        .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                        .TakeWhile(line => line != "0")
                        .ToArray();
        }

        foreach (var Line in FileLines)
        {
            var parts = Line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (int.TryParse(Line, out int number) && number > 0)
            {
                resultLine.Append(Line + " " + ArabicToRomanBacktracking(number) + new string(' ', 4));
            }
            else
            {
                Console.WriteLine($"La línea: {Line} no pudo ser evaluada.");
            }
        }
        string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "salidaromanos.txt");
        await System.IO.File.WriteAllTextAsync(outputFilePath, resultLine.ToString().TrimEnd());

        ShowDownloadButton = true;

        return Page();
    }

    private static string ArabicToRomanBacktracking(int number)
    {
        if (number == 0) return "";

        foreach (var pair in arabicToRoman)
        {
            if (number >= pair.Key)
            {
                return pair.Value + ArabicToRomanBacktracking(number - pair.Key);
            }
        }

        return "";
    }

    public IActionResult OnGetDownloadFile()
    {
        string outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), "salidaromanos.txt");

        if (!System.IO.File.Exists(outputFilePath))
        {
            return NotFound();
        }

        var fileBytes = System.IO.File.ReadAllBytes(outputFilePath);
        return File(fileBytes, "application/octet-stream", "salidaromanos.txt");
    }
}
