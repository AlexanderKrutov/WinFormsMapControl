using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Forms
{
    internal class LayersCollection : ILayersCollection
    {
        private List<Layer> _Layers = new List<Layer>();

        public int Count => _Layers.Count;

        public Layer this[int index]
        {
            get => _Layers[index];
            set {

                LayersCollectionBeforeChange?.Invoke(this, EventArgs.Empty);



                _Layers[index] = value;
                LayersCollectionAfterChange?.Invoke(this, EventArgs.Empty);
            }
        }

        public event LayersCollectionChangedDelegate LayersCollectionBeforeChange;
        public event LayersCollectionChangedDelegate LayersCollectionAfterChange;

        public void Add(Layer layer)
        {
            LayersCollectionBeforeChange?.Invoke(this, EventArgs.Empty);
            _Layers.Add(layer);
            LayersCollectionAfterChange?.Invoke(this, EventArgs.Empty);
        }

        public bool Remove(Layer layer)
        {
            try
            {
                LayersCollectionBeforeChange?.Invoke(this, EventArgs.Empty);
                return _Layers.Remove(layer);
            }
            finally
            {
                LayersCollectionAfterChange?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveAt(int index)
        {
            LayersCollectionBeforeChange?.Invoke(this, EventArgs.Empty);
            _Layers.RemoveAt(index);
            LayersCollectionAfterChange?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerator<Layer> GetEnumerator() => _Layers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _Layers.GetEnumerator();
    }
}
