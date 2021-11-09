using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Text;

namespace Bishop_BSG_Tool
{
    class BSG
    {
        static readonly byte[] Signature = Encoding.ASCII.GetBytes("BSS-Graphics\x00\x00\x00\x00");

        public static void Create(string filePath, string sourcePath)
        {
            // Load source image file

            var source = Image.Load(sourcePath);

            if (source.PixelType.BitsPerPixel != 32)
            {
                throw new Exception("Only 32-bit image file are supported.");
            }

            // Copy pixel data

            var image = source.CloneAs<Bgra32>();

            // Flip image

            image.Mutate(x => x.Flip(FlipMode.Vertical));

            // Header information

            var unpackedSize = image.Width * image.Height * 4;

            var imageWidth = Convert.ToUInt16(image.Width);
            var imageHeight = Convert.ToUInt16(image.Height);

            // Create file

            using var stream = File.Create(filePath);
            using var writer = new BinaryWriter(stream);

            // Write file header

            writer.Write(Signature);
            writer.WriteByte(0);
            writer.WriteByte(1);
            writer.WriteInt32(unpackedSize);    // unpacked size
            writer.WriteUInt16(imageWidth);     // width
            writer.WriteUInt16(imageHeight);    // height
            writer.WriteInt32(0x10);
            writer.WriteUInt16(0);

            writer.WriteUInt16(0); // offset x
            writer.WriteUInt16(0); // offset y
            writer.WriteUInt16(0);
            writer.WriteUInt16(0);
            writer.WriteUInt16(imageWidth);     // width
            writer.WriteUInt16(imageHeight);    // height
            writer.WriteByte(0);
            writer.WriteByte(1);
            writer.WriteUInt16(0);

            writer.WriteByte(0);        // image type, 0 - bgra32, 1 - bgr32, 2 - indexed
            writer.WriteByte(1);        // compression method, 0 - none, 1 - rle, 2 - lz
            writer.WriteInt32(0x40);    // data offset
            writer.WriteInt32(0);       // packed size
            writer.WriteInt32(0);       // palette offset, 0 - none
            writer.WriteByte(0);
            writer.WriteByte(2);

            // Pack pixel data

            var memStream = new MemoryStream(unpackedSize);
            var memWriter = new BinaryWriter(memStream);

            for (var y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (var x = 0; x < image.Width; x++)
                {
                    memWriter.Write(row[x].PackedValue);
                }
            }

            var pixelData = memStream.ToArray();

            // Compress and write out pixel data

            var dataOffset = stream.Position;

            Compression.RleCompress(writer, pixelData, 4);

            // Update file header

            var dataSize = Convert.ToUInt32(stream.Position - dataOffset);
            stream.Position = 0x36;
            writer.Write(dataSize);

            // Done

            writer.Flush();
        }
    }
}
