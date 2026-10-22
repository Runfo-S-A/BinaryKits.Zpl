using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

namespace BinaryKits.Zpl.Viewer.UnitTest
{
    [TestClass]
    public class FontManagerTest
    {
        [TestMethod]
        [DataRow("0")]
        [DataRow("1")]
        [DataRow("2")]
        [DataRow("A")]
        [DataRow("Z")]
        [DataRow("unknown")]
        [DataRow("")]
        [DataRow(null)]
        public void DefaultFontLoaderLoadsUnifiedFont(string fontName)
        {
            FontManager fontManager = new FontManager();
            using SKTypeface expected = SKTypeface.FromStream(typeof(FontManager).Assembly.GetManifestResourceStream("BinaryKits.Zpl.Viewer.fonts.Unified-CondensedBold.ttf"));

            SKTypeface actual = fontManager.FontLoader(fontName);

            Assert.IsNotNull(actual);
            Assert.IsNotNull(expected);
            Assert.AreEqual(expected.FamilyName, actual.FamilyName);
            Assert.AreSame(fontManager.FontLoader("0"), actual);
        }
    }
}
