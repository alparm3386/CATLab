using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CAT.Helpers
{
    public class EnumHelper
    {
        public static string GetEnumDisplayName(Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                                           .GetMember(enumValue.ToString())
                                           .First()
                                           .GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}
