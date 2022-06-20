using System;
using System.Collections.Generic;

namespace BuilderGenerator
{
    public interface IBuilderGeneratorConfig
    {
        List<Type> GetTypesToCreateBuildersFor { get; }
    }
}
