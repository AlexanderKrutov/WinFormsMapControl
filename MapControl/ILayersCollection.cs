using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    public interface ILayersCollection : IReadOnlyCollection<Layer>
    {
        void Add(Layer layer);
        bool Remove(Layer layer);
        void RemoveAt(int index);
        Layer this[int index] { get; set; }

        event LayersCollectionChangedDelegate LayersCollectionBeforeChange;
        event LayersCollectionChangedDelegate LayersCollectionAfterChange;
    }

    public delegate void LayersCollectionChangedDelegate(object sender, EventArgs eventArgs);

}
