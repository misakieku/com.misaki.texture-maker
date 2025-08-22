using Misaki.GraphProcessor.Editor;
using Misaki.TextureMaker.CodeGen;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal interface ITextureExecutable : INode
    {
        // Remove the old Execute method as we'll use job-based execution
    }

    internal abstract class TextureExecutableNode : DataNode, ITextureExecutable, ICodeGeneratable
    {
        // Remove the old Execute method

        public virtual void GenerateCode(ICodeGenContext context, string nodeId)
        {
            // Default implementation - can be overridden for custom behavior
            GenerateDefaultCode(context, nodeId);
        }

        public virtual void GenerateDataFields(ICodeGenContext context, string nodeId)
        {
            // Default: no additional data fields needed
            // Override in derived classes that need cached data
        }

        public virtual void GenerateDataInitialization(ICodeGenContext context, string nodeId)
        {
            // Default: no initialization needed
            // Override in derived classes that need to initialize cached data
        }

        public virtual void GeneratePrepareCode(ICodeGenContext context, string nodeId)
        {
            // Default: no preparation needed
            // Override in derived classes that need preparation (e.g., ImportTexture2D)
        }

        public virtual void GenerateFinalizeCode(ICodeGenContext context, string nodeId)
        {
            // Default: no finalization needed
            // Override in derived classes that need finalization (e.g., ExportTexture2d)
        }

        protected virtual void GenerateDefaultCode(ICodeGenContext context, string nodeId)
        {
            context.AddLine($"// Node: {GetType().Name} (ID: {nodeId})");
            
            // Generate input and output variables for all ports
            foreach (var port in GetInputPorts())
            {
                var inputVar = context.GetInputVariable(port);
                if (!port.isConnected)
                {
                    var portType = GetPortTypeName(port.dataType);
                    var defaultValue = GetDefaultValue(port);
                    context.DeclareVariable(portType, inputVar, defaultValue);
                }
            }

            foreach (var port in GetOutputPorts())
            {
                var outputVar = context.GetOutputVariable(port);
                var portType = GetPortTypeName(port.dataType);
                var defaultValue = GetDefaultOutputValue(port.dataType);
                context.DeclareVariable(portType, outputVar, defaultValue);
                context.RegisterOutputVariable(port, outputVar);
            }
            
            // Generate the execution logic (override in derived classes)
            GenerateExecutionCode(context, nodeId);
            
            context.AddLine();
        }

        protected virtual void GenerateExecutionCode(ICodeGenContext context, string nodeId)
        {
            // Default: fallback message
            context.AddLine($"// Fallback to runtime execution for {GetType().Name}");
            context.AddLine($"// TODO: Implement GenerateExecutionCode for better performance");
        }

        protected string GetDefaultValue(IPort port)
        {
            var portType = port.dataType;
            if (portType == typeof(Color))
                return "Color.black";
            if (portType == typeof(float))
                return "0f";
            if (portType == typeof(Vector2))
                return "Vector2.zero";
            if (portType == typeof(int))
                return "0";
            return "default";
        }

        protected string GetDefaultOutputValue(System.Type portType)
        {
            if (portType == typeof(Color))
                return "Color.black";
            if (portType == typeof(float))
                return "0f";
            if (portType == typeof(Vector2))
                return "Vector2.zero";
            if (portType == typeof(int))
                return "0";
            return "default";
        }

        protected string GetPortTypeName(System.Type portType)
        {
            if (portType == typeof(Color)) return "UnityEngine.Color";
            if (portType == typeof(float)) return "float";
            if (portType == typeof(Vector2)) return "UnityEngine.Vector2";
            if (portType == typeof(TextureData)) return "TextureData";
            if (portType == typeof(int)) return "int";
            return portType.Name;
        }
    }
}