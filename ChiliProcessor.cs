using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace PluginToChili
{
    public class ChiliProcessor
    {
        public string documentXml;

        private XmlDocument chiliDocument;

        public void LoadChiliDocumentXml(string documentXmlString)
        {
            if (string.IsNullOrEmpty(documentXmlString))
            {
                throw new ArgumentException("message", nameof(documentXmlString));
            }

            chiliDocument = new XmlDocument();

            try
            {
                this.chiliDocument.LoadXml(documentXmlString);
            }
            catch (XmlException e)
            {
                throw e;
            }

        }

        //private List<Theme> themes = new List<Theme>();
        public string ProcessDocument(string parentlessThemeName, IEnumerable<string> themeTags, IEnumerable<ILayerConversion> layerConversions = null)
        {
            // Get the main document node
            //XmlNode documentNode = chiliDocument.SelectSingleNode("//document");

            // Process Layer Nodes
            List<Theme> themes = ProcessLayersToThemes(chiliDocument, parentlessThemeName, themeTags, layerConversions).ToList();
            ProcessFrames(themes, chiliDocument);
            AddPrivateData(themes, chiliDocument);

            return chiliDocument.InnerXml;
        }

        private IEnumerable<Theme> ProcessLayersToThemes(XmlDocument xmlDocument, string parentlessThemeName, IEnumerable<string> themeTags = null, IEnumerable<ILayerConversion> layerConversions = null)
        {
            // Get the main document node
            XmlNode documentNode = chiliDocument.SelectSingleNode("//document");

            XmlNodeList layers = documentNode.SelectNodes("./layers/item");

            List<Theme> themes = new List<Theme>()
            {
                new Theme("parentlessThemeName")
            };

            foreach (XmlNode layerNode in layers)
            {
                //? Should we lower it - what if we want tags to be case sensitive???
                string layerName = layerNode.Attributes["name"].Value.ToLower();
                string layerId = layerNode.Attributes["id"].Value;

                Theme currentTheme = themes[0];

                if (themeTags != null)
                {
                    // Let's look for themes first
                    foreach (string themeTag in themeTags)
                    {
                        if (layerName.Contains(themeTag))
                        {
                            string themeName = GetThemeNameFromLayerName(layerName, themeTag);

                            // Is this a new theme?
                            int index = themes.FindIndex(t => t.name == themeName);

                            // No theme exist, lets create one and add to themes list
                            if (index == -1)
                            {
                                currentTheme = new Theme(themeName);

                                themes.Add(currentTheme);
                            }
                            // Themes does exist - so this is a sub layer, please get theme
                            else
                            {
                                currentTheme = themes[index];
                            }
                        }
                    }
                }

                if (layerConversions != null)
                {
                    // Go through each layer conversion and assign it the layer if it meets the proper tag
                    foreach (ILayerConversion layerConversion in layerConversions)
                    {
                        foreach (string tag in layerConversion.LayerTags)
                        {
                            if (layerName.Contains(tag))
                            {
                                layerConversion.LayerId = layerId;
                                currentTheme.AddLayerConversion(layerConversion);
                                break;
                            }
                        }
                    }
                }


            }

            return themes;
        }

        private void ProcessFrames(IEnumerable<Theme> themes, XmlNode xmlDocument)
        {
            // Get the main document node
            XmlNode documentNode = chiliDocument.SelectSingleNode("//document");

            // We must go through each frame
            foreach (Theme theme in themes)
            {
                // Then we must go through each LayerConversion
                foreach (ILayerConversion layerConversion in theme.layerConversions)
                {
                    // Then we must go through each frametype
                    foreach (string frameType in layerConversion.FrameTypes)
                    {
                        // Then we must for through each frame that matches the layer and frame type
                        foreach (XmlNode frameNode in documentNode.SelectNodes($"//pages/item/frames/item[@layer='{layerConversion.LayerId}' and @type='{frameType}' ]"))
                        {
                            // Change the frame name
                            string frameName = $"{theme.name}-{layerConversion.ConversionName}";

                            // Run the conversion on the frame
                            layerConversion.FrameConversion(frameNode, frameName);
                        }
                    }
                }
            }
        }

        private void AddPrivateData(IEnumerable<Theme> themes, XmlDocument xmlDocument)
        {
            // Get the main document node
            XmlNode documentNode = xmlDocument.SelectSingleNode("//document");

            XmlNode privateData = null;

            privateData = documentNode.SelectSingleNode("./privateData");

            if (privateData == null)
            {
                privateData = xmlDocument.CreateElement("privateData");
            }

            XmlNode itemNode = xmlDocument.CreateElement("item");

            XmlAttribute pdAttribute = xmlDocument.CreateAttribute("tag");
            pdAttribute.Value = "themes";
            itemNode.Attributes.Append(pdAttribute);

            pdAttribute = xmlDocument.CreateAttribute("id");
            pdAttribute.Value = Guid.NewGuid().ToString();
            itemNode.Attributes.Append(pdAttribute);

            pdAttribute = xmlDocument.CreateAttribute("value");
            pdAttribute.Value = JsonConvert.SerializeObject(themes);
            itemNode.Attributes.Append(pdAttribute);

            privateData.AppendChild(itemNode);

            documentNode.AppendChild(privateData);
        }

        private string GetThemeNameFromLayerName(string layerName, string themeTag)
        {
            if (layerName.Contains(themeTag))
            {

                int layerCutoffIndex = layerName.IndexOf("/");

                if (layerCutoffIndex == -1)
                {
                    layerCutoffIndex = layerName.Length;
                }

                int themeStartIndex = layerName.IndexOf(themeTag);
                int themeEndIndex = themeStartIndex + (themeTag.Length - 1);

                int lengthName = layerCutoffIndex - themeEndIndex - 1;

                string themeName = (layerName.Substring((themeStartIndex + themeTag.Length), lengthName)).Trim();

                return themeName;
            }

            return "";
        }
    
    }
}
