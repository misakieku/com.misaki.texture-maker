using Misaki.TextureMaker.CodeGen;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace Misaki.TextureMaker
{
    internal class Shuffle : TextureExecutableNode
    {
        private IPort _inputPort;
        private IPort _outputPort;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            _inputPort = context.AddInputPort<Color>("Input").Build();
            _outputPort = context.AddOutputPort<Color>("Output").Build();
        }

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<TextureChannel>("R Channel").WithDefaultValue(TextureChannel.R).Build();
            context.AddOption<TextureChannel>("G Channel").WithDefaultValue(TextureChannel.G).Build();
            context.AddOption<TextureChannel>("B Channel").WithDefaultValue(TextureChannel.B).Build();
            context.AddOption<TextureChannel>("A Channel").WithDefaultValue(TextureChannel.A).Build();
        }

        public override void GenerateCode(ICodeGenContext context, string nodeId)
        {
            context.AddUsing("Unity.Mathematics");
            context.AddUsing("UnityEngine");
            
            var inputVar = context.GetInputVariable(_inputPort);
            var outputVar = context.GetOutputVariable(_outputPort);
            
            var rChannelField = context.GetDataFieldName(nodeId, "rChannel");
            var gChannelField = context.GetDataFieldName(nodeId, "gChannel");
            var bChannelField = context.GetDataFieldName(nodeId, "bChannel");
            var aChannelField = context.GetDataFieldName(nodeId, "aChannel");
            
            context.AddLine($"// Shuffle Node {nodeId}");
            
            // Declare input variable if not connected
            if (!_inputPort.isConnected)
            {
                var inputType = GetPortTypeName(_inputPort.dataType);
                context.DeclareVariable(inputType, inputVar, "Color.black");
            }
            
            context.AddLine($"var inputFloat4_{nodeId} = new Unity.Mathematics.float4({inputVar}.r, {inputVar}.g, {inputVar}.b, {inputVar}.a);");
            context.AddLine($"var shuffledFloat4_{nodeId} = new Unity.Mathematics.float4(");
            context.AddLine($"    GeneratedCodeHelpers.GetChannelValue(inputFloat4_{nodeId}, data.{rChannelField}),");
            context.AddLine($"    GeneratedCodeHelpers.GetChannelValue(inputFloat4_{nodeId}, data.{gChannelField}),");
            context.AddLine($"    GeneratedCodeHelpers.GetChannelValue(inputFloat4_{nodeId}, data.{bChannelField}),");
            context.AddLine($"    GeneratedCodeHelpers.GetChannelValue(inputFloat4_{nodeId}, data.{aChannelField})");
            context.AddLine($");");
            context.AddLine($"var {outputVar} = new UnityEngine.Color(shuffledFloat4_{nodeId}.x, shuffledFloat4_{nodeId}.y, shuffledFloat4_{nodeId}.z, shuffledFloat4_{nodeId}.w);");
            
            // Register the output variable
            context.RegisterOutputVariable(_outputPort, outputVar);
        }

        public override void GenerateDataFields(ICodeGenContext context, string nodeId)
        {
            var rChannelField = context.GetDataFieldName(nodeId, "rChannel");
            var gChannelField = context.GetDataFieldName(nodeId, "gChannel");
            var bChannelField = context.GetDataFieldName(nodeId, "bChannel");
            var aChannelField = context.GetDataFieldName(nodeId, "aChannel");
            
            context.AddDataField("int", rChannelField, $"R channel source for {nodeId}");
            context.AddDataField("int", gChannelField, $"G channel source for {nodeId}");
            context.AddDataField("int", bChannelField, $"B channel source for {nodeId}");
            context.AddDataField("int", aChannelField, $"A channel source for {nodeId}");
        }

        public override void GenerateDataInitialization(ICodeGenContext context, string nodeId)
        {
            var rChannel = GetOptionValue<TextureChannel>("R Channel");
            var gChannel = GetOptionValue<TextureChannel>("G Channel");
            var bChannel = GetOptionValue<TextureChannel>("B Channel");
            var aChannel = GetOptionValue<TextureChannel>("A Channel");
            
            var rChannelField = context.GetDataFieldName(nodeId, "rChannel");
            var gChannelField = context.GetDataFieldName(nodeId, "gChannel");
            var bChannelField = context.GetDataFieldName(nodeId, "bChannel");
            var aChannelField = context.GetDataFieldName(nodeId, "aChannel");
            
            context.AddInitializationLine($"data.{rChannelField} = {(int)rChannel}; // {rChannel}");
            context.AddInitializationLine($"data.{gChannelField} = {(int)gChannel}; // {gChannel}");
            context.AddInitializationLine($"data.{bChannelField} = {(int)bChannel}; // {bChannel}");
            context.AddInitializationLine($"data.{aChannelField} = {(int)aChannel}; // {aChannel}");
        }
    }
}