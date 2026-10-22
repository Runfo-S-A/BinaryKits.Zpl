using BinaryKits.Zpl.Viewer.ElementDrawers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;

using System;
using System.Linq;
using System.Threading.Tasks;


namespace BinaryKits.Zpl.Viewer.UnitTest
{
    [TestClass]
    public class DrawerTest
    {
        [TestMethod]
        public void FontAssignment()
        {
            string zplString = Common.LoadZPL("font-assign");

            var drawOptions = new DrawerOptions()
            {
                FontLoader = fontName =>
                {
                    if (fontName == "0")
                    {
                        //typeface = SKTypeface.FromFile(@"swiss-721-black-bt.ttf");
                        return SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                    }
                    else if (fontName == "1")
                    {
                        return SKTypeface.FromFamilyName("SIMSUN", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                    }

                    return SKTypeface.Default;
                }
            };
            Common.DefaultPrint(zplString, "font-assign.png", 300, 300, 8, drawOptions);
        }

        [TestMethod]
        public void FormatHandling()
        {
            string zplString = Common.LoadZPL("merge");
            Common.DefaultPrint(zplString, "merge.png", 100, 100, 8);
        }

        [TestMethod]
        public void InvertColor()
        {
            // Example in ZPL manual
            string test1 = Common.LoadZPL("invert1");
            // from https://github.com/BinaryKits/BinaryKits.Zpl/pull/64
            string test2 = Common.LoadZPL("invert2");

            Common.DefaultPrint(test1, "inverted1.png", 100, 100, 8);
            Common.DefaultPrint(test2, "inverted2.png");
        }

        [TestMethod]
        public void ConcurrentDrawsMatchSequential()
        {
            // Like the WebApi: every request has its own storage, analyzer and drawer
            const string zpl = "^XA^FO20,20^GB760,560,4^FS^FO50,60^A0N,40,40^FDSynthetic label^FS" +
                "^FO50,130^BY3^BCN,100,Y,N,N^FD0123456789^FS^FO50,300^GB300,80,80^FS" +
                "^FO70,320^FR^A0N,40,40^FDREVERSE^FS^FO420,300^GE300,150,4^FS^XZ";

            static byte[] Render(string zplData)
            {
                IPrinterStorage printerStorage = new PrinterStorage();
                ZplAnalyzer analyzer = new ZplAnalyzer(printerStorage);
                ZplElementDrawer drawer = new ZplElementDrawer(printerStorage);
                return drawer.Draw(analyzer.Analyze(zplData).LabelInfos[0].ZplElements, 101.6, 76.2, 8);
            }

            byte[] expected = Render(zpl);
            byte[][] results = new byte[64][];
            Parallel.For(0, results.Length, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i => results[i] = Render(zpl));

            int mismatches = results.Count(r => !r.AsSpan().SequenceEqual(expected));
            Assert.AreEqual(0, mismatches, $"{mismatches} of {results.Length} concurrent renders differ from the sequential render");
        }
    }
}
