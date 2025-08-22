namespace Misaki.TextureMaker
{
    internal abstract class OutputNode : TextureExecutableNode
    {
        public const string WIDTH_PORT_NAME = "width";
        public const string HEIGHT_PORT_NAME = "height";

        protected sealed override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<int>(WIDTH_PORT_NAME).WithDefaultValue(1024).Build();
            context.AddInputPort<int>(HEIGHT_PORT_NAME).WithDefaultValue(1024).Build();

            OnDefineInputPorts(context);
        }

        protected virtual void OnDefineInputPorts(IPortDefinitionContext context)
        {
        }

        /// <summary>
        /// Initializes the node. This is called before any execution.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Completes the execution of the node. This is called after all executions are done.
        /// </summary>
        public virtual void Complete()
        {
        }
    }
}