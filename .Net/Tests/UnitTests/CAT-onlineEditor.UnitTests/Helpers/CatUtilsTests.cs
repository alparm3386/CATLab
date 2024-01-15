using CAT.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT_onlineEditor.UnitTests.Helpers
{
    public class CatUtilsTests
    {

        [Fact]
        public void IsCompressedMemoQXliff_ShouldReturnTrueForCompressedXlf()
        {
            Assert.Throws<Exception>(() => CAT.Helpers.CatUtils.IsCompressedMemoQXliff(null));
        }

        [Fact]
        public void IsCompressedMemoQXliff_ShouldReturnFalseForNonCompressedXlf()
        {
            Assert.True(CAT.Helpers.CatUtils.IsCompressedMemoQXliff("C:/Test.mqxlz"));
            Assert.True(CAT.Helpers.CatUtils.IsCompressedMemoQXliff("C:/Test.xlz"));
        }

        [Fact]
        public void IsCompressedMemoQXliff_ShouldTrowExcepton_ForInvalidFilePath()
        {
            Assert.False(CAT.Helpers.CatUtils.IsCompressedMemoQXliff("C:/Test.docx"));
        }
    }
}
