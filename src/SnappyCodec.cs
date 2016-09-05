using System;
using System.Runtime.InteropServices;

namespace Snappy
{
    public class SnappyCodec
    {
        public static byte[] Compress(byte[] input)
        {
            IntPtr source_length = new IntPtr(input.Length);
            IntPtr compressed_length = snappy_max_compressed_length(source_length);
            IntPtr compressed = Marshal.AllocHGlobal(compressed_length);
            try
            {
                switch (snappy_compress(input, source_length, compressed, ref compressed_length))
                {
                case SnappyStatus.SNAPPY_BUFFER_TOO_SMALL:
                case SnappyStatus.SNAPPY_INVALID_INPUT:
                    throw new Exception();
                default:
                    break;
                }
                int outputLength = compressed_length.ToInt32();
                byte[] output = new byte[outputLength];
                Marshal.Copy(compressed, output, 0, outputLength);

                return output;
            }
            finally
            {
                Marshal.FreeHGlobal(compressed);
            }
        }

        public static byte[] Uncompress(byte[] compressed)
        {
            IntPtr compressed_length = new IntPtr(compressed.Length);
            IntPtr uncompressed_length = new IntPtr(GetUncompressedLength(compressed));
            IntPtr uncompressed = Marshal.AllocHGlobal(uncompressed_length);
            try
            {
                switch (snappy_uncompress(compressed, compressed_length, uncompressed, ref uncompressed_length))
                {
                case SnappyStatus.SNAPPY_BUFFER_TOO_SMALL:
                case SnappyStatus.SNAPPY_INVALID_INPUT:
                    throw new Exception();
                default:
                    break;
                }
                int outputLength = uncompressed_length.ToInt32();
                byte[] output = new byte[outputLength];
                Marshal.Copy(uncompressed, output, 0, outputLength);

                return output;
            }
            finally
            {
                Marshal.FreeHGlobal(uncompressed);
            }
        }

        public static int GetUncompressedLength(byte[] compressed)
        {
            IntPtr compressed_length = new IntPtr(compressed.Length);
            IntPtr result = new IntPtr(0);

            switch (snappy_uncompressed_length(compressed, compressed_length, ref result))
            {
            case SnappyStatus.SNAPPY_BUFFER_TOO_SMALL:
            case SnappyStatus.SNAPPY_INVALID_INPUT:
                throw new Exception();
            default:
                break;
            }

            return result.ToInt32();
        }

        public static bool Validate(byte[] compressed)
        {
            IntPtr compressed_length = new IntPtr(compressed.Length);

            switch (snappy_validate_compressed_buffer(compressed, compressed_length))
            {
            case SnappyStatus.SNAPPY_BUFFER_TOO_SMALL:
            case SnappyStatus.SNAPPY_INVALID_INPUT:
                return false;
            default:
                return true;
            }
        }

        [DllImport("snappy")]
        private static extern SnappyStatus snappy_compress(byte[] input, IntPtr input_length, IntPtr compressed, ref IntPtr compressed_length);

        [DllImport("snappy")]
        private static extern SnappyStatus snappy_uncompress(byte[] compressed, IntPtr compressed_length, IntPtr uncompressed, ref IntPtr uncompressed_length);

        [DllImport("snappy")]
        private static extern IntPtr snappy_max_compressed_length(IntPtr source_length);

        [DllImport("snappy")]
        private static extern SnappyStatus snappy_uncompressed_length(byte[] compressed, IntPtr compressed_length, ref IntPtr result);

        [DllImport("snappy")]
        private static extern SnappyStatus snappy_validate_compressed_buffer(byte[] compressed, IntPtr compressed_length);
    }
}