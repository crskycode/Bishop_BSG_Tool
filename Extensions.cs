using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bishop_BSG_Tool
{
    static class Extensions
    {
        public static void WriteByte(this BinaryWriter writer, byte value)
        {
            writer.Write(value);
        }

        public static void WriteInt16(this BinaryWriter writer, short value)
        {
            writer.Write(value);
        }

        public static void WriteUInt16(this BinaryWriter writer, ushort value)
        {
            writer.Write(value);
        }

        public static void WriteInt32(this BinaryWriter writer, int value)
        {
            writer.Write(value);
        }
    }
}
