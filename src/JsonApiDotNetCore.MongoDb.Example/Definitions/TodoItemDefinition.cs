using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.MongoDb.Example.Models;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.MongoDb.Example.Definitions
{
    public sealed class TodoItemDefinition : JsonApiResourceDefinition<TodoItem, string>
    {
        public TodoItemDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
        {
        }

        public override IDictionary<string, object> GetMeta(TodoItem resource)
        {
            if (resource.Description != null && resource.Description.StartsWith("Important:"))
            {
                return new Dictionary<string, object>
                {
                    ["hasHighPriority"] = true
                };
            }
            
            return base.GetMeta(resource);
        }
    }
}
