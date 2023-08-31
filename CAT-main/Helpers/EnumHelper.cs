using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CAT.Helpers
{
    public static class EnumHelper
    {
        public static string GetDisplayName(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString())!;
            DisplayAttribute[] attributes = (DisplayAttribute[])fi.GetCustomAttributes(typeof(DisplayAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Name!;
            }
            else
            {
                return value.ToString();
            }
        }

        //public static string GetDisplayName(Enum enumValue)
        //{
        //    var displayAttribute = enumValue.GetType()
        //                                   .GetMember(enumValue.ToString())
        //                                   .First()
        //                                   .GetCustomAttribute<DisplayAttribute>();

        //    return displayAttribute?.Name ?? enumValue.ToString();
        //}
    }
}
