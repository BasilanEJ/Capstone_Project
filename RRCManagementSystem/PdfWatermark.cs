using iTextSharp.text;
using iTextSharp.text.pdf;

namespace RRCManagementSystem.Helpers
{
    public class PdfWatermark : PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfContentByte canvas = writer.DirectContent;

            BaseColor watermarkColor = new BaseColor(180, 180, 180); // soft gray
            Font watermarkFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 40, Font.BOLD, watermarkColor);
            Phrase watermark = new Phrase("RRC Management System Property", watermarkFont);

            float xStep = 300f;
            float yStep = 250f;

            for (float x = xStep / 2; x < document.PageSize.Width; x += xStep)
            {
                for (float y = yStep / 2; y < document.PageSize.Height; y += yStep)
                {
                    ColumnText.ShowTextAligned(
                        canvas,
                        Element.ALIGN_CENTER,
                        watermark,
                        x,
                        y,
                        45f
                    );
                }
            }
        }
    }
}