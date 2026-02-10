using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace JsonCrudApp.Controllers
{
    public class ToolsController : BaseController
    {
        private readonly IWebHostEnvironment _env;
        private readonly Services.UserPdfService _pdfService;
        private readonly Services.JsonFileStudentService _studentService;

        public ToolsController(IWebHostEnvironment env, Services.UserPdfService pdfService, Services.JsonFileStudentService studentService)
        {
            _env = env;
            _pdfService = pdfService;
            _studentService = studentService;
        }

        private int GetCurrentUserId()
        {
            int? id = HttpContext.Session.GetInt32("Id");
            if (id.HasValue && id.Value > 0) return id.Value;

            string? email = HttpContext.Session.GetString("StudentUser");
            if (!string.IsNullOrEmpty(email))
            {
                var student = _studentService.GetStudents().FirstOrDefault(s => s.Email == email);
                if (student != null)
                {
                    HttpContext.Session.SetInt32("Id", student.Id);
                    return student.Id;
                }
            }
            return 0;
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToPdf(List<IFormFile> files, string targetFormat, bool mergeFiles, string pageSize, string orientation, string quality)
        {
            if (!IsPinVerified()) return Unauthorized(new { needsPin = true });

            if (files == null || files.Count == 0 || files.Sum(f => f.Length) == 0)
                return Json(new { success = false, message = "No files uploaded." });

            try
            {
                string tempDir = Path.Combine(_env.WebRootPath, "temp_conversions");
                if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

                string ext = targetFormat?.ToLower() == "jpg" ? ".zip" : ".pdf"; // If JPG and multiple, we might zip. If single, .jpg. 
                                                                                 // However, let's simplify: The requested "Merge" usually implies PDF. 
                                                                                 // If JPG is selected, we probably process each file. 

                // For this streamlined widget:
                // If PDF -> Merge all into one PDF (if merge=true or just default behavior for this widget?). 
                // The widget has a "Merge" checkbox.

                if (targetFormat == "jpg")
                {
                    // Simple JPG handling: If input is image, just copy/resize. If valid.
                    // We will just zip them if multiple, or return the single file.
                    // Since we can't easily convert Text/Doc to JPG without heavy libs (System.Drawing is limited/non-cross-platform), 
                    // we will restrict this to Image->JPG or just renaming.

                    if (files.Count == 1)
                    {
                        var f = files[0];
                        var validImgs = new[] { ".jpg", ".jpeg", ".png" };
                        if (!validImgs.Contains(Path.GetExtension(f.FileName).ToLower()))
                            return Json(new { success = false, message = "Conversion of strictly Documents to JPG is not supported without OCR libraries. Please upload Images." });

                        string outName = "Converted_" + DateTime.Now.Ticks + ".jpg";
                        string p = Path.Combine(tempDir, outName);
                        using (var s = new FileStream(p, FileMode.Create)) { await f.CopyToAsync(s); }

                        return Json(new { success = true, downloadUrl = $"/temp_conversions/{outName}", fileName = outName });
                    }
                    else
                    {
                        // Zip multiple images
                        string zipName = "Converted_Images_" + DateTime.Now.Ticks + ".zip";
                        string zipPath = Path.Combine(tempDir, zipName);
                        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                        {
                            foreach (var f in files)
                            {
                                var entry = archive.CreateEntry(f.FileName, CompressionLevel.Fastest);
                                using (var entryStream = entry.Open())
                                using (var fileStream = f.OpenReadStream())
                                {
                                    await fileStream.CopyToAsync(entryStream);
                                }
                            }
                        }
                        return Json(new { success = true, downloadUrl = $"/temp_conversions/{zipName}", fileName = zipName });
                    }
                }

                // PDF Path
                string newFileName = "Converted_" + DateTime.Now.Ticks + ".pdf";
                string outputPath = Path.Combine(tempDir, newFileName);

                byte[] pdfBytes = await SimplePdfGenerator.GeneratePdf(files, pageSize, orientation);

                await System.IO.File.WriteAllBytesAsync(outputPath, pdfBytes);

                await System.IO.File.WriteAllBytesAsync(outputPath, pdfBytes);

                // --- SAVE METADATA ---
                int userId = GetCurrentUserId();
                string publicUrl = $"/temp_conversions/{newFileName}";
                int newPdfId = 0;

                if (userId > 0)
                {
                    var pdfModel = new Models.UserPdf
                    {
                        UserId = userId,
                        FileName = newFileName,
                        FilePath = publicUrl,
                        CreatedAt = DateTime.Now
                    };
                    _pdfService.AddPdf(pdfModel);
                    newPdfId = pdfModel.Id;
                }

                return Json(new
                {
                    success = true,
                    downloadUrl = publicUrl,
                    fileName = newFileName,
                    id = newPdfId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Conversion failed: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetUserPdfs()
        {
            if (!IsPinVerified()) return Unauthorized(new { needsPin = true });

            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            var pdfs = _pdfService.GetUserPdfs(userId);
            return Json(pdfs);
        }

        [HttpPost]
        public IActionResult RenameUserPdf([FromBody] Models.UserPdf model)
        {
            if (!IsPinVerified()) return Unauthorized(new { needsPin = true });
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            _pdfService.UpdatePdfName(model.Id, userId, model.FileName);
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteUserPdf([FromBody] int id)
        {
            if (!IsPinVerified()) return Unauthorized(new { needsPin = true });
            int userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            _pdfService.DeletePdf(id, userId);
            return Ok();
        }
    }

    // Minimal PDF Generator (No external libraries)
    public static class SimplePdfGenerator
    {
        public static async Task<byte[]> GeneratePdf(List<IFormFile> files, string pageSize = "A4", string orientation = "Portrait")
        {
            // Dimensions
            double pgW = 595.0;
            double pgH = 842.0;

            if (pageSize == "Letter") { pgW = 612.0; pgH = 792.0; }
            if (orientation == "Landscape") { double t = pgW; pgW = pgH; pgH = t; }
            // PDF Object tracking
            List<long> offsets = new List<long>();
            List<string> pageRefs = new List<string>(); // Store "X 0 R" for each page object

            // Helper to add object and track offset (Function removed as unused)

            // We need a dummy offset for the first zero object
            offsets.Add(0); // This will be fixed at final write but for now we need a placeholder index 0

            // 1. Catalog (Will be Object 1)
            // 2. Pages (Will be Object 2) - we need to know children first generally, or update later.
            //    We'll reserve spots 1 and 2.

            // Actually, simplest way with linear writer: 
            // Write Objects 3+ (Pages & Resources), then write 1 & 2 at start? 
            // No, PDF is seekable. We can write objects in any order if XREF is correct.
            // Let's write pages first, then index them.

            // To handle multiple pages properly we iterate files.

            // We start object numbering at 3.
            // 1 = Catalog, 2 = Pages Root
            // 1 = Catalog, 2 = Pages Root

            // We will build the body string which contains objects 3....N
            // And we need to track their offsets relative to the START of the file.
            // So we can't write to body yet without knowing header size.

            // Better approach: Use a MemoryStream to write everything sequentially.

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms, Encoding.ASCII))
            {
                writer.Write("%PDF-1.4\n");
                writer.Flush();
                long startOffset = ms.Position;

                // Function to get current offset
                long GetPos() => ms.Position;

                // We'll store objects as we generate them, then write all at once? 
                // Or write sequentially and track offsets.

                // Let's reserve object IDs.
                int catalogId = 1;
                int pagesRootId = 2;
                int currentId = 3;

                // We need to buffer Page Objects to write them after we know their resource IDs (images etc)
                // Actually, resources can be referenced before definition.

                List<int> pageIds = new List<int>();

                // Define Font Object (Standard Helvetica)
                int fontId = currentId++;

                // Store Key Objects to write later
                var objectBodies = new Dictionary<int, byte[]>();

                // Font Body
                string fontBodyStr = $"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>";
                objectBodies[fontId] = Encoding.ASCII.GetBytes(fontBodyStr);

                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file.FileName).ToLower();
                    int pageId = currentId++;
                    pageIds.Add(pageId);

                    int contentId = currentId++;

                    if (IsImage(ext))
                    {
                        // Image Page
                        using (var imgStream = new MemoryStream())
                        {
                            await file.CopyToAsync(imgStream);
                            byte[] imgBytes = imgStream.ToArray();

                            int imgObjId = currentId++;

                            // 1. Get Image Dimensions
                            var (w, h) = GetJpegDimensions(imgBytes);

                            // 2. Create Image Object
                            string imgDict = $"<< /Type /XObject /Subtype /Image /Width {w} /Height {h} /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {imgBytes.Length} >>\nstream\n";

                            // Combine dict + binary + endstream
                            var header = Encoding.ASCII.GetBytes(imgDict);
                            var footer = Encoding.ASCII.GetBytes("\nendstream");

                            byte[] fullImgObj = new byte[header.Length + imgBytes.Length + footer.Length];
                            Buffer.BlockCopy(header, 0, fullImgObj, 0, header.Length);
                            Buffer.BlockCopy(imgBytes, 0, fullImgObj, header.Length, imgBytes.Length);
                            Buffer.BlockCopy(footer, 0, fullImgObj, header.Length + imgBytes.Length, footer.Length);

                            objectBodies[imgObjId] = fullImgObj;

                            // 3. Create Content Stream (Scale to fit Page)
                            // Margin ~40pts.
                            double margin = 40.0;
                            double maxWidth = pgW - (margin * 2);
                            double maxHeight = pgH - (margin * 2);

                            double scale = Math.Min(maxWidth / w, maxHeight / h);
                            // Do not upscale small images, only downscale
                            if (scale > 1.0) scale = 1.0;

                            int finalW = (int)(w * scale);
                            int finalH = (int)(h * scale);
                            int x = (int)((pgW - finalW) / 2); // Center X
                            int y = (int)((pgH - finalH) / 2); // Center Y

                            // Correct Q/q stack for scaling
                            string contentStr = $"q {finalW} 0 0 {finalH} {x} {y} cm /Im{imgObjId} Do Q";

                            string contentObjStr = $"<< /Length {contentStr.Length} >>\nstream\n{contentStr}\nendstream";
                            objectBodies[contentId] = Encoding.ASCII.GetBytes(contentObjStr);

                            // 4. Create Page Object
                            string pageStr = $"<< /Type /Page /Parent {pagesRootId} 0 R /MediaBox [0 0 {pgW} {pgH}] /Contents {contentId} 0 R /Resources << /XObject << /Im{imgObjId} {imgObjId} 0 R >> >> >>";
                            objectBodies[pageId] = Encoding.ASCII.GetBytes(pageStr);
                        }
                    }
                    else
                    {
                        // Text Page (Doc/Txt)
                        string text = "";
                        if (ext == ".docx") text = ExtractTextFromDocx(file.OpenReadStream());
                        else
                        {
                            using (var r = new StreamReader(file.OpenReadStream())) text = await r.ReadToEndAsync();
                        }

                        // Sanitize & Format
                        string formattedContent = FormatTextToPdfStream(text);

                        string contentObjStr = $"<< /Length {formattedContent.Length} >>\nstream\n{formattedContent}\nendstream";
                        objectBodies[contentId] = Encoding.ASCII.GetBytes(contentObjStr);

                        string pageStr = $"<< /Type /Page /Parent {pagesRootId} 0 R /MediaBox [0 0 {pgW} {pgH}] /Contents {contentId} 0 R /Resources << /Font << /F1 {fontId} 0 R >> >> >>";
                        objectBodies[pageId] = Encoding.ASCII.GetBytes(pageStr);
                    }
                }

                // NOW WRITE EVERYTHING

                // Track actual offsets for XREF
                Dictionary<int, long> finalOffsets = new Dictionary<int, long>();

                // Helper to write object
                void WriteObj(int id, byte[] data)
                {
                    finalOffsets[id] = GetPos();
                    writer.Write($"{id} 0 obj\n");
                    writer.Flush();
                    ms.Write(data, 0, data.Length);
                    writer.Write("\nendobj\n");
                    writer.Flush();
                }

                // 1. Catalog
                string catalogStr = $"<< /Type /Catalog /Pages {pagesRootId} 0 R >>";
                WriteObj(catalogId, Encoding.ASCII.GetBytes(catalogStr));

                // 2. Pages Root
                string kids = string.Join(" ", pageIds.Select(id => $"{id} 0 R"));
                string pagesStr = $"<< /Type /Pages /Kids [{kids}] /Count {pageIds.Count} >>";
                WriteObj(pagesRootId, Encoding.ASCII.GetBytes(pagesStr));

                // 3. All other objects (sorted by ID for neatness)
                foreach (var kvp in objectBodies.OrderBy(x => x.Key))
                {
                    WriteObj(kvp.Key, kvp.Value);
                }

                // XREF
                long xrefPos = GetPos();
                writer.Write("xref\n");
                writer.Write($"0 {currentId}\n"); // 0 to MaxID
                writer.Write("0000000000 65535 f \n"); // Object 0

                for (int i = 1; i < currentId; i++)
                {
                    if (finalOffsets.ContainsKey(i))
                        writer.Write($"{finalOffsets[i].ToString("D10")} 00000 n \n");
                    else
                        writer.Write("0000000000 00000 f \n"); // Should not happen
                }

                // Trailer
                writer.Write($"trailer\n<< /Size {currentId} /Root {catalogId} 0 R >>\n");
                writer.Write($"startxref\n{xrefPos}\n%%EOF\n");
                writer.Flush();

                return ms.ToArray();
            }
        }

        private static (int width, int height) GetJpegDimensions(byte[] bytes)
        {
            // Minimal JPEG parser to find SOF0/SOF2
            try
            {
                int i = 0;
                if (bytes.Length < 2 || bytes[i++] != 0xFF || bytes[i++] != 0xD8) return (500, 500); // Not JPEG

                while (i < bytes.Length - 1)
                {
                    // Find next marker
                    while (i < bytes.Length && bytes[i] != 0xFF) i++;
                    if (i >= bytes.Length - 1) break;
                    i++;

                    byte marker = bytes[i++];
                    if (marker == 0x00 || (marker >= 0xD0 && marker <= 0xD9)) continue; // Stuffing, Restart, EOI

                    // Length
                    if (i + 2 > bytes.Length) break;
                    int len = (bytes[i] << 8) | bytes[i + 1];

                    // SOF0 (Baseline) = C0, SOF2 (Progressive) = C2
                    if (marker == 0xC0 || marker == 0xC2)
                    {
                        if (i + 2 + 1 + 4 > bytes.Length) break;
                        // Structure: [Len 2] [Precision 1] [Height 2] [Width 2]
                        int h = (bytes[i + 2 + 1] << 8) | bytes[i + 2 + 2];
                        int w = (bytes[i + 2 + 3] << 8) | bytes[i + 2 + 4];
                        return (w, h);
                    }

                    i += len;
                }
            }
            catch { }
            return (500, 500); // Fallback
        }

        private static string FormatTextToPdfStream(string text)
        {
            // Simple text formatting: Escape chars, split lines
            var sb = new StringBuilder();
            sb.AppendLine("BT /F1 12 Tf 50 800 Td 14 TL");

            // Clean text
            text = text.Replace("\r\n", "\n");

            int y = 800;
            foreach (var line in text.Split('\n'))
            {
                // Simple wrapping (very basic, char count based)
                string remaining = line;
                while (remaining.Length > 0)
                {
                    int chunkLen = Math.Min(90, remaining.Length);
                    string chunk = remaining.Substring(0, chunkLen);
                    remaining = remaining.Substring(chunkLen);

                    // Escape PDF special chars
                    chunk = chunk.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

                    sb.AppendLine($"({chunk}) Tj T*");
                    y -= 14;
                    if (y < 50)
                    {
                        // Page break needed ideally, but for this simple version we just stop or wrap?
                        // Multi-page text not implemented in this 'Simple' version, we just clip.
                        // Ideally we would create a new Page object.
                        break;
                    }
                }
                if (y < 50) break;
            }
            sb.AppendLine("ET");
            return sb.ToString();
        }

        private static bool IsImage(string ext)
        {
            return ext == ".jpg" || ext == ".jpeg";
        }

        private static string ExtractTextFromDocx(Stream stream)
        {
            try
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var entry = archive.GetEntry("word/document.xml");
                    if (entry != null)
                    {
                        using (var reader = new StreamReader(entry.Open()))
                        {
                            string xml = reader.ReadToEnd();
                            var doc = Regex.Replace(xml, "<.*?>", " ");
                            return System.Net.WebUtility.HtmlDecode(doc);
                        }
                    }
                }
            }
            catch { }
            return "Could not extract text.";
        }
    }
}
