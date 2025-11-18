using System;
using System.Linq;
using Unity.GraphToolkit.Editor;

namespace Misaki.TextureMaker
{
    [Serializable]
    internal abstract class MultiDimensionNode : CodeGenerationNode
    {
        [Flags]
        internal enum TargetDimension
        {
            Default = 0,
            Float = 1 << 0,
            Float2 = 1 << 1,
            Float3 = 1 << 2,
            Float4 = 1 << 3,
        }

        internal enum PortDimension
        {
            Float = TargetDimension.Float,
            Float2 = TargetDimension.Float2,
            Float3 = TargetDimension.Float3,
            Float4 = TargetDimension.Float4,
        }

        internal struct PortDeclaration
        {
            public string displayName;
            public ShaderVariableType valueType;
            public TargetDimension targetDimension;
        }

        protected const string DIMENSION_OPTION_NAME = "Dimension";

        private IPort[] _outputPort;
        private IPort[] _inputPorts;

        protected IPort[] InputPorts => _inputPorts;
        protected IPort[] OutputPorts => _outputPort;

        protected PortDimension Dimension => GetOptionValue<PortDimension>(DIMENSION_OPTION_NAME);
        protected int ComponentCount => Dimension switch
        {
            PortDimension.Float => 1,
            PortDimension.Float2 => 2,
            PortDimension.Float3 => 3,
            PortDimension.Float4 => 4,
            _ => 4
        };

        protected ShaderVariableType ValueType => ToShaderVariableType(GetOptionValue<PortDimension>(DIMENSION_OPTION_NAME));

        protected virtual PortDeclaration[] InputDeclarations
        {
            get;
        }

        protected virtual PortDeclaration[] OutputDeclarations
        {
            get;
        }

        private static ShaderVariableType ToShaderVariableType(PortDimension dataType)
        {
            return dataType switch
            {
                PortDimension.Float => ShaderVariableType.Float,
                PortDimension.Float2 => ShaderVariableType.Float2,
                PortDimension.Float3 => ShaderVariableType.Float3,
                PortDimension.Float4 => ShaderVariableType.Float4,
                _ => ShaderVariableType.Float4
            };
        }

        private static bool HasDimensionFlag(TargetDimension dimension, PortDimension flag)
        {
            if (dimension == TargetDimension.Default)
            {
                return true;
            }

            return (dimension & (TargetDimension)flag) != 0;
        }

        protected sealed override void OnDefinePorts(IPortDefinitionContext context)
        {
            var dimension = GetOptionValue<PortDimension>(DIMENSION_OPTION_NAME);

            var validInputs = InputDeclarations.Where(decl => HasDimensionFlag(decl.targetDimension, dimension)).ToArray();
            _inputPorts = new IPort[validInputs.Length];
            for (var i = 0; i < validInputs.Length; i++)
            {
                var decl = validInputs[i];
                _inputPorts[i] = context.AddInputPort(i.ToString()).WithDisplayName(decl.displayName).WithDataType(decl.valueType.ToType()).Build();
            }

            var validOutputs = OutputDeclarations.Where(decl => HasDimensionFlag(decl.targetDimension, dimension)).ToArray();
            _outputPort = new IPort[validOutputs.Length];
            for (var i = 0; i < validOutputs.Length; i++)
            {
                var decl = validOutputs[i];
                _outputPort[i] = context.AddOutputPort(i.ToString()).WithDisplayName(decl.displayName).WithDataType(decl.valueType.ToType()).Build();
            }
        }

        protected sealed override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PortDimension>(DIMENSION_OPTION_NAME).WithDefaultValue(PortDimension.Float4).ShowInInspectorOnly().Build();
            OnDefineNodeOptions(context);
        }

        protected virtual void OnDefineNodeOptions(IOptionDefinitionContext context)
        {
        }
    }
}