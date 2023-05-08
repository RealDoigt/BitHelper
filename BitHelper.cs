using System;
namespace BitHelper
{
    public class GreaterThanMaximumException : Exception
    {
        public GreaterThanMaximumException(byte max) : base($"The value cannot be greater than {max}!") { }
    }

    public class ByteOverflowException : Exception
    {
        public ByteOverflowException() : base("An attempt was made to add more bits than a byte can hold.") {}
    }

    // A byte cannot be separated into more than 8 bits.
    public class NotEnoughBitsException : Exception {}

    public class MissingArgumentException : ArgumentException {}

    public enum StorageType : byte
    {
        Bit    = 1,
        Crumb  = 2,
        Tribit = 3,
        Nibble = 4,
        Pentad = 5,
        Hexad  = 6,
        Heptad = 7,
        Byte   = 8
    }

    static class StorageServices
    {
        static internal StorageType Combine(this StorageType a, StorageType b)
        {
            if ((byte)a + (byte)b > (byte)StorageType.Byte)
                throw new ByteOverflowException();

            return (StorageType)((byte)a + (byte)b);
        }

        static public byte[] ToByteArray(this StorageUnit[] storageUnits)
        {
            var result = new byte[storageUnits.Length];

            for (int index = 0; index < storageUnits.Length; ++index)
                result[index] = storageUnits[index].Value;

            return result;
        }
    }

    public class StorageUnit
    {
        private byte value;

        public StorageType Type { get; private set; }
        public byte MaxValue => (byte)(Math.Pow(2, (double)Type) - 1);
        /***************************\
        * Maximum Value = 2ᵀʸᵖᵉ - 1 *     
        \***************************/

        public byte Value 
        {
            get => value;

            set
            {
                if (value > MaxValue) throw new GreaterThanMaximumException(MaxValue);
                this.value = value;
            }
        }

        public StorageUnit(byte value, StorageType type)
        {
            Type = type;

            if (value > MaxValue) throw new GreaterThanMaximumException(MaxValue);
            this.value = value;
        }

        public override string ToString() => Convert.ToString(value, 2);

        public StorageUnit Combine(params StorageUnit[] storageUnits)
        {
            var type = Type;
            var val = value;

            foreach (var unit in storageUnits)
            {
                val |= (byte)(unit.value << (byte)unit.Type);
                type = type.Combine(unit.Type);
            }

            return new StorageUnit(val, type);
        }

        static public StorageUnit[] Separate(byte value, params StorageType[] storageTypes)
        {
            // validation loop to make sure the method isn't trying to split more data than available.
            byte count = 0;
            for (int index = 0; index < storageTypes.Length; ++index)
            {
                count += (byte)storageTypes[index];
                if (count > 8) throw new NotEnoughBitsException();
            }

            // Validation check to make sure the method isn't missing any data.
            if (count != 8) throw new MissingArgumentException();

            var storageUnits = new StorageUnit[storageTypes.Length];

            byte lastValue = 0;
            for (int index = 0; index < storageUnits.Length; ++index)
            {
                byte newValue = (byte)(value << lastValue),
                     currentValue = (byte)(newValue >> (byte)(8 - (byte)storageTypes[index]));

                storageUnits[index] = new StorageUnit(currentValue, storageTypes[index]);

                lastValue += (byte)storageTypes[index];
            }

            return storageUnits;
        }
    }
}
