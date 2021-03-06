// Copyright (c) Juanjo Montiel and contributors. All Rights Reserved. Licensed under the GNU General Public License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PhotoLabeler.PhotoStorageReader.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace PhotoLabeler.PhotoStorageReader.Implementations
{
	public class PhotoReaderBase64 : IPhotoReader
	{
		public async Task<string> GetImgSrcAsync(string path)
		{
			var result = await Task.FromResult(GetImgSrc(path));
			return result;
		}

		public string GetGenericImageSrc() => PhotoReaderBase64thumbnails.B64ImageThumbnail;
		public string GetImgSrc(string path)
		{
			const int uiSize = 100;
			var postedFileExtension = Path.GetExtension(path);

			//tiff: still not supported https://github.com/SixLabors/ImageSharp/issues/12
			//raw, heic: must test if supported
			var supported_images = new[] { ".jpg", ".png", ".gif", ".jpeg", "bmp" };
			var video = new[] { ".mp4" };
			var nom_supported_images = new[] { "tiff", "heic" };
			var isImage =
				supported_images
				.Any(extension => string.Equals(postedFileExtension, extension, StringComparison.OrdinalIgnoreCase));
			var isVideo =
				video
				.Any(extension => string.Equals(postedFileExtension, extension, StringComparison.OrdinalIgnoreCase));
			var isNonSupportedImage =
				nom_supported_images
				.Any(extension => string.Equals(postedFileExtension, extension, StringComparison.OrdinalIgnoreCase));

			if (isImage)
			{
				using (var image = Image.Load(path))
				{
					var (x, y) = (image.Width, image.Height);
					var minSide = Math.Min(x, y);
					var (resizedX, resizedY) = (uiSize * x / minSide, uiSize * y / minSide);
					var centerArea = new Rectangle((resizedX - uiSize) / 2, (resizedY - uiSize) / 2, uiSize, uiSize);

					image.Mutate(x => x
						.Resize(resizedX, resizedY)
						.Crop(centerArea)
						);

					var b64 = image.ToBase64String(JpegFormat.Instance);


					return b64;
				}
			}
			else if (isVideo)
			{
				return PhotoReaderBase64thumbnails.B64VideoThumbnail;
			}
			else if (isNonSupportedImage)
			{
				return PhotoReaderBase64thumbnails.B64ImageThumbnail;
			}
			else
			{
				var attr = File.GetAttributes(path);
				var isFolder = ((attr & FileAttributes.Directory) == FileAttributes.Directory);
				if (isFolder)
				{
					return PhotoReaderBase64thumbnails.B64Folderhumbnail;
				}
				else
				{
					return PhotoReaderBase64thumbnails.B64FileThumbnail;
				}
			}
		}
	}
}
