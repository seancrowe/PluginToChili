This is a work in progress library for doing some logic from a document create by the Illustrator or InDesign plugin.

Requirements:
.Net Core 2.2
Newtonsoft.Json

How to use?
Add the PluginToChili.dll as a reference to your project

Then you need to create an instance of ChiliProcessor by:

  PluginToChili.ChiliProcessor chiliProcessor = new PluginToChili.ChiliProcessor();



Load your document into the processor by simply calling the method chiliProcessor.LoadChiliDocumentXml(). The method takes one parameter: the string version of your CHILI document XML.


Then use chiliProcessor.ProcessDocument() to process the document. This method requires three parameters:
  * parentlessThemeName : Name of the base theme which all layers that don't have a theme tag will be added to
  * themeTags : Tags for themes that will be used to identify themes on layers
  * layerConversions : ILayerConversions that allow you to add function of converting frames under layers
  
  
  ILayerConversion is how you modify the document (add logic) based on tags. Please see TextVariableConversion as an example of how to turn all text frames (in the proper layer with the proper tag) into variables. 