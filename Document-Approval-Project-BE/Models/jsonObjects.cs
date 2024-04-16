using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Document_Approval_Project_BE.Models
{
    public class jsonObjects
    {
        #nullable enable
        public string? document { get; set; }

        public string? password { get; set; }

        public string? zoomFactor { get; set; }

        public string? isFileName { get; set; }

        public string? xCoordinate { get; set; }

        public string? yCoordinate { get; set; }

        public string? pageNumber { get; set; }

        public string? documentId { get; set; }

        public string? hashId { get; set; }

        public string? sizeX { get; set; }

        public string? sizeY { get; set; }

        public string? startPage { get; set; }

        public string? endPage { get; set; }

        public string? stampAnnotations { get; set; }

        public string? textMarkupAnnotations { get; set; }

        public string? stickyNotesAnnotation { get; set; }

        public string? shapeAnnotations { get; set; }

        public string? measureShapeAnnotations { get; set; }

        public string? action { get; set; }

        public string? pageStartIndex { get; set; }

        public string? pageEndIndex { get; set; }

        public string? fileName { get; set; }
        public string? filePath { get; set; }
        public string? elementId { get; set; }

        public string? pdfAnnotation { get; set; }

        public string? importPageList { get; set; }

        public string? uniqueId { get; set; }

        public string? data { get; set; }

        public string? viewPortWidth { get; set; }

        public string? viewportHeight { get; set; }

        public string? tilecount { get; set; }

        public string? isCompletePageSizeNotReceived { get; set; }

        public string? freeTextAnnotation { get; set; }

        public string? signatureData { get; set; }

        public string? fieldsData { get; set; }

        public string? FormDesigner { get; set; }

        public string? inkSignatureData { get; set; }

    }
}