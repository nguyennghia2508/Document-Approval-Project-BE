using Document_Approval_Project_BE.Models;
using Newtonsoft.Json;
using Syncfusion.DocIO.DLS;
using Syncfusion.EJ2.PdfViewer;
using Syncfusion.Pdf.Parsing;
using Syncfusion.OfficeChartToImageConverter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Syncfusion.DocIO;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.OfficeChart;
using Syncfusion.Pdf.Graphics;
using System.Web.WebPages;
using System.Runtime.ExceptionServices;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/pdf-viewers")]
    public class FileUploadController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        private System.Web.HttpContext currentContext = System.Web.HttpContext.Current;

        [HttpPost]
        [Route("Load")]
        public IHttpActionResult Load([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            MemoryStream stream = new MemoryStream();
            try
            {
                object jsonResult = new object();

                if (jsonObject != null && (jsonObject.ContainsKey("document") || jsonObject.ContainsKey("documentId")))
                {
                    if (jsonObject.ContainsKey("documenApprovaltId") && jsonObject.ContainsKey("fileId"))
                    {
                        if (Guid.TryParse(jsonObject["documenApprovaltId"], out Guid documentApprovalId) &&
                        Guid.TryParse(jsonObject["fileId"], out Guid fileId))
                        {
                            var fileExist = db.DocumentApprovalFiles.FirstOrDefault(file =>
                                file.DocumentApprovalId == documentApprovalId && file.DocumentFileId == fileId);

                            if (fileExist != null)
                            {
                                string documentPath = GetDocumentPath(fileExist.FilePath);

                                if (!string.IsNullOrEmpty(documentPath) && File.Exists(documentPath))
                                {
                                    if (fileExist.FileType.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
                                    {
                                        WordDocument wordDocument = new WordDocument(documentPath, FormatType.Docx);
                                        wordDocument.ChartToImageConverter = new ChartToImageConverter();
                                        wordDocument.ChartToImageConverter.ScalingMode = ScalingMode.Normal;

                                        DocToPDFConverter converter = new DocToPDFConverter();
                                        converter.Settings.EnableFastRendering = true;

                                        PdfDocument pdfDocument = converter.ConvertToPDF(wordDocument);

                                        MemoryStream pdfStream = new MemoryStream();
                                        pdfDocument.Save(pdfStream);
                                        pdfDocument.Close(true);

                                        stream = new MemoryStream(pdfStream.ToArray());
                                    }
                                    else
                                    {
                                        using (FileStream fileStream = File.OpenRead(documentPath))
                                        {
                                            byte[] bytes = new byte[fileStream.Length];
                                            fileStream.Read(bytes, 0, (int)fileStream.Length);

                                            stream = new MemoryStream(bytes);
                                        }
                                    }
                                }
                                else
                                {
                                    return Content(HttpStatusCode.NotFound, jsonObject["document"] + " is not found");
                                }
                            }
                        }
                        jsonResult = pdfviewer.Load(stream, jsonObject);
                        return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
                    }
                    else
                    {
                        return Content(HttpStatusCode.BadRequest, "Invalid parameters provided");
                    }
                }
                else
                {
                    return Content(HttpStatusCode.NotFound, jsonObject["document"] + " is not found");
                }
            }
            catch (AccessViolationException avEx)
            {
                Console.WriteLine("Access violation exception occurred: " + avEx.Message);
                return InternalServerError(avEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("SaveDocument")]
        public IHttpActionResult SaveDocument([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();
                MemoryStream stream = new MemoryStream();

                if (jsonObject != null && jsonObject.ContainsKey("documentApprovalId") && jsonObject.ContainsKey("fileId"))
                {
                    if (Guid.TryParse(jsonObject["documentApprovalId"], out Guid documentApprovalId) &&
                        Guid.TryParse(jsonObject["fileId"], out Guid fileId))
                    {

                        var fileExist = db.DocumentApprovalFiles.FirstOrDefault(file =>
                            file.DocumentApprovalId == documentApprovalId && file.DocumentFileId == fileId);

                        var documentApproval = db.DocumentApprovals.FirstOrDefault(d => d.DocumentApprovalId == fileExist.DocumentApprovalId);
                        var personSigner = db.ApprovalPersons.Where(p => p.DocumentApprovalId == documentApproval.DocumentApprovalId
                        && p.PersonDuty == 2).ToList();

                        if (fileExist != null)
                        {
                            string Filepath = Path.Combine(HostingEnvironment.MapPath("~/"), "Upload\\Files", fileExist.DocumentApprovalId.ToString());

                            if (!Directory.Exists(Filepath))
                            {
                                Directory.CreateDirectory(Filepath);
                            }

                            string documentPath = GetDocumentPath(fileExist.FilePath);


                            if (!string.IsNullOrEmpty(documentPath) && File.Exists(documentPath))
                            {
                                if (fileExist.FileType.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document"))
                                {
                                    string fileName = Path.GetFileNameWithoutExtension(fileExist.FileName);

                                    string newFileName = fileName + "_" + DateTime.Now.Ticks.ToString() + ".pdf";

                                    string tempFilePath = Path.Combine(Filepath, newFileName);

                                    byte[] pdfBytes = Convert.FromBase64String(jsonObject["documentData"]);

                                    File.WriteAllBytes(tempFilePath, pdfBytes);

                                    fileExist.FileName = fileName + ".pdf";
                                    fileExist.FileType = "application/pdf";
                                    fileExist.FilePath = "Upload/Files/" +
                                        fileExist.DocumentApprovalId.ToString() + "/" + newFileName;

                                    db.SaveChanges();

                                    if (File.Exists(tempFilePath))
                                    {
                                        using (FileStream docStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.ReadWrite))
                                        {
                                            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                                            PdfLoadedForm form = loadedDocument.Form;
                                            if (form != null && form.Fields != null)
                                            {
                                                form.SetDefaultAppearance(false);
                                                PdfLoadedFormFieldCollection fieldCollection = form.Fields;
                                                if (fieldCollection.Count > 0)
                                                {
                                                    foreach (PdfLoadedField field in fieldCollection)
                                                    {
                                                        if (field.Name != null && field is PdfLoadedTextBoxField)
                                                        {
                                                            string fieldName = field.Name;

                                                            if (fieldName.StartsWith("[{RequestCode") && fieldName.EndsWith("}]"))
                                                            {
                                                                (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                (field as PdfLoadedTextBoxField).Text = documentApproval.RequestCode;
                                                            }

                                                            if (fieldName.StartsWith("[{Title") && fieldName.EndsWith("}]"))
                                                            {
                                                                (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                (field as PdfLoadedTextBoxField).Text = documentApproval.Subject;
                                                            }

                                                            //if (fieldName.StartsWith("[{Signature") && fieldName.EndsWith("}]"))
                                                            //{
                                                            //    (field as PdfLoadedTextBoxField).Text = "Signature";
                                                            //}

                                                            //if (fieldName.StartsWith("[{SignedDate") && fieldName.EndsWith("}]"))
                                                            //{
                                                            //    (field as PdfLoadedTextBoxField).Text = "Date";
                                                            //}

                                                            //if (fieldName.StartsWith("[{SignerJobTitle") && fieldName.EndsWith("}]"))
                                                            //{
                                                            //    (field as PdfLoadedTextBoxField).Text = "Title";
                                                            //}

                                                            if (fieldName.StartsWith("[{SignerName") && fieldName.EndsWith("}]"))
                                                            {
                                                                foreach (var person in personSigner)
                                                                {
                                                                    int startIndex = "[{SignerName".Length; // Độ dài của "[{SignerName"
                                                                    int dotIndex = fieldName.IndexOf('.', startIndex); // Tìm vị trí của dấu chấm (.) sau "SignerName"

                                                                    if (dotIndex != -1)
                                                                    {
                                                                        // Lấy phần số giữa "SignerName" và dấu chấm (.)
                                                                        string signerNumber = fieldName.Substring(startIndex, dotIndex - startIndex);

                                                                        if (int.TryParse(signerNumber, out int signerIndex))
                                                                        {
                                                                            if (signerIndex == person.Index)
                                                                            {
                                                                                (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                                (field as PdfLoadedTextBoxField).Text = person.ApprovalPersonName;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    form.SetDefaultAppearance(false);
                                                    loadedDocument.Save(docStream);
                                                    loadedDocument.Close(true);
                                                }
                                                else
                                                {
                                                    loadedDocument.Save(docStream);
                                                    loadedDocument.Close(true);
                                                }
                                            }
                                        }
                                        File.Delete(documentPath);

                                    }
                                    else
                                    {
                                        return Content(HttpStatusCode.NotFound, jsonObject["documentId"] + " is not found");
                                    }
                                }
                                else
                                {
                                    string fileName = Path.GetFileNameWithoutExtension(fileExist.FileName);

                                    string fileExtension = Path.GetExtension(fileExist.FileName);

                                    string alterPath = "Upload/Files/" +
                                        fileExist.DocumentApprovalId.ToString() + "/" + fileName + "_" + DateTime.Now.Ticks.ToString() + fileExtension;

                                    string fullPath = Path.Combine(HostingEnvironment.MapPath("~/"),alterPath);

                                    byte[] pdfBytes = Convert.FromBase64String(jsonObject["documentData"]);

                                    File.WriteAllBytes(fullPath, pdfBytes);

                                    if (File.Exists(fullPath))
                                    {
                                        using (FileStream docStream = new FileStream(fullPath, FileMode.Open, FileAccess.ReadWrite))
                                        {
                                            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                                            PdfLoadedForm form = loadedDocument.Form;
                                            if (form != null && form.Fields != null)
                                            {
                                                form.SetDefaultAppearance(false);
                                                PdfLoadedFormFieldCollection fieldCollection = form.Fields;

                                                if (fieldCollection.Count > 0)
                                                {
                                                    if (jsonObject.ContainsKey("isDelete") && jsonObject["isDelete"].AsBool())
                                                    {
                                                        for (int i = fieldCollection.Count - 1; i >= 0; i--)
                                                        {
                                                            PdfLoadedField field = (PdfLoadedField)fieldCollection[i];
                                                            fieldCollection.Remove(field);
                                                        }
                                                        loadedDocument.Save(docStream);
                                                        loadedDocument.Close(true);
                                                    }
                                                    else
                                                    {
                                                        foreach (PdfLoadedField field in fieldCollection)
                                                        {
                                                            if (field.Name != null && field is PdfLoadedTextBoxField)
                                                            {
                                                                string fieldName = field.Name;

                                                                if (fieldName.StartsWith("[{RequestCode") && fieldName.EndsWith("}]"))
                                                                {
                                                                    (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                    (field as PdfLoadedTextBoxField).Text = documentApproval.RequestCode;
                                                                }

                                                                if (fieldName.StartsWith("[{Title") && fieldName.EndsWith("}]"))
                                                                {
                                                                    (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                    (field as PdfLoadedTextBoxField).Text = documentApproval.Subject;
                                                                }

                                                                //if (fieldName.StartsWith("[{Signature") && fieldName.EndsWith("}]"))
                                                                //{
                                                                //    (field as PdfLoadedTextBoxField).Text = "Signature";
                                                                //}

                                                                //if (fieldName.StartsWith("[{SignedDate") && fieldName.EndsWith("}]"))
                                                                //{
                                                                //    (field as PdfLoadedTextBoxField).Text = "Date";
                                                                //}

                                                                //if (fieldName.StartsWith("[{SignerJobTitle") && fieldName.EndsWith("}]"))
                                                                //{
                                                                //    (field as PdfLoadedTextBoxField).Text = "Title";
                                                                //}

                                                                if (fieldName.StartsWith("[{SignerName") && fieldName.EndsWith("}]"))
                                                                {
                                                                    foreach (var person in personSigner)
                                                                    {
                                                                        int startIndex = "[{SignerName".Length; // Độ dài của "[{SignerName"
                                                                        int dotIndex = fieldName.IndexOf('.', startIndex); // Tìm vị trí của dấu chấm (.) sau "SignerName"

                                                                        if (dotIndex != -1)
                                                                        {
                                                                            // Lấy phần số giữa "SignerName" và dấu chấm (.)
                                                                            string signerNumber = fieldName.Substring(startIndex, dotIndex - startIndex);

                                                                            if (int.TryParse(signerNumber, out int signerIndex))
                                                                            {
                                                                                if (signerIndex == person.Index)
                                                                                {
                                                                                    (field as PdfLoadedTextBoxField).Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9, PdfFontStyle.Bold);
                                                                                    (field as PdfLoadedTextBoxField).Text = person.ApprovalPersonName;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        form.SetDefaultAppearance(false);
                                                        loadedDocument.Save(docStream);
                                                        loadedDocument.Close(true);
                                                    }
                                                }
                                                else
                                                {
                                                    loadedDocument.Close(true);
                                                }
                                            }
                                        }
                                        File.Delete(documentPath);
                                        fileExist.FilePath = alterPath;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        return Content(HttpStatusCode.NotFound, jsonObject["documentId"] + " is not found");
                                    }
                                }
                            }
                        }

                        return Content(HttpStatusCode.OK, new
                        {
                            state = "true",
                            documentApprovalId = fileExist.DocumentApprovalId,
                            fileId = fileExist.DocumentFileId,
                        });
                    }
                    else
                    {
                        return Content(HttpStatusCode.BadRequest, "Invalid parameters provided");
                    }
                }
                else
                {
                    return Content(HttpStatusCode.NotFound, jsonObject["fileName"] + " is not found");
                }
            }
            catch (AccessViolationException avEx)
            {
                Console.WriteLine("Access violation exception occurred: " + avEx.Message);
                return BadRequest("Error occurred during PDF rendering.");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        //public Dictionary<string, string> JsonConverter(jsonObjects results)
        //{
        //    Dictionary<string, object> resultObjects = new Dictionary<string, object>();
        //    resultObjects = results.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
        //        .ToDictionary(prop => prop.Name, prop => prop.GetValue(results, null));

        //    var emptyObjects = (from kv in resultObjects

        //                        where kv.Value != null
        //                        select kv).ToDictionary(kv => kv.Key, kv => kv.Value);

        //    Dictionary<string, string> jsonResult = emptyObjects.ToDictionary(k => k.Key, k => k.Value.ToString());

        //    return jsonResult;

        //}

        [HttpPost]
        [Route("ExportAnnotations")]
        public IHttpActionResult ExportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                string jsonResult = pdfviewer.ExportAnnotation(jsonObject);

                return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("ImportAnnotations")]
        public IHttpActionResult ImportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                string jsonResult = string.Empty;
                object JsonResult;

                if (jsonObject != null && jsonObject.ContainsKey("fileName"))
                {

                    string documentPath = GetDocumentPath(jsonObject["filePath"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        //Returns a string containing all the text in the file.
                        jsonResult = File.ReadAllText(documentPath);
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, jsonObject["document"] + " is not found");
                    }
                }
                else
                {
                    string extension = Path.GetExtension(jsonObject["importedData"]);
                    if (extension != ".xfdf")
                    {
                        JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                        return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
                    }
                    else
                    {
                        //Gets the path of the PDF document.
                        string documentPath = GetDocumentPath(jsonObject["importedData"]);
                        if (!string.IsNullOrEmpty(documentPath))
                        {
                            byte[] bytes = File.ReadAllBytes(documentPath);
                            //Returns the byte as base64 string.
                            jsonObject["importedData"] = Convert.ToBase64String(bytes);
                            //Gets the annotation from the document.
                            JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                            return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, jsonObject["document"] + " is not found");
                        }
                    }
                }
                return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("ImportFormFields")]
        public IHttpActionResult ImportFormFields([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                object jsonResult = pdfviewer.ImportFormFields(jsonObject);

                return Ok(JsonConvert.SerializeObject(jsonResult));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("ExportFormFields")]
        public IHttpActionResult ExportFormFields([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                string jsonResult = pdfviewer.ExportFormFields(jsonObject);

                return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("RenderPdfPages")]
        public IHttpActionResult RenderPdfPages([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                object jsonResult = pdfviewer.GetPage(jsonObject);

                return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
            }
            catch (AccessViolationException avEx)
            {
                Console.WriteLine("Access violation exception occurred: " + avEx.Message);
                return InternalServerError(avEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("Unload")]
        public IHttpActionResult Unload([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                pdfviewer.ClearCache(jsonObject);

                return Content(HttpStatusCode.OK, "Document cache is cleared");
            }
            catch (AccessViolationException avEx)
            {
                Console.WriteLine("Access violation exception occurred: " + avEx.Message);
                return InternalServerError(avEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [HttpPost]
        [Route("RenderThumbnailImages")]
        public IHttpActionResult RenderThumbnailImages([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                object result = pdfviewer.GetThumbnailImages(jsonObject);

                return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(result));
            }
            catch (AccessViolationException avEx)
            {
                Console.WriteLine("Access violation exception occurred: " + avEx.Message);
                return InternalServerError(avEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("Bookmarks")]
        public IHttpActionResult Bookmarks([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                object jsonResult = pdfviewer.GetBookmarks(jsonObject);

                return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
            }
            catch (AccessViolationException avEx)
            {
                Console.WriteLine("Access violation exception occurred: " + avEx.Message);
                return InternalServerError(avEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("RenderAnnotationComments")]
        public IHttpActionResult RenderAnnotationComments([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);

                return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
            }
            catch (AccessViolationException avEx)
            {
                Console.WriteLine("Access violation exception occurred: " + avEx.Message);
                return InternalServerError(avEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("Download")]
        public IHttpActionResult Download([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(documentBase, System.Text.Encoding.UTF8, "text/plain");

                return ResponseMessage(response);
            }
            catch (AccessViolationException avEx)
            {
                Console.WriteLine("Access violation exception occurred: " + avEx.Message);
                return InternalServerError(avEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [HttpPost]
        [Route("PrintImages")]
        public IHttpActionResult PrintImages([FromBody] Dictionary<string, string> jsonObject)
        {
            try
            {
                PdfRenderer pdfviewer = new PdfRenderer();

                object pageImage = pdfviewer.GetPrintImage(jsonObject);

                return Content(HttpStatusCode.OK, pageImage);
            }
            catch (AccessViolationException avEx)
            {
                Console.WriteLine("Access violation exception occurred: " + avEx.Message);
                return InternalServerError(avEx);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private string GetDocumentPath(string FilePath)
        {
            string rootPath = HostingEnvironment.MapPath("~/");
            string documentPath = Path.Combine(rootPath, FilePath);

            if (System.IO.File.Exists(documentPath))
            {
                return documentPath;
            }
            return string.Empty;

        }
    }
}
