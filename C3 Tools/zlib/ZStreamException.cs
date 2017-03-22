namespace C3Tools.zlib
{
	public class ZStreamException:System.IO.IOException
	{
		public ZStreamException()
		{
		}
		public ZStreamException(System.String s):base(s)
		{
		}
	}
}