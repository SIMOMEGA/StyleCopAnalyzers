﻿namespace StyleCop.Analyzers.SpacingRules
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// A line within a documentation header above a C# element does not begin with a single space.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when a line within a documentation header does not begin with a single
    /// space. For example:</para>
    ///
    /// <code language="cs">
    /// ///&lt;summary&gt;
    /// ///The summary text.
    /// ///&lt;/summary&gt;
    /// ///   &lt;param name="x"&gt;The document root.&lt;/param&gt;
    /// ///    &lt;param name="y"&gt;The Xml header token.&lt;/param&gt;
    /// private void Method1(int x, int y)
    /// {
    /// }
    /// </code>
    ///
    /// <para>The header lines should begin with a single space after the three leading forward slashes:</para>
    ///
    /// <code language="cs">
    /// /// &lt;summary&gt;
    /// /// The summary text.
    /// /// &lt;/summary&gt;
    /// /// &lt;param name="x"&gt;The document root.&lt;/param&gt;
    /// /// &lt;param name="y"&gt;The Xml header token.&lt;/param&gt;
    /// private void Method1(int x, int y)
    /// {
    /// }
    /// </code>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1004DocumentationLinesMustBeginWithSingleSpace : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SA1004";
        internal const string Title = "Documentation Lines Must Begin With Single Space";
        internal const string MessageFormat = "Documentation line must begin with a space.";
        internal const string Category = "StyleCop.CSharp.SpacingRules";
        internal const string Description = "A line within a documentation header above a C# element does not begin with a single space.";
        internal const string HelpLink = "http://www.stylecop.com/docs/SA1004.html";

        public static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.DisabledNoTests, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return _supportedDiagnostics;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(HandleSyntaxTree);
        }

        private void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
            foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
            {
                switch (trivia.CSharpKind())
                {
                case SyntaxKind.DocumentationCommentExteriorTrivia:
                    HandleDocumentationCommentExteriorTrivia(context, trivia);
                    break;

                default:
                    break;
                }
            }
        }

        private void HandleDocumentationCommentExteriorTrivia(SyntaxTreeAnalysisContext context, SyntaxTrivia trivia)
        {
            SyntaxToken token = trivia.Token;
            if (token.IsMissing)
                return;

            switch (token.CSharpKind())
            {
            case SyntaxKind.EqualsToken:
            case SyntaxKind.DoubleQuoteToken:
            case SyntaxKind.SingleQuoteToken:
            case SyntaxKind.IdentifierToken:
            case SyntaxKind.GreaterThanToken:
            case SyntaxKind.SlashGreaterThanToken:
            case SyntaxKind.LessThanToken:
            case SyntaxKind.LessThanSlashToken:
            case SyntaxKind.XmlCommentStartToken:
            case SyntaxKind.XmlCommentEndToken:
            case SyntaxKind.XmlCDataStartToken:
            case SyntaxKind.XmlCDataEndToken:
                if (!token.HasLeadingTrivia)
                    break;

                SyntaxTrivia lastLeadingTrivia = token.LeadingTrivia.Last();
                switch (lastLeadingTrivia.CSharpKind())
                {
                case SyntaxKind.WhitespaceTrivia:
                    if (lastLeadingTrivia.ToFullString().StartsWith(" "))
                        return;

                    break;

                case SyntaxKind.DocumentationCommentExteriorTrivia:
                    if (lastLeadingTrivia.ToFullString().EndsWith(" "))
                        return;

                    break;

                default:
                    break;
                }

                break;

            case SyntaxKind.EndOfDocumentationCommentToken:
            case SyntaxKind.XmlTextLiteralNewLineToken:
                return;

            case SyntaxKind.XmlTextLiteralToken:
                if (token.Text.StartsWith(" "))
                {
                    return;
                }
                else if (trivia.ToFullString().EndsWith(" "))
                {
                    // javadoc-style documentation comments without a leading * on one of the lines.
                    return;
                }

                break;

            default:
                break;
            }

            // Documentation line must begin with a space.
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, token.GetLocation()));
        }
    }
}
