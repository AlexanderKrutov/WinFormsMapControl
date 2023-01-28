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

                _Layers[index] = value;
                LayersCollectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event LayersCollectionChangedDelegate LayersCollectionChanged;

        public void Add(Layer layer)
        {
            _Layers.Add(layer);
            LayersCollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool Remove(Layer layer)
        {
            try
            {
                return _Layers.Remove(layer);
            }
            finally
            {
                LayersCollectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveAt(int index)
        {
            _Layers.RemoveAt(index);
        }

        public IEnumerator<Layer> GetEnumerator() => _Layers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _Layers.GetEnumerator();
    }
}
