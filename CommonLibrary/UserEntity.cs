using System;
using System.Collections.Generic;

namespace CommonLibrary
{
    [Serializable]
    public abstract class UserEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public List<int> ImagesId { get; set; }
            = new List<int>();
    }
}
