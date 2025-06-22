using System;

namespace YooAsset.Editor
{
    public class BuildPipelineAttribute : Attribute
    {
        public string PipelineName;

        public BuildPipelineAttribute(string name)
        {
            this.PipelineName = name;
        }
    }
}