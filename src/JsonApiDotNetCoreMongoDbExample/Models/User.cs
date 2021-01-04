using System;
using JsonApiDotNetCore.MongoDb.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreMongoDbExample.Models
{
    public class User : MongoDbIdentifiable
    {
        private string _password;

        [Attr] public string UserName { get; set; }

        [Attr(Capabilities = AttrCapabilities.AllowCreate | AttrCapabilities.AllowChange)]
        public string Password
        {
            get => _password;
            set
            {
                if (value != _password)
                {
                    _password = value;
                    LastPasswordChange = DateTime.UtcNow.ToLocalTime();
                }
            }
        }

        [Attr] public DateTime LastPasswordChange { get; set; }
    }
}
