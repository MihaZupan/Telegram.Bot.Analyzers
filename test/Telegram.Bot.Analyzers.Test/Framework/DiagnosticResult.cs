using Microsoft.CodeAnalysis;
using System;

namespace Telegram.Bot.Analyzers.Test.Framework
{
    public struct DiagnosticResultLocation
    {
        public DiagnosticResultLocation(string path, int line, int column)
        {
            Path = path;
            Line = line >= -1 ? line : throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
            Column = column >= -1 ? column : throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
        }

        public readonly string Path;
        public readonly int Line;
        public readonly int Column;
    }

    public struct DiagnosticResult
    {
        private DiagnosticResultLocation[] locations;
        public DiagnosticResultLocation[] Locations
        {
            get
            {
                if (locations == null)
                {
                    locations = new DiagnosticResultLocation[] { };
                }
                return locations;
            }
            set => locations = value;
        }

        public DiagnosticSeverity Severity;
        public string Id;
        public string Message;

        public string Path => Locations.Length > 0 ? Locations[0].Path : "";
        public int Line => Locations.Length > 0 ? Locations[0].Line : -1;
        public int Column => Locations.Length > 0 ? Locations[0].Column : -1;
    }
}
