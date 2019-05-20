using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PluginToChili
{
    public interface ILayerConversion
    {   
        /// <summary>
        /// Contains all tags to grab the layers during processing.
        /// For example, the layer may be THEME 1/TEXT, thus the tag you would want to add is TEXT
        /// </summary>
        IEnumerable<string> LayerTags { get; set; }

        /// <summary>
        /// These are the ids of frames that are added via FrameConversion
        /// </summary>
        IEnumerable<string> LayerFrameIds { get; }

        /// <summary>
        /// Id of the layer this object is tied to
        /// Set by ProcessLayersToThemes
        /// <see cref="Converter.ProcessLayersToThemes(XmlNode, string, IEnumerable{string}, IEnumerable{ILayerConversion})"/>
        /// </summary>
        string LayerId { get; set; }

        /// <summary>
        /// Frame type that is used to select frames sent to FrameConversion
        /// </summary>
        IEnumerable<string> FrameTypes { get; }

        /// <summary>
        /// Name of the layer conversion
        /// </summary>
        string ConversionName { get; }

        // Script to be ran on frames that meet the FrameType
        void FrameConversion(XmlNode frameNode, string frameName);
    }
}
