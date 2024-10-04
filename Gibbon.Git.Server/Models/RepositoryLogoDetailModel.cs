using System.ComponentModel.DataAnnotations;
using Gibbon.Git.Server.Middleware.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Gibbon.Git.Server.Models;

public class RepositoryLogoDetailModel
{
    private byte[] _data;

    public RepositoryLogoDetailModel() { }

    public RepositoryLogoDetailModel(byte[] data)
    {
        _data = data;
    }

    [FileUploadExtensions(Extensions = "PNG,JPG,JPEG,GIF")]
    [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_Logo_PostedFile")]
    public IFormFile PostedFile { get; set; }

    [Display(ResourceType = typeof(Resources), Name = "Repository_Detail_RemoveLogo")]
    public bool RemoveLogo { get; set; }

    public bool Exists => BinaryData != null;

    public byte[] BinaryData
    {
        get
        {
            if (_data != null || PostedFile == null)
                return _data;

            using (var ms = new MemoryStream())
            {
                using (var image = Image.Load(PostedFile.OpenReadStream()))
                {
                    var logoWidth = image.Width >= 72 ? 72 : 36;
                    image.Mutate(x => x.Resize(logoWidth, logoWidth * image.Height / image.Width));
                    image.Save(ms, new PngEncoder());
                }

                _data = ms.ToArray();
            }

            return _data;
        }
    }

    public string Base64Image
    {
        get
        {
            if (_data != null)
                return Convert.ToBase64String(_data);

            return null;
        }
    }
}