using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ASCIIImageConvertorMain
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private BitmapImage? myImg;

		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Load image from the device and show to the image
		/// </summary>
		private void browseButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					myImg = new BitmapImage(new Uri(openFileDialog.FileName));
					image.Source = myImg;
					urlTextBox.Text = openFileDialog.FileName;
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}

			}
		}

		private string ConvertToColoredAscii(Bitmap image, double containerWidth, double containerHeight)
		{
			var asciiChars = " .'^\",:;Il!i><~+_-?][}{1)(|\\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$";
			double charAspectRatio = 0.5; // Adjust for the font
			double fontSize = Math.Min(containerWidth / image.Width, containerHeight / (image.Height * charAspectRatio)) * 1.2;

			StringBuilder asciiArt = new StringBuilder();
			asciiArt.Append($"<div style='display:flex; justify-content:center; align-items:center; width:100%; height:100%; overflow:auto; background-color:black;'>");
			asciiArt.Append($"<pre style='margin:0; font-size:{fontSize}px; line-height:{fontSize * charAspectRatio}px; color:white; text-align:center;'>");


			var rect = new Rectangle(0, 0, image.Width, image.Height);
			var imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			unsafe
			{
				byte* imagePtr = (byte*)imageData.Scan0;

				for (int y = 0; y < image.Height; y += 3)
				{
					for (int x = 0; x < image.Width; x += 3)
					{
						byte* pixel = imagePtr + (y * imageData.Stride) + (x * 4);
						int brightness = (int)(pixel[2] * 0.3 + pixel[1] * 0.59 + pixel[0] * 0.11);
						int index = brightness * (asciiChars.Length - 1) / 255;
						string asciiChar = asciiChars[index].ToString();

						asciiArt.Append($"<span style='color:rgb({pixel[2]},{pixel[1]},{pixel[0]})'>{asciiChar}</span>");
					}
					asciiArt.AppendLine();
				}
			}

			image.UnlockBits(imageData);
			asciiArt.Append("</pre></div>");
			return asciiArt.ToString();
		}
		private Bitmap ResizeImage(Bitmap original, double maxWidth, double maxHeight)
		{
			float aspectRatio = (float)original.Width / original.Height;

			int newWidth = (int)Math.Min(maxWidth * 1.5, maxHeight * aspectRatio * 1.5);
			int newHeight = (int)(newWidth / aspectRatio);

			if (newHeight > maxHeight)
			{
				newHeight = (int)maxHeight;
				newWidth = (int)(newHeight * aspectRatio);
			}

			return new Bitmap(original, new System.Drawing.Size(newWidth, newHeight));
		}
		private void convertButton_Click(object sender, RoutedEventArgs e)
		{
			if (myImg == null) return;

			using (MemoryStream ms = new MemoryStream())
			{
				BitmapEncoder encoder = new BmpBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(myImg));
				encoder.Save(ms);

				var bitmapImage = new Bitmap(ms);

				// Resize the image while maintaining the aspect ratio
				double maxWidth = asciiArtWebBrowser.ActualWidth;
				double maxHeight = asciiArtWebBrowser.ActualHeight;

				Bitmap resizedBitmap = ResizeImage(bitmapImage, maxWidth, maxHeight);

				// Convert to colored ASCII art with better resolution
				string asciiArt = ConvertToColoredAscii(resizedBitmap, maxWidth, maxHeight);

				// Display ASCII art in the WebBrowser
				asciiArtWebBrowser.NavigateToString($"<html><body>{asciiArt}</body></html>");
			}
		}
	}
}