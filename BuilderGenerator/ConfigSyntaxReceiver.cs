using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace BuilderGenerator
{
    class ConfigSyntaxReceiver : ISyntaxReceiver
    {
        public ClassDeclarationSyntax ClassToAugment { get; private set; }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax cds && HasInterface(cds, nameof(IBuilderGeneratorConfig)))
            {
                ClassToAugment = cds;
            }
        }

        public bool HasInterface(ClassDeclarationSyntax source, string interfaceName)
        {
            IEnumerable<BaseTypeSyntax> baseTypes = source.BaseList?.Types.Select(baseType => baseType);
            return baseTypes != null && baseTypes.Any(baseType => baseType.ToString() == interfaceName);
        }
    }
}
