using System.Text;
using Unity.GraphToolkit.Editor;

namespace Misaki.TextureMaker.CodeGen
{
    /// <summary>
    /// Interface for nodes that can generate executable code
    /// </summary>
    public interface ICodeGeneratable
    {
        /// <summary>
        /// Generate code for this node's execution logic
        /// </summary>
        /// <param name="context">Code generation context</param>
        /// <param name="nodeId">Unique identifier for this node instance</param>
        void GenerateCode(ICodeGenContext context, string nodeId);

        /// <summary>
        /// Get the data structure fields needed for this node
        /// </summary>
        /// <param name="context">Code generation context</param>
        /// <param name="nodeId">Unique identifier for this node instance</param>
        void GenerateDataFields(ICodeGenContext context, string nodeId);

        /// <summary>
        /// Generate initialization code for node data
        /// </summary>
        /// <param name="context">Code generation context</param>
        /// <param name="nodeId">Unique identifier for this node instance</param>
        void GenerateDataInitialization(ICodeGenContext context, string nodeId);

        /// <summary>
        /// Generate preparation code that runs before the job (e.g., loading textures)
        /// </summary>
        /// <param name="context">Code generation context</param>
        /// <param name="nodeId">Unique identifier for this node instance</param>
        void GeneratePrepareCode(ICodeGenContext context, string nodeId);

        /// <summary>
        /// Generate finalization code that runs after the job (e.g., saving textures)
        /// </summary>
        /// <param name="context">Code generation context</param>
        /// <param name="nodeId">Unique identifier for this node instance</param>
        void GenerateFinalizeCode(ICodeGenContext context, string nodeId);
    }

    /// <summary>
    /// Code generation context that provides utilities and state
    /// </summary>
    public interface ICodeGenContext
    {
        StringBuilder Code { get; }
        StringBuilder DataStructure { get; }
        StringBuilder Initialization { get; }
        StringBuilder PrepareCode { get; }
        StringBuilder FinalizeCode { get; }
        StringBuilder HelperClass { get; }

        // Variable management with IPort support
        string DeclareVariable(string type, string name, string value = null);
        string GetInputVariable(IPort port);
        string GetOutputVariable(IPort port);
        string GetDataFieldName(string nodeId, string fieldName);

        // Code generation utilities
        void AddUsing(string namespaceName);
        void AddLine(string line = null);
        void AddDataField(string type, string name, string comment = null);
        void AddInitializationLine(string line);
        void AddPrepareLine(string line);
        void AddFinalizeLine(string line);
        void AddHelperLine(string line);
        void AddBlock(string blockStart, System.Action blockContent);

        // Performance optimizations
        void CacheConstant(string name, string value);
        string GetCachedConstant(string name);

        // Job system support
        void MarkAsJobCompatible();
        bool IsJobCompatible { get; }

        // Port variable mapping
        void RegisterOutputVariable(IPort port, string variableName);
        string GetConnectedOutputVariable(IPort inputPort);

        // New methods for improved code generation
        void SetOutputNodeInfo(string nodeId, string className);
        string GetOutputNodeClassName(string nodeId);
        void AddJobStruct(string nodeId, string jobStructCode);
    }
}