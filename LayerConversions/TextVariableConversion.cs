using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PluginToChili.LayerConversions
{
    public class TextVariableConversion : ILayerConversion
    {
        public IEnumerable<string> LayerTags { get; set; }

        private List<string> layerFrameIds = new List<string>();
        public IEnumerable<string> LayerFrameIds { get { return layerFrameIds; } }
        public string LayerId { get; set; }

        public IEnumerable<string> FrameTypes { get { return new string[] { "text" }; } }

        public string ConversionName { get { return "Text Variables"; } }

        private static List<Variable> variables = new List<Variable>();

        public void FrameConversion(XmlNode frameNode, string frameName)
        {
            string id = frameNode.Attributes["id"].Value;

            layerFrameIds.Add(id);

            if (frameNode.Attributes["name"] == null)
            {
                XmlAttribute tagAtrribute = frameNode.OwnerDocument.CreateAttribute("name");
                frameNode.Attributes.Append(tagAtrribute);
            }

            frameNode.Attributes["name"].Value = frameName;

            if (frameNode.Attributes["tag"] == null)
            {
                XmlAttribute tagAtrribute = frameNode.OwnerDocument.CreateAttribute("tag");
                frameNode.Attributes.Append(tagAtrribute);
            }

            frameNode.Attributes["tag"].Value = $"{frameName}-{layerFrameIds.Count}";


            if (frameNode.Attributes["isVariable"] == null)
            {
                XmlAttribute variableAtrribute = frameNode.OwnerDocument.CreateAttribute("isVariable");
                frameNode.Attributes.Append(variableAtrribute);
            }

            frameNode.Attributes["isVariable"].Value = "true";

            XmlNode textFlowNode = null;

            foreach (XmlNode xmlNode in frameNode.ChildNodes)
            {
                if (xmlNode.Name == "textFlow")
                {
                    textFlowNode = xmlNode.FirstChild;
                }
            }


            // TODO: Need to refactor this for multiline text
            //? What if the text has special bullets points or is formatted???

            XmlNode paragraphNode = textFlowNode.FirstChild;

            /* BIG NOTE TO DEV TEAM */
            // -----------------------------------------------------------------
            // We assume here that we want the whole text to be a variable, but 
            // instead we could require variables to be labeled with a {} or %%
            // This would allow us to refactor our code to be much simplier - I like that idea :)
            XmlNode spanNode = paragraphNode.FirstChild;

            // We need to see if the variable already exist with this value because then they are the same variables
            // We assume the designer has the same values for every textbox in Illustrator or InDesign that is meant to be a variable
            int findIndex = variables.FindIndex(v => v.value == spanNode.InnerText);

            if (findIndex == -1)
            {
                // TODO: Get name from span tag, so if {variable} or %variable%
                //? What if we have multiple variables in a span tag???
                //? Or worse... what if we have a multiline text with multiple variables

                // Get  random name from frame id
                string name = id.Substring(0, 8);
                Variable newVariable = new Variable(spanNode.InnerXml, name);

                variables.Add(newVariable);
                spanNode.InnerText = $"%{name}%";

                newVariable.AddVariableToDocument(frameNode.OwnerDocument);
            }
            else
            {
                string name = variables[findIndex].name;
                spanNode.InnerText = $"%{name}%";
            }  
        }

        public TextVariableConversion(IEnumerable<string> tags)
        {
            LayerTags = tags;
        }

        private struct Variable
        {
            public string value;
            public string name;

            public Variable(string value, string name)
            {
                this.value = value;
                this.name = name;
            }

            public void AddVariableToDocument(XmlDocument xmlDocument)
            {
                XmlNode documentNode = xmlDocument.SelectSingleNode("//document");

                XmlNode variablesNode = documentNode.SelectSingleNode("./variables");

                if (variablesNode == null)
                {
                    variablesNode = xmlDocument.CreateElement("variables");
                    documentNode.AppendChild(variablesNode);
                };

                    XmlNode variableNode = xmlDocument.CreateElement("item");

                    XmlAttribute attr = xmlDocument.CreateAttribute("id");
                    attr.Value = Guid.NewGuid().ToString();
                    variableNode.Attributes.Append(attr);

                    attr = xmlDocument.CreateAttribute("name");
                    attr.Value = name;
                    variableNode.Attributes.Append(attr);

                    attr = xmlDocument.CreateAttribute("displayName");
                    attr.Value = name;
                    variableNode.Attributes.Append(attr);

                    attr = xmlDocument.CreateAttribute("value");
                    attr.Value = value;
                    variableNode.Attributes.Append(attr);

                    attr = xmlDocument.CreateAttribute("displayValue");
                    attr.Value = value;
                    variableNode.Attributes.Append(attr);

                    variablesNode.AppendChild(variableNode);
            }
        }
    }
}
