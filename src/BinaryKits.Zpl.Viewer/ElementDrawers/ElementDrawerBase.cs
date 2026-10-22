using BinaryKits.Zpl.Label;
using BinaryKits.Zpl.Label.Elements;

using SkiaSharp;
using SkiaSharp.HarfBuzz;

using System;

namespace BinaryKits.Zpl.Viewer.ElementDrawers
{
    public abstract class ElementDrawerBase : IElementDrawer
    {
        internal IPrinterStorage printerStorage;
        internal SKCanvas skCanvas;

        ///<inheritdoc/>
        public void Prepare(
            IPrinterStorage printerStorage,
            SKCanvas skCanvas)
        {
            this.printerStorage = printerStorage;
            this.skCanvas = skCanvas;
        }

        ///<inheritdoc/>
        public abstract bool CanDraw(ZplElementBase element);

        /// <summary>
        /// DrawShapedText, but ^FPR lays every character right to left whatever its script, as the printer does
        /// </summary>
        protected void DrawShapedText(string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint, bool rightToLeft)
        {
            if (!rightToLeft)
            {
                this.skCanvas.DrawShapedText(text, x, y, textAlign, font, paint);
                return;
            }

            using SKShaper shaper = new(font.Typeface);
            using HarfBuzzSharp.Buffer buffer = new();
            buffer.AddUtf16(text);
            buffer.GuessSegmentProperties();
            buffer.Direction = HarfBuzzSharp.Direction.RightToLeft;
            SKShaper.Result result = shaper.Shape(buffer, font);

            using SKTextBlobBuilder builder = new();
            SKPositionedRunBuffer run = builder.AllocatePositionedRun(font, result.Codepoints.Length);
            Span<ushort> glyphs = run.Glyphs;
            Span<SKPoint> positions = run.Positions;
            for (int i = 0; i < result.Codepoints.Length; i++)
            {
                glyphs[i] = (ushort)result.Codepoints[i];
                positions[i] = result.Points[i];
            }

            float width = result.Width;
            x -= textAlign switch
            {
                SKTextAlign.Center => width / 2,
                SKTextAlign.Right => width,
                _ => 0
            };

            using SKTextBlob blob = builder.Build();
            if (blob != null)
            {
                this.skCanvas.DrawText(blob, x, y, paint);
            }
        }

        ///<inheritdoc/>
        public virtual bool IsReverseDraw(ZplElementBase element)
        {
            return false;
        }

        ///<inheritdoc/>
        public virtual bool IsWhiteDraw(ZplElementBase element)
        {
            return false;
        }

        ///<inheritdoc/>
        public virtual bool ForceBitmapDraw(ZplElementBase element)
        {
            return false;
        }

        ///<inheritdoc/>
        public virtual SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition)
        {
            return currentPosition;
        }

        ///<inheritdoc/>
        public virtual SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont)
        {
            return this.Draw(element, options, currentPosition);
        }

        ///<inheritdoc/>
        public virtual SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            return this.Draw(element, options, currentPosition, internationalFont);
        }

        protected virtual SKPoint CalculateNextDefaultPosition(float x, float y, float elementWidth, float elementHeight, bool useFieldOrigin, Label.FieldOrientation fieldOrientation, SKPoint currentPosition)
        {
            if (useFieldOrigin)
            {
                switch (fieldOrientation)
                {
                    case Label.FieldOrientation.Normal:
                        return new SKPoint(x + elementWidth, y + elementHeight);
                    case Label.FieldOrientation.Rotated90:
                        return new SKPoint(x, y + elementHeight);
                    case Label.FieldOrientation.Rotated180:
                        return new SKPoint(x - elementWidth, y);
                    case Label.FieldOrientation.Rotated270:
                        return new SKPoint(x, y - elementHeight);
                }
            }
            else
            {
                switch (fieldOrientation)
                {
                    case Label.FieldOrientation.Normal:
                        return new SKPoint(x + elementWidth, y);
                    case Label.FieldOrientation.Rotated90:
                        return new SKPoint(x, y + elementWidth);
                    case Label.FieldOrientation.Rotated180:
                        return new SKPoint(x - elementWidth, y);
                    case Label.FieldOrientation.Rotated270:
                        return new SKPoint(x, y - elementWidth);
                }
            }

            return currentPosition;
        }

    }
}
