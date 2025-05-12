using Libraries.Articles;
using Libraries.Articles.Blocks;
using Libraries.Outputs;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libraries.Renderers
{
    public class NewspaperRenderer :Renderer
    {
        const string IMAGE_HOLDING_DIRECTORY = "Temp";
        const int IMAGE_URL_TIMEOUT = 30 * 1000;
        const long IMAGE_QUALITY = 50;
        public bool RenderImages { get; set; }

        public NewspaperRenderer()
        {
            RenderImages = false;
        }

        public override RenderedContent Render(NewspaperContent content)
        {
            RefreshImageDirectory();

            var result = new RenderedPDF();
            RenderImages = content.Paper.RenderImages;
            result.Document = RenderNewspaper(content);
            ClearOldImageDirectory();
            return result;
        }

        private void RefreshImageDirectory()
        {
            ClearOldImageDirectory();
            Directory.CreateDirectory(IMAGE_HOLDING_DIRECTORY);
        }

        private void ClearOldImageDirectory()
        {
            if (Directory.Exists(IMAGE_HOLDING_DIRECTORY))
            {
                Directory.Delete(IMAGE_HOLDING_DIRECTORY, true);
            }
        }

        private PdfDocument RenderNewspaper(NewspaperContent content)
        {
            var renderer = new PdfDocumentRenderer
            {
                Document = CreateDocument(content)
            };

            renderer.RenderDocument();
            return renderer.PdfDocument;
        }

        private Document CreateDocument(NewspaperContent content)
        {
            var result = new Document();

            var header = CreateNewspaperHeader(result, content.Timestamp, content.Paper);

            AddCategories(header, content.CategorisedArticles);

            return result;
        }

        private Section CreateNewspaperHeader(Document document, DateTime timestamp, Newspaper paper)
        {
            var section = CreateSection(document);
            AddNewspaperTitle(section, paper);
            AddNewspaperEdition(section, timestamp);
            return section;
        }

        private void AddNewspaperTitle(Section section, Newspaper paper)
        {
            if(string.IsNullOrEmpty(paper.MainImage) || !File.Exists(paper.MainImage))
            {
                var header = CreateParagraph(section);
                //TODO - parameterise font sizing
                header.Format.Font.Size = 32;
                header.Format.Alignment = ParagraphAlignment.Center;
                //TODO - parameterise font style
                header.AddFormattedText(paper.Name, TextFormat.Bold);
            }
            else
            {
                var image = section.AddImage(paper.MainImage);
                //TODO - parameterise image scaling
                image.Width = section.Document.DefaultPageSetup.EffectivePageWidth - (section.PageSetup.LeftMargin + section.PageSetup.RightMargin) * 0.5;
                image.LockAspectRatio = true;
                image.Left = (-section.PageSetup.LeftMargin + section.PageSetup.RightMargin) * 0.25;
            }
        }

        private void AddNewspaperEdition(Section section, DateTime timestamp)
        {
            var edition = CreateParagraph(section);
            edition.Format.Alignment = ParagraphAlignment.Right;
            //TODO - parameterise font and style
            edition.Format.Font.Size = 16;
            edition.AddFormattedText($"{timestamp.ToString("ddd dd MMM yyyy")} Edition");
            CreateParagraph(section);
        }

        private void AddCategories(Section section, Dictionary<string, List<Article>> categorisedArticles)
        {
            foreach (var pair in categorisedArticles)
            {
                if(pair.Value.Count == 0)
                {
                    continue;
                }

                AddSection(section,pair.Key, pair.Value);
                section = CreateSection(section.Document);
            }
        }

        private void AddSection(Section section, string sectionName, List<Article> articles)
        {
            AddSectionCategoryHeader(section, sectionName);

            foreach (var article in articles)
            {
                AddArticle(section, article);
                CreateParagraph(section);
            }
        }

        private void AddSectionCategoryHeader(Section section, string sectionName)
        {
            if(string.IsNullOrEmpty(sectionName))
            {
                return;
            }

            var sectionHeader = CreateParagraph(section);
            //TODO - parameterise style
            sectionHeader.Format.Font.Size = 20;
            sectionHeader.AddFormattedText(sectionName, TextFormat.Bold);
            sectionHeader.Format.Alignment = ParagraphAlignment.Center;
            CreateParagraph(section);
        }

        private void AddArticle(Section section, Article article)
        {
            AddArticleImage(section,article.Thumbnail);
            AddArticleTitle(section, article.Title);
            AddArticleSource(section, article.Author, article.Source);

            foreach(var block in article.Content)
            {
                if(block is TextBlock text)
                {
                    AddArticleContent(section, text.Text);
                }
                else if (block is ImageUrlBlock image)
                {
                    AddArticleImage(section, image.Url);
                }
                else if(block is TableBlock table)
                {
                    AddArticleTable(section, table);
                }
            }
        }

        private void AddArticleTable(Section section, TableBlock tableBlock)
        {
            var table = section.AddTable();
            table.KeepTogether = true;
            table.Borders.Width = 0.25;
            var columns = tableBlock.GetMaxColumns();
            var columnSize = (section.Document.DefaultPageSetup.PageWidth - (section.PageSetup.LeftMargin + section.PageSetup.RightMargin)) / columns;
            while (table.Columns.Count < columns)
            {
                var column = table.AddColumn();
                column.Width = columnSize;
            }

            foreach(var tableRow in tableBlock.Content)
            {
                var row = table.AddRow();

                for(var i = 0; i < tableRow.Count; ++i)
                {
                    var paragraph = row[i].AddParagraph(tableRow[i]);
                    //TODO - parameterise size
                    paragraph.Format.Font.Size = 14;
                }
            }
        }

        private void AddArticleImage(Section section, string imageUrl)
        {
            if (!RenderImages || string.IsNullOrEmpty(imageUrl) || !File.Exists(imageUrl))
            {
                return;
            }
            try
            {
                var maxWidth = section.Document.DefaultPageSetup.EffectivePageWidth - (section.PageSetup.LeftMargin + section.PageSetup.RightMargin);
                var maxHeight = section.Document.DefaultPageSetup.EffectivePageHeight - (section.PageSetup.TopMargin + section.PageSetup.BottomMargin) * 0.5;

                var localUrl = DownloadImage(imageUrl, maxWidth, maxHeight);

               
            }
            catch
            {

            }
        }

        private string DownloadImage(string url, Unit maxWidth, Unit maxHeight)
        {
            var localUrl = string.Empty;
            //TODO - this is reused from source, could possibly collate into a function
            var httpClient = new HttpClient()
            {
                Timeout = TimeSpan.FromMilliseconds(IMAGE_URL_TIMEOUT)
            };
            var message = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
            };

            var response = httpClient.Send(message);

            var stream = response.Content.ReadAsStream();

            using (var bitmap = new Bitmap(stream))
            {
                var xScale = 1.0;
                var yScale = 1.0;

                if (bitmap.Width > maxWidth.Point)
                {
                    xScale = maxWidth.Point / (double)bitmap.Width;
                }

                if (bitmap.Height > maxHeight.Point)
                {
                    yScale = maxHeight.Point / (double)bitmap.Height;
                }

                var scale = Math.Min(xScale, yScale);
                var newWidth = (double)bitmap.Width * scale;
                var newHeight = (double)bitmap.Height * scale;

                using (var newImage = new Bitmap(bitmap, new Size((int)newWidth, (int)newHeight)))
                {
                    using (var graphics = Graphics.FromImage(newImage))
                    {
                        graphics.DrawImage(bitmap, new Rectangle(0, 0, newImage.Width, newImage.Height), new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
                    }

                    localUrl = Path.Combine(IMAGE_HOLDING_DIRECTORY, Guid.NewGuid().ToString() + ".jpeg");
                    SaveImage(localUrl, newImage, IMAGE_QUALITY);
                }
            }

            return localUrl;
        }

        private void SaveImage(string filename, Bitmap image, long qualty)
        {
            var qualityParameter = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, IMAGE_QUALITY);
            var codec = GetEncoderInfo("image/jpeg");

            if(codec == null)
            {
                throw new InvalidOperationException("Codec not found");
            }

            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = qualityParameter;

            image.Save(filename,codec,encoderParameters);
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(x => x.MimeType.ToLower() == mimeType);
        }

        private void AddArticleTitle(Section section, string title)
        {
            var paragraph = CreateParagraph(section);
            paragraph.Format.KeepWithNext = true;
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            // TODO - parameterise font and style
            paragraph.Format.Font.Size = 16;
            paragraph.AddFormattedText(title, TextFormat.Bold);
        }

        private void AddArticleSource(Section section, string author, string source)
        {
            var authorAndSource = string.Empty;

            if (!string.IsNullOrEmpty(author))
            {
                authorAndSource = author;
            }

            if (!string.IsNullOrEmpty(source))
            {
                if(!string.IsNullOrEmpty(authorAndSource))
                {
                    authorAndSource += " - ";
                }

                authorAndSource += source;
            }

            if(!string.IsNullOrEmpty(authorAndSource))
            {
                var paragraph = CreateParagraph(section);
                paragraph.Format.KeepWithNext = true;
                paragraph.Format.Alignment = ParagraphAlignment.Left;
                //TODO - parameterise formatting
                paragraph.AddFormattedText(authorAndSource.Trim(),TextFormat.Italic);
            }
        }

        private void AddArticleContent(Section section, string content)
        {
            CreateParagraph(section);
            var paragraph = CreateParagraph(section);
            //TODO - parameterise formatting
            paragraph.Format.Font.Size = 14;
            paragraph.AddFormattedText(content);
        }

        private Section CreateSection(Document document)
        {
            var result = document.AddSection();
            //TODO - parameterise margins
            result.PageSetup.BottomMargin = "1cm";
            result.PageSetup.TopMargin = "1cm";
            result.PageSetup.LeftMargin = "1cm";
            result.PageSetup.RightMargin = "1cm";

            return result;
        }

        private Paragraph CreateParagraph(Section section)
        {
            var result = section.AddParagraph();
            return result;
        }
    }
}
