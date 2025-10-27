using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;

namespace RRCManagementSystem.Helpers
{
    public class PdfHeaderFooter : PdfPageEventHelper
    {
        // Header/Footer Properties
        private string _reportTitle;
        private string _generatedBy;
        private DateTime _generatedDate;
        private string _logoPath;

        // Watermark Constants
        private const float WM_OPACITY = 0.10f;
        private const float WM_FONTSIZE = 20f;
        private const float WM_ANGLE = 55f;
        private static readonly BaseColor WM_COLOR = new BaseColor(180, 180, 180);
        private const string WM_TEXT = "RRC Management System Property";

        private readonly Font _watermarkFont =
            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, WM_FONTSIZE, Font.BOLD, WM_COLOR);

        public PdfHeaderFooter(string reportTitle, string generatedBy, string logoPath)
        {
            _reportTitle = reportTitle;
            _generatedBy = generatedBy;
            _generatedDate = DateTime.Now;
            _logoPath = logoPath;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            PdfContentByte cb = writer.DirectContent;
            Rectangle page = document.PageSize;

            // ==================== 1. WATERMARK (Behind Content) ====================
            PdfContentByte under = writer.DirectContentUnder;

            // Opacity settings
            PdfGState gs = new PdfGState { FillOpacity = WM_OPACITY };
            under.SaveState();
            under.SetGState(gs);

            Phrase watermarkPhrase = new Phrase(WM_TEXT, _watermarkFont);

            // Center of page coordinates
            float cx = page.Width / 2f;
            float cy = page.Height / 2f;

            // Draw the watermark
            ColumnText.ShowTextAligned(
                under,
                Element.ALIGN_CENTER,
                watermarkPhrase,
                cx,
                cy,
                WM_ANGLE
            );

            under.RestoreState();
            // ==================== 2. HEADER/FOOTER (In Front of Content) ====================

            // Define fonts for Header/Footer
            Font headerFont = FontFactory.GetFont("Arial", 14, Font.BOLD, BaseColor.BLACK);
            Font titleFont = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.DARK_GRAY);
            Font footerFont = FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.GRAY);

            // --- HEADER ---
            // Y position for the main text in the header (e.g., "RRC Management System")
            float headerY = page.Height - 30;

            // LEFT: Logo
            if (File.Exists(_logoPath))
            {
                try
                {
                    Image logo = Image.GetInstance(_logoPath);


                    logo.ScaleToFit(100f, 100f);

                    logo.SetAbsolutePosition(document.LeftMargin, page.Height - 40f);

                    cb.AddImage(logo); // Use cb.AddImage for foreground drawing in OnEndPage
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Logo Error: {ex.Message}");
                }
            }

            // CENTER: "RRC Management"
            ColumnText.ShowTextAligned(
                cb,
                Element.ALIGN_CENTER,
                new Phrase("RRC Management System", headerFont),
                page.Width / 2,
                headerY,
                0
            );

            // RIGHT: Report Title
            ColumnText.ShowTextAligned(
                cb,
                Element.ALIGN_RIGHT,
                new Phrase(_reportTitle, titleFont),
                page.Width - document.RightMargin,
                headerY,
                0
            );

            // Header separator line (at headerY - 10)
            cb.SetLineWidth(0.5f);
            cb.SetColorStroke(BaseColor.LIGHT_GRAY);
            cb.MoveTo(document.LeftMargin, headerY - 10);
            cb.LineTo(page.Width - document.RightMargin, headerY - 10);
            cb.Stroke();

            // --- FOOTER ---
            float footerY = document.BottomMargin - 10;

            // RIGHT: Generated info and page number
            string footerText = $"Generated: {_generatedDate:yyyy-MM-dd HH:mm:ss} | By: {_generatedBy} | Page {writer.PageNumber}";

            ColumnText.ShowTextAligned(
                cb,
                Element.ALIGN_RIGHT,
                new Phrase(footerText, footerFont),
                page.Width - document.RightMargin,
                footerY,
                0
            );

            // Footer separator line
            cb.SetLineWidth(0.5f);
            cb.SetColorStroke(BaseColor.LIGHT_GRAY);
            cb.MoveTo(document.LeftMargin, footerY + 15);
            cb.LineTo(page.Width - document.RightMargin, footerY + 15);
            cb.Stroke();
        }
    }
}
