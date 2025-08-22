using System;
using System.Collections.Generic;
using System.Text;
using Unity.GraphToolkit.Editor;

namespace Misaki.TextureMaker.CodeGen
{
    public class CodeGenContext : ICodeGenContext
    {
        public StringBuilder Code { get; } = new StringBuilder();
        public StringBuilder DataStructure { get; } = new StringBuilder();
        public StringBuilder Initialization { get; } = new StringBuilder();
        public StringBuilder PrepareCode { get; } = new StringBuilder();
        public StringBuilder FinalizeCode { get; } = new StringBuilder();
        public StringBuilder HelperClass { get; } = new StringBuilder();

        private readonly HashSet<string> _usings = new HashSet<string>();
        private readonly Dictionary<string, string> _variables = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _cachedConstants = new Dictionary<string, string>();
        private readonly Dictionary<IPort, string> _portVariableMapping = new Dictionary<IPort, string>();
        private readonly Dictionary<string, string> _dataFieldMappings = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _outputNodeClassNames = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _jobStructs = new Dictionary<string, string>();
        private int _variableCounter = 0;
        private int _codeIndentLevel = 0;
        private int _dataIndentLevel = 1; // Inside struct
        private int _initIndentLevel = 2; // Inside method
        private int _prepareIndentLevel = 2; // Inside method
        private int _finalizeIndentLevel = 2; // Inside method
        private int _helperIndentLevel = 1; // Inside class

        public bool IsJobCompatible { get; private set; } = true;

        public string DeclareVariable(string type, string name, string value = null)
        {
            var varName = name ?? $"var{_variableCounter++}";

            if (value != null)
            {
                AddLine($"{type} {varName} = {value};");
            }
            else
            {
                AddLine($"{type} {varName};");
            }

            _variables[varName] = type;
            return varName;
        }

        public string GetInputVariable(IPort port)
        {
            // If this input port is connected, get the variable from the connected output port
            if (port.isConnected && port.firstConnectedPort != null)
            {
                return GetConnectedOutputVariable(port);
            }

            // If not connected, check if we already have a variable for this port
            if (_portVariableMapping.TryGetValue(port, out var existingVar))
            {
                return existingVar;
            }

            // Create a new input variable for this port
            var hashCode = port.GetNode().GetHashCode();
            var varName = $"input_{hashCode:X}_{port.name}";
            _portVariableMapping[port] = varName;
            return varName;
        }

        public string GetOutputVariable(IPort port)
        {
            // Check if we already have a variable for this port
            if (_portVariableMapping.TryGetValue(port, out var existingVar))
            {
                return existingVar;
            }

            // Create a new output variable for this port
            var hashCode = port.GetNode().GetHashCode();
            var varName = $"output_{hashCode:X}_{port.name}";
            _portVariableMapping[port] = varName;
            return varName;
        }

        public string GetConnectedOutputVariable(IPort inputPort)
        {
            if (inputPort.isConnected && inputPort.firstConnectedPort != null)
            {
                var connectedOutputPort = inputPort.firstConnectedPort;
                if (_portVariableMapping.TryGetValue(connectedOutputPort, out var connectedVar))
                {
                    return connectedVar;
                }

                // If the connected output port doesn't have a variable yet, create one
                return GetOutputVariable(connectedOutputPort);
            }

            return null;
        }

        public void RegisterOutputVariable(IPort port, string variableName)
        {
            _portVariableMapping[port] = variableName;
        }

        public string GetDataFieldName(string nodeId, string fieldName)
        {
            var key = $"{nodeId}_{fieldName}";
            return _dataFieldMappings.GetValueOrDefault(key, $"{nodeId}_{fieldName}");
        }

        public void AddUsing(string namespaceName)
        {
            _usings.Add(namespaceName);
        }

        public void AddLine(string line = null)
        {
            if (string.IsNullOrEmpty(line))
            {
                Code.AppendLine();
                return;
            }

            Code.AppendLine(new string(' ', _codeIndentLevel * 4) + line);
        }

        public void AddDataField(string type, string name, string comment = null)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                DataStructure.AppendLine(new string(' ', _dataIndentLevel * 4) + $"// {comment}");
            }
            DataStructure.AppendLine(new string(' ', _dataIndentLevel * 4) + $"public {type} {name};");
        }

        public void AddInitializationLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                Initialization.AppendLine();
                return;
            }

            Initialization.AppendLine(new string(' ', _initIndentLevel * 4) + line);
        }

        public void AddPrepareLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                PrepareCode.AppendLine();
                return;
            }

            PrepareCode.AppendLine(new string(' ', _prepareIndentLevel * 4) + line);
        }

        public void AddFinalizeLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                FinalizeCode.AppendLine();
                return;
            }

            FinalizeCode.AppendLine(new string(' ', _finalizeIndentLevel * 4) + line);
        }

        public void AddHelperLine(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                HelperClass.AppendLine();
                return;
            }

            HelperClass.AppendLine(new string(' ', _helperIndentLevel * 4) + line);
        }

        public void AddBlock(string blockStart, Action blockContent)
        {
            AddLine(blockStart);
            AddLine("{");
            _codeIndentLevel++;
            blockContent();
            _codeIndentLevel--;
            AddLine("}");
        }

        public void CacheConstant(string name, string value)
        {
            _cachedConstants[name] = value;
        }

        public string GetCachedConstant(string name)
        {
            return _cachedConstants.GetValueOrDefault(name);
        }

        public void SetDataFieldMapping(string nodeId, string fieldName, string dataFieldName)
        {
            _dataFieldMappings[$"{nodeId}_{fieldName}"] = dataFieldName;
        }

        public void MarkAsJobCompatible()
        {
            IsJobCompatible = true;
        }

        public void SetOutputNodeInfo(string nodeId, string className)
        {
            _outputNodeClassNames[nodeId] = className;
        }

        public string GetOutputNodeClassName(string nodeId)
        {
            return _outputNodeClassNames.GetValueOrDefault(nodeId, $"TextureExecution_{nodeId}");
        }

        public void AddJobStruct(string nodeId, string jobStructCode)
        {
            _jobStructs[nodeId] = jobStructCode;
        }

        public string GenerateFullCode(string className = "GeneratedTextureExecution")
        {
            var fullCode = new StringBuilder();

            // Add usings
            foreach (var usingDirective in _usings)
            {
                fullCode.AppendLine($"using {usingDirective};");
            }
            fullCode.AppendLine();

            // Add namespace and class
            fullCode.AppendLine("namespace Misaki.TextureMaker.Generated");
            fullCode.AppendLine("{");

            // Add data structure - Remove the non-existent NativeContainerAttribute
            fullCode.AppendLine("    [System.Serializable]");
            fullCode.AppendLine("    public struct TextureExecutionData");
            fullCode.AppendLine("    {");
            fullCode.Append(DataStructure.ToString());
            fullCode.AppendLine("    }");
            fullCode.AppendLine();

            // Add main execution class
            fullCode.AppendLine($"    public static class {className}");
            fullCode.AppendLine("    {");

            // Add initialization method
            fullCode.AppendLine("        public static TextureExecutionData Initialize()");
            fullCode.AppendLine("        {");
            fullCode.AppendLine("            var data = new TextureExecutionData();");
            fullCode.Append(Initialization.ToString());
            fullCode.AppendLine("            return data;");
            fullCode.AppendLine("        }");
            fullCode.AppendLine();

            // Add prepare method
            if (PrepareCode.Length > 0)
            {
                fullCode.AppendLine("        public static void Prepare(ref TextureExecutionData data)");
                fullCode.AppendLine("        {");
                fullCode.Append(PrepareCode.ToString());
                fullCode.AppendLine("        }");
                fullCode.AppendLine();
            }

            // Add execution method
            fullCode.AppendLine("        public static UnityEngine.Color ExecutePixel(UnityEngine.Vector2 uv, ref TextureExecutionData data)");
            fullCode.AppendLine("        {");
            fullCode.Append(Code.ToString());
            fullCode.AppendLine("        }");
            fullCode.AppendLine();

            // Add finalize method
            if (FinalizeCode.Length > 0)
            {
                fullCode.AppendLine("        public static void Finalize(ref TextureExecutionData data)");
                fullCode.AppendLine("        {");
                fullCode.Append(FinalizeCode.ToString());
                fullCode.AppendLine("        }");
                fullCode.AppendLine();
            }

            // Add job structs for each output node
            foreach (var jobStruct in _jobStructs.Values)
            {
                fullCode.AppendLine(jobStruct);
                fullCode.AppendLine();
            }

            fullCode.AppendLine("    }");

            // Add helper classes
            if (HelperClass.Length > 0)
            {
                fullCode.AppendLine();
                fullCode.Append(HelperClass.ToString());
            }

            fullCode.AppendLine("}");

            return fullCode.ToString();
        }
    }
}