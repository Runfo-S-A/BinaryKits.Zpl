using BinaryKits.Zpl.Label.Elements;

namespace BinaryKits.Zpl.Viewer.CommandAnalyzers
{
    public class FieldParameterZplCommandAnalyzer : ZplCommandAnalyzerBase
    {
        public FieldParameterZplCommandAnalyzer() : base("^FP") { }

        ///<inheritdoc/>
        public override ZplElementBase Analyze(string zplCommand, VirtualPrinter virtualPrinter, IPrinterStorage printerStorage)
        {
            // shortcut: only R (right-to-left) is handled; V (vertical) and the character gap are ignored, add them if a label needs them.
            if (this.SplitCommand(zplCommand)[0] == "R")
            {
                virtualPrinter.SetNextElementFieldReverseDirection();
            }

            return null;
        }
    }
}
