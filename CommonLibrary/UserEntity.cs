using CommonLibrary.Containers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Media;

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
