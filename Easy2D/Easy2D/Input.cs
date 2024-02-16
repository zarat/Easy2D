using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Easy2D
{
	public class Input
	{
		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(int vKey);

		public static bool Down(Keys key)
		{
			return (GetAsyncKeyState((int)key) & 0x8000) != 0;
		}

		public static bool Pressed(Keys key)
		{
			return Down(key) && (GetAsyncKeyState((int)key) & 1) == 0;
		}

		public static bool Released(Keys key)
		{
			return (GetAsyncKeyState((int)key) & 0x8000) == 0 && (GetAsyncKeyState((int)key) & 1) != 0;
		}
	}
}
