using CommonLibrary.Containers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CommonLibrary
{
    [Serializable]
    public abstract class UserEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public List<ImageContainer> Images { get; set; }

        [NotMapped]
        public ImageSource Avatar => Images == null && Images.Count == 0 ? null : Images[0].ImageSource;

        public virtual void AddImage(string path)
        {
            if (Images == null)
                Images = new List<ImageContainer>();
            Images.Add(ImageContainer.FromFile(path));
        }
    }
}
