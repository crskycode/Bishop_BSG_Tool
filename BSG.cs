using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
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

        public static void ExtractBpp8(string filePath, string sourcePath)
        {
            using var stream = File.OpenRead(sourcePath);
            using var reader = new BinaryReader(stream);

            // File Header
            var signature = reader.ReadBytes(16);

            if (!signature.SequenceEqual(Signature))
            {
                throw new Exception("Not a valid BSG image file.");
            }

            reader.ReadByte();      // This value is 1 when 'compression method' is greater than or equal to 2
            reader.ReadByte();      // Always is 1
            reader.ReadInt32();     // Length of uncompressed pixel data
            int width = reader.ReadUInt16();    // Width
            int height = reader.ReadUInt16();   // Height
            reader.ReadInt32();     // Always is 16
            reader.ReadByte();      // Unused, always is 0
            reader.ReadByte();      // Unused, always is 0

            // Image Description
            reader.ReadInt16();     // Always is 0, maybe offset X of this clip?
            reader.ReadInt16();     // Always is 0, maybe offset Y of this clip?
            reader.ReadInt16();     // Always is 0, unknow
            reader.ReadInt16();     // Always is 0, unknow
            reader.ReadInt16();     // Width
            reader.ReadInt16();     // Height
            reader.ReadByte();      // Always is 0
            reader.ReadByte();      // Always is 1
            reader.ReadInt16();     // Always is 0

            // Data Description
            int bppType = reader.ReadByte();            // BPP type, 0 - BPP32, 1 - BPP24, 2 - BPP8
            int compressionMethod = reader.ReadByte();  // Compression method, 0 - NONE, 1 - RLE, 2 - LZ
            int dataPosition = reader.ReadInt32();      // Position of compressed pixel data
            int dataLength = reader.ReadInt32();        // Length of compressed pixel data
            reader.ReadInt32();     // Position of palette pixel data
            reader.ReadByte();      // Always is 0
            reader.ReadByte();      // Always is 0

            if (bppType != 2)
            {
                throw new Exception("This method only supports BPP8 format.");
            }

            reader.BaseStream.Position = dataPosition;

            var pixels = Array.Empty<byte>();

            switch (compressionMethod)
            {
                case 0:
                {
                    pixels = reader.ReadBytes(dataLength);
                    break;
                }
                case 1:
                {
                    var data = reader.ReadBytes(dataLength);
                    pixels = new byte[width * height];
                    Compression.RleUncompress(data, pixels, 0, 1);
                    break;
                }
                default:
                {
                    throw new Exception("This method only supports RAW and RLE compression methods.");
                }
            }

            if (pixels.Length != width * height)
            {
                throw new Exception("No pixels were read.");
            }

            var image = Image.LoadPixelData<L8>(pixels, width, height);

            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var pngEncoder = new PngEncoder
            {
                BitDepth = PngBitDepth.Bit8,
                ColorType = PngColorType.Grayscale
            };

            image.SaveAsPng(filePath, pngEncoder);

            stream.Close();
            stream.Dispose();
        }

        public static void CreateBpp8(string filePath, string sourcePath)
        {
            var sourceInfo = Image.Identify(sourcePath, out var sourceFormat);

            if (sourceInfo == null || sourceFormat == null || sourceFormat.Name != "PNG")
            {
                throw new Exception("This method only supports PNG format.");
            }

            var pngMetadata = sourceInfo.Metadata.GetPngMetadata();

            if (pngMetadata.BitDepth != PngBitDepth.Bit8)
            {
                throw new Exception("This method only supports BPP8 format.");
            }

            if (pngMetadata.ColorType != PngColorType.Grayscale)
            {
                throw new Exception("This method only supports BPP8 format.");
            }

            var image = Image.Load<L8>(sourcePath);

            if (image.Width > short.MaxValue || image.Height > short.MaxValue)
            {
                throw new Exception("This image is too large.");
            }

            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var bufferSize = image.Width * image.Height;

            var memStream = new MemoryStream(bufferSize);
            var memWriter = new BinaryWriter(memStream);

            for (var y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (var x = 0; x < image.Width; x++)
                {
                    memWriter.Write(row[x].PackedValue);
                }
            }

            var pixels = memStream.ToArray();

            using var stream = File.Create(filePath);
            using var writer = new BinaryWriter(stream);

            writer.Write(Signature);
            writer.WriteByte(0);    // This value is 1 when 'compression method' is greater than or equal to 2
            writer.WriteByte(1);    // Always is 1
            writer.WriteInt32(bufferSize);  // Length of uncompressed pixel data
            writer.WriteInt16(Convert.ToInt16(image.Width));    // Width
            writer.WriteInt16(Convert.ToInt16(image.Height));   // Height
            writer.WriteInt32(16);  // Always is 16
            writer.WriteByte(0);    // Unused, always is 0
            writer.WriteByte(0);    // Unused, always is 0

            // Image Description
            writer.WriteInt16(0);   // Always is 0, maybe offset X of this clip?
            writer.WriteInt16(0);   // Always is 0, maybe offset Y of this clip?
            writer.WriteInt16(0);   // Always is 0, unknow
            writer.WriteInt16(0);   // Always is 0, unknow
            writer.WriteInt16(Convert.ToInt16(image.Width));    // Width
            writer.WriteInt16(Convert.ToInt16(image.Height));   // Height
            writer.WriteByte(0);     // Always is 0
            writer.WriteByte(1);     // Always is 1
            writer.WriteInt16(0);    // Always is 0

            // Data Description
            writer.WriteByte(2);    // BPP type, 0 - BPP32, 1 - BPP24, 2 - BPP8
            writer.WriteByte(1);    // Compression method, 0 - NONE, 1 - RLE, 2 - LZ
            writer.WriteInt32(0x40);    // Position of compressed pixel data
            writer.WriteInt32(0);   // Length of compressed pixel data
            writer.WriteInt32(0);   // Position of palette pixel data, 0 - No palette
            writer.WriteByte(0);    // Always is 0
            writer.WriteByte(0);    // Always is 0

            var dataPosition = stream.Position;

            Compression.RleCompress(writer, pixels, 1);

            var dataLength = Convert.ToUInt32(stream.Position - dataPosition);
            stream.Position = 0x36;
            writer.Write(dataLength);

            writer.Flush();
            writer.Close();
            writer.Dispose();
        }
    }
}
