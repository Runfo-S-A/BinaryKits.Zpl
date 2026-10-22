using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BinaryKits.Zpl.Viewer.UnitTest
{
    [TestClass]
    public class FieldParameterTest
    {
        private static ZplElementBase[] Analyze(string zpl)
        {
            return [.. new ZplAnalyzer(new PrinterStorage()).Analyze(zpl).LabelInfos[0].ZplElements];
        }

        [TestMethod]
        public void ReverseFieldBlockEndsAtOrigin()
        {
            ZplElementBase[] elements = Analyze("^XA^FPR,0^FT400,100^A0N,30^FB400,1,0,L^FDabc^FS^FT400,200^A0N,30^FB400,1,0,L^FDabc^FS^XZ");

            Assert.AreEqual(0, ((ZplFieldBlock)elements[0]).PositionX);
            // ^FS resets the direction
            Assert.AreEqual(400, ((ZplFieldBlock)elements[1]).PositionX);
        }

        [TestMethod]
        public void ReverseFieldBlockMirrorsJustification()
        {
            ZplElementBase[] elements = Analyze("^XA^FPR^FT600,100^A0N,30^FB1040,1,0,L^FDabc^FS^FPR^FT600,200^A0N,30^FB1040,1,0,R^FDabc^FS^XZ");

            Assert.AreEqual(-440, ((ZplFieldBlock)elements[0]).PositionX);
            // L is where right-to-left text starts: the right edge of the block
            Assert.AreEqual(TextJustification.Right, ((ZplFieldBlock)elements[0]).TextJustification);
            Assert.AreEqual(TextJustification.Left, ((ZplFieldBlock)elements[1]).TextJustification);
        }

        [TestMethod]
        public void ReverseCenteredFieldBlockShiftsByFontWidth()
        {
            // Measured on Zebra prints of primaria_ulsa_nt_enf (^A1N,60,50) and primaria_ulsa_nt_menudencia (^A1N,60)
            ZplElementBase[] elements = Analyze("^XA^FPR,0^FT1040,660^A1N,60,50^FB1040,1,0,C^FDabc^FS^FPR,0^FT1040,660^A1N,60^FB1040,1,0,C^FDabc^FS^XZ");

            Assert.AreEqual(42, ((ZplFieldBlock)elements[0]).PositionX);
            Assert.AreEqual(50, ((ZplFieldBlock)elements[1]).PositionX);
        }

        [TestMethod]
        public void ReverseMultiLineFieldBlockShiftsLeft()
        {
            // Measured on Zebra prints of primaria_ulsa_nt_enf (^A1N,28 and ^A1N,34 in ^FB600,6)
            ZplElementBase[] elements = Analyze("^XA^FPR,0^FT520,550^A1N,28^FB600,6,,L^FDabc^FS^XZ");

            Assert.AreEqual(520 - 600 - 18, ((ZplFieldBlock)elements[0]).PositionX);
        }

        [TestMethod]
        public void ReverseDirectionLaysLatinRightToLeft()
        {
            // The printer lays every character right to left, whatever its script
            static byte[] Draw(string zpl)
            {
                IPrinterStorage printerStorage = new PrinterStorage();
                return new ZplElementDrawer(printerStorage).Draw(new ZplAnalyzer(printerStorage).Analyze(zpl).LabelInfos[0].ZplElements, 50, 10, 8);
            }

            CollectionAssert.AreEqual(Draw("^XA^FO300,10,1^A0N,40^FDBA^FS^XZ"), Draw("^XA^FPR^FO300,10^A0N,40^FDAB^FS^XZ"));
            CollectionAssert.AreEqual(
                Draw("^XA^FT0,50^A0N,40^FB400,1,0,R^FDBA^FS^XZ"),
                Draw("^XA^FPR^FT400,50^A0N,40^FB400,1,0,L^FDAB^FS^XZ"));
            // Nor mirrors brackets: data stores ")x(" so it prints "(x)"
            CollectionAssert.AreEqual(
                Draw("^XA^FT0,50^A0N,40^FB400,1,0,R^FD(A)^FS^XZ"),
                Draw("^XA^FPR^FT400,50^A0N,40^FB400,1,0,L^FD)A(^FS^XZ"));
        }

        [TestMethod]
        public void LowercaseYesIsAccepted()
        {
            ZplElementBase[] elements = Analyze("^XA^FT660,890^BY3^BEN,160,y,n^FD729000113211^FS^XZ");

            Assert.IsTrue(((ZplBarcodeEan13)elements[0]).PrintInterpretationLine);
        }

        [TestMethod]
        public void ReverseTextFieldIsRightJustified()
        {
            ZplElementBase[] elements = Analyze("^XA^FPR^FT400,100^A0N,30^FDabc^FS^XZ");

            ZplTextField textField = (ZplTextField)elements[0];
            Assert.AreEqual(400, textField.PositionX);
            Assert.AreEqual(FieldJustification.Right, textField.FieldTypeset.FieldJustification);
        }
    }
}
