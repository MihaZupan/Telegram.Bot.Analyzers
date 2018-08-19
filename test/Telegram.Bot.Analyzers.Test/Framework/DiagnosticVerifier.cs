using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telegram.Bot.Analyzers.Test.Framework
{
    public abstract partial class DiagnosticVerifier
    {
        public static readonly DiagnosticAnalyzer Analyzer = new TelegramBotAnalyzers();

        protected void VerifyDiagnostic(string source, params DiagnosticResult[] expected)
        {
            VerifyDiagnostics(new[] { source }, expected);
        }

        protected void VerifyDiagnostics(string[] sources, params DiagnosticResult[] expected)
        {
            var diagnostics = GetSortedDiagnostics(sources);
            VerifyDiagnosticResults(diagnostics, expected);
        }

        private void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, params DiagnosticResult[] expectedResults)
        {
            int expectedCount = expectedResults.Count();
            int actualCount = actualResults.Count();

            if (expectedCount != actualCount)
            {
                string diagnosticsOutput = actualResults.Any() ? FormatDiagnostics(actualResults.ToArray()) : "    NONE.";

                Assert.IsTrue(false,
                    string.Format("Mismatch between number of diagnostics returned, expected \"{0}\" actual \"{1}\"\r\n\r\nDiagnostics:\r\n{2}\r\n", expectedCount, actualCount, diagnosticsOutput));
            }

            for (int i = 0; i < expectedResults.Length; i++)
            {
                var actual = actualResults.ElementAt(i);
                var expected = expectedResults[i];

                if (expected.Line == -1 && expected.Column == -1)
                {
                    if (actual.Location != Location.None)
                    {
                        Assert.IsTrue(false,
                            string.Format("Expected:\nA project diagnostic with No location\nActual:\n{0}",
                            FormatDiagnostics(actual)));
                    }
                }
                else
                {
                    VerifyDiagnosticLocation(actual, actual.Location, expected.Locations.First());
                    var additionalLocations = actual.AdditionalLocations.ToArray();

                    if (additionalLocations.Length != expected.Locations.Length - 1)
                    {
                        Assert.IsTrue(false,
                            string.Format("Expected {0} additional locations but got {1} for Diagnostic:\r\n    {2}\r\n",
                                expected.Locations.Length - 1, additionalLocations.Length,
                                FormatDiagnostics(actual)));
                    }

                    for (int j = 0; j < additionalLocations.Length; ++j)
                    {
                        VerifyDiagnosticLocation(actual, additionalLocations[j], expected.Locations[j + 1]);
                    }
                }

                if (actual.Id != expected.Id)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic id to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Id, actual.Id, FormatDiagnostics(actual)));
                }

                if (actual.Severity != expected.Severity)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic severity to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Severity, actual.Severity, FormatDiagnostics(actual)));
                }

                if (actual.GetMessage() != expected.Message)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic message to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Message, actual.GetMessage(), FormatDiagnostics(actual)));
                }
            }
        }

        private void VerifyDiagnosticLocation(Diagnostic diagnostic, Location actual, DiagnosticResultLocation expected)
        {
            var actualSpan = actual.GetLineSpan();

            Assert.IsTrue(actualSpan.Path == expected.Path || (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") && expected.Path.Contains("Test.")),
                string.Format("Expected diagnostic to be in file \"{0}\" was actually in file \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                    expected.Path, actualSpan.Path, FormatDiagnostics(diagnostic)));

            var actualLinePosition = actualSpan.StartLinePosition;

            // Only check line position if there is an actual line in the real diagnostic
            if (actualLinePosition.Line > 0)
            {
                if (actualLinePosition.Line + 1 != expected.Line)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic to be on line \"{0}\" was actually on line \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Line, actualLinePosition.Line + 1, FormatDiagnostics(diagnostic)));
                }
            }

            // Only check column position if there is an actual column position in the real diagnostic
            if (actualLinePosition.Character > 0)
            {
                if (actualLinePosition.Character + 1 != expected.Column)
                {
                    Assert.IsTrue(false,
                        string.Format("Expected diagnostic to start at column \"{0}\" was actually at column \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
                            expected.Column, actualLinePosition.Character + 1, FormatDiagnostics(diagnostic)));
                }
            }
        }

        private static string FormatDiagnostics(params Diagnostic[] diagnostics)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < diagnostics.Length; ++i)
            {
                builder.AppendLine("// " + diagnostics[i].ToString());

                var analyzerType = Analyzer.GetType();
                var rules = Analyzer.SupportedDiagnostics;

                foreach (var rule in rules)
                {
                    if (rule != null && rule.Id == diagnostics[i].Id)
                    {
                        var location = diagnostics[i].Location;
                        if (location == Location.None)
                        {
                            builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
                        }
                        else
                        {
                            Assert.IsTrue(location.IsInSource,
                                $"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}\r\n");

                            string resultMethodName = diagnostics[i].Location.SourceTree.FilePath.EndsWith(".cs") ? "GetCSharpResultAt" : "GetBasicResultAt";
                            var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

                            builder.AppendFormat("{0}({1}, {2}, {3}.{4})",
                                resultMethodName,
                                linePosition.Line + 1,
                                linePosition.Character + 1,
                                analyzerType.Name,
                                rule.Id);
                        }

                        if (i != diagnostics.Length - 1)
                        {
                            builder.Append(',');
                        }

                        builder.AppendLine();
                        break;
                    }
                }
            }
            return builder.ToString();
        }
    }
}
