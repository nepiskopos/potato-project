using System.Text;

namespace PotatoProject.Endpoints
{
    public sealed record PostValidateSignatureRequest
    {
        [FastEndpoints.BindFrom("pdfFile")]
        public required IFormFile PdfFile { get; set; }
    }
    public sealed record PostValidateSignatureResponse
    {
        public required bool result { get; set; }
    }

    public sealed class PostValidateSignatureFastEndpoint : FastEndpoints.Endpoint<PostValidateSignatureRequest, PostValidateSignatureResponse>
    {
        private readonly AutoMapper.IMapper _mapper;

        public PostValidateSignatureFastEndpoint(AutoMapper.IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<iText.Kernel.Pdf.PdfDocument?> CreatePdfFromUploadedFile(IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                await formFile.CopyToAsync(ms);
                var fileBytes = ms.ToArray();
                string s = Convert.ToBase64String(fileBytes);
                byte[] bytes = Convert.FromBase64String(s);
                // act on the Base64 binary data

                StringBuilder text = new StringBuilder();

                MemoryStream memory = new MemoryStream(bytes);
                BinaryReader BRreader = new BinaryReader(memory);

                iText.Kernel.Pdf.PdfReader iTextReader = new iText.Kernel.Pdf.PdfReader(memory);
                iText.Kernel.Pdf.PdfDocument pdfDoc = new iText.Kernel.Pdf.PdfDocument(iTextReader);

                int numberofpages = pdfDoc.GetNumberOfPages();
                for (int page = 1; page <= numberofpages; page++)
                {
                    iText.Kernel.Pdf.Canvas.Parser.Listener.ITextExtractionStrategy strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();
                    string currentText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);
                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(
                        Encoding.Default, 
                        Encoding.UTF8, 
                        Encoding.Default.GetBytes(currentText)
                    ));
                    text.Append(currentText);
                }

                return pdfDoc;
            }
        }

        public async Task<bool> isPdfDigitallySigned(iText.Kernel.Pdf.PdfDocument pdfDocument)
        {
            return false;
        }

        public override void Configure()
        {
            Post("/api/checkSignedPdf");
            //Allow file uploads for this endpoint
            AllowFileUploads(dontAutoBindFormData: false);
            // Allow anonymous access to the endpoint
            AllowAnonymous();
        }

        public override async Task HandleAsync(PostValidateSignatureRequest r, CancellationToken c)
        {
            if (Files.Count < 1)
            {
                Console.WriteLine("No files");
                Serilog.Log.Information("No files");
                await SendErrorsAsync((int)System.Net.HttpStatusCode.UnprocessableEntity, cancellation: c);
                return;
            }

            iText.Kernel.Pdf.PdfDocument pdfDocument = await CreatePdfFromUploadedFile(Files[0]);
            if (pdfDocument is null)
            {
                Console.WriteLine("NULL file");
                Serilog.Log.Information("NULL file");
                await SendErrorsAsync((int)System.Net.HttpStatusCode.UnprocessableEntity, cancellation: c);
                return;
            }


            iText.Forms.PdfAcroForm acroForm = iText.Forms.PdfAcroForm.GetAcroForm(pdfDocument, false);
            if (acroForm is null || acroForm.GetAllFormFields().Count == 0)
            {
                Console.WriteLine("No signatures found in the PDF.");
                Serilog.Log.Information("No signatures found in the PDF.");
                await SendErrorsAsync((int)System.Net.HttpStatusCode.UnprocessableEntity, cancellation: c);
                return;
            }

            bool verification = await isPdfDigitallySigned(pdfDocument);
        }
    }
}