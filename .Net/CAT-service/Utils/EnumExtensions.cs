using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace utils
{
	public static class EnumExtensions
	{
		public static string GetEnumDescription(this Enum value)
		{
			System.Reflection.FieldInfo fi = value.GetType().GetField(value.ToString());

			DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

			if (attributes != null && attributes.Length > 0)
				return attributes[0].Description;
			else
				return value.ToString();
		}
	}

	public static class JavaExtensions
	{
		//public static List<T> ToList<T>(this java.util.List list)
		//{
		//	var aItems = list.toArray(new Object[list.size()]);
		//	var aTItems = Array.ConvertAll(aItems, item => (T)item);
		//	var outList = new List<T>(aTItems);

		//	return outList;
		//}

		//public static HashSet<T> ToHashSet<T>(this java.util.Set set)
		//{
		//	var aItems = set.toArray(new Object[set.size()]);
		//	var aTItems = Array.ConvertAll(aItems, item => (T)item);
		//	var outList = new HashSet<T>(aTItems);

		//	return outList;
		//}
	}
}
