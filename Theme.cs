using System;
using System.Collections.Generic;
using System.Text;

namespace PluginToChili
{
    class Theme
    {
        public string name;
        public List<ILayerConversion> layerConversions = new List<ILayerConversion>();

        public Theme(string name)
        {
            this.name = name;
        }

        public void AddLayerConversion(ILayerConversion layerConversion)
        {
            if (!layerConversions.Contains(layerConversion))
            {
                layerConversions.Add(layerConversion);
            }
        }
    }
}
