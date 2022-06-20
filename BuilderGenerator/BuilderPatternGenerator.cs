using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace BuilderGenerator
{
    [Generator]
    public class BuilderPatternGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (ConfigSyntaxReceiver)context.SyntaxReceiver;

            var configClass = receiver.ClassToAugment;

            if (configClass != null)
            {
                var property = configClass.Members.OfType<PropertyDeclarationSyntax>().FirstOrDefault(x => x.Identifier.Text == nameof(IBuilderGeneratorConfig.GetTypesToCreateBuildersFor));

                if(property != null && property.ExpressionBody != null && property.ExpressionBody.Expression is ObjectCreationExpressionSyntax obj && obj.Initializer != null)
                {
                    var expressions = obj.Initializer.Expressions;

                    foreach (var expression in expressions.OfType<TypeOfExpressionSyntax>())
                    {
                        var model = context.Compilation.GetSemanticModel(expression.Type.SyntaxTree);
                        var symbol = model.GetSymbolInfo(expression.Type);
                        if (symbol.Symbol != null)
                        {
                            var namedType = symbol.Symbol as INamedTypeSymbol;
                            var properties = namedType.GetMembers().OfType<IPropertySymbol>();
                            SyntaxNodeHelpers.TryGetParentSyntax(configClass, out NamespaceDeclarationSyntax namespaceDeclarationSyntax);
                            var sourceText = new IndentedStringBuilder();

                            sourceText
                                .AppendLine($"namespace {namespaceDeclarationSyntax.Name}")
                                .AppendLine("{");

                            using (sourceText.Indent())
                            {
                                sourceText.AppendLine($"public class {GetBuilderName(namedType)}")
                                    .AppendLine("{");

                                using (sourceText.Indent())
                                {
                                    foreach (var prop in properties)
                                    {
                                        sourceText.AppendLine($"private {prop.Type} _{prop.Name.ToLower()};");

                                        sourceText
                                            .AppendLine()
                                            .AppendLine($"public {GetBuilderName(namedType)} With{prop.Name}({prop.Type} {prop.Name.ToLower()})")
                                            .AppendLine("{");

                                        using (sourceText.Indent())
                                        {
                                            sourceText
                                                .AppendLine($"_{prop.Name.ToLower()} = {prop.Name.ToLower()};")
                                                .AppendLine("return this;");
                                        }

                                        sourceText
                                            .AppendLine("}")
                                            .AppendLine();
                                    }

                                    sourceText                                        
                                        .AppendLine($"public {namedType.Name} Build()")
                                        .AppendLine("{");

                                    using (sourceText.Indent())
                                    {
                                        sourceText
                                            .AppendLine($"return new {namedType.Name}")
                                            .AppendLine("{");

                                        using (sourceText.Indent())
                                        {
                                            foreach(var prop in properties)
                                            {
                                                sourceText.AppendLine($"{prop.Name} = _{prop.Name.ToLower()},");
                                            }
                                        }

                                        sourceText.AppendLine("};");
                                    }

                                    sourceText.AppendLine("}");
                                }

                                sourceText.AppendLine("}");
                            }

                            sourceText.AppendLine("}");

                            context.AddSource($"{GetBuilderName(namedType)}.g.cs", sourceText.ToString());
                        }
                    }                    
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ConfigSyntaxReceiver());
        }

        private string GetBuilderName(INamedTypeSymbol namedType)
        {
            return $"{namedType.Name}Builder";
        }
    }
}
