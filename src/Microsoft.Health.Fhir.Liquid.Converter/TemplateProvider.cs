// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotLiquid;
using DotLiquid.FileSystems;
using Microsoft.Health.Fhir.Liquid.Converter.DotLiquids;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Utilities;

namespace Microsoft.Health.Fhir.Liquid.Converter
{
    public class TemplateProvider : ITemplateProvider
    {
        private readonly IFhirConverterTemplateFileSystem _fileSystem;
        private readonly string _templateDirectory;
        private readonly bool _isDefaultTemplateProvider = false;

        public TemplateProvider(string templateDirectory)
        {
            _fileSystem = new TemplateLocalFileSystem(templateDirectory);
            _templateDirectory = templateDirectory;
        }

        public TemplateProvider(string templateDirectory, DataType dataType)
        {
            _fileSystem = new TemplateLocalFileSystem(templateDirectory);
            _templateDirectory = templateDirectory;
        }

        public TemplateProvider(List<Dictionary<string, Template>> templateCollection, bool isDefaultTemplateProvider = false)
        {
            _fileSystem = new MemoryFileSystem(templateCollection);
            _isDefaultTemplateProvider = isDefaultTemplateProvider;
        }

        public bool IsDefaultTemplateProvider => _isDefaultTemplateProvider;

        public Template GetTemplate(string templateName)
        {
            return _fileSystem.GetTemplate(templateName);
        }

        public ITemplateFileSystem GetTemplateFileSystem()
        {
            return _fileSystem;
        }

        public IEnumerable<string> ListAllTemplates()
        {
            var results = new List<string>();

            // All .liquid templates (root or nested)
            var liquidFiles = Directory.GetFiles(_templateDirectory, "*.liquid", SearchOption.AllDirectories);

            foreach (var file in liquidFiles)
            {
                var relative = Path.GetRelativePath(_templateDirectory, file);
                var cleaned = ConvertPathToTemplateKey(relative);
                results.Add(cleaned);
            }

            // JSON templates (code mappings, schemas)
            var jsonFiles = Directory.GetFiles(_templateDirectory, "*.json", SearchOption.AllDirectories);

            foreach (var file in jsonFiles)
            {
                var relative = Path.GetRelativePath(_templateDirectory, file);
                var cleaned = ConvertPathToTemplateKey(relative);
                results.Add(cleaned);
            }

            return results
                .Distinct()
                .OrderBy(x => x);
        }

        private string ConvertPathToTemplateKey(string relativePath)
        {
            // Normalize to forward slashes
            relativePath = relativePath.Replace("\\", "/");

            // JSON schema templates → leave exactly as-is
            if (TemplateUtility.IsJsonSchemaTemplate(relativePath))
            {
                return relativePath;
            }

            // Liquid template processing
            if (relativePath.EndsWith(".liquid", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Substring(0, relativePath.Length - ".liquid".Length);

                var segments = relativePath.Split('/');

                // If the last segment starts with "_", strip the underscore → snippet
                var last = segments[^1];
                if (last.StartsWith("_"))
                {
                    segments[^1] = last.Substring(1);
                }

                return string.Join("/", segments);
            }

            // JSON code mapping templates
            if (relativePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return relativePath.Substring(0, relativePath.Length - ".json".Length);
            }

            // Fallback: return raw
            return relativePath;
        }
    }
}
