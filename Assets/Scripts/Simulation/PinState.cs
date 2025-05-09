// 
using System.Numerics;

namespace DLS.Simulation
{
	// Helper class for dealing with pin state.
	// Pin state is stored as a BigInteger, with format:
	// Tristate flags (most significant 256 bits) | Bit states (least significant 256 bits)
	// BigIntegers can (practically) store as many bits as needed, so 256-bit values will take up 256-bits in memory and 512-bit values (like this one) will take up 512-bits in memory.
	public static class PinState
	{
		// Each bit has three possible states (tri-state logic):
		public const BigInteger LogicLow = 0;
		public const BigInteger LogicHigh = 1;
		public const BigInteger LogicDisconnected = 2;

		// Mask for single bit value (bit state, and tristate flag)
		public const BigInteger SingleBitMask = 1 | (1 << 256);
		
		public static BigInteger GetBitStates(BigInteger state) => (BigInteger)state; // Is this correct? Shouldn't it be (BigInteger)(state & BigInteger.Parse(0xffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff, NumberStyles.AllowHexSpecifier))?
		public static Biginteger GetTristateFlags(BigInteger state) => (BigInteger)(state >> 256);

		public static void Set(ref uint state, BigInteger bitStates, BigInteger tristateFlags)
		{
			state = (uint)(bitStates | (tristateFlags << 256));
		}

		public static void Set(ref uint state, uint other) => state = other;

		public static ushort GetBitTristatedValue(BigInteger state, int bitIndex)
		{
			ushort bitState = (ushort)((GetBitStates(state) >> bitIndex) & 1);
			ushort tri = (ushort)((GetTristateFlags(state) >> bitIndex) & 1);
			return (ushort)(bitState | (tri << 1)); // Combine to form tri-stated value: 0 = LOW, 1 = HIGH, 2 = DISCONNECTED
		}

		public static bool FirstBitHigh(uint state) => (state & 1) == LogicHigh;

		// ---- Set (smaller bit value) from (larger bit value) Source ----
		// Sets the values in a bit state from a larger one. All required versions can be generated using a series of these ones.
		
		public static void Set4BitFrom8BitSource(ref BigInteger state, BigInteger source8bit, bool firstNibble)
		{
			ushort sourceBitStates = (ushort)GetBitStates(source8bit);
			ushort sourceTristateFlags = (ushort)GetTristateFlags(source8bit);

			if (firstNibble)
			{
				const ushort mask = 0x0f;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const ushort mask = 0xf0;
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 4), (BigInteger)((sourceTristateFlags & mask) >> 4));
			}
		}

		public static void Set8BitFrom16BitSource(ref BigInteger state, BigInteger source16bit, bool firstByte)
		{
			ushort sourceBitStates = (ushort)GetBitStates(source16bit);
			ushort sourceTristateFlags = (ushort)GetTristateFlags(source16bit);

			if (firstByte)
			{
				const ushort mask = 0xff;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const ushort mask = 0xff00;
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 8), (BigInteger)((sourceTristateFlags & mask) >> 8));
			}
		}

		public static void Set16BitFrom32BitSource(ref BigInteger state, BigInteger source32bit, bool firstBytePair)
		{
			uint sourceBitStates = (uint)GetBitStates(source32bit);
			uint sourceTristateFlags = (uint)GetTristateFlags(source32bit);

			if (firstBytePair)
			{
				const ushort mask = 0xffff;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const uint mask = 0xffff0000;
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 16), (BigInteger)((sourceTristateFlags & mask) >> 16);
			}
		}

		public static void Set32BitFrom64BitSource(ref BigInteger state, BigInteger source64bit, bool firstByteQuad)
		{
			ulong sourceBitStates = (ulong)GetBitStates(source64bit);
			ulong sourceTristateFlags = (ulong)GetTristateFlags(source64bit);

			if (firstByteQuad)
			{
				const uint mask = 0xffffffff;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const ulong mask = 0xffffffff00000000;
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 32), (BigInteger)((sourceTristateFlags & mask) >> 32);
			}
		}

		public static void Set64BitFrom128BitSource(ref BigInteger state, BigInteger source128bit, bool firstByteOctuplet)
		{
			BigInteger sourceBitStates = (BigInteger)GetBitStates(source128bit);
			BigInteger sourceTristateFlags = (BigInteger)GetTristateFlags(source128bit);

			if (firstByteOctuplet)
			{
				const ulong mask = 0xffffffffffffffff;
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const BigInteger mask = BigInteger.Parse("0xffffffffffffffff0000000000000000", NumberStyles.AllowHexSpecifier);
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 64), (BigInteger)((sourceTristateFlags & mask) >> 64);
			}
		}

		public static void Set128BitFrom256BitSource(ref BigInteger state, BigInteger source256bit, bool firstByteQuadQuad)
		{
			BigInteger sourceBitStates = (BigInteger)GetBitStates(source256bit);
			BigInteger sourceTristateFlags = (BigInteger)GetTristateFlags(source256bit);

			if (firstByteQuadQuad)
			{
				const BigInteger mask = BigInteger.Parse("0xffffffffffffffffffffffffffffffff", NumberStyles.AllowHexSpecifier);
				Set(ref state, (BigInteger)(sourceBitStates & mask), (BigInteger)(sourceTristateFlags & mask));
			}
			else
			{
				const BigInteger mask = BigInteger.Parse("0xffffffffffffffffffffffffffffffff00000000000000000000000000000000", NumberStyles.AllowHexSpecifier);
				Set(ref state, (BigInteger)((sourceBitStates & mask) >> 128), (BigInteger)((sourceTristateFlags & mask) >> 128);
			}
		}
		
		// ---- Set (larger bit value) From (smaller bit value) Sources ----
		// Sets the values in a bit state from smaller ones. All required versions can be generated using a series of these ones.

		public static void Set8BitFrom4BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			ushort bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 4));
			ushort tristateFlags = (BigInteger)((GetTristateFlags(a) & 0b1111) | ((GetTristateFlags(b) & 0b1111) << 4));
			Set(ref state, (BigInteger)bitStates, (BigInteger)tristateFlags);
		}

		public static void Set16BitFrom8BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			ushort bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 8));
			ushort tristateFlags = (BigInteger)((GetTristateFlags(a) & 0xff) | ((GetTristateFlags(b) & 0xff) << 8));
			Set(ref state, (BigInteger)bitStates, (BigInteger)tristateFlags);
		}
			
		public static void Set32BitFrom16BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			uint bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 16));
			uint tristateFlags = (BigInteger)((GetTristateFlags(a) & 0xffff) | ((GetTristateFlags(b) & 0xffff) << 16));
			Set(ref state, (BigInteger)bitStates, (BigInteger)tristateFlags);
		}

		public static void Set64BitFrom32BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			ulong bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 32));
			ulong tristateFlags = (BigInteger)((GetTristateFlags(a) & 0xffffffff) | ((GetTristateFlags(b) & 0xffffffff) << 32));
			Set(ref state, (BigInteger)bitStates, (BigInteger)tristateFlags);
		}

		public static void Set128BitFrom64BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			BigInteger bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 64));
			BigInteger tristateFlags = (BigInteger)((GetTristateFlags(a) & 0xffffffffffffffff) | ((GetTristateFlags(b) & 0xffffffffffffffff) << 64));
			Set(ref state, bitStates, tristateFlags);
		}
		
		public static void Set256BitFrom128BitSources(ref BigInteger state, BigInteger a, BigInteger b)
		{
			BigInteger bitStates = (BigInteger)(GetBitStates(a) | (GetBitStates(b) << 128));
			BigInteger tristateFlags = (BigInteger)((GetTristateFlags(a) & BigInteger.Parse("0xffffffffffffffffffffffffffffffff", NumberStyles.AllowHexSpecifier)) | ((GetTristateFlags(b) & BigInteger.Parse("0xffffffffffffffffffffffffffffffff", NumberStyles.AllowHexSpecifier)) << 128));
			Set(ref state, bitStates, tristateFlags);
		}
		
		public static void Toggle(ref BigInteger state, int bitIndex)
		{
			BigInteger bitStates = GetBitStates(state);
			bitStates ^= (BigInteger)(1u << bitIndex);

			// Clear tristate flags (can't be disconnected if toggling as only input dev pins are allowed)
			Set(ref state, bitStates, (BigInteger)0);
		}

		public static void SetAllDisconnected(ref uint state) => Set(ref state, 0, ushort.MaxValue);
	}
}
