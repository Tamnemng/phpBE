using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Think4.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageBase64Async(string base64String);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudinarySection = configuration.GetSection("Cloudinary");
            
            var account = new Account(
                cloudinarySection["CloudName"],
                cloudinarySection["ApiKey"],
                cloudinarySection["ApiSecret"]
            );
            
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageBase64Async(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                throw new ArgumentException("No base64 string was provided");

            // Trích xuất dữ liệu từ chuỗi base64
            string base64Data = base64String;
            
            // Kiểm tra và xử lý tiền tố data:image nếu có
            if (base64String.Contains(","))
            {
                // Ví dụ: data:image/png;base64,iVBORw0KGgo...
                string[] parts = base64String.Split(',');
                base64Data = parts[1];
            }
            
            // Chuyển đổi base64 thành byte array
            byte[] fileBytes = Convert.FromBase64String(base64Data);
            
            // Tạo một unique filename
            string fileName = $"gift_{Guid.NewGuid()}";
            
            // Upload bằng byte array thay vì trực tiếp dùng base64
            using var stream = new MemoryStream(fileBytes);
            
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = "think4_app/gifts",
                UseFilename = true,
                UniqueFilename = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult.Error != null)
                throw new Exception($"Failed to upload image: {uploadResult.Error.Message}");
                
            return uploadResult.SecureUrl.ToString();
        }
    }
}