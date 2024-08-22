using Microsoft.AspNetCore.Mvc;

public class FileController : Controller
{
    [HttpGet("/UserImg/{filename}")]
    public IActionResult GetUserImage(string filename)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UserImg", filename);
        if (System.IO.File.Exists(filePath))
        {
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "image/jpeg"); // Điều chỉnh loại MIME theo loại ảnh thực tế
        }
        return NotFound();
    }
}
