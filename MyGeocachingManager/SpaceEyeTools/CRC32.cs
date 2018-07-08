using System;
using System.Security.Cryptography;
using System.IO;

namespace SpaceEyeTools
{
    /// <summary>
    /// Perform CRC32 computation
    /// </summary>
    public class CRC32 : HashAlgorithm
    {
        /// <summary>
        /// Default polynomial value (0xedb88320)
        /// </summary>
        public const UInt32 DefaultPolynomial = 0xedb88320;

        /// <summary>
        /// Default seed value (0xffffffff)
        /// </summary>
        public const UInt32 DefaultSeed = 0xffffffff;

        private UInt32 hash;
        private UInt32 seed;
        private UInt32[] table;
        private static UInt32[] defaultTable;

        /// <summary>
        /// Constructor
        /// </summary>
        public CRC32()
        {
            table = InitializeTable(DefaultPolynomial);
            seed = DefaultSeed;
            Initialize();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="polynomial">polynomial value</param>
        /// <param name="seed">seed value</param>
        public CRC32(UInt32 polynomial, UInt32 seed)
        {
            table = InitializeTable(polynomial);
            this.seed = seed;
            Initialize();
        }

        /// <summary>
        /// Initialize hash
        /// </summary>
        public override void Initialize()
        {
            hash = seed;
        }

        /// <summary>
        /// Calculate Hash
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <param name="start">start position</param>
        /// <param name="length">length</param>
        protected override void HashCore(byte[] buffer, int start, int length)
        {
            hash = CalculateHash(table, hash, buffer, start, length);
        }

        /// <summary>
        /// Performs final hash
        /// </summary>
        /// <returns>final hash</returns>
        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt32ToBigEndianBytes(~hash);
            this.HashValue = hashBuffer;
            return hashBuffer;
        }

        /// <summary>
        /// Get hashsize (32)
        /// </summary>
        public override int HashSize
        {
            get { return 32; }
        }

        /// <summary>
        /// Compute hash
        /// </summary>
        /// <param name="buffer">buffer</param>
        /// <returns>hash value</returns>
        public static UInt32 Compute(byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Compute hash
        /// </summary>
        /// <param name="seed">seed value</param>
        /// <param name="buffer">buffer</param>
        /// <returns>hash value</returns>
        public static UInt32 Compute(UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(DefaultPolynomial), seed, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Compute hash
        /// </summary>
        /// <param name="polynomial">polynomial value</param>
        /// <param name="seed">seed value</param>
        /// <param name="buffer">buffer</param>
        /// <returns>hash value</returns>
        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            UInt32[] createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, byte[] buffer, int start, int size)
        {
            UInt32 crc = seed;
            for (int i = start; i < size; i++)
                unchecked
                {
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                }
            return crc;
        }

        private byte[] UInt32ToBigEndianBytes(UInt32 x)
        {
            return new byte[] {
			    (byte)((x >> 24) & 0xff),
			    (byte)((x >> 16) & 0xff),
			    (byte)((x >> 8) & 0xff),
			    (byte)(x & 0xff)
		    };
        }

        /// <summary>
        /// Get CRC from a file
        /// </summary>
        /// <param name="file">file to open (read only)</param>
        /// <returns>CRC32 value</returns>
        public static String GetCRC(String file)
        {
            CRC32 crc32 = new CRC32();
            String hash = String.Empty;

            using (FileStream fs = File.Open(file, FileMode.Open))
                foreach (byte b in crc32.ComputeHash(fs)) hash += b.ToString("x2").ToLower();

            return hash;
        }

    }
}