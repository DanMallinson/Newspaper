using PdfSharp.Pdf;

namespace Libraries.Outputs
{
    public class RenderedPDF : RenderedContent
    {
        public PdfDocument? Document { get; set; }

        public override void Save(string filename)
        {
            Document?.Save(filename);
        }
    }
}
