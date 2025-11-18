
using System;

namespace Misaki.TextureMaker
{
    [Serializable]
    internal abstract class OutputNode : CodeGenerationNode
    {
        public const string WIDTH_PORT_NAME = "width";
        public const string HEIGHT_PORT_NAME = "height";

        public int Width => GetOptionValue<int>(WIDTH_PORT_NAME);
        public int Height => GetOptionValue<int>(HEIGHT_PORT_NAME);

        protected sealed override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(WIDTH_PORT_NAME).WithDefaultValue(1024).Build();
            context.AddOption<int>(HEIGHT_PORT_NAME).WithDefaultValue(1024).Build();

            OnDefineNodeOptions(context);
        }

        protected virtual void OnDefineNodeOptions(IOptionDefinitionContext context)
        {
        }
    }
}