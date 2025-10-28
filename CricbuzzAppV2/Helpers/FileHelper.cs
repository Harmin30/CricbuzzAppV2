using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CricbuzzAppV2.Helpers
{
    public static class FileHelper
    {
      public static async Task<string> SaveImageAsync(IFormFile file, IWebHostEnvironment webHostEnvironment, string folder)
      {
        if (file == null || file.Length == 0)
             return null;

    // Validate file extension
         var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
    throw new Exception("Invalid file type. Only jpg, jpeg, png, and gif are allowed.");

        // Create unique filename
    var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", folder);
  
            // Create directory if it doesn't exist
   if (!Directory.Exists(uploadsFolder))
         Directory.CreateDirectory(uploadsFolder);

    var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
          using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
     await file.CopyToAsync(fileStream);
       }

            // Return relative path
            return $"/uploads/{folder}/{fileName}";
        }

        public static void DeleteImage(string imagePath, IWebHostEnvironment webHostEnvironment)
        {
     if (string.IsNullOrEmpty(imagePath))
           return;

     // Only delete files from our uploads folder
   if (!imagePath.StartsWith("/uploads/"))
       return;

   var fullPath = Path.Combine(webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
        if (File.Exists(fullPath))
  {
       File.Delete(fullPath);
            }
        }
    }
}