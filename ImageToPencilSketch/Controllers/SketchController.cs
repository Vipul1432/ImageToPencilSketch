using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace ImageToPencilSketch.Controllers
{
    public class SketchController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GenerateSketch(IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    // Load the image from the uploaded file
                    using (var imageStream = file.OpenReadStream())
                    {
                        Bitmap image = new Bitmap(imageStream);

                        // Convert the image to grayscale
                        Bitmap grayscaleImage = ToGrayscale(image);

                        // Invert the grayscale image
                        Bitmap invertedImage = InvertImage(grayscaleImage);

                        // Apply Gaussian blur to the inverted grayscale image
                        Bitmap blurredImage = ApplyGaussianBlur(invertedImage, 4);

                        // Dodge blend the blurred and grayscale image
                        Bitmap sketch = DodgeBlend(grayscaleImage, blurredImage);

                        // Convert the sketch bitmap to a byte array
                        MemoryStream stream = new MemoryStream();
                        sketch.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        byte[] sketchData = stream.ToArray();

                        // Convert the byte array to a base64 string
                        string base64Sketch = Convert.ToBase64String(sketchData);

                        // Pass the base64 string to the view
                        ViewBag.SketchData = base64Sketch;

                        return View("ViewSketch");
                    }
                }
                else
                {
                    // If no file was uploaded, return to the upload view
                    return RedirectToAction("GenerateSketch");
                }
            }
            catch (Exception ex)
            {
                // If an error occurs, return an error view
                ViewBag.ErrorMessage = ex.Message;
                return View("Error");
            }
        }
        static Bitmap ToGrayscale(Bitmap image)
        {
            Bitmap grayscaleImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int grayValue = (int)(pixelColor.R * 0.2125 + pixelColor.G * 0.7154 + pixelColor.B * 0.0721);
                    grayscaleImage.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }

            return grayscaleImage;
        }

        static Bitmap InvertImage(Bitmap image)
        {
            Bitmap invertedImage = new Bitmap(image.Width, image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    int invertedValue = 255 - pixelColor.R;
                    invertedImage.SetPixel(x, y, Color.FromArgb(invertedValue, invertedValue, invertedValue));
                }
            }

            return invertedImage;
        }

        static Bitmap ApplyGaussianBlur(Bitmap image, int radius)
        {
            AForge.Imaging.Filters.GaussianBlur filter = new AForge.Imaging.Filters.GaussianBlur(radius);
            return filter.Apply(image);
        }

        static Bitmap DodgeBlend(Bitmap baseImage, Bitmap blendImage)
        {
            Bitmap resultImage = new Bitmap(baseImage.Width, baseImage.Height);

            for (int y = 0; y < baseImage.Height; y++)
            {
                for (int x = 0; x < baseImage.Width; x++)
                {
                    Color baseColor = baseImage.GetPixel(x, y);
                    Color blendColor = blendImage.GetPixel(x, y);

                    int resultR = DodgeBlendComponent(baseColor.R, blendColor.R);
                    int resultG = DodgeBlendComponent(baseColor.G, blendColor.G);
                    int resultB = DodgeBlendComponent(baseColor.B, blendColor.B);

                    resultImage.SetPixel(x, y, Color.FromArgb(resultR, resultG, resultB));
                }
            }

            return resultImage;
        }

        static int DodgeBlendComponent(int baseValue, int blendValue)
        {
            if (blendValue == 255)
            {
                // If blendValue is 255, return 255 to avoid division by zero
                return 255;
            }
            else
            {
                // Otherwise, perform dodge blend calculation
                int resultValue = (baseValue * 255) / (255 - blendValue);
                return Math.Min(resultValue, 255);
            }
        }
    }
}
