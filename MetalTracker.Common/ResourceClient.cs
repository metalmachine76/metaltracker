using System.Reflection;

namespace MetalTracker.Common
{
	public static class ResourceClient
	{
		public static string[] GetResourceLines(Assembly assembly, string resName)
		{
			string resString;

			using (var str = assembly.GetManifestResourceStream(resName))
			{
				using (StreamReader sr = new StreamReader(str))
				{
					resString = sr.ReadToEnd();
				}
			}

			string spliton = (resString.Contains("\r\n")) ? "\r\n" : "\n";

			string[] resLines = resString.Split(spliton);

			return resLines;
		}
	}
}


