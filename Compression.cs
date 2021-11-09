using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bishop_BSG_Tool
{
    class Compression
    {
        public unsafe static void RleCompress(BinaryWriter writer, byte[] data, int pixel_size)
        {
            fixed (byte* ptr = data)
            {
                for (int i = 0; i < pixel_size; i++)
                {
                    RleCompressStep(writer, ptr + i, data.Length, pixel_size);
                }
            }
        }

        unsafe static void RleCompressStep(BinaryWriter writer, byte* data, int data_size, int pixel_size)
        {
            int scan_p = 0;
            int total = 0;
            int offset = Convert.ToInt32(writer.BaseStream.Position);
            writer.Write(0);
            if (data_size > 0)
            {
                byte* data_end = data + data_size;
                do
                {
                    byte* next_p = data + scan_p + pixel_size;
                    byte* curr_p = data + scan_p;
                    byte curr_c = data[scan_p];
                    int count;
                    if (*next_p == curr_c)
                    {
                        int i;
                        for (i = 0; i < 127; i++)
                        {
                            if (next_p >= data_end)
                            {
                                break;
                            }
                            if (*next_p != curr_c)
                            {
                                break;
                            }
                            next_p += pixel_size;
                        }
                        scan_p += pixel_size * i;
                        count = 256 - i;
                    }
                    else
                    {
                        byte* prev_p = next_p - pixel_size;
                        int v16 = 0;
                        count = 0;
                        while (next_p < data_end)
                        {
                            v16 = count;
                            if (*next_p == *prev_p)
                            {
                                break;
                            }
                            v16 = count + 1;
                            prev_p += pixel_size;
                            next_p += pixel_size;
                            count = v16;
                            if (v16 >= 127)
                            {
                                break;
                            }
                        }
                        if (v16 != 0)
                        {
                            count = v16 - 1;
                            scan_p += pixel_size * (v16 - 1);
                            writer.Write((byte)count);
                            if (count + 1 > 0)
                            {
                                int j = 0;
                                do
                                {
                                    writer.Write(*curr_p);
                                    curr_p += pixel_size;
                                    j++;
                                }
                                while (j < count + 1);
                            }
                            total += count + 2;
                            scan_p += pixel_size;
                            continue;
                        }
                        count = 256;
                    }
                    writer.Write((byte)count);
                    writer.Write(*curr_p);
                    total += 2;
                    scan_p += pixel_size;
                }
                while (scan_p < data_size);
            }
            int offset2 = Convert.ToInt32(writer.BaseStream.Position);
            writer.Seek(offset, SeekOrigin.Begin);
            writer.Write(total);
            writer.Seek(offset2, SeekOrigin.Begin);
        }
    }
}
