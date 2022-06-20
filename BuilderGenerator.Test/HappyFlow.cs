namespace BuilderGenerator.Test
{
    using Microsoft.CodeAnalysis.Text;
    using System.Text;    
    using VerifyCS = CSharpSourceGeneratorVerifier<BuilderGenerator.BuilderPatternGenerator>;

    [TestClass]
    public class HappyFlow
    {
        [TestMethod]
        public async Task Poc()
        {
            var code = @"
namespace MyCode
{
    using System;
    using System.Collections.Generic;

    public class Config : IBuilderGeneratorConfig
    {
        public List<Type> GetTypesToCreateBuildersFor => new List<Type> { typeof(Entity) };
    }

    public class Entity {
        public int Test {get;set;}
    }

    public interface IBuilderGeneratorConfig {
        List<Type> GetTypesToCreateBuildersFor { get; }
    }
}";
            var generated = @"namespace MyCode
{
    public class EntityBuilder
    {
        private int _test;

        public EntityBuilder WithTest(int test)
        {
            _test = test;
            return this;
        }

        public Entity Build()
        {
            return new Entity
            {
                Test = _test,
            };
        }
    }
}
";
            await new VerifyCS.Test
            {
                TestState =
                {
                    Sources = { code },
                    GeneratedSources =
                    {
                        (typeof(BuilderGenerator.BuilderPatternGenerator), "EntityBuilder.g.cs", SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha1)),
                    },
                },
            }.RunAsync();
        }
    }
}