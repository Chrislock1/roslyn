﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Completion.Providers
{
    internal class TypeImportCompletionItem : CompletionItem
    {
        public int TypeArity { get; }

        public int OverloadCount { get; }

        public string ContainingNamespace { get; }

        private string _metadataName = null;
        public string MetadataName
        {
            get
            {
                if (_metadataName == null)
                {
                    _metadataName = ComposeAritySuffixedMetadataName(
                       GetFullyQualifiedName(this.ContainingNamespace, this.DisplayText),
                       this.TypeArity);
                }

                return _metadataName;
            }
        }

        public object CachedObject { get; set; }

        public static TypeImportCompletionItem Create(INamedTypeSymbol typeSymbol, string containingNamespace, int overloadCount)
        {
            return new TypeImportCompletionItem(
                 displayText: typeSymbol.Name,
                 displayTextSuffix: typeSymbol.Arity == 0 ? null : "<>",
                 glyph: typeSymbol.GetGlyph(),
                 inlineDescription: containingNamespace,
                 typeArity: typeSymbol.Arity,
                 containingNamespace: containingNamespace,
                 overloadCount: overloadCount);
        }

        protected override CompletionItem With(
            Optional<TextSpan> span = default,
            Optional<string> displayText = default,
            Optional<string> filterText = default,
            Optional<string> sortText = default,
            Optional<ImmutableDictionary<string, string>> properties = default,
            Optional<ImmutableArray<string>> tags = default,
            Optional<CompletionItemRules> rules = default,
            Optional<string> displayTextPrefix = default,
            Optional<string> displayTextSuffix = default,
            Optional<string> inlineDescription = default)
        {
            var newSpan = span.HasValue ? span.Value : this.Span;
            var newDisplayText = displayText.HasValue ? displayText.Value : this.DisplayText;
            var newFilterText = filterText.HasValue ? filterText.Value : this.FilterText;
            var newSortText = sortText.HasValue ? sortText.Value : this.SortText;
            var newInlineDescription = inlineDescription.HasValue ? inlineDescription.Value : this.InlineDescription;
            var newProperties = properties.HasValue ? properties.Value : this.Properties;
            var newTags = tags.HasValue ? tags.Value : this.Tags;
            var newRules = rules.HasValue ? rules.Value : this.Rules;
            var newDisplayTextPrefix = displayTextPrefix.HasValue ? displayTextPrefix.Value : this.DisplayTextPrefix;
            var newDisplayTextSuffix = displayTextSuffix.HasValue ? displayTextSuffix.Value : this.DisplayTextSuffix;

            if (newSpan == this.Span &&
                newDisplayText == this.DisplayText &&
                newFilterText == this.FilterText &&
                newSortText == this.SortText &&
                newProperties == this.Properties &&
                newTags == this.Tags &&
                newRules == this.Rules &&
                newDisplayTextPrefix == this.DisplayTextPrefix &&
                newDisplayTextSuffix == this.DisplayTextSuffix &&
                newInlineDescription == this.InlineDescription)
            {
                return this;
            }

            return new TypeImportCompletionItem(
                displayText: newDisplayText,
                filterText: newFilterText,
                span: newSpan,
                sortText: newSortText,
                properties: newProperties,
                tags: newTags,
                rules: newRules,
                displayTextPrefix: newDisplayTextPrefix,
                displayTextSuffix: newDisplayTextSuffix,
                inlineDescription: newInlineDescription,
                typeArity: this.TypeArity,
                containingNamespace: this.ContainingNamespace,
                overloadCount: this.OverloadCount);
        }

        private TypeImportCompletionItem(
            string displayText,
            string filterText,
            string sortText,
            TextSpan span,
            ImmutableDictionary<string, string> properties,
            ImmutableArray<string> tags,
            CompletionItemRules rules,
            string displayTextPrefix,
            string displayTextSuffix,
            string inlineDescription,
            int typeArity,
            string containingNamespace,
            int overloadCount)
            : base(displayText: displayText, filterText: filterText, sortText: sortText, span: span,
                   properties: properties, tags: tags, rules: rules, displayTextPrefix: displayTextPrefix,
                   displayTextSuffix: displayTextSuffix, inlineDescription: inlineDescription)
        {
            TypeArity = typeArity;
            ContainingNamespace = containingNamespace;
            OverloadCount = overloadCount;
        }

        private TypeImportCompletionItem(
            string displayText,
            string displayTextSuffix,
            Glyph glyph,
            int typeArity,
            string containingNamespace,
            string inlineDescription = null,
            int overloadCount = 0)
            : this(
                displayText: displayText,
                filterText: displayText,
                sortText: displayText,
                span: default,
                properties: null,
                tags: GlyphTags.GetTags(glyph),
                rules: CompletionItemRules.Default,
                displayTextPrefix: null,
                displayTextSuffix: displayTextSuffix,
                inlineDescription: inlineDescription,
                typeArity: typeArity,
                containingNamespace: containingNamespace,
                overloadCount: overloadCount)
        {
        }

        private const string GenericTypeNameManglingString = "`";
        private static readonly string[] s_aritySuffixesOneToNine = { "`1", "`2", "`3", "`4", "`5", "`6", "`7", "`8", "`9" };

        private static string GetAritySuffix(int arity)
        {
            Debug.Assert(arity > 0);
            return (arity <= 9) ? s_aritySuffixesOneToNine[arity - 1] : string.Concat(GenericTypeNameManglingString, arity.ToString(CultureInfo.InvariantCulture));
        }

        private static string GetFullyQualifiedName(string namespaceName, string typeName)
        {
            if (namespaceName.Length == 0)
            {
                return typeName;
            }
            else
            {
                return namespaceName + "." + typeName;
            }
        }

        private static string ComposeAritySuffixedMetadataName(string name, int arity)
        {
            return arity == 0 ? name : name + GetAritySuffix(arity);
        }
    }
}
