using iTextSharp.text;
using iTextSharp.text.pdf;

namespace RRCManagementSystem.Helpers
{
    public class PdfWatermark : PdfPageEventHelper
    {
        // super visible test settings
        private const float OPACITY = 0.25f;
        private const float FONTSIZE = 40f;
        private const float ANGLE = 45f;
        private static readonly BaseColor COLOR = new BaseColor(180, 180, 180);
        private const string TEXT = "RRC Management System Property";

        private readonly Font _font =
            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, FONTSIZE, Font.BOLD, COLOR);

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            // draw behind content
            PdfContentByte under = writer.DirectContentUnder;

            // opacity
            PdfGState gs = new PdfGState { FillOpacity = OPACITY };
            under.SaveState();
            under.SetGState(gs);

            Phrase phrase = new Phrase(TEXT, _font);

            // center of page
            Rectangle page = document.PageSize;
            float cx = page.Width / 2f;
            float cy = page.Height / 2f;

            // big centered mark (behind everything)
            ColumnText.ShowTextAligned(
                under,
                Element.ALIGN_CENTER,
                phrase,
                cx,
                cy,
                ANGLE
            );

            under.RestoreState();

            // ---- OPTIONAL DEBUG: draw a hairline border of the content box so you can see margins ----
            // Uncomment to verify margins vs content area.
            /*
            PdfContentByte top = writer.DirectContent;
            top.SaveState();
            top.SetColorStroke(BaseColor.LIGHT_GRAY);
            top.SetLineWidth(0.5f);
            float left   = document.LeftMargin;
            float right  = page.Width - document.RightMargin;
            float bottom = document.BottomMargin;
            float topY   = page.Height - document.TopMargin;
            top.Rectangle(left, bottom, right - left, topY - bottom);
            top.Stroke();
            top.RestoreState();
            */
        }
    }
}
